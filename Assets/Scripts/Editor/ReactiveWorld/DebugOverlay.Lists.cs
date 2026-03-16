using System.Collections.Generic;
using Runtime.ReactiveWorld;
using Runtime.ReactiveWorld.Reactor;
using UnityEditor;
using UnityEngine;

namespace Editor.ReactiveWorld
{
    internal sealed partial class ReactiveWorldDebugOverlay
    {
        private void DrawReactorList(WorldManager worldManager)
        {
            switch (_groupMode)
            {
                case ReactorGroupMode.ByArea:
                    DrawGroupedReactors(worldManager, _expandedAreas, GetAreaGroupName);
                    break;
                case ReactorGroupMode.ByType:
                    DrawGroupedReactors(worldManager, _expandedTypes, GetTypeGroupName);
                    break;
                default:
                    DrawFlatReactors(worldManager);
                    break;
            }
        }

        private void DrawPerformanceReactorList(WorldManager worldManager)
        {
            switch (_performanceGroupMode)
            {
                case ReactorGroupMode.ByArea:
                    DrawGroupedPerformanceReactors(worldManager, _expandedAreas, GetAreaGroupName);
                    break;
                case ReactorGroupMode.ByType:
                    DrawGroupedPerformanceReactors(worldManager, _expandedTypes, GetTypeGroupName);
                    break;
                default:
                    DrawFlatPerformanceReactors(worldManager);
                    break;
            }
        }

        private void DrawFlatReactors(WorldManager worldManager)
        {
            foreach (var reactor in _reactorBuffer)
            {
                if (!DrawListRow(worldManager, reactor))
                    break;
            }
        }

        private void DrawFlatPerformanceReactors(WorldManager worldManager)
        {
            foreach (var reactor in _performanceBuffer)
            {
                if (!MatchesSearch(reactor, _performanceSearchTerm))
                    continue;

                if (!DrawPerformanceRow(worldManager, reactor))
                    break;
            }
        }

        private void DrawGroupedReactors(WorldManager worldManager, Dictionary<string, bool> expansionState, System.Func<IReactor, string> getGroupName)
        {
            string currentGroup = null;

            foreach (var reactor in _reactorBuffer)
            {
                var groupName = getGroupName(reactor);
                if (!string.Equals(currentGroup, groupName, System.StringComparison.Ordinal))
                {
                    currentGroup = groupName;
                    var isExpanded = expansionState.TryGetValue(groupName, out var expanded) ? expanded : true;
                    isExpanded = EditorGUILayout.Foldout(isExpanded, $"{groupName} ({CountGroupEntries(_reactorBuffer, getGroupName, groupName)})", true);
                    expansionState[groupName] = isExpanded;

                    if (!isExpanded)
                        continue;
                }

                if (expansionState.TryGetValue(groupName, out var groupExpanded) && groupExpanded)
                {
                    if (!DrawListRow(worldManager, reactor))
                        break;
                }
            }
        }

        private void DrawGroupedPerformanceReactors(WorldManager worldManager, Dictionary<string, bool> expansionState, System.Func<IReactor, string> getGroupName)
        {
            string currentGroup = null;

            foreach (var reactor in _performanceBuffer)
            {
                if (!MatchesSearch(reactor, _performanceSearchTerm))
                    continue;

                var groupName = getGroupName(reactor);
                if (!string.Equals(currentGroup, groupName, System.StringComparison.Ordinal))
                {
                    currentGroup = groupName;
                    var isExpanded = expansionState.TryGetValue(groupName, out var expanded) ? expanded : true;
                    isExpanded = EditorGUILayout.Foldout(isExpanded, $"{groupName} ({CountFilteredGroupEntries(_performanceBuffer, getGroupName, groupName, _performanceSearchTerm)})", true);
                    expansionState[groupName] = isExpanded;

                    if (!isExpanded)
                        continue;
                }

                if (expansionState.TryGetValue(groupName, out var groupExpanded) && groupExpanded)
                {
                    if (!DrawPerformanceRow(worldManager, reactor))
                        break;
                }
            }
        }

        private int CountGroupEntries(List<IReactor> source, System.Func<IReactor, string> getGroupName, string groupName)
        {
            var count = 0;

            foreach (var reactor in source)
            {
                if (string.Equals(getGroupName(reactor), groupName, System.StringComparison.Ordinal))
                    count++;
            }

            return count;
        }

        private int CountFilteredGroupEntries(List<IReactor> source, System.Func<IReactor, string> getGroupName, string groupName, string searchTerm)
        {
            var count = 0;

            foreach (var reactor in source)
            {
                if (!MatchesSearch(reactor, searchTerm))
                    continue;

                if (string.Equals(getGroupName(reactor), groupName, System.StringComparison.Ordinal))
                    count++;
            }

            return count;
        }

        private bool DrawListRow(WorldManager worldManager, IReactor reactor)
        {
            var hasCustomDebugGui = HasCustomDebugGUI(reactor);

            GUILayout.BeginVertical(_rowStyle);
            DrawStatusNameRow(reactor, true);

            GUILayout.BeginHorizontal();

            var toggleLabel = reactor.IsEnabled ? "Disable" : "Enable";
            if (GUILayout.Button(toggleLabel, GUILayout.Width(80f)))
                worldManager.SetReactorEnabled(reactor, !reactor.IsEnabled);

            using (new EditorGUI.DisabledScope(!hasCustomDebugGui))
            {
                if (GUILayout.Button("Open", GUILayout.Width(64f)) && hasCustomDebugGui)
                    _selectedReactor = reactor;
            }

            if (GUILayout.Button("Remove", GUILayout.Width(72f)))
            {
                worldManager.RemoveReactor(reactor);
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                return false;
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            return true;
        }

        private bool DrawPerformanceRow(WorldManager worldManager, IReactor reactor)
        {
            var stats = worldManager.GetReactorEventStats(reactor);

            GUILayout.BeginVertical(_rowStyle);
            DrawStatusNameRow(reactor, true);
            GUILayout.Label($"Calls: {stats.EventsRecieved}", _subtleLabelStyle);
            GUILayout.Label($"Avarage: {stats.AverageCallTimeMs:F3} ms", _subtleLabelStyle);
            GUILayout.Label($"Last: {stats.LastCallTimeMs:F3} ms", _subtleLabelStyle);

            GUILayout.BeginHorizontal();

            var toggleLabel = reactor.IsEnabled ? "Disable" : "Enable";
            if (GUILayout.Button(toggleLabel, GUILayout.Width(80f)))
                worldManager.SetReactorEnabled(reactor, !reactor.IsEnabled);

            if (GUILayout.Button("Show", GUILayout.Width(64f)))
                _selectedReactor = reactor;

            if (GUILayout.Button("Remove", GUILayout.Width(72f)))
            {
                worldManager.RemoveReactor(reactor);
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                return false;
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            return true;
        }
    }
}
