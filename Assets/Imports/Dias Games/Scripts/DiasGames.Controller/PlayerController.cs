using DiasGames.AbilitySystem.Core;
using DiasGames.Components;
using DiasGames.Command;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DiasGames.Controller
{
    public class PlayerController : MonoBehaviour, IPlayerController
    {
        public Vector2 MoveInput { get; private set; } = Vector2.zero;

        protected PlayerInput _playerInput = default;
        protected IAbilitySystem _abilitySystem = null;
        protected IMovement _movement;
        protected ICommandInvoker _commandInvoker;
        protected IInteractionComponent _interactionComponent;
        protected IAudioPlayer _audioPlayer;
        protected CameraController _cameraController;
        protected AttributesComponent _attributesComponent;
        protected LevelController _levelController;

        [SerializeField] private UpdateMode _updateMode = UpdateMode.Update;
        [SerializeField] private AudioClipContainer _landingClips;
        [SerializeField] private GameObject[] _detachGameObjectsOnPlay;

        public ILevelController LevelController => _levelController;
        private Vector2 _mouseInputBuffer;

        protected virtual void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            _abilitySystem = GetComponent<IAbilitySystem>();
            _movement = GetComponent<IMovement>();
            _commandInvoker = GetComponent<ICommandInvoker>();
            _interactionComponent = GetComponent<IInteractionComponent>();
            _audioPlayer = GetComponent<IAudioPlayer>();
            _cameraController = GetComponent<CameraController>();
            _attributesComponent = GetComponent<AttributesComponent>();
            _levelController = FindAnyObjectByType<LevelController>();

            DetachGameObjects();
        }

        private void DetachGameObjects()
        {
            foreach (GameObject go in _detachGameObjectsOnPlay)
            {
                go.transform.SetParent(null);
            }
        }

        protected virtual void OnEnable()
        {
            BindInputEvents();
            _movement.OnLanded += HandleLanded;
            _movement.OnStartFalling += () => _abilitySystem.StartAbilityByName("Fall", gameObject);
        }

        protected void Start()
        {
            _attributesComponent.GetAttributeByType(AttributeType.Health).OnAttributeEmpty += HandleHealthEmpty;
        }

        protected virtual void OnDisable()
        {
            UnbindInputEvents();
            _movement.OnLanded -= HandleLanded;
            _attributesComponent.GetAttributeByType(AttributeType.Health).OnAttributeEmpty -= HandleHealthEmpty;
        }

        protected virtual void BindInputEvents()
        {
            _playerInput.actions["Jump"].performed += Jump;
            _playerInput.actions["Roll"].performed += Roll;
            _playerInput.actions["Walk"].performed += StartSprint;
            _playerInput.actions["Walk"].canceled += StopSprint;
            _playerInput.actions["Crouch"].performed += StartCrouch;
            _playerInput.actions["Crouch"].canceled += StopCrouch;
            _playerInput.actions["Zoom"].performed += StartZoom;
            _playerInput.actions["Zoom"].canceled += StopZoom;
            _playerInput.actions["Crawl"].performed += Crawl;
            _playerInput.actions["Interact"].performed += Interact;
            _playerInput.actions["MouseLook"].performed += HandleMouseLook;
        }

        protected virtual void UnbindInputEvents()
        {
            _playerInput.actions["Jump"].performed -= Jump;
            _playerInput.actions["Roll"].performed -= Roll;
            _playerInput.actions["Walk"].performed -= StartSprint;
            _playerInput.actions["Walk"].canceled -= StopSprint;
            _playerInput.actions["Crouch"].performed -= StartCrouch;
            _playerInput.actions["Crouch"].canceled -= StopCrouch;
            _playerInput.actions["Zoom"].performed -= StartZoom;
            _playerInput.actions["Zoom"].canceled -= StopZoom;
            _playerInput.actions["Crawl"].performed -= Crawl;
            _playerInput.actions["Interact"].performed -= Interact;
            _playerInput.actions["MouseLook"].performed -= HandleMouseLook;
        }

        protected virtual void Update()
        {
            MoveInput = _playerInput.actions["Move"].ReadValue<Vector2>();
            _movement.AddMoveInput(MoveInput);
        }

        private void LateUpdate()
        {
            if (_updateMode == UpdateMode.Update)
            {
                CameraUpdate(Time.deltaTime);
            }
        }

        private void FixedUpdate()
        {
            if (_updateMode == UpdateMode.FixedUpdate)
            {
                CameraUpdate(Time.fixedDeltaTime);
            }
        }

        private void CameraUpdate(float deltaTime)
        {
            Vector2 lookInput = _playerInput.actions["Look"].ReadValue<Vector2>();
            if (lookInput == Vector2.zero)
            {
                _cameraController.CameraRotationByMouse(_mouseInputBuffer);
            }
            else
            {
                _cameraController.CameraRotation(lookInput, deltaTime);
            }

            _mouseInputBuffer = Vector2.zero;
        }

        private void HandleHealthEmpty()
        {
            _abilitySystem.StartAbilityByName("Die", gameObject);
        }

        private void HandleLanded()
        {
            if (_abilitySystem.StartAbilityByName("Locomotion", gameObject))
            {
                _audioPlayer?.PlayEffect(_landingClips);
            }
        }

        private void HandleMouseLook(InputAction.CallbackContext ctx)
        {
            _mouseInputBuffer += ctx.ReadValue<Vector2>();
        }

        private void Jump(InputAction.CallbackContext ctx)
        {
            JumpCommand jumpCommand = new JumpCommand(_abilitySystem);
            _commandInvoker.AddCommand(jumpCommand);
        }

        private void Roll(InputAction.CallbackContext ctx)
        {
            RollCommand rollCommand = new RollCommand(_abilitySystem);
            _commandInvoker.AddCommand(rollCommand);
        }

        private void StartSprint(InputAction.CallbackContext ctx)
        {
            WalkCommand walkCommand = new WalkCommand(_abilitySystem, true);
            _commandInvoker.AddCommand(walkCommand);
        }

        private void StopSprint(InputAction.CallbackContext ctx)
        {
            WalkCommand walkCommand = new WalkCommand(_abilitySystem, false);
            _commandInvoker.AddCommand(walkCommand);
        }

        private void StartCrouch(InputAction.CallbackContext ctx)
        {
            CrouchCommand crouchCommand =
                new CrouchCommand(_abilitySystem, true);
            _commandInvoker.AddCommand(crouchCommand);
        }

        private void StopCrouch(InputAction.CallbackContext ctx)
        {
            CrouchCommand crouchCommand =
                new CrouchCommand(_abilitySystem, false);
            _commandInvoker.AddCommand(crouchCommand);
        }

        private void StartZoom(InputAction.CallbackContext ctx)
        {
            ZoomCommand zoomCommand = new ZoomCommand(_abilitySystem, true);
            _commandInvoker.AddCommand(zoomCommand);
        }

        private void StopZoom(InputAction.CallbackContext ctx)
        {
            ZoomCommand zoomCommand = new ZoomCommand(_abilitySystem, false);
            _commandInvoker.AddCommand(zoomCommand);
        }

        private void Crawl(InputAction.CallbackContext ctx)
        {
            CrawlCommand crawlCommand = new CrawlCommand(_abilitySystem);
            _commandInvoker.AddCommand(crawlCommand);
        }

        private void Interact(InputAction.CallbackContext ctx)
        {
            InteractCommand interactCommand = new InteractCommand(_interactionComponent);
            interactCommand.Execute();
        }
    }
}