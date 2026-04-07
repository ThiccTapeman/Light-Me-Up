using System;
using System.Collections.Generic;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using Runtime.ReactiveWorld.Reactor;
using Runtime.ReactiveWorld.Events;
using UnityEngine;
using Runtime.ReactiveWorld.Partitions;

namespace Runtime.ReactiveWorld
{
    /// <summary>
    /// Central hub of the ReactiveWorld system.
    /// Manages the lifecycle of <see cref="IReactor"/> instances and handles
    /// typed event dispatching via a pub/sub pattern.
    /// <para>
    /// Reactors register themselves on <see cref="IReactor.Initialize"/> and
    /// unregister on <see cref="IReactor.Shutdown"/>. Any system can publish
    /// events via <see cref="Raise{TEvent}"/> without knowing who is listening.
    /// </para>
    /// </summary>
    public partial class WorldManager : MonoBehaviour
    {
        private static WorldManager instance;
        private static readonly IReadOnlyList<IReactor> EmptyReactors = Array.Empty<IReactor>();

        [SerializeField] private float _pendingEventLifetime = 30f;

        /// <summary>
        /// Spatial partitioning system. Exposes area queries (position lookup, reactor listing)
        /// and is populated automatically during <c>Awake</c> from scene <see cref="AreaVolume"/> components.
        /// </summary>
        public PartitionManager Partitions { get; private set; }

        public bool IsFlushing { get; private set; }

        /// <summary>All reactors currently registered in the world.</summary>
        public IReadOnlyList<IReactor> Reactors => _reactors;

        /// <summary>All reactors currently registered in the world.</summary>
        private List<IReactor> _reactors = new();

        /// <summary>
        /// Event subscribers indexed by event type.
        /// Each key holds a list of callbacks to invoke when that event is raised.
        /// </summary>
        private Dictionary<Type, List<Delegate>> _subscribers = new();

        private Dictionary<Type, List<(object evt, float timestamp)>> _pendingEvents = new();

        /// <summary>
        /// Gets or creates new instance for WorldManager
        /// </summary>
        /// <returns>The instance for WorldManager</returns>
        public static WorldManager GetInstance()
        {
            if (instance != null) return instance;

            instance = FindFirstObjectByType<WorldManager>();

            if (instance != null) return instance;

            GameObject gameObject = new("WorldManager");
            instance = gameObject.AddComponent<WorldManager>();

            return instance;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;

            Partitions = new PartitionManager();

            foreach (var area in FindObjectsByType<AreaVolume>(FindObjectsSortMode.None))
                Partitions.RegisterArea(area);
        }

        /// <summary>
        /// Registers a reactor in the world. Has no effect if already registered.
        /// Called automatically by <see cref="BaseReactor.Initialize"/>.
        /// </summary>
        /// <param name="reactor">The reactor to register.</param>
        public void Register(IReactor reactor)
        {
            if (reactor == null)
            {
                Debug.LogWarning("[WorldManager] Tried to register a null reactor.");
                return;
            }

            if (!_reactors.Contains(reactor))
            {
                if (reactor is IAreaReactor areaReactor)
                {
                    Partitions.Register(areaReactor.AreaId, reactor);
                }

                _reactors.Add(reactor);
#if UNITY_EDITOR
                GetOrCreateReactorStats(reactor);
#endif
                Debug.Log($"[WorldManager] Reactor '{reactor.Name}' registered.");
            }
            else
            {
                Debug.LogWarning($"[WorldManager] Reactor '{reactor.Name}' is already registered.");
            }
        }

        /// <summary>
        /// Unregisters a reactor from the world. Has no effect if not found.
        /// Called automatically by <see cref="BaseReactor.Shutdown"/>.
        /// </summary>
        /// <param name="reactor">The reactor to unregister.</param>
        public void Unregister(IReactor reactor)
        {
            if (reactor == null)
            {
                Debug.LogWarning("[WorldManager] Tried to unregister a null reactor.");
                return;
            }

            if (_reactors.Contains(reactor))
            {
                if (reactor is IAreaReactor areaReactor)
                {
                    Partitions.Unregister(areaReactor.AreaId, reactor);
                }

                _reactors.Remove(reactor);
#if UNITY_EDITOR
                _reactorEventStats.Remove(reactor);
#endif
                Debug.Log($"[WorldManager] Reactor '{reactor.Name}' unregistered.");
            }
            else
            {
                Debug.LogWarning($"[WorldManager] Tried to unregister reactor '{reactor.Name}', but it was not found.");
            }
        }

