using System.Collections.Generic;
using System.Reflection;
using Runtime.ReactiveWorld;
using Runtime.ReactiveWorld.Reactor;
using UnityEditor;
using UnityEngine;

namespace Editor.ReactiveWorld
{
    internal sealed partial class ReactiveWorldDebugOverlay
    {
        private static void DrawStatusNameRow(IReactor reactor, bool clampNameWidth = false)
        {
            GUILayout.BeginHorizontal();
            DrawEnabledIndicator(reactor);

            if (clampNameWidth)
            {
                var title = GetTitle(reactor);
                var style = GUI.skin.label;
                var availableWidth = Mathf.Max(120f, EditorGUIUtility.currentViewWidth - 170f);
                var content = new GUIContent(title);
                var size = style.CalcSize(content);

                if (size.x > availableWidth)
                    title = TruncateToFit(title, style, availableWidth);

                GUILayout.Label(title, GUILayout.ExpandWidth(true));
            }
            else
            {
                GUILayout.Label(GetTitle(reactor));
            }

            GUILayout.EndHorizontal();
        }

        private static void DrawEnabledIndicator(IReactor reactor)
        {
            var previousColor = GUI.color;
            GUI.color = reactor.IsEnabled ? Color.green : Color.red;
            GUILayout.Label("\u25cf", GUILayout.Width(16f));
            GUI.color = previousColor;
        }

        private static void SortReactors(List<IReactor> reactors, ReactorGroupMode groupMode, ReactorOrderMode orderMode, SortDirection direction)
        {
            reactors.Sort((left, right) =>
            {
                var groupResult = groupMode switch
                {
                    ReactorGroupMode.ByArea => CompareByArea(left, right),
                    ReactorGroupMode.ByType => CompareByType(left, right),
                    _ => 0
                };

                if (groupResult != 0)
                    return groupResult;

                var orderResult = orderMode switch
                {
                    ReactorOrderMode.ByEnabled => CompareByEnabled(left, right),
                    _ => CompareByName(left, right)
                };

                if (direction == SortDirection.Descending)
                    orderResult *= -1;

                return orderResult != 0 ? orderResult : CompareByName(left, right);
            });
        }

        private static void SortPerformanceReactors(
            WorldManager worldManager,
            List<IReactor> reactors,
            PerformanceSortMode sortMode,
            ReactorOrderMode orderMode,
            SortDirection direction)
        {
            reactors.Sort((left, right) =>
            {
                var leftStats = worldManager.GetReactorEventStats(left);
                var rightStats = worldManager.GetReactorEventStats(right);

                var performanceResult = sortMode switch
                {
                    PerformanceSortMode.ByCalls => (leftStats.EventsSent + leftStats.EventsReceived).CompareTo(rightStats.EventsSent + rightStats.EventsReceived),
                    _ => Mathf.Max((float)leftStats.AverageSentCallTimeMs, (float)leftStats.AverageReceivedCallTimeMs)
                        .CompareTo(Mathf.Max((float)rightStats.AverageSentCallTimeMs, (float)rightStats.AverageReceivedCallTimeMs))
                };

                if (performanceResult != 0)
                {
                    if (direction == SortDirection.Descending)
                        performanceResult *= -1;

                    return performanceResult;
                }

                var orderResult = orderMode switch
                {
                    ReactorOrderMode.ByEnabled => CompareByEnabled(left, right),
                    _ => CompareByName(left, right)
                };

                if (direction == SortDirection.Descending)
                    orderResult *= -1;

                return orderResult != 0 ? orderResult : CompareByName(left, right);
            });
        }

        private static int CompareByArea(IReactor left, IReactor right)
        {
            var leftArea = left is IAreaReactor leftAreaReactor ? leftAreaReactor.AreaId : string.Empty;
            var rightArea = right is IAreaReactor rightAreaReactor ? rightAreaReactor.AreaId : string.Empty;
            return string.Compare(leftArea, rightArea, System.StringComparison.OrdinalIgnoreCase);
        }

        private static int CompareByEnabled(IReactor left, IReactor right)
        {
            return left.IsEnabled.CompareTo(right.IsEnabled);
        }

        private static int CompareByType(IReactor left, IReactor right)
        {
            return string.Compare(GetTypeGroupName(left), GetTypeGroupName(right), System.StringComparison.OrdinalIgnoreCase);
        }

        private static int CompareByName(IReactor left, IReactor right)
        {
            return string.Compare(left.Name, right.Name, System.StringComparison.OrdinalIgnoreCase);
        }

