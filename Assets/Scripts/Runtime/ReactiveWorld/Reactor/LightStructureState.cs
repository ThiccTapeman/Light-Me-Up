namespace Runtime.ReactiveWorld.Reactor
{
    public class LightStructureState : StructureState
    {
        public bool IsLit;

        public LightStructureState(StructureFlags flags = StructureFlags.None, bool isLit = false)
            : base(flags)
        {
            IsLit = isLit;
        }
    }
}
