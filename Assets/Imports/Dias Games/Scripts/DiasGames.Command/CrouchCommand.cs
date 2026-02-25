using DiasGames.AbilitySystem.Abilities;
using DiasGames.AbilitySystem.Core;

namespace DiasGames.Command
{
    public class CrouchCommand : IActionCommand
    {
        private readonly IAbilitySystem _abilitySystem;
        private readonly CrouchAbility _crouchAbility;
        private readonly bool _pressed;

        public CrouchCommand(IAbilitySystem abilitySystem, bool pressed)
        {
            _abilitySystem = abilitySystem;
            _crouchAbility = abilitySystem.GetAbility<CrouchAbility>();
            _pressed = pressed;
        }

        public void Execute()
        {
            if (_abilitySystem == null || _crouchAbility == null)
            {
                return;
            }

            _crouchAbility.CrouchPressed(_pressed);
            if (_pressed)
            {
                _abilitySystem.StartAbilityByName("Crouch", _abilitySystem.GameObject);
            }
        }
    }
}