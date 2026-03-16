using Runtime.ReactiveWorld;
using UnityEngine;

namespace Editor.ReactiveWorld
{
    internal sealed partial class ReactiveWorldDebugOverlay
    {
        private void DrawPerformanceSummary(WorldManager worldManager)
        {
            var snapshot = worldManager.GetRaisePerformanceSnapshot();

            GUILayout.Label("Performance Summary", _headerStyle);
            GUILayout.Label($"Calls Made In Last 60: {snapshot.CallsInLast60}");
            GUILayout.Label($"Average Call -> Action Time: {snapshot.AverageCallDurationMs:F3} ms");
            GUILayout.Label($"Last Call -> Action Time: {snapshot.LastCallDurationMs:F3} ms");
            GUILayout.Label($"Last Event: {snapshot.LastEventName}");
            GUILayout.Label($"Last Subscriber Count: {snapshot.LastSubscriberCount}");
        }

        private void DrawPerformanceGroupToolbar()
        {
            GUILayout.Label("Group", _subtleLabelStyle);
            GUILayout.BeginHorizontal();

            foreach (ReactorGroupMode groupMode in System.Enum.GetValues(typeof(ReactorGroupMode)))
            {
                var isActive = _performanceGroupMode == groupMode;
                var style = isActive ? _activeTabStyle : _pillStyle;
                if (GUILayout.Toggle(isActive, GroupLabel(groupMode), style) && !isActive)
                    _performanceGroupMode = groupMode;
            }

            GUILayout.EndHorizontal();
        }

        private void DrawPerformanceSortToolbar()
        {
            GUILayout.Label("Sort", _subtleLabelStyle);
            GUILayout.BeginHorizontal();

            foreach (PerformanceSortMode sortMode in System.Enum.GetValues(typeof(PerformanceSortMode)))
            {
                var isActive = _performanceSortMode == sortMode;
                var style = isActive ? _activeTabStyle : _pillStyle;
                if (GUILayout.Toggle(isActive, PerformanceSortLabel(sortMode), style) && !isActive)
                    _performanceSortMode = sortMode;
            }

            GUILayout.EndHorizontal();
        }

        private void DrawPerformanceDirectionToolbar()
        {
            GUILayout.Label("Direction", _subtleLabelStyle);
            GUILayout.BeginHorizontal();

            foreach (SortDirection direction in System.Enum.GetValues(typeof(SortDirection)))
            {
                var isActive = _performanceSortDirection == direction;
                var style = isActive ? _activeTabStyle : _pillStyle;
                if (GUILayout.Toggle(isActive, DirectionLabel(direction), style) && !isActive)
                    _performanceSortDirection = direction;
            }

            GUILayout.EndHorizontal();
        }

        private void DrawSearchBar()
        {
            GUILayout.Label("Search", _subtleLabelStyle);
            GUILayout.BeginHorizontal();
            _searchTerm = GUILayout.TextField(_searchTerm ?? string.Empty, _searchFieldStyle);

            if (GUILayout.Button("Clear", GUILayout.Width(52f)))
                _searchTerm = string.Empty;

            GUILayout.EndHorizontal();
        }

        private void DrawPerformanceSearchBar()
        {
            GUILayout.Label("Search", _subtleLabelStyle);
            GUILayout.BeginHorizontal();
            _performanceSearchTerm = GUILayout.TextField(_performanceSearchTerm ?? string.Empty, _searchFieldStyle);

            if (GUILayout.Button("Clear", GUILayout.Width(52f)))
                _performanceSearchTerm = string.Empty;

            GUILayout.EndHorizontal();
        }

        private void DrawGroupToolbar()
        {
            GUILayout.Label("Group", _subtleLabelStyle);
            GUILayout.BeginHorizontal();

            foreach (ReactorGroupMode groupMode in System.Enum.GetValues(typeof(ReactorGroupMode)))
            {
                var isActive = _groupMode == groupMode;
                var style = isActive ? _activeTabStyle : _pillStyle;
                if (GUILayout.Toggle(isActive, GroupLabel(groupMode), style) && !isActive)
                    _groupMode = groupMode;
            }

            GUILayout.EndHorizontal();
        }

        private void DrawOrderToolbar()
        {
            GUILayout.Label("Sort", _subtleLabelStyle);
            GUILayout.BeginHorizontal();

            foreach (ReactorOrderMode orderMode in System.Enum.GetValues(typeof(ReactorOrderMode)))
            {
                var isActive = _orderMode == orderMode;
                var style = isActive ? _activeTabStyle : _pillStyle;
                if (GUILayout.Toggle(isActive, OrderLabel(orderMode), style) && !isActive)
                    _orderMode = orderMode;
            }

            GUILayout.EndHorizontal();
        }

        private void DrawOrderDirectionToolbar()
        {
            GUILayout.Label("Direction", _subtleLabelStyle);
            GUILayout.BeginHorizontal();

            foreach (SortDirection direction in System.Enum.GetValues(typeof(SortDirection)))
            {
                var isActive = _orderDirection == direction;
                var style = isActive ? _activeTabStyle : _pillStyle;
                if (GUILayout.Toggle(isActive, DirectionLabel(direction), style) && !isActive)
                    _orderDirection = direction;
            }

            GUILayout.EndHorizontal();
        }
    }
}
