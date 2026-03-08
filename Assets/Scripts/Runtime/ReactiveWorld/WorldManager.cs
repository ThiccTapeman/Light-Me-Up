using System;
using System.Collections.Generic;
using Runtime.ReactiveWorld.Reactor;
using Runtime.ReactiveWorld.Events;
using UnityEngine;

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
    public class WorldManager
    {
        /// <summary>All reactors currently registered in the world.</summary>
        private List<IReactor> _reactors  = new();

        /// <summary>
        /// Event subscribers indexed by event type.
        /// Each key holds a list of callbacks to invoke when that event is raised.
        /// </summary>
        private Dictionary<Type, List<Delegate>> _subscribers = new();

        /// <summary>
        /// Registers a reactor in the world. Has no effect if already registered.
        /// Called automatically by <see cref="BaseReactor.Initialize"/>.
        /// </summary>
        /// <param name="reactor">The reactor to register.</param>
        public void Register(IReactor reactor)
        {
            if (!_reactors.Contains(reactor))
            {
                _reactors.Add(reactor);
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
            if (_reactors.Contains(reactor))
            {
                _reactors.Remove(reactor);
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
            var key = typeof(TEvent);
            if (!_subscribers.ContainsKey(key) || _subscribers[key].Count == 0)
            {
                Debug.LogWarning($"[WorldManager] Event '{key.Name}' raised but no subscribers are listening.");
                return;
            }
            Debug.Log($"[WorldManager] Raising event '{key.Name}' to {_subscribers[key].Count} subscriber(s).");
            foreach (var sub in _subscribers[key])
                ((Action<TEvent>)sub)?.Invoke(evt);
        }

        /// <summary>
        /// Enables or disables a registered reactor by name.
        /// Has no effect if the reactor name is not found.
        /// </summary>
        /// <param name="reactorName">The <see cref="IReactor.Name"/> of the target reactor.</param>
        /// <param name="enabled"><c>true</c> to enable, <c>false</c> to disable.</param>
        public void SetReactorEnabled(string reactorName, bool enabled)
        {
            var currentReactor = _reactors.Find((reactor) => reactor.Name == reactorName);
            if (currentReactor != null)
            {
                currentReactor.IsEnabled = enabled;
                Debug.Log($"[WorldManager] Reactor '{reactorName}' set to {(enabled ? "enabled" : "disabled")}.");
            }
            else
            {
                Debug.LogWarning($"[WorldManager] SetReactorEnabled: reactor '{reactorName}' not found.");
            }
        }
    }
}
