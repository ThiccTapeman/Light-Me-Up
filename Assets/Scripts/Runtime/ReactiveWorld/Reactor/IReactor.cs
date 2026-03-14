namespace Runtime.ReactiveWorld.Reactor
{
    /// <summary>
    /// Base contract for any reactor in the ReactiveWorld system.
    /// A reactor is a self-contained module that subscribes to world events
    /// and responds accordingly. It is managed by the <see cref="WorldManager"/>.
    /// </summary>
    public interface IReactor
    {
        /// <summary>
        /// Unique name identifying this reactor (useful for debugging and logging).
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Indicates whether this reactor is active. When disabled, it remains registered
        /// in the <see cref="WorldManager"/> but is no longer solicited.
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// Initializes the reactor and registers it with the <see cref="WorldManager"/>.
        /// Event subscriptions should be set up here.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Shuts down the reactor and unregisters it from the <see cref="WorldManager"/>.
        /// Event subscriptions must be removed here to avoid memory leaks.
        /// </summary>
        void Shutdown();
    }
}
