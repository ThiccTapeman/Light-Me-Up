using Runtime.ReactiveWorld.Events;
using Runtime.ReactiveWorld.Reactor;
using UnityEngine;

public class DoorReactor : BaseAreaReactor
{
    [SerializeField] private bool _isOpened;

    public bool IsOpened => _isOpened;

    public override void OnInitialize()
    {
        WorldManager.Subscribe<TestAreaReactEvent>(OnTestAreaReact);
    }

    public override void OnShutdown()
    {
        WorldManager.Unsubscribe<TestAreaReactEvent>(OnTestAreaReact);
    }

    private void OnTestAreaReact(TestAreaReactEvent evt)
    {
        if (evt.AreaId == AreaId)
        {
            Debug.Log($"DoorReactor in Area {AreaId} received react event: {evt.Message}");
        }
    }

    public void ToggleDoor()
    {
        _isOpened = !_isOpened;
        RaiseEvent(new DoorStateChangedEvent(AreaId, _isOpened));
    }

    public override void DrawDebugGUI()
    {
        GUILayout.Label($"Opened: {_isOpened}");

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Open"))
        {
            _isOpened = true;
            RaiseEvent(new DoorStateChangedEvent(AreaId, _isOpened));
        }

        if (GUILayout.Button("Close"))
        {
            _isOpened = false;
            RaiseEvent(new DoorStateChangedEvent(AreaId, _isOpened));
        }

        GUILayout.EndHorizontal();
    }
}

public class DoorStateChangedEvent : IWorldEvent
{
    public string AreaId { get; }
    public bool IsOpened { get; }

    public DoorStateChangedEvent(string areaId, bool isOpened)
    {
        AreaId = areaId;
        IsOpened = isOpened;
    }
}
