using Runtime.ReactiveWorld.Reactor;
using UnityEngine;

public class TestAreaReactor : BaseReactor, IAreaReactor
{
    [SerializeField] private string _areaId;

    public string AreaId => _areaId;

}
