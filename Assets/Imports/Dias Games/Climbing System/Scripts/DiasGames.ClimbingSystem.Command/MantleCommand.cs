using DiasGames.AbilitySystem.Core;
using DiasGames.Command;

namespace DiasGames.ClimbingSystem.Command
{
    public class MantleCommand : IActionCommand
    {
        private readonly IAbilitySystem _abilitySystem;

        public MantleCommand(IAbilitySystem abilitySystem)
        {
            _abilitySystem = abilitySystem;
        }
        
        public void Execute()
        {
            if (_abilitySystem != null)
            {
                _abilitySystem.StartAbilityByName("Mantle", _abilitySystem.GameObject);
            }
        }
    }
}