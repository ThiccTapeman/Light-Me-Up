using DiasGames.AbilitySystem.Abilities;
using DiasGames.AbilitySystem.Core;

namespace DiasGames.Command
{
    public class WalkCommand: IActionCommand
    {
        private readonly Locomotion _locomotion;
        private readonly bool _pressed;

        public WalkCommand(IAbilitySystem abilitySystem, bool pressed)
        {
            _locomotion = abilitySystem.GetAbility<Locomotion>();
            _pressed = pressed;
        }

        public void Execute()
        {
            if (_locomotion)
            {
                _locomotion.WalkButtonPressed(_pressed);
            }
        }
    }
}