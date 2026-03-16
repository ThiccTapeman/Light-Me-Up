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

        private static void SortPerformanceReactors(WorldManager worldManager, List<IReactor> reactors, PerformanceSortMode sortMode, SortDirection direction)
        {
            reactors.Sort((left, right) =>
            {
                var leftStats = worldManager.GetReactorEventStats(left);
                var rightStats = worldManager.GetReactorEventStats(right);

                var result = sortMode switch
                {
                    PerformanceSortMode.ByCalls => leftStats.EventsRecieved.CompareTo(rightStats.EventsRecieved),
                    _ => leftStats.AverageCallTimeMs.CompareTo(rightStats.AverageCallTimeMs)
                };

                if (direction == SortDirection.Descending)
                    result *= -1;

                return result != 0 ? result : CompareByName(left, right);
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
