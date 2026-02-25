using DiasGames.AbilitySystem.Core;
using DiasGames.Components;
using UnityEngine;

namespace DiasGames.AbilitySystem.Effects
{
    [CreateAssetMenu(fileName = "Attribute Change Effect", menuName = "Dias Games/Effects/Attribute Change Effect", order = 0)]
    public class AttributeChangeEffect : GameplayEffect
    {
        [SerializeField] private AttributeType _attributeType;
        [SerializeField] private float _absoluteValue;

        private AttributesComponent _attributesComponent;
        private float _value;

        public float AbsoluteValue => _absoluteValue;

        public override void Setup(IAbilitySystem ownerSystem)
        {
            base.Setup(ownerSystem);
            _attributesComponent = OwnerSystem.GameObject.GetComponent<AttributesComponent>();
        }

        protected override void OnStartAbility(GameObject instigator)
        {
            _value = UseDeltaTime ? _absoluteValue / Duration : _absoluteValue;

            base.OnStartAbility(instigator);
        }

        protected override void ExecuteEffect(float deltaTime, GameObject instigator)
        {
            _attributesComponent.ApplyChange(_attributeType, _value * deltaTime, instigator);
        }
    }
}