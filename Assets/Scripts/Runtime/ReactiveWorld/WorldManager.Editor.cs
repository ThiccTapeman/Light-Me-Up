using System;
using System.Collections.Generic;
using System.Diagnostics;
using Runtime.ReactiveWorld.Reactor;

namespace Runtime.ReactiveWorld
{
    public partial class WorldManager
    {
#if UNITY_EDITOR
        private const int MaxPerformanceSamples = 60;
        private const int MaxEventTraceRoots = 40;

        private readonly Queue<RaisePerformanceSample> _raisePerformanceSamples = new();
        private readonly Dictionary<IReactor, ReactorEventStats> _reactorEventStats = new();
        private readonly List<EventTraceSnapshot> _eventTraceRoots = new();
        private readonly Stack<SubscriberTraceSnapshot> _activeSubscriberTraceStack = new();

        private double _totalRaiseDurationMs;

        public readonly struct RaisePerformanceSnapshot
        {
            public RaisePerformanceSnapshot(int callsInLast60, double averageCallDurationMs, double lastCallDurationMs, int lastSubscriberCount, string lastEventName)
            {
                CallsInLast60 = callsInLast60;
                AverageCallDurationMs = averageCallDurationMs;
                LastCallDurationMs = lastCallDurationMs;
                LastSubscriberCount = lastSubscriberCount;
                LastEventName = lastEventName;
            }

            public int CallsInLast60 { get; }
            public double AverageCallDurationMs { get; }
            public double LastCallDurationMs { get; }
            public int LastSubscriberCount { get; }
            public string LastEventName { get; }
        }

        private readonly struct RaisePerformanceSample
        {
            public RaisePerformanceSample(string eventName, double durationMs, int subscriberCount)
            {
                EventName = eventName;
                DurationMs = durationMs;
                SubscriberCount = subscriberCount;
            }

            public string EventName { get; }
            public double DurationMs { get; }
            public int SubscriberCount { get; }
        }

        public readonly struct ReactorEventStatsSnapshot
        {
            public ReactorEventStatsSnapshot(int eventsRecieved, double averageCallTimeMs, double lastCallTimeMs)
            {
                EventsRecieved = eventsRecieved;
                AverageCallTimeMs = averageCallTimeMs;
                LastCallTimeMs = lastCallTimeMs;
            }

            public int EventsRecieved { get; }
            public double AverageCallTimeMs { get; }
            public double LastCallTimeMs { get; }
        }

        private sealed class ReactorEventStats
        {
            public int EventsRecieved;
            public double TotalCallTimeMs;
            public double LastCallTimeMs;
        }

        public sealed class EventTraceSnapshot
        {
            public EventTraceSnapshot(string eventName, string sourceName, int subscriberCount)
            {
                EventName = eventName;
                SourceName = sourceName;
                SubscriberCount = subscriberCount;
                Subscribers = new List<SubscriberTraceSnapshot>();
            }

            public string EventName { get; }
            public string SourceName { get; }
            public int SubscriberCount { get; }
            public double DurationMs { get; internal set; }
            public List<SubscriberTraceSnapshot> Subscribers { get; }
        }

        public sealed class SubscriberTraceSnapshot
        {
            public SubscriberTraceSnapshot(string targetName, string targetTypeName, string methodName)
            {
                TargetName = targetName;
                TargetTypeName = targetTypeName;
                MethodName = methodName;
                ChildEvents = new List<EventTraceSnapshot>();
            }

            public string TargetName { get; }
            public string TargetTypeName { get; }
            public string MethodName { get; }
            public double DurationMs { get; internal set; }
            public List<EventTraceSnapshot> ChildEvents { get; }
        }

        public RaisePerformanceSnapshot GetRaisePerformanceSnapshot()
        {
            if (_raisePerformanceSamples.Count == 0)
                return new RaisePerformanceSnapshot(0, 0d, 0d, 0, "None");

            var lastSample = default(RaisePerformanceSample);
            foreach (var sample in _raisePerformanceSamples)
                lastSample = sample;

            return new RaisePerformanceSnapshot(
                _raisePerformanceSamples.Count,
                _totalRaiseDurationMs / _raisePerformanceSamples.Count,
                lastSample.DurationMs,
                lastSample.SubscriberCount,
                lastSample.EventName);
        }

