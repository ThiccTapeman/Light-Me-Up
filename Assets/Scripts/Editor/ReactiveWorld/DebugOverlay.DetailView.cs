using Runtime.ReactiveWorld;
using Runtime.ReactiveWorld.Reactor;
using UnityEngine;
namespace Editor.ReactiveWorld
{
    internal sealed partial class ReactiveWorldDebugOverlay
    {
        private void DrawDetailView(WorldManager worldManager, IReactor reactor)
        {
            BeginSection();
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Back", GUILayout.Width(72f)))
            {
                _selectedReactor = null;
                _detailTab = ReactorDetailTab.Overview;
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

            DrawDetailTabs();
            _detailScrollPosition = GUILayout.BeginScrollView(_detailScrollPosition);

            switch (_detailTab)
            {
                case ReactorDetailTab.Stats:
                    DrawReactorStats(worldManager, reactor);
                    break;
                default:
                    DrawReactorOverview(reactor);
                    break;
            }

            GUILayout.EndScrollView();
        }

        private void DrawDetailTabs()
        {
            GUILayout.BeginHorizontal();

            foreach (ReactorDetailTab tab in System.Enum.GetValues(typeof(ReactorDetailTab)))
            {
                var isActive = _detailTab == tab;
                var style = isActive ? _activeTabStyle : _tabStyle;
                if (GUILayout.Toggle(isActive, tab.ToString(), style) && !isActive)
                    _detailTab = tab;
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(6f);
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