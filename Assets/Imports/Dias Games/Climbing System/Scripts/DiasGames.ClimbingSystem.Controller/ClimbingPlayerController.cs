using DiasGames.ClimbingSystem.Command;
using DiasGames.ClimbingSystem.Components;
using DiasGames.Controller;
using UnityEngine.InputSystem;

namespace DiasGames.ClimbingSystem.Controller
{
    public class ClimbingPlayerController : PlayerController
    {
        private ClimbingComponent _climbingComponent;

        protected override void Awake()
        {
            base.Awake();
            _climbingComponent = GetComponent<ClimbingComponent>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _movement.OnLanded += ResetLedge;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _movement.OnLanded -= ResetLedge;
        }

        protected override void BindInputEvents()
        {
            base.BindInputEvents();
            _playerInput.actions["Jump"].performed += ClimbJump;
            _playerInput.actions["Drop"].performed += Drop;
            _playerInput.actions["Walk"].performed += HoldDropToLedge;
            _playerInput.actions["Walk"].canceled += UnholdDropToLedge;
        }

        protected override void UnbindInputEvents()
        {
            base.UnbindInputEvents();
            _playerInput.actions["Jump"].performed -= ClimbJump;
            _playerInput.actions["Drop"].performed -= Drop;
            _playerInput.actions["Walk"].performed -= HoldDropToLedge;
            _playerInput.actions["Walk"].canceled -= UnholdDropToLedge;
        }

        private void ResetLedge()
        {
            _climbingComponent.SetCurrentLedge(new ClimbCastData());
        }

        private void Drop(InputAction.CallbackContext ctz)
        {
            DropCommand dropCommand = new DropCommand(_abilitySystem);
            _commandInvoker.AddCommand(dropCommand);
        }

        private void ClimbJump(InputAction.CallbackContext ctx)
        {
            ClimbJumpCommand jumpCommand = new ClimbJumpCommand(_abilitySystem);
            MantleCommand mantleCommand = new MantleCommand(_abilitySystem);
            _commandInvoker.AddCommand(jumpCommand);
            _commandInvoker.AddCommand(mantleCommand);
        }

        private void HoldDropToLedge(InputAction.CallbackContext obj)
        {
            DropLedgeCommand dropLedgeCommand = new DropLedgeCommand(_abilitySystem, true);
            _commandInvoker.AddCommand(dropLedgeCommand);
        }

        private void UnholdDropToLedge(InputAction.CallbackContext obj)
        {
            DropLedgeCommand dropLedgeCommand = new DropLedgeCommand(_abilitySystem, false);
            _commandInvoker.AddCommand(dropLedgeCommand);
        }
    }
}