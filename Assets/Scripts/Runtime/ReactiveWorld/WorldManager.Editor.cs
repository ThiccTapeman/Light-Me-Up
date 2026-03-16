using System;
using System.Collections.Generic;
using Runtime.ReactiveWorld.Reactor;

namespace Runtime.ReactiveWorld
{
    public partial class WorldManager
    {
#if UNITY_EDITOR
        private const int MaxPerformanceSamples = 60;

        private readonly Queue<RaisePerformanceSample> _raisePerformanceSamples = new();
        private readonly Dictionary<IReactor, ReactorEventStats> _reactorEventStats = new();
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
