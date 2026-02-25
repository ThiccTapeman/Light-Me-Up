using DiasGames.AbilitySystem.Abilities;
using DiasGames.AbilitySystem.Core;

namespace DiasGames.Command
{
    public class ZoomCommand : IActionCommand
    {
        private readonly bool _isZooming;
        private readonly StrafeAbility _strafeAbility;

        public ZoomCommand(IAbilitySystem abilitySystem,  bool isZooming)
        {
            _isZooming = isZooming;
            _strafeAbility = abilitySystem.GetAbility<StrafeAbility>();
        }

        public void Execute()
        {
            if (_strafeAbility)
            {
                _strafeAbility.Zoom(_isZooming);
            }
        }
    }
}