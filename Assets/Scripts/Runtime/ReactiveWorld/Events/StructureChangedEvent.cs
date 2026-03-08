namespace Runtime.ReactiveWorld.Events
{
    /// <summary>
    /// Event published when the state of a world structure changes.
    /// A structure can represent an obstacle, a mechanism, a platform, etc.
    /// Subscribed reactors can adapt gameplay or the environment accordingly.
    /// </summary>
    public struct StructureChangedEvent : IWorldEvent
    {
        /// <summary>
        /// Unique identifier of the structure that changed.
        /// </summary>
        public readonly string StructureId;

        /// <summary>
        /// New state of the structure: <c>true</c> if active/open/triggered, <c>false</c> otherwise.
        /// The exact semantics depend on the type of structure.
        /// </summary>
        public readonly bool State;

        public StructureChangedEvent(string structureId, bool state)
        {
            StructureId = structureId;
            State = state;
        }
    }
}
