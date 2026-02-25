using UnityEngine;

namespace DiasGames.AbilitySystem.Core
{
    public abstract class GameplayEffect : Ability
    {
        [SerializeField] private bool _useDeltaTime;
        [SerializeField] private float _duration = 0;

        protected float ElapsedTime => Time.time - StartTime;
        public override bool AutoStart => true;
        public float Duration => !_useDeltaTime ? 0 : _duration;
        public bool UseDeltaTime => _useDeltaTime;

        private GameObject _instigator;

        public override bool CanStart()
        {
            bool canStart = base.CanStart();
            if (!canStart)
            {
                OwnerSystem.RemoveAbilityByName(AbilityName, OwnerSystem.GameObject);
            }

            return canStart;
        }

        protected override void OnStartAbility(GameObject instigator)
        {
            _instigator = instigator;
            if (Duration <= 0.0f)
            {
                ExecuteEffect(1.0f, _instigator);
                StopAbility(instigator);
            }
        }

        public override void UpdateAbility(float deltaTime)
        {
            if (ElapsedTime >= Duration)
            {
                StopAbility(OwnerSystem.GameObject);
            }

            if (_useDeltaTime)
            {
                ExecuteEffect(deltaTime, _instigator);
            }
        }

        protected override void OnStopAbility(GameObject instigator)
        {
            OwnerSystem.RemoveAbilityByName(AbilityName, instigator);
        }

        protected abstract void ExecuteEffect(float deltaTime, GameObject instigator);
    }
}