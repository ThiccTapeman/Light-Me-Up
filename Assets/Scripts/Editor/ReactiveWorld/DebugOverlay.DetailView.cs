using Runtime.ReactiveWorld;
using Runtime.ReactiveWorld.Reactor;
using UnityEditor;
using UnityEngine;
namespace Editor.ReactiveWorld
{
    internal sealed partial class ReactiveWorldDebugOverlay
    {
        private void DrawDetailView(WorldManager worldManager, IReactor reactor)
        {
            var showTraceTabs = _selectedReactorSourceTab == OverlayMainTab.Performance;

            BeginSection();
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Back", GUILayout.Width(72f)))
            {
                _selectedReactor = null;
                _detailTab = showTraceTabs ? ReactorDetailTab.Trace : ReactorDetailTab.Overview;
                GUILayout.EndHorizontal();
                EndSection();
                return;
            }

            GUILayout.Label(GetTitle(reactor), _headerStyle);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            DrawEnabledIndicator(reactor);
            GUILayout.Label(GetTitle(reactor), _subtleLabelStyle);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            var toggleLabel = reactor.IsEnabled ? "Disable" : "Enable";
            if (GUILayout.Button(toggleLabel))
                worldManager.SetReactorEnabled(reactor, !reactor.IsEnabled);

            if (GUILayout.Button("Remove"))
            {
                worldManager.RemoveReactor(reactor);
                _selectedReactor = null;
                GUILayout.EndHorizontal();
                EndSection();
                return;
            }

            GUILayout.EndHorizontal();
            EndSection();

            DrawDetailTabs(showTraceTabs);
            _detailScrollPosition = GUILayout.BeginScrollView(_detailScrollPosition);

            switch (_detailTab)
            {
                case ReactorDetailTab.Trace:
                    DrawReactorTrace(worldManager, reactor);
                    break;
                case ReactorDetailTab.Stats:
                    DrawReactorStats(worldManager, reactor);
                    break;
                default:
                    DrawReactorOverview(reactor);
                    break;
            }

            GUILayout.EndScrollView();
        }

        private void DrawDetailTabs(bool showTraceTabs)
        {
            GUILayout.BeginHorizontal();

            var tabs = showTraceTabs
                ? new[] { ReactorDetailTab.Trace, ReactorDetailTab.Stats }
                : new[] { ReactorDetailTab.Overview, ReactorDetailTab.Stats };

            foreach (var tab in tabs)
            {
                var isActive = _detailTab == tab;
                var style = isActive ? _activeTabStyle : _tabStyle;
                if (GUILayout.Toggle(isActive, tab.ToString(), style) && !isActive)
                    _detailTab = tab;
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(6f);
        }

        private void DrawReactorTrace(WorldManager worldManager, IReactor reactor)
        {
            var traces = worldManager.GetRecentEventTraces();
            var hasTrace = false;
            var traceIndex = 0;

            foreach (var trace in traces)
            {
                if (!TraceContainsReactor(trace, reactor))
                {
                    traceIndex++;
                    continue;
                }

                hasTrace = true;

                if (string.Equals(trace.SourceName, reactor.Name, System.StringComparison.Ordinal))
                {
                    DrawEventTrace(worldManager, trace, reactor, $"trace:{traceIndex}", 0, true, null);
                }
                else if (TryFindFocusedSubscriber(trace, reactor, out var focusedSubscriber))
                {
                    DrawSubscriberTrace(worldManager, focusedSubscriber, reactor, $"trace:{traceIndex}", 0, trace.SourceName, true);
                }

                traceIndex++;
            }

            if (hasTrace)
                return;

            BeginSection();
            GUILayout.Label("No trace data for this reactor yet.", _subtleLabelStyle);
            EndSection();
        }

        private void DrawEventTrace(
            WorldManager worldManager,
            WorldManager.EventTraceSnapshot trace,
            IReactor reactor,
            string traceKey,
            int depth,
            bool isFocusedRoot = false,
            string originName = null)
        {
            var isExpanded = GetExpandedState(_expandedTraceEvents, traceKey);
            var shouldHighlight = ShouldHighlightTrace(trace);

            BeginTraceTint(shouldHighlight, isExpanded);
            GUILayout.BeginVertical(GetTraceSectionStyle(this, shouldHighlight));
            GUILayout.BeginHorizontal();
            GUILayout.Space(depth * 14f);
            GUILayout.BeginVertical();
            isExpanded = EditorGUILayout.Foldout(isExpanded, $"Event Raised: {trace.EventName}", true);
            SetExpandedState(_expandedTraceEvents, traceKey, isExpanded);
            GUILayout.Label($"Status | {GetTraceStatusLabel(trace.ResultStatus)}    Subscribers | {trace.SubscriberCount}    Time | {trace.DurationMs:F3} ms", shouldHighlight ? _redLabelStyle : _subtleLabelStyle);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            if (isExpanded)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space((depth + 1) * 14f);
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                DrawRichLabel("Triggered ", _subtleLabelStyle);
                DrawReactorLink(worldManager, trace.SourceName, true);
                GUILayout.EndHorizontal();

                if (isFocusedRoot && !string.IsNullOrWhiteSpace(originName))
                {
                    GUILayout.BeginHorizontal();
                    DrawRichLabel("Origin: ", _subtleLabelStyle);
                    DrawReactorLink(worldManager, originName, true);
                    GUILayout.EndHorizontal();
                }

                GUILayout.Label($"Context | Scene {trace.Context.SceneName} | Frame {trace.Context.Frame} | Time {trace.Context.Time:F3} | Unscaled {trace.Context.UnscaledTime:F3}", _subtleLabelStyle);
                GUILayout.Label("Event Stack Trace", _subtleLabelStyle);
                GUILayout.TextArea(trace.EventStackTrace ?? "No stack trace captured.", _stackTraceStyle);

                if (trace.HasException)
                {
                    GUILayout.Label("Exception Details", _redLabelStyle);
                    GUILayout.TextArea(GetExceptionDetails(trace.ExceptionTypeName, trace.ExceptionMessage, trace.ExceptionStackTrace), _stackTraceStyle);
                }

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();

                for (var i = 0; i < trace.Subscribers.Count; i++)
                    DrawSubscriberTrace(worldManager, trace.Subscribers[i], reactor, $"{traceKey}/call:{i}", depth + 1, originName, false);
            }

            GUILayout.EndVertical();
            EndTraceTint(shouldHighlight);
        }

