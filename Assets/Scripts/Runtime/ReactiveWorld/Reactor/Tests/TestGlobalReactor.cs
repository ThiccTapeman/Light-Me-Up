using Runtime.ReactiveWorld.Reactor;
using UnityEngine;

public class TestGlobalReactor : BaseReactor
{
    [SerializeField] private string _reactorName;

    public override string Name => _reactorName;
}
