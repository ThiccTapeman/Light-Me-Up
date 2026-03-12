namespace Runtime.ReactiveWorld.Reactor
{
    /// <summary>
    /// Extends <see cref="IReactor"/> for reactors that are scoped to a specific world area.
    /// The <see cref="WorldManager"/> uses <see cref="AreaId"/> to route the reactor
    /// to the correct partition in the <see cref="Partitions.PartitionManager"/> on registration.
    /// </summary>
    public interface IAreaReactor : IReactor
    {
        /// <summary>
        /// The identifier of the area this reactor is bound to.
        /// Must match an <see cref="Partitions.AreaVolume.AreaId"/> present in the scene.
        /// </summary>
        string AreaId { get; }
    }
}