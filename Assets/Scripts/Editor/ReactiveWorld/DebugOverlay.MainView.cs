using Runtime.ReactiveWorld;
using UnityEngine;

namespace Editor.ReactiveWorld
{
    internal sealed partial class ReactiveWorldDebugOverlay
    {
        private void DrawMainView(WorldManager worldManager)
        {
            DrawTopHeader();
            DrawMainTabs();

            _mainScrollPosition = GUILayout.BeginScrollView(_mainScrollPosition);

            switch (_mainTab)
            {
                case OverlayMainTab.Performance:
                    DrawPerformanceView(worldManager);
                    break;
                default:
                    DrawListTab(worldManager);
                    break;
            }

            GUILayout.EndScrollView();
        }

        private void DrawTopHeader()
        {
            GUILayout.Label("Reactive World", _headerStyle);
            GUILayout.Label("F9 toggles this panel", _subtleLabelStyle);
            GUILayout.Label($"Registered Reactors: {_performanceBuffer.Count}", _subtleLabelStyle);
            GUILayout.Space(4f);
        }

        private void DrawMainTabs()
        {
            GUILayout.BeginHorizontal();
            DrawTabButton(OverlayMainTab.List, "List");
            DrawTabButton(OverlayMainTab.Performance, "Performance");
            GUILayout.EndHorizontal();
            GUILayout.Space(6f);
        }

        private void DrawTabButton(OverlayMainTab tab, string label)
        {
            var isActive = _mainTab == tab;
            var style = isActive ? _activeTabStyle : _tabStyle;
            if (GUILayout.Toggle(isActive, label, style) && !isActive)
                _mainTab = tab;
        }

        private void DrawListTab(WorldManager worldManager)
        {
            BeginSection();
            DrawGroupToolbar();
            DrawOrderToolbar();
            DrawOrderDirectionToolbar();
            DrawSearchBar();
            GUILayout.Label("Component List", _headerStyle);
            EndSection();

            if (_reactorBuffer.Count == 0)
            {
                BeginSection();
                GUILayout.Label("No registered reactors.", _subtleLabelStyle);
                EndSection();
                return;
            }

            DrawReactorList(worldManager);
        }

        private void DrawPerformanceView(WorldManager worldManager)
        {
            BeginSection();
            DrawPerformanceSummary(worldManager);
            EndSection();

            BeginSection();
            DrawPerformanceGroupToolbar();
            DrawPerformanceSortToolbar();
            DrawPerformanceOrderToolbar();
            DrawPerformanceDirectionToolbar();
            DrawPerformanceSearchBar();
            GUILayout.Label("Component Performance", _headerStyle);
            EndSection();

            if (_performanceBuffer.Count == 0)
            {
                BeginSection();
                GUILayout.Label("No registered reactors.", _subtleLabelStyle);
                EndSection();
                return;
            }

            DrawPerformanceReactorList(worldManager);
        }
    }
}