        /// <summary>
        /// Subscribes a callback to a specific event type.
        /// The callback will be invoked every time <see cref="Raise{TEvent}"/> is called
        /// with a matching event type.
        /// </summary>
        /// <typeparam name="TEvent">The event type to listen to.</typeparam>
        /// <param name="callback">The method to call when the event is raised.</param>
        public void Subscribe<TEvent>(Action<TEvent> callback) where TEvent : IWorldEvent
        {
            var key = typeof(TEvent);
            if (!_subscribers.ContainsKey(key))
                _subscribers[key] = new List<Delegate>();
            _subscribers[key].Add(callback);

            Debug.Log($"[WorldManager] Subscribed to '{key.Name}' (method: {callback.Method.Name}).");

            // Flush pending events for this type
            if (_pendingEvents.ContainsKey(key) && _pendingEvents[key].Count > 0)
            {
                var pending = _pendingEvents[key];
                var now = Time.time;
                var flushed = 0;

                for (int i = pending.Count - 1; i >= 0; i--)
                {
                    if (now - pending[i].timestamp > _pendingEventLifetime)
                    {
                        pending.RemoveAt(i);
                        continue;
                    }

                    IsFlushing = true;
                    Raise((TEvent)pending[i].evt);
                    flushed++;
                    pending.RemoveAt(i);
                }

                IsFlushing = false;

                if (flushed > 0)
                    Debug.Log($"[WorldManager] Flushed {flushed} pending '{key.Name}' event(s).");
            }
        }

        /// <summary>
        /// Unsubscribes a callback from a specific event type.
        /// Must be called in <see cref="IReactor.Shutdown"/> to avoid memory leaks.
        /// </summary>
        /// <typeparam name="TEvent">The event type to stop listening to.</typeparam>
        /// <param name="callback">The callback to remove.</param>
        public void Unsubscribe<TEvent>(Action<TEvent> callback) where TEvent : IWorldEvent
        {
            var key = typeof(TEvent);
            if (_subscribers.ContainsKey(key))
            {
                _subscribers[key].Remove(callback);
                Debug.Log($"[WorldManager] Unsubscribed from '{key.Name}' (method: {callback.Method.Name}).");
            }
            else
            {
                Debug.LogWarning($"[WorldManager] Tried to unsubscribe from '{key.Name}', but no subscribers were found.");
            }
        }

        /// <summary>
        /// Publishes an event to all subscribers registered for its type.
        /// Logs a warning if no subscriber is listening, which may indicate a missing initialization.
        /// </summary>
        /// <typeparam name="TEvent">The event type to raise.</typeparam>
        /// <param name="evt">The event instance to dispatch.</param>
        public void Raise<TEvent>(TEvent evt) where TEvent : IWorldEvent
        {
            if (evt == null)
            {
                Debug.LogWarning("[WorldManager] Tried to raise a null event.");
                return;
            }

            RaiseInternal(null, evt);
        }

        /// <summary>
        /// Publishes an event on behalf of a specific reactor. Used to track event sources in the debug overlay.
        /// </summary>
        /// <typeparam name="TEvent">The event type to raise.</typeparam>
        /// <param name="reactor">The reactor raising the event.</param>
        /// <param name="evt">The event instance to dispatch.</param>
        public void Raise<TEvent>(IReactor reactor, TEvent evt) where TEvent : IWorldEvent
        {
            if (evt == null)
            {
                Debug.LogWarning("[WorldManager] Tried to raise a null event.");
                return;
            }

            RaiseInternal(reactor, evt);
        }

        /// <summary>
        /// Core logic for dispatching events to subscribers. Handles tracing and logging for the debug overlay.
        /// </summary>
        /// <typeparam name="TEvent">The event type to raise.</typeparam>
        /// <param name="reactor">The reactor raising the event.</param>
        /// <param name="evt">The event instance to dispatch.</param>
        private void RaiseInternal<TEvent>(IReactor reactor, TEvent evt) where TEvent : IWorldEvent
        {
            var key = typeof(TEvent);

            if (!TryGetSubscribers(key, reactor, out var subscribers))
                return;

            var eventTrace = BeginRaiseTrace(key, reactor, subscribers.Count);

            try
            {
                foreach (var sub in subscribers)
                {
                    var subscriberTrace = BeginSubscriberTraceScope(eventTrace, sub);

                    try
                    {
                        ((Action<TEvent>)sub)?.Invoke(evt);
                    }
                    catch (Exception exception)
                    {
                        MarkSubscriberTraceException(subscriberTrace, exception);
                        throw;
                    }
                    finally
                    {
                        EndSubscriberTraceScope(sub, subscriberTrace);
                    }
                }
            }
            catch (Exception exception)
            {
                MarkEventTraceException(eventTrace, exception);
                throw;
            }
            finally
            {
                EndRaiseTrace(key, subscribers.Count, eventTrace);
            }
        }

