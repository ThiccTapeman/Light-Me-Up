namespace Runtime.ReactiveWorld.Events
{
    /// <summary>
    /// Marker interface for all reactive world events.
    /// Allows the <see cref="WorldManager"/> to type and dispatch events
    /// generically via the pub/sub pattern.
    /// <para>
    /// Any struct or class representing a game event must implement this interface.
    /// Prefer <c>struct</c> for high-frequency events to avoid heap allocations.
    /// </para>
    /// </summary>
    public interface IWorldEvent {}
}
