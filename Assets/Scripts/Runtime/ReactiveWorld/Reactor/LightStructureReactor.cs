using Runtime.ReactiveWorld.Events;

namespace Runtime.ReactiveWorld.Reactor
{
    public class LightStructureReactor : StructureReactor<AreaLightChangedEvent>
    {
        protected override StructureState CreateInitialState() => new LightStructureState();

        public override void OnEvent(AreaLightChangedEvent worldEvent)
        {
            if (!IsEnabled) return;
            if (worldEvent.AreaId != AreaId) return;

            var state = (LightStructureState)CurrentState;
            state.IsLit = worldEvent.IsLit;

            if (worldEvent.IsLit)
                CurrentState.SetFlag(StructureFlags.Powered);
            else
                CurrentState.ClearFlag(StructureFlags.Powered);

            OnLightStateChanged(worldEvent.IsLit);
        }

        protected virtual void OnLightStateChanged(bool isLit) {}
    }
}
