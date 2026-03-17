using Runtime.ReactiveWorld.Events;
using Runtime.ReactiveWorld.Reactor;
using UnityEngine;

public class TestAreaReactor : BaseReactor, IAreaReactor
{
    [SerializeField] private string _areaId;

    public string AreaId => _areaId;

    public override void OnInitialize()
    {
        WorldManager.Subscribe<DoorStateChangedEvent>(OnDoorStateChanged);
    }

    public override void OnShutdown()
    {
        WorldManager.Unsubscribe<DoorStateChangedEvent>(OnDoorStateChanged);
    }

    private void OnDoorStateChanged(DoorStateChangedEvent evt)
    {
        if (evt.AreaId == AreaId)
        {
            Debug.Log($"Area {AreaId} received door state change: Opened={evt.IsOpened}");
            RaiseEvent(new TestAreaReactEvent(AreaId, $"Reactor is now {(evt.IsOpened ? "opened" : "closed")}"));
        }
    }
}

public class TestAreaReactEvent : IWorldEvent
{
    public string AreaId { get; }
    public string Message { get; }

    public TestAreaReactEvent(string areaId, string message)
    {
        AreaId = areaId;
        Message = message;
    }
}