        /// <summary>
        /// Attempts to retrieve the list of subscribers for a given event type. If no subscribers are found, logs a warning.
        /// </summary>
        /// <param name="key">The event type for which to retrieve subscribers.</param>
        /// <param name="reactor">The reactor raising the event.</param>
        /// <param name="subscribers">The list of subscribers, if found.</param>
        /// <returns>True if subscribers are found, otherwise false.</returns>
        private bool TryGetSubscribers(Type key, IReactor reactor, out List<Delegate> subscribers)
        {
            if (_subscribers.TryGetValue(key, out subscribers) && subscribers.Count > 0)
                return true;

            HandleNoSubscribers(key, reactor);
            subscribers = null;
            return false;
        }

        /// <summary>
        /// Handles the case where an event is raised but no subscribers are listening. Completes the event trace for the debug overlay and logs a warning.
        /// </summary>
        /// <param name="key">The event type for which no subscribers were found.</param>
        /// <param name="reactor">The reactor raising the event.</param>
        private void HandleNoSubscribers(Type key, IReactor reactor)
        {
            CompleteEmptyRaiseTrace(key, reactor);
            Debug.LogWarning($"[WorldManager] Event '{key.Name}' raised but no subscribers are listening.");
        }

        /// <summary>
        /// Tries to find the first registered reactor with the given name.
        /// </summary>
        public bool TryGetReactor(string reactorName, out IReactor reactor)
        {
            reactor = _reactors.Find(current => current.Name == reactorName);
            return reactor != null;
        }

        /// <summary>
        /// Returns all registered reactors for the provided area.
        /// </summary>
        public IReadOnlyList<IReactor> GetReactorsForArea(string areaId)
        {
            if (Partitions == null || string.IsNullOrWhiteSpace(areaId))
                return EmptyReactors;

            return Partitions.GetReactors(areaId);
        }

        /// <summary>
        /// Enables or disables a registered reactor.
        /// </summary>
        public bool SetReactorEnabled(IReactor reactor, bool enabled)
        {
            if (reactor == null)
            {
                Debug.LogWarning("[WorldManager] SetReactorEnabled failed: reactor is null.");
                return false;
            }

            if (!_reactors.Contains(reactor))
            {
                Debug.LogWarning($"[WorldManager] SetReactorEnabled failed: reactor '{reactor.Name}' is not registered.");
                return false;
            }

            reactor.IsEnabled = enabled;
            Debug.Log($"[WorldManager] Reactor '{reactor.Name}' set to {(enabled ? "enabled" : "disabled")}.");
            return true;
        }

        public void RaiseWhenListener<TEvent>(TEvent evt) where TEvent : IWorldEvent
        {
            var key = typeof(TEvent);

            if (_subscribers.ContainsKey(key) && _subscribers[key].Count > 0)
            {
                Raise(evt);
                return;
            }

            if (!_pendingEvents.ContainsKey(key))
                _pendingEvents[key] = new List<(object evt, float timestamp)>();
            
            _pendingEvents[key].Add((evt, Time.time));
            Debug.Log($"[WorldManager] Event '{key.Name}' queued (no subscribers yet)");
        }

        /// <summary>
        /// Enables or disables a registered reactor by name.
        /// </summary>
        public bool SetReactorEnabled(string reactorName, bool enabled)
        {
            if (!TryGetReactor(reactorName, out var reactor))
            {
                Debug.LogWarning($"[WorldManager] SetReactorEnabled failed: reactor '{reactorName}' not found.");
                return false;
            }

            return SetReactorEnabled(reactor, enabled);
        }

        /// <summary>
        /// Removes a registered reactor from the world and optionally destroys its backing component.
        /// </summary>
        public bool RemoveReactor(IReactor reactor, bool destroyComponent = true)
        {
            if (reactor == null)
            {
                Debug.LogWarning("[WorldManager] RemoveReactor failed: reactor is null.");
                return false;
            }

            if (!_reactors.Contains(reactor))
            {
                Debug.LogWarning($"[WorldManager] RemoveReactor failed: reactor '{reactor.Name}' is not registered.");
                return false;
            }

            reactor.Shutdown();

            if (destroyComponent && reactor is Component component)
                Destroy(component);

            return true;
        }

        /// <summary>
        /// Removes a registered reactor by name and optionally destroys its backing component.
        /// </summary>
        public bool RemoveReactor(string reactorName, bool destroyComponent = true)
        {
            if (!TryGetReactor(reactorName, out var reactor))
            {
                Debug.LogWarning($"[WorldManager] RemoveReactor failed: reactor '{reactorName}' not found.");
                return false;
            }

            return RemoveReactor(reactor, destroyComponent);
        }

    }
}
