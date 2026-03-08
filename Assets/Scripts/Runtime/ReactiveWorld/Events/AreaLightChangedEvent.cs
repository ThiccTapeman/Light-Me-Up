namespace Runtime.ReactiveWorld.Events
{
    /// <summary>
    /// Event published when the lighting state of an area changes.
    /// Allows interested reactors (UI, audio, gameplay...) to respond
    /// without being directly coupled to the lighting system.
    /// </summary>
    public struct AreaLightChangedEvent : IWorldEvent
    {
        /// <summary>
        /// Unique identifier of the area whose lighting has changed.
        /// </summary>
        public readonly string AreaId;

        /// <summary>
        /// New state of the area: <c>true</c> if the area is lit, <c>false</c> otherwise.
        /// </summary>
        public readonly bool IsLit;

        public AreaLightChangedEvent(string areaId, bool isLit)
        {
            AreaId = areaId;
            IsLit = isLit;
        }
    }
}
