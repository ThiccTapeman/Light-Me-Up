using Runtime.ReactiveWorld.Reactor;
using UnityEngine;

public class DoorReactor : BaseReactor, IAreaReactor
{
    [SerializeField] private string _areaId;
    [SerializeField] private bool _isOpened;

    public string AreaId => _areaId;
    public bool IsOpened => _isOpened;

    public override void DrawDebugGUI()
    {
        GUILayout.Label($"Opened: {_isOpened}");

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Open"))
            _isOpened = true;

        if (GUILayout.Button("Close"))
            _isOpened = false;

        GUILayout.EndHorizontal();
    }
}