        private static string GetTitle(IReactor reactor)
        {
            if (reactor is IAreaReactor areaReactor)
                return $"{reactor.Name} [{areaReactor.AreaId}]";

            return reactor.Name;
        }

        private static string GetAreaGroupName(IReactor reactor)
        {
            if (reactor is IAreaReactor areaReactor)
                return string.IsNullOrWhiteSpace(areaReactor.AreaId) ? "Unassigned Area" : areaReactor.AreaId;

            return "Global";
        }

        private static string GetTypeGroupName(IReactor reactor)
        {
            return reactor.GetType().Name;
        }

        private static bool HasCustomDebugGUI(IReactor reactor)
        {
            return reactor is BaseReactor baseReactor && HasCustomDebugGUI(baseReactor);
        }

        private static bool HasCustomDebugGUI(BaseReactor reactor)
        {
            var method = reactor.GetType().GetMethod(nameof(BaseReactor.DrawDebugGUI), BindingFlags.Instance | BindingFlags.Public);
            return method != null && method.DeclaringType != typeof(BaseReactor);
        }

        private static bool MatchesSearch(IReactor reactor, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return true;

            var normalizedSearch = searchTerm.Trim();
            if (reactor.Name != null && reactor.Name.IndexOf(normalizedSearch, System.StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            if (reactor.GetType().Name.IndexOf(normalizedSearch, System.StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            if (reactor is IAreaReactor areaReactor &&
                areaReactor.AreaId != null &&
                areaReactor.AreaId.IndexOf(normalizedSearch, System.StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            return false;
        }

        private static bool TraceContainsReactor(WorldManager.EventTraceSnapshot trace, IReactor reactor)
        {
            if (string.Equals(trace.SourceName, reactor.Name, System.StringComparison.Ordinal))
                return true;

            foreach (var subscriber in trace.Subscribers)
            {
                if (SubscriberTraceContainsReactor(subscriber, reactor))
                    return true;
            }

            return false;
        }

        private static bool TryFindFocusedSubscriber(
            WorldManager.EventTraceSnapshot trace,
            IReactor reactor,
            out WorldManager.SubscriberTraceSnapshot subscriber)
        {
            foreach (var currentSubscriber in trace.Subscribers)
            {
                if (TryFindFocusedSubscriber(currentSubscriber, reactor, out subscriber))
                    return true;
            }

            subscriber = null;
            return false;
        }

        private static bool TryFindFocusedSubscriber(
            WorldManager.SubscriberTraceSnapshot subscriber,
            IReactor reactor,
            out WorldManager.SubscriberTraceSnapshot match)
        {
            if (string.Equals(subscriber.TargetName, reactor.Name, System.StringComparison.Ordinal))
            {
                match = subscriber;
                return true;
            }

            foreach (var childEvent in subscriber.ChildEvents)
            {
                if (TryFindFocusedSubscriber(childEvent, reactor, out match))
                    return true;
            }

            match = null;
            return false;
        }

        private static bool SubscriberTraceContainsReactor(WorldManager.SubscriberTraceSnapshot subscriber, IReactor reactor)
        {
            if (string.Equals(subscriber.TargetName, reactor.Name, System.StringComparison.Ordinal))
                return true;

            foreach (var childEvent in subscriber.ChildEvents)
            {
                if (TraceContainsReactor(childEvent, reactor))
                    return true;
            }

            return false;
        }

        private static string GetTraceStatusLabel(WorldManager.TraceStatus status)
        {
            return status switch
            {
                WorldManager.TraceStatus.NoSubscribers => "No Subscribers",
                WorldManager.TraceStatus.Faulted => "Faulted",
                _ => "Completed"
            };
        }

        private static bool ShouldHighlightTrace(WorldManager.EventTraceSnapshot trace)
        {
            if (trace == null)
                return false;

            if (trace.ResultStatus == WorldManager.TraceStatus.Faulted || trace.HasException)
                return true;

            foreach (var subscriber in trace.Subscribers)
            {
                if (ShouldHighlightTrace(subscriber))
                    return true;
            }

            return false;
        }

        private static bool ShouldHighlightTrace(WorldManager.SubscriberTraceSnapshot subscriber)
        {
            if (subscriber == null)
                return false;

            if (subscriber.ResultStatus == WorldManager.TraceStatus.Faulted || subscriber.HasException)
                return true;

            foreach (var childEvent in subscriber.ChildEvents)
            {
                if (ShouldHighlightTrace(childEvent))
                    return true;
            }

            return false;
        }

        private static string GetExceptionDetails(string exceptionTypeName, string exceptionMessage, string exceptionStackTrace)
        {
            if (string.IsNullOrWhiteSpace(exceptionMessage))
                return "None";

            if (!string.IsNullOrWhiteSpace(exceptionStackTrace))
                return exceptionStackTrace.Trim();

            return string.IsNullOrWhiteSpace(exceptionTypeName)
                ? exceptionMessage
                : $"{exceptionTypeName}: {exceptionMessage}";
        }

        private static GUIStyle GetTraceSectionStyle(ReactiveWorldDebugOverlay overlay, bool highlight)
        {
            return highlight ? overlay._faultedSectionStyle : overlay._sectionStyle;
        }

        private static GUIStyle GetTraceRowStyle(ReactiveWorldDebugOverlay overlay, bool highlight)
        {
            return highlight ? overlay._faultedRowStyle : overlay._rowStyle;
        }

        private static void BeginTraceTint(bool highlight, bool expanded)
        {
            if (!highlight)
                return;

            GUI.color = expanded
                ? new Color(1f, 0.82f, 0.82f)
                : new Color(1f, 0.68f, 0.68f);
        }

        private static void EndTraceTint(bool highlight)
        {
            if (!highlight)
                return;

            GUI.color = Color.white;
        }

        private void DrawRichLabel(string text, GUIStyle style = null)
        {
            var content = new GUIContent(text);
            var labelStyle = style ?? _richLabelStyle;
            var width = Mathf.Ceil(labelStyle.CalcSize(content).x);
            GUILayout.Label(content, labelStyle, GUILayout.Width(width));
        }

        private bool DrawReactorLink(WorldManager worldManager, string reactorName, bool bold = false)
        {
            if (string.IsNullOrWhiteSpace(reactorName))
            {
                GUILayout.Label("Unknown", _subtleLabelStyle);
                return false;
            }

            var label = bold ? $"<b>{reactorName}</b>" : reactorName;

            if (!worldManager.TryGetReactor(reactorName, out var targetReactor))
            {
                GUILayout.Label(label, _richLabelStyle);
                return false;
            }

            var content = new GUIContent(label);
            var width = Mathf.Ceil(_linkStyle.CalcSize(content).x);
            if (!GUILayout.Button(content, _linkStyle, GUILayout.Width(width)))
                return false;

            _selectedReactor = targetReactor;
            _selectedReactorSourceTab = OverlayMainTab.Performance;
            _detailTab = ReactorDetailTab.Trace;
            _detailScrollPosition = Vector2.zero;
            return true;
        }

        private static bool GetExpandedState(Dictionary<string, bool> stateMap, string key)
        {
            return !stateMap.TryGetValue(key, out var isExpanded) || isExpanded;
        }

        private static void SetExpandedState(Dictionary<string, bool> stateMap, string key, bool isExpanded)
        {
            stateMap[key] = isExpanded;
        }

        private static string TruncateToFit(string text, GUIStyle style, float maxWidth)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            const string ellipsis = "...";
            if (style.CalcSize(new GUIContent(text)).x <= maxWidth)
                return text;

            for (var length = text.Length - 1; length > 0; length--)
            {
                var candidate = text.Substring(0, length) + ellipsis;
                if (style.CalcSize(new GUIContent(candidate)).x <= maxWidth)
                    return candidate;
            }

            return ellipsis;
        }

        private static string GroupLabel(ReactorGroupMode mode)
        {
            return mode switch
            {
                ReactorGroupMode.ByArea => "Area",
                ReactorGroupMode.ByType => "Type",
                _ => "None"
            };
        }

        private static string OrderLabel(ReactorOrderMode mode)
        {
            return mode switch
            {
                ReactorOrderMode.ByEnabled => "Enabled",
                _ => "Name"
            };
        }

        private static string DirectionLabel(SortDirection direction)
        {
            return direction switch
            {
                SortDirection.Descending => "Descending",
                _ => "Ascending"
            };
        }

        private static string PerformanceSortLabel(PerformanceSortMode mode)
        {
            return mode switch
            {
                PerformanceSortMode.ByCalls => "Calls",
                _ => "Delay"
            };
        }

        private void BeginSection()
        {
            GUILayout.BeginVertical(_sectionStyle);
        }

        private void EndSection()
        {
            GUILayout.EndVertical();
        }
    }
}
