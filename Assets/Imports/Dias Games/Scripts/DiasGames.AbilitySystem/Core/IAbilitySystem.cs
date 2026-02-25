using UnityEngine;

namespace DiasGames.AbilitySystem.Core
{
    public interface IAbilitySystem
    {
        GameObject GameObject { get; }
        AbilityTagContainer GetActiveTags();
        TAbility GetAbility<TAbility>() where TAbility : Ability;
        void CancelAbilitiesWithTag(AbilityTagContainer cancelTags, GameObject instigator);
        bool StartAbilityByName(string abilityName, GameObject instigator);
        bool StopAbilityByName(string abilityName, GameObject instigator);
        Ability AddAbility(Ability newAbility, GameObject instigator);
        bool RemoveAbilityByName(string abilityName, GameObject instigator);
    }
}