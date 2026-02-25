using DiasGames.AbilitySystem.Core;
using DiasGames.Command;

namespace DiasGames.ClimbingSystem.Command
{
    public class DropCommand : IActionCommand
    {
        private readonly IAbilitySystem _abilitySystem;

        public DropCommand(IAbilitySystem abilitySystem)
        {
            _abilitySystem = abilitySystem;
        }
        
        public void Execute()
        {
            if (_abilitySystem != null)
            {
                _abilitySystem.StartAbilityByName("Climb Drop", _abilitySystem.GameObject);
            }
        }
    }
}