using Runtime.ReactiveWorld.Events;
using UnityEngine;

namespace Runtime.ReactiveWorld.Reactor
{
    public abstract class StructureReactor<TEvent> : BaseAreaReactor where TEvent : IWorldEvent
    {
        [SerializeField] private string _structureId;

        public string Id => _structureId;

        protected StructureState InitialState;
        protected StructureState CurrentState;

        protected override void OnInitialize()
        {
            WorldManager.Subscribe<TEvent>(OnEvent);
            InitialState = CreateInitialState();
            CurrentState = CreateInitialState();
        }

        protected override void OnShutdown()
        {
            WorldManager.Unsubscribe<TEvent>(OnEvent);            
        }

        protected virtual StructureState CreateInitialState() => default;

        public virtual void OnEvent(TEvent worldEvent) {}
    }
}
