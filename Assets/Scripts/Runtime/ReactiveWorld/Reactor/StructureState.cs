namespace Runtime.ReactiveWorld.Reactor
{
    [System.Flags]
    public enum StructureFlags
    {
        None = 0,
        Powered = 1 << 0,
        Locked = 1 << 1,
        Disabled = 1 << 2,
        Interactable = 1 << 3,
    }

    public abstract class StructureState
    {
        public StructureFlags Flags { get; private set; }

        public StructureState(StructureFlags flags = StructureFlags.None)
        {
            Flags = flags;
        }

        public bool HasFlag(StructureFlags flag) => (Flags & flag) != 0;

        public void SetFlag(StructureFlags flag) => Flags |= flag;

        public void ClearFlag(StructureFlags flag) => Flags &= ~flag;
    }
}
