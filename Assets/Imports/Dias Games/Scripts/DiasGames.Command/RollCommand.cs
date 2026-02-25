using DiasGames.AbilitySystem.Core;

namespace DiasGames.Command
{
    public class RollCommand : IActionCommand
    {
        private readonly IAbilitySystem _abilitySystem;

        public RollCommand(IAbilitySystem abilitySystem)
        {
            _abilitySystem = abilitySystem;
        }

        public void Execute()
        {
            if (_abilitySystem != null)
            {
                _abilitySystem.StartAbilityByName("Roll", _abilitySystem.GameObject);
            }
        }
    }
}