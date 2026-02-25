using DiasGames.AbilitySystem.Core;
using UnityEngine;

namespace DiasGames.Trigger
{
    public class DamageTrigger : MonoBehaviour
    {
        [SerializeField] private GameplayEffect _damageEffect;
        [SerializeField] private string ignoreTag = string.Empty;

        private void OnTriggerEnter(Collider other)
        {
            if (!enabled || (!string.IsNullOrEmpty(ignoreTag) && other.CompareTag(ignoreTag))) return;

            if (other.TryGetComponent(out IAbilitySystem abilitySystem))
            {
                abilitySystem.AddAbility(_damageEffect, gameObject);
            }
        }
    }
}