        private void DrawSubscriberTrace(
            WorldManager worldManager,
            WorldManager.SubscriberTraceSnapshot subscriber,
            IReactor reactor,
            string traceKey,
            int depth,
            string originName,
            bool isFocusedRoot)
        {
            var isSelectedTarget = string.Equals(subscriber.TargetName, reactor.Name, System.StringComparison.Ordinal);
            var hasSubCalls = subscriber.ChildEvents.Count > 0;
            var isExpanded = hasSubCalls && GetExpandedState(_expandedTraceCalls, traceKey);
            var hasExceptionTrail = ShouldHighlightTrace(subscriber);
            var rowStyle = GetTraceRowStyle(this, hasExceptionTrail);

            BeginTraceTint(hasExceptionTrail, isExpanded);
            GUILayout.BeginHorizontal();
            GUILayout.Space(depth * 14f);
            GUILayout.BeginVertical(rowStyle);

            GUILayout.BeginHorizontal();
            if (hasSubCalls)
            {
                isExpanded = EditorGUILayout.Foldout(isExpanded, GUIContent.none, true);
                SetExpandedState(_expandedTraceCalls, traceKey, isExpanded);
            }
            else
            {
                GUILayout.Space(16f);
            }

            DrawRichLabel("Triggered ", hasExceptionTrail ? _redLabelStyle : _subtleLabelStyle);
            DrawReactorLink(worldManager, subscriber.TargetName, true);
            GUILayout.EndHorizontal();

            GUILayout.Label($"Method: {subscriber.MethodName}", hasExceptionTrail ? _redLabelStyle : _subtleLabelStyle);
            GUILayout.Label($"Time: {subscriber.DurationMs:F3} ms", _subtleLabelStyle);
            GUILayout.Label($"Status: {GetTraceStatusLabel(subscriber.ResultStatus)}", hasExceptionTrail ? _redLabelStyle : _subtleLabelStyle);

            if (isFocusedRoot && !string.IsNullOrWhiteSpace(originName))
            {
                GUILayout.BeginHorizontal();
                DrawRichLabel("Origin: ", _subtleLabelStyle);
                DrawReactorLink(worldManager, originName, true);
                GUILayout.EndHorizontal();
            }

            if (subscriber.HasException)
            {
                GUILayout.Label("Exception Details", _redLabelStyle);
                GUILayout.TextArea(GetExceptionDetails(subscriber.ExceptionTypeName, subscriber.ExceptionMessage, subscriber.ExceptionStackTrace), _stackTraceStyle);
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            EndTraceTint(hasExceptionTrail);

            if (!hasSubCalls || !isExpanded)
                return;

            for (var i = 0; i < subscriber.ChildEvents.Count; i++)
                DrawEventTrace(worldManager, subscriber.ChildEvents[i], reactor, $"{traceKey}/event:{i}", depth + 1, false, originName);
        }

        private void DrawReactorOverview(IReactor reactor)
        {
            if (reactor is BaseReactor baseReactor && HasCustomDebugGUI(baseReactor))
            {
                baseReactor.DrawDebugGUI();
                return;
            }

            GUILayout.Label("No custom debug UI for this reactor.");
        }

        private void DrawReactorStats(WorldManager worldManager, IReactor reactor)
        {
            var stats = worldManager.GetReactorEventStats(reactor);

            BeginSection();
            GUILayout.Label("Stats", _headerStyle);
            GUILayout.Label($"Events Sent: {stats.EventsSent}");
            GUILayout.Label($"Average Sent Call Time: {stats.AverageSentCallTimeMs:F3} ms");
            GUILayout.Label($"Last Sent Call Time: {stats.LastSentCallTimeMs:F3} ms");
            GUILayout.Label($"Last Sent Event: {stats.LastSentEventName}");
            GUILayout.Space(4f);
            GUILayout.Label($"Events Received: {stats.EventsReceived}");
            GUILayout.Label($"Average Received Call Time: {stats.AverageReceivedCallTimeMs:F3} ms");
            GUILayout.Label($"Last Received Call Time: {stats.LastReceivedCallTimeMs:F3} ms");
            GUILayout.Label($"Last Received Event: {stats.LastReceivedEventName}");
            EndSection();
        }
    }
}
