using Runtime.ReactiveWorld.Reactor;
using UnityEngine;

public class TestAreaReactor : BaseReactor, IAreaReactor
{
    [SerializeField] private string _areaId;
    [SerializeField] private string _reactorName;

    public string AreaId => _areaId;

    public override string Name => _reactorName;
}