        public ReactorEventStatsSnapshot GetReactorEventStats(IReactor reactor)
        {
            if (reactor == null || !_reactorEventStats.TryGetValue(reactor, out var stats))
                return new ReactorEventStatsSnapshot(0, 0d, 0d);

            var averageCallTimeMs = stats.EventsRecieved > 0 ? stats.TotalCallTimeMs / stats.EventsRecieved : 0d;
            return new ReactorEventStatsSnapshot(stats.EventsRecieved, averageCallTimeMs, stats.LastCallTimeMs);
        }

        public IReadOnlyList<EventTraceSnapshot> GetRecentEventTraces()
        {
            return _eventTraceRoots;
        }

        private void RecordRaisePerformance(string eventName, double durationMs, int subscriberCount)
        {
            var sample = new RaisePerformanceSample(eventName, durationMs, subscriberCount);
            _raisePerformanceSamples.Enqueue(sample);
            _totalRaiseDurationMs += durationMs;

            while (_raisePerformanceSamples.Count > MaxPerformanceSamples)
            {
                var removedSample = _raisePerformanceSamples.Dequeue();
                _totalRaiseDurationMs -= removedSample.DurationMs;
            }
        }

        private EventTraceSnapshot BeginEventTrace(string eventName, IReactor sourceReactor, int subscriberCount)
        {
            var sourceName = sourceReactor != null ? sourceReactor.Name : "External";
            var trace = new EventTraceSnapshot(eventName, sourceName, subscriberCount);

            if (_activeSubscriberTraceStack.Count > 0)
            {
                _activeSubscriberTraceStack.Peek().ChildEvents.Add(trace);
                return trace;
            }

            _eventTraceRoots.Add(trace);
            if (_eventTraceRoots.Count > MaxEventTraceRoots)
                _eventTraceRoots.RemoveAt(0);

            return trace;
        }

        private SubscriberTraceSnapshot BeginSubscriberTrace(EventTraceSnapshot parentEventTrace, Delegate subscriber)
        {
            var targetName = "Static";
            var targetTypeName = subscriber.Method.DeclaringType?.Name ?? "Unknown";

            if (subscriber.Target is IReactor targetReactor)
            {
                targetName = targetReactor.Name;
                targetTypeName = targetReactor.GetType().Name;
            }
            else if (subscriber.Target != null)
            {
                targetName = subscriber.Target.GetType().Name;
                targetTypeName = subscriber.Target.GetType().Name;
            }

            var trace = new SubscriberTraceSnapshot(targetName, targetTypeName, subscriber.Method.Name);
            parentEventTrace.Subscribers.Add(trace);
            _activeSubscriberTraceStack.Push(trace);
            return trace;
        }

        private void CompleteSubscriberTrace(SubscriberTraceSnapshot trace, double durationMs)
        {
            trace.DurationMs = durationMs;

            if (_activeSubscriberTraceStack.Count > 0 && ReferenceEquals(_activeSubscriberTraceStack.Peek(), trace))
                _activeSubscriberTraceStack.Pop();
        }

        private void CompleteEventTrace(EventTraceSnapshot trace, double durationMs)
        {
            trace.DurationMs = durationMs;
        }

        private ReactorEventStats GetOrCreateReactorStats(IReactor reactor)
        {
            if (!_reactorEventStats.TryGetValue(reactor, out var stats))
            {
                stats = new ReactorEventStats();
                _reactorEventStats[reactor] = stats;
            }

            return stats;
        }

        private void TrackRecievedEvent(Delegate subscriber, double callTimeMs)
        {
            if (subscriber.Target is not IReactor reactor)
                return;

            if (!_reactors.Contains(reactor))
                return;

            var stats = GetOrCreateReactorStats(reactor);
            stats.EventsRecieved++;
            stats.TotalCallTimeMs += callTimeMs;
            stats.LastCallTimeMs = callTimeMs;
        }
#endif
    }
}
