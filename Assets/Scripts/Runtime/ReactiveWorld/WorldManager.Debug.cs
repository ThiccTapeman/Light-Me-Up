using System;
using System.Collections.Generic;
using System.Diagnostics;
using Runtime.ReactiveWorld.Reactor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Runtime.ReactiveWorld
{
    public partial class WorldManager
    {
        private readonly struct EventTraceScope
        {
            public EventTraceScope(EventTraceSnapshot trace, Stopwatch stopwatch)
            {
                Trace = trace;
                Stopwatch = stopwatch;
            }

            public EventTraceSnapshot Trace { get; }
            public Stopwatch Stopwatch { get; }
        }

        private readonly struct SubscriberTraceScope
        {
            public SubscriberTraceScope(SubscriberTraceSnapshot trace, Stopwatch stopwatch)
            {
                Trace = trace;
                Stopwatch = stopwatch;
            }

            public SubscriberTraceSnapshot Trace { get; }
            public Stopwatch Stopwatch { get; }
        }

#if UNITY_EDITOR
        private const int MaxPerformanceSamples = 60;
        private const int MaxEventTraceRoots = 40;

        private readonly Queue<RaisePerformanceSample> _raisePerformanceSamples = new();
        private readonly Dictionary<IReactor, ReactorEventStats> _reactorEventStats = new();
        private readonly List<EventTraceSnapshot> _eventTraceRoots = new();
        private readonly Stack<SubscriberTraceSnapshot> _activeSubscriberTraceStack = new();

        private double _totalRaiseDurationMs;

        public enum TraceStatus
        {
            Completed,
            NoSubscribers,
            Faulted
        }

        public readonly struct RaisePerformanceSnapshot
        {
            public RaisePerformanceSnapshot(
                int callsInLast60,
                double averageCallDurationMs,
                double lastCallDurationMs,
                int lastSubscriberCount,
                string lastEventName,
                int totalEventsSent,
                int totalEventsReceived)
            {
                CallsInLast60 = callsInLast60;
                AverageCallDurationMs = averageCallDurationMs;
                LastCallDurationMs = lastCallDurationMs;
                LastSubscriberCount = lastSubscriberCount;
                LastEventName = lastEventName;
                TotalEventsSent = totalEventsSent;
                TotalEventsReceived = totalEventsReceived;
            }

            public int CallsInLast60 { get; }
            public double AverageCallDurationMs { get; }
            public double LastCallDurationMs { get; }
            public int LastSubscriberCount { get; }
            public string LastEventName { get; }
            public int TotalEventsSent { get; }
            public int TotalEventsReceived { get; }
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
            public ReactorEventStatsSnapshot(
                int eventsSent,
                double averageSentCallTimeMs,
                double lastSentCallTimeMs,
                string lastSentEventName,
                int eventsReceived,
                double averageReceivedCallTimeMs,
                double lastReceivedCallTimeMs,
                string lastReceivedEventName)
            {
                EventsSent = eventsSent;
                AverageSentCallTimeMs = averageSentCallTimeMs;
                LastSentCallTimeMs = lastSentCallTimeMs;
                LastSentEventName = lastSentEventName;
                EventsReceived = eventsReceived;
                AverageReceivedCallTimeMs = averageReceivedCallTimeMs;
                LastReceivedCallTimeMs = lastReceivedCallTimeMs;
                LastReceivedEventName = lastReceivedEventName;
            }

            public int EventsSent { get; }
            public double AverageSentCallTimeMs { get; }
            public double LastSentCallTimeMs { get; }
            public string LastSentEventName { get; }
            public int EventsReceived { get; }
            public double AverageReceivedCallTimeMs { get; }
            public double LastReceivedCallTimeMs { get; }
            public string LastReceivedEventName { get; }
        }

        public sealed class TraceContextSnapshot
        {
            public TraceContextSnapshot(int frame, float time, float unscaledTime, string sceneName, string activeSource)
            {
                Frame = frame;
                Time = time;
                UnscaledTime = unscaledTime;
                SceneName = sceneName;
                ActiveSource = activeSource;
            }

            public int Frame { get; }
            public float Time { get; }
            public float UnscaledTime { get; }
            public string SceneName { get; }
            public string ActiveSource { get; }
        }

        private sealed class ReactorEventStats
        {
            public int EventsSent;
            public double TotalSentCallTimeMs;
            public double LastSentCallTimeMs;
            public string LastSentEventName;
            public int EventsReceived;
            public double TotalReceivedCallTimeMs;
            public double LastReceivedCallTimeMs;
            public string LastReceivedEventName;
        }

        public sealed class EventTraceSnapshot
        {
            public EventTraceSnapshot(
                string eventName,
                string sourceName,
                string sourceTypeName,
                int subscriberCount,
                string eventStackTrace,
                TraceContextSnapshot context)
            {
                EventName = eventName;
                SourceName = sourceName;
                SourceTypeName = sourceTypeName;
                SubscriberCount = subscriberCount;
                EventStackTrace = eventStackTrace;
                Context = context;
                ResultStatus = TraceStatus.Completed;
                Subscribers = new List<SubscriberTraceSnapshot>();
            }

            public string EventName { get; }
            public string SourceName { get; }
            public string SourceTypeName { get; }
            public int SubscriberCount { get; }
            public double DurationMs { get; internal set; }
            public string EventStackTrace { get; }
            public string ExceptionTypeName { get; internal set; }
            public string ExceptionMessage { get; internal set; }
            public string ExceptionStackTrace { get; internal set; }
            public TraceStatus ResultStatus { get; internal set; }
            public TraceContextSnapshot Context { get; }
            public List<SubscriberTraceSnapshot> Subscribers { get; }
            public bool HasException => !string.IsNullOrEmpty(ExceptionMessage);
        }

        public sealed class SubscriberTraceSnapshot
        {
            public SubscriberTraceSnapshot(
                string targetName,
                string targetTypeName,
                string methodName,
                string sourceName,
                string sourceTypeName,
                string stackTrace,
                IReactor targetReactor)
            {
                TargetName = targetName;
                TargetTypeName = targetTypeName;
                MethodName = methodName;
                SourceName = sourceName;
                SourceTypeName = sourceTypeName;
                StackTrace = stackTrace;
                TargetReactor = targetReactor;
                ResultStatus = TraceStatus.Completed;
                ChildEvents = new List<EventTraceSnapshot>();
            }

            public string TargetName { get; }
            public string TargetTypeName { get; }
            public string MethodName { get; }
            public string SourceName { get; }
            public string SourceTypeName { get; }
            public double DurationMs { get; internal set; }
            public string StackTrace { get; }
            internal IReactor TargetReactor { get; }
            public string ExceptionTypeName { get; internal set; }
            public string ExceptionMessage { get; internal set; }
            public string ExceptionStackTrace { get; internal set; }
            public TraceStatus ResultStatus { get; internal set; }
            public List<EventTraceSnapshot> ChildEvents { get; }
            public bool HasException => !string.IsNullOrEmpty(ExceptionMessage);
        }

        public RaisePerformanceSnapshot GetRaisePerformanceSnapshot()
        {
            if (_raisePerformanceSamples.Count == 0)
                return new RaisePerformanceSnapshot(0, 0d, 0d, 0, "None", GetTotalEventsSent(), GetTotalEventsReceived());

            var lastSample = default(RaisePerformanceSample);
            foreach (var sample in _raisePerformanceSamples)
                lastSample = sample;

            return new RaisePerformanceSnapshot(
                _raisePerformanceSamples.Count,
                _totalRaiseDurationMs / _raisePerformanceSamples.Count,
                lastSample.DurationMs,
                lastSample.SubscriberCount,
                lastSample.EventName,
                GetTotalEventsSent(),
                GetTotalEventsReceived());
        }

        public ReactorEventStatsSnapshot GetReactorEventStats(IReactor reactor)
        {
            if (reactor == null || !_reactorEventStats.TryGetValue(reactor, out var stats))
                return new ReactorEventStatsSnapshot(0, 0d, 0d, "None", 0, 0d, 0d, "None");

            var averageSentCallTimeMs = stats.EventsSent > 0
                ? stats.TotalSentCallTimeMs / stats.EventsSent
                : 0d;
            var averageReceivedCallTimeMs = stats.EventsReceived > 0
                ? stats.TotalReceivedCallTimeMs / stats.EventsReceived
                : 0d;

            return new ReactorEventStatsSnapshot(
                stats.EventsSent,
                averageSentCallTimeMs,
                stats.LastSentCallTimeMs,
                string.IsNullOrWhiteSpace(stats.LastSentEventName) ? "None" : stats.LastSentEventName,
                stats.EventsReceived,
                averageReceivedCallTimeMs,
                stats.LastReceivedCallTimeMs,
                string.IsNullOrWhiteSpace(stats.LastReceivedEventName) ? "None" : stats.LastReceivedEventName);
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
            var sourceName = GetTraceSourceName(sourceReactor);
            var sourceTypeName = GetTraceSourceTypeName(sourceReactor);
            var trace = new EventTraceSnapshot(
                eventName,
                sourceName,
                sourceTypeName,
                subscriberCount,
                CaptureStackTrace(),
                CreateTraceContextSnapshot(sourceName));

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

            var sourceTypeName = subscriber.Method.DeclaringType?.Name ?? targetTypeName;
            var trace = new SubscriberTraceSnapshot(
                targetName,
                targetTypeName,
                subscriber.Method.Name,
                parentEventTrace.SourceName,
                sourceTypeName,
                CaptureStackTrace(),
                subscriber.Target as IReactor);

            parentEventTrace.Subscribers.Add(trace);
            _activeSubscriberTraceStack.Push(trace);
            return trace;
        }

        private void CompleteSubscriberTrace(SubscriberTraceSnapshot trace, double durationMs)
        {
            trace.DurationMs = durationMs;

            if (_activeSubscriberTraceStack.Count > 0 &&
                ReferenceEquals(_activeSubscriberTraceStack.Peek(), trace))
            {
                _activeSubscriberTraceStack.Pop();
            }
        }

        private static void CompleteEventTrace(EventTraceSnapshot trace, double durationMs)
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

        private void TrackSentEvent(IReactor reactor, string eventName, double dispatchTimeMs)
        {
            if (reactor == null)
                return;

            if (!_reactors.Contains(reactor))
                return;

            var stats = GetOrCreateReactorStats(reactor);
            stats.EventsSent++;
            stats.TotalSentCallTimeMs += dispatchTimeMs;
            stats.LastSentCallTimeMs = dispatchTimeMs;
            stats.LastSentEventName = eventName;
        }

        private void TrackReceivedEvent(IReactor reactor, string eventName, double dispatchTimeMs)
        {
            if (reactor == null)
                return;

            if (!_reactors.Contains(reactor))
                return;

            var stats = GetOrCreateReactorStats(reactor);
            stats.EventsReceived++;
            stats.TotalReceivedCallTimeMs += dispatchTimeMs;
            stats.LastReceivedCallTimeMs = dispatchTimeMs;
            stats.LastReceivedEventName = eventName;
        }

        private void TrackReceivedEventsForTrace(EventTraceSnapshot trace)
        {
            foreach (var subscriber in trace.Subscribers)
            {
                TrackReceivedEvent(subscriber.TargetReactor, trace.EventName, trace.DurationMs);
            }
        }

        private EventTraceScope BeginRaiseTrace(Type key, IReactor reactor, int subscriberCount)
        {
            return new EventTraceScope(
                BeginEventTrace(key.Name, reactor, subscriberCount),
                Stopwatch.StartNew());
        }

        private void EndRaiseTrace(Type key, int subscriberCount, EventTraceScope scope)
        {
            scope.Stopwatch.Stop();
            CompleteEventTrace(scope.Trace, scope.Stopwatch.Elapsed.TotalMilliseconds);
            if (scope.Trace.Context != null && !string.Equals(scope.Trace.SourceName, "External", StringComparison.Ordinal))
            {
                TrackSentEvent(FindReactorByName(scope.Trace.SourceName), key.Name, scope.Stopwatch.Elapsed.TotalMilliseconds);
            }

            TrackReceivedEventsForTrace(scope.Trace);
            RecordRaisePerformance(key.Name, scope.Stopwatch.Elapsed.TotalMilliseconds, subscriberCount);
        }

        private IReactor FindReactorByName(string reactorName)
        {
            if (string.IsNullOrWhiteSpace(reactorName))
                return null;

            return _reactors.Find(reactor => string.Equals(reactor.Name, reactorName, StringComparison.Ordinal));
        }

        private void MarkEventTraceException(EventTraceScope scope, Exception exception)
        {
            scope.Trace.ResultStatus = TraceStatus.Faulted;
            scope.Trace.ExceptionTypeName = exception.GetType().Name;
            scope.Trace.ExceptionMessage = exception.Message;
            scope.Trace.ExceptionStackTrace = exception.ToString();
        }

        private void CompleteEmptyRaiseTrace(Type key, IReactor reactor)
        {
            var trace = BeginEventTrace(key.Name, reactor, 0);
            trace.ResultStatus = TraceStatus.NoSubscribers;
            CompleteEventTrace(trace, 0d);
            RecordRaisePerformance(key.Name, 0d, 0);
        }

        private SubscriberTraceScope BeginSubscriberTraceScope(EventTraceScope eventTrace, Delegate subscriber)
        {
            return new SubscriberTraceScope(
                BeginSubscriberTrace(eventTrace.Trace, subscriber),
                Stopwatch.StartNew());
        }

        private void EndSubscriberTraceScope(Delegate subscriber, SubscriberTraceScope scope)
        {
            scope.Stopwatch.Stop();
            CompleteSubscriberTrace(scope.Trace, scope.Stopwatch.Elapsed.TotalMilliseconds);
        }

        private static void MarkSubscriberTraceException(SubscriberTraceScope scope, Exception exception)
        {
            scope.Trace.ResultStatus = TraceStatus.Faulted;
            scope.Trace.ExceptionTypeName = exception.GetType().Name;
            scope.Trace.ExceptionMessage = exception.Message;
            scope.Trace.ExceptionStackTrace = exception.ToString();
        }

        private static string CaptureStackTrace()
        {
            return new StackTrace(2, true).ToString();
        }

        private static string GetTraceSourceName(IReactor sourceReactor)
        {
            return sourceReactor != null ? sourceReactor.Name : "External";
        }

        private static string GetTraceSourceTypeName(IReactor sourceReactor)
        {
            return sourceReactor != null ? sourceReactor.GetType().Name : "External";
        }

        private static TraceContextSnapshot CreateTraceContextSnapshot(string activeSource)
        {
            var sceneName = SceneManager.GetActiveScene().name;
            return new TraceContextSnapshot(
                Time.frameCount,
                Time.time,
                Time.unscaledTime,
                string.IsNullOrWhiteSpace(sceneName) ? "Unknown Scene" : sceneName,
                activeSource);
        }

        private int GetTotalEventsSent()
        {
            var total = 0;
            foreach (var stats in _reactorEventStats.Values)
                total += stats.EventsSent;

            return total;
        }

        private int GetTotalEventsReceived()
        {
            var total = 0;
            foreach (var stats in _reactorEventStats.Values)
                total += stats.EventsReceived;

            return total;
        }

#else
        public enum TraceStatus
        {
            Completed,
            NoSubscribers,
            Faulted
        }

        public readonly struct RaisePerformanceSnapshot
        {
            public RaisePerformanceSnapshot(
                int callsInLast60,
                double averageCallDurationMs,
                double lastCallDurationMs,
                int lastSubscriberCount,
                string lastEventName,
                int totalEventsSent,
                int totalEventsReceived)
            {
                CallsInLast60 = callsInLast60;
                AverageCallDurationMs = averageCallDurationMs;
                LastCallDurationMs = lastCallDurationMs;
                LastSubscriberCount = lastSubscriberCount;
                LastEventName = lastEventName;
                TotalEventsSent = totalEventsSent;
                TotalEventsReceived = totalEventsReceived;
            }

            public int CallsInLast60 { get; }
            public double AverageCallDurationMs { get; }
            public double LastCallDurationMs { get; }
            public int LastSubscriberCount { get; }
            public string LastEventName { get; }
            public int TotalEventsSent { get; }
            public int TotalEventsReceived { get; }
        }

        public readonly struct ReactorEventStatsSnapshot
        {
            public ReactorEventStatsSnapshot(
                int eventsSent,
                double averageSentCallTimeMs,
                double lastSentCallTimeMs,
                string lastSentEventName,
                int eventsReceived,
                double averageReceivedCallTimeMs,
                double lastReceivedCallTimeMs,
                string lastReceivedEventName)
            {
                EventsSent = eventsSent;
                AverageSentCallTimeMs = averageSentCallTimeMs;
                LastSentCallTimeMs = lastSentCallTimeMs;
                LastSentEventName = lastSentEventName;
                EventsReceived = eventsReceived;
                AverageReceivedCallTimeMs = averageReceivedCallTimeMs;
                LastReceivedCallTimeMs = lastReceivedCallTimeMs;
                LastReceivedEventName = lastReceivedEventName;
            }

            public int EventsSent { get; }
            public double AverageSentCallTimeMs { get; }
            public double LastSentCallTimeMs { get; }
            public string LastSentEventName { get; }
            public int EventsReceived { get; }
            public double AverageReceivedCallTimeMs { get; }
            public double LastReceivedCallTimeMs { get; }
            public string LastReceivedEventName { get; }
        }

        public sealed class TraceContextSnapshot
        {
            public TraceContextSnapshot(int frame, float time, float unscaledTime, string sceneName, string activeSource)
            {
                Frame = frame;
                Time = time;
                UnscaledTime = unscaledTime;
                SceneName = sceneName;
                ActiveSource = activeSource;
            }

            public int Frame { get; }
            public float Time { get; }
            public float UnscaledTime { get; }
            public string SceneName { get; }
            public string ActiveSource { get; }
        }

        public sealed class EventTraceSnapshot
        {
            public EventTraceSnapshot(
                string eventName,
                string sourceName,
                string sourceTypeName,
                int subscriberCount,
                string eventStackTrace,
                TraceContextSnapshot context)
            {
                EventName = eventName;
                SourceName = sourceName;
                SourceTypeName = sourceTypeName;
                SubscriberCount = subscriberCount;
                EventStackTrace = eventStackTrace;
                Context = context;
                Subscribers = Array.Empty<SubscriberTraceSnapshot>();
            }

            public string EventName { get; }
            public string SourceName { get; }
            public string SourceTypeName { get; }
            public int SubscriberCount { get; }
            public double DurationMs { get; internal set; }
            public string EventStackTrace { get; }
            public string ExceptionTypeName { get; internal set; }
            public string ExceptionMessage { get; internal set; }
            public string ExceptionStackTrace { get; internal set; }
            public TraceStatus ResultStatus { get; internal set; }
            public TraceContextSnapshot Context { get; }
            public IReadOnlyList<SubscriberTraceSnapshot> Subscribers { get; }
            public bool HasException => !string.IsNullOrEmpty(ExceptionMessage);
        }

        public sealed class SubscriberTraceSnapshot
        {
            public SubscriberTraceSnapshot(
                string targetName,
                string targetTypeName,
                string methodName,
                string sourceName,
                string sourceTypeName,
                string stackTrace,
                IReactor targetReactor)
            {
                TargetName = targetName;
                TargetTypeName = targetTypeName;
                MethodName = methodName;
                SourceName = sourceName;
                SourceTypeName = sourceTypeName;
                StackTrace = stackTrace;
                TargetReactor = targetReactor;
                ChildEvents = Array.Empty<EventTraceSnapshot>();
            }

            public string TargetName { get; }
            public string TargetTypeName { get; }
            public string MethodName { get; }
            public string SourceName { get; }
            public string SourceTypeName { get; }
            public double DurationMs { get; internal set; }
            public string StackTrace { get; }
            internal IReactor TargetReactor { get; }
            public string ExceptionTypeName { get; internal set; }
            public string ExceptionMessage { get; internal set; }
            public string ExceptionStackTrace { get; internal set; }
            public TraceStatus ResultStatus { get; internal set; }
            public IReadOnlyList<EventTraceSnapshot> ChildEvents { get; }
            public bool HasException => !string.IsNullOrEmpty(ExceptionMessage);
        }

        public RaisePerformanceSnapshot GetRaisePerformanceSnapshot()
        {
            return new RaisePerformanceSnapshot(0, 0d, 0d, 0, "None", 0, 0);
        }

        public ReactorEventStatsSnapshot GetReactorEventStats(IReactor reactor)
        {
            return new ReactorEventStatsSnapshot(0, 0d, 0d, "None", 0, 0d, 0d, "None");
        }

        public IReadOnlyList<EventTraceSnapshot> GetRecentEventTraces()
        {
            return Array.Empty<EventTraceSnapshot>();
        }

        private EventTraceScope BeginRaiseTrace(Type key, IReactor reactor, int subscriberCount) => default;
        private void EndRaiseTrace(Type key, int subscriberCount, EventTraceScope scope) { }
        private void MarkEventTraceException(EventTraceScope scope, Exception exception) { }
        private void CompleteEmptyRaiseTrace(Type key, IReactor reactor) { }
        private SubscriberTraceScope BeginSubscriberTraceScope(EventTraceScope eventTrace, Delegate subscriber) => default;
        private void EndSubscriberTraceScope(Delegate subscriber, SubscriberTraceScope scope) { }
        private void MarkSubscriberTraceException(SubscriberTraceScope scope, Exception exception) { }

#endif
    }
}
