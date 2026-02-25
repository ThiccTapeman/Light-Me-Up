using DiasGames.AbilitySystem.Core;
using DiasGames.Components;
using UnityEngine;

namespace DiasGames.AbilitySystem.Effects
{
    [CreateAssetMenu(fileName = "Falling Health Damage", menuName = "Dias Games/Effects/FallingHealthDamage", order = 0)]
    public class FallingHealthDamage : GameplayEffect
    {
        private const float MIN_DAMAGE = 10.0f;
        
        [SerializeField] private float _minHeight = 5.0f;
        [SerializeField] private float _heightForKillOnLand = 10f;

        private AttributesComponent _attributesComponent;
        [SerializeField][HideInInspector] private float _highestPosition = 1.0f;

        public override void Setup(IAbilitySystem ownerSystem)
        {
            base.Setup(ownerSystem);
            _attributesComponent = ownerSystem.GameObject.GetComponent<AttributesComponent>();
        }

        protected override void ExecuteEffect(float deltaTime, GameObject instigator)
        {
            float currentHeight = _highestPosition - transform.position.y - _minHeight;
            float ratio = currentHeight / (_heightForKillOnLand - _minHeight);

            float value = Mathf.Max(MIN_DAMAGE, _attributesComponent.GetAttributeByType(AttributeType.Health).MaxValue * ratio);
            _attributesComponent.ApplyChange(AttributeType.Health, -value, null);
        }

        public void SetHighestPosition(float highestPosition)
        {
            _highestPosition = highestPosition;
        }
    }
}