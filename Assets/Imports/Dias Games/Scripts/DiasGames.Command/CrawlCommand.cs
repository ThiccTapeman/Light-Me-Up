using DiasGames.AbilitySystem.Abilities;
using DiasGames.AbilitySystem.Core;

namespace DiasGames.Command
{
    public class CrawlCommand : IActionCommand
    {
        private readonly IAbilitySystem _abilitySystem;
        private readonly CrawlAbility _crawlAbility;

        public CrawlCommand(IAbilitySystem abilitySystem)
        {
            _abilitySystem = abilitySystem;
            _crawlAbility = abilitySystem.GetAbility<CrawlAbility>();
        }
        
        public void Execute()
        {
            if (_crawlAbility != null)
            {
                _crawlAbility.ToggleCrawl();
                _abilitySystem.StartAbilityByName("Crawl", _abilitySystem.GameObject);
            }
        }
    }
}