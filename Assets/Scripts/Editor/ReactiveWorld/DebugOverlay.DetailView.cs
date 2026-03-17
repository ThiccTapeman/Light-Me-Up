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
                DrawEventTrace(trace, reactor, $"trace:{traceIndex}", 0);
                traceIndex++;
            }

            if (hasTrace)
                return;

            BeginSection();
            GUILayout.Label("No trace data for this reactor yet.", _subtleLabelStyle);
            EndSection();
        }

        private void DrawEventTrace(WorldManager.EventTraceSnapshot trace, IReactor reactor, string traceKey, int depth)
        {
            var isExpanded = GetExpandedState(_expandedTraceEvents, traceKey);

            BeginSection();
            GUILayout.BeginHorizontal();
            GUILayout.Space(depth * 14f);
            GUILayout.BeginVertical();
            isExpanded = EditorGUILayout.Foldout(isExpanded, $"Call | {trace.EventName}", true);
            SetExpandedState(_expandedTraceEvents, traceKey, isExpanded);
            GUILayout.Label($"Subscribers | {trace.SubscriberCount}    Time | {trace.DurationMs:F3} ms", _subtleLabelStyle);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            if (isExpanded)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space((depth + 1) * 14f);
                GUILayout.Label($"Source | {trace.SourceName}", _subtleLabelStyle);
                GUILayout.EndHorizontal();

                for (var i = 0; i < trace.Subscribers.Count; i++)
                    DrawSubscriberTrace(trace.Subscribers[i], reactor, $"{traceKey}/call:{i}", depth + 1);
            }

            EndSection();
        }

        private void DrawSubscriberTrace(WorldManager.SubscriberTraceSnapshot subscriber, IReactor reactor, string traceKey, int depth)
        {
            var isSelectedTarget = string.Equals(subscriber.TargetName, reactor.Name, System.StringComparison.Ordinal);
            var hasSubCalls = subscriber.ChildEvents.Count > 0;
            var isExpanded = hasSubCalls && GetExpandedState(_expandedTraceCalls, traceKey);

            GUILayout.BeginHorizontal();
            GUILayout.Space(depth * 14f);
            GUILayout.BeginVertical(_rowStyle);

            if (hasSubCalls)
            {
                isExpanded = EditorGUILayout.Foldout(isExpanded, $"{(isSelectedTarget ? "Target" : "Call")} | {subscriber.TargetName}", true);
                SetExpandedState(_expandedTraceCalls, traceKey, isExpanded);
            }
            else
            {
                GUILayout.Label($"{(isSelectedTarget ? "Target" : "Call")} | {subscriber.TargetName}", isSelectedTarget ? _headerStyle : GUI.skin.label);
            }

            GUILayout.Label($"Method | {subscriber.MethodName}", _subtleLabelStyle);
            GUILayout.Label($"Type | {subscriber.TargetTypeName}    Time | {subscriber.DurationMs:F3} ms", _subtleLabelStyle);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            if (!hasSubCalls || !isExpanded)
                return;

            for (var i = 0; i < subscriber.ChildEvents.Count; i++)
                DrawEventTrace(subscriber.ChildEvents[i], reactor, $"{traceKey}/event:{i}", depth + 1);
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
            GUILayout.Label($"Events Recieved: {stats.EventsRecieved}");
            GUILayout.Label($"Avarage Call Time: {stats.AverageCallTimeMs:F3} ms");
            GUILayout.Label($"Last Call Time: {stats.LastCallTimeMs:F3} ms");
            EndSection();
        }
    }
}
