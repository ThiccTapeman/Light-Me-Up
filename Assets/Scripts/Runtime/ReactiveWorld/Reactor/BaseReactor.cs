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

        protected virtual void OnInitialize() {}
        protected virtual void OnShutdown() {}
    }
}