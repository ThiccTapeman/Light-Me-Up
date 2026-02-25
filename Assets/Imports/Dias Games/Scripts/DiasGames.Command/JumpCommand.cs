using DiasGames.AbilitySystem.Core;

namespace DiasGames.Command
{
    public class JumpCommand : IActionCommand
    {
        private readonly IAbilitySystem _abilitySystem;

        public JumpCommand(IAbilitySystem abilitySystem)
        {
            _abilitySystem = abilitySystem;
        }

        public void Execute()
        {
            if (_abilitySystem != null)
            {
                _abilitySystem.StartAbilityByName("Predicted Jump", _abilitySystem.GameObject);
                _abilitySystem.StartAbilityByName("Jump", _abilitySystem.GameObject);
            }
        }
    }
}