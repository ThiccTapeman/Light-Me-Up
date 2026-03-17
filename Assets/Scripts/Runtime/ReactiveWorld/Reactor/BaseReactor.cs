using Runtime.ReactiveWorld.Events;
using UnityEngine;

namespace Runtime.ReactiveWorld.Reactor
{
    public abstract class BaseReactor : MonoBehaviour, IReactor
    {
        [SerializeField] private string ReactorName;

        protected WorldManager WorldManager { get; private set; }

        public string Name => ReactorName;
        public bool IsEnabled { get; set; } = true;

        private void Awake()
        {
            WorldManager = WorldManager.GetInstance();
        }

        private void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            WorldManager.Register(this);
            OnInitialize();
        }

        public void Shutdown()
        {
            OnShutdown();
            WorldManager.Unregister(this);
        }

        public virtual void OnInitialize() { }
        public virtual void OnShutdown() { }

        protected void RaiseEvent<TEvent>(TEvent evt) where TEvent : IWorldEvent
        {
            WorldManager.Raise(this, evt);
        }

        /// <summary>
        /// Draws reactor-specific debug controls inside the ReactiveWorld editor overlay.
        /// Override this in derived reactors to expose custom runtime debug UI.
        /// </summary>
        public virtual void DrawDebugGUI() { }
    }
}
