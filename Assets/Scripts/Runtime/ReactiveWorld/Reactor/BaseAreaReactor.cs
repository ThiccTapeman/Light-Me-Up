using UnityEngine;

namespace Runtime.ReactiveWorld.Reactor
{
    public abstract class BaseAreaReactor : BaseReactor, IAreaReactor
    {
        [SerializeField] private string _areaId;

        public string AreaId => _areaId;
    }
}
