using DiasGames.AbilitySystem.Core;
using DiasGames.Command;

namespace DiasGames.ClimbingSystem.Command
{
    public class ClimbJumpCommand : IActionCommand
    {
        private readonly IAbilitySystem _abilitySystem;

        public ClimbJumpCommand(IAbilitySystem abilitySystem)
        {
            _abilitySystem = abilitySystem;
        }
        
        public void Execute()
        {
            if (_abilitySystem != null)
            {
                _abilitySystem.StartAbilityByName("Climb Jump", _abilitySystem.GameObject);
                _abilitySystem.StartAbilityByName("Climb Up", _abilitySystem.GameObject);
                _abilitySystem.StartAbilityByName("Climb Jump Back", _abilitySystem.GameObject);
                _abilitySystem.StartAbilityByName("Climb Jump Side", _abilitySystem.GameObject);
            }
        }
    }
}