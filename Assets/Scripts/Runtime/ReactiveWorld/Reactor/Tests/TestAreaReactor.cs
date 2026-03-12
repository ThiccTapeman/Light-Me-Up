using Runtime.ReactiveWorld;
using Runtime.ReactiveWorld.Reactor;
using UnityEngine;

public class TestAreaReactor : MonoBehaviour, IAreaReactor
{
    [SerializeField] private string areaId;
    [SerializeField] private string areaName;
    [SerializeField] private WorldManager _worldManager;

    public string AreaId => areaId;

    public string Name => areaName;

    public bool IsEnabled { get; set; } = true;

    public void Initialize(WorldManager worldManager)
    {
        worldManager.Register(this);
    }

    public void Shutdown()
    {
        _worldManager.Unregister(this);
    }

    private void Start()
    {
        Initialize(_worldManager);
    }
}
