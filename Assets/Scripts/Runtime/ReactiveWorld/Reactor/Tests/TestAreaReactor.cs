using Runtime.ReactiveWorld;
using Runtime.ReactiveWorld.Reactor;
using UnityEngine;

public class TestAreaReactor : MonoBehaviour, IAreaReactor
{
    [SerializeField] private string _areaId;
    [SerializeField] private string _reactorName;

    private WorldManager _worldManager;

    public string AreaId => _areaId;

    public string Name => _reactorName;

    public bool IsEnabled { get; set; } = true;

    private void Awake()
    {
        _worldManager = WorldManager.GetInstance();
    }

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        _worldManager.Register(this);
    }

    public void Shutdown()
    {
        _worldManager.Unregister(this);
    }
}
