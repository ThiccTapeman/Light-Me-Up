using DiasGames.AbilitySystem.Core;
using DiasGames.ClimbingSystem.Abilities;
using DiasGames.Command;

namespace DiasGames.ClimbingSystem.Command
{
    public class DropLedgeCommand : IActionCommand
    {
        private readonly DropToLedge _dropToLedgeAbility;
        private readonly bool _pressed;

        public DropLedgeCommand(IAbilitySystem abilitySystem, bool pressed)
        {
            _dropToLedgeAbility = abilitySystem.GetAbility<DropToLedge>();
            _pressed = pressed;
        }
        
        public void Execute()
        {
            if (_dropToLedgeAbility)
            {
                _dropToLedgeAbility.SetHoldDrop(_pressed);
            }
        }
    }
}