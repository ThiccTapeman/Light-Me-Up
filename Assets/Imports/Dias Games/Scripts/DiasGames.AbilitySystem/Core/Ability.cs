using System;
using System.Collections.Generic;
using UnityEngine;

namespace DiasGames.AbilitySystem.Core
{
    public abstract class Ability : ScriptableObject
    {
        public event Action<Ability> OnAbilityStarted; 
        public event Action<Ability> OnAbilityStopped; 
        
        [SerializeField] internal string _abilityName = string.Empty;
        [SerializeField] private bool _autoStart;
        [Header("Tags")] 
        [SerializeField] private AbilityTagContainer _activationTags = new AbilityTagContainer(new List<AbilityTag>());
        [SerializeField] private AbilityTagContainer _requiredTags = new AbilityTagContainer(new List<AbilityTag>());
        [SerializeField] private AbilityTagContainer _blockingTags = new AbilityTagContainer(new List<AbilityTag>());
        [SerializeField] private AbilityTagContainer _cancelWithTags = new AbilityTagContainer(new List<AbilityTag>());

        public string AbilityName => _abilityName;
        public virtual bool AutoStart => _autoStart;
        public AbilityTagContainer ActivationTags => _activationTags;
        public AbilityTagContainer RequiredTags => _requiredTags;
        public AbilityTagContainer BlockingTags => _blockingTags;
        public AbilityTagContainer CancelWithTags => _cancelWithTags;
        protected Transform transform => OwnerSystem.GameObject.transform;

        public bool IsRunning { get; private set; }
        protected float StartTime;
        protected float StopTime;
        protected IAbilitySystem OwnerSystem;
        protected IAnimationController AnimationController;
        protected IMovement Movement;

        public virtual void Setup(IAbilitySystem ownerSystem)
        {
            OwnerSystem = ownerSystem;
            AnimationController = ownerSystem.GameObject.GetComponent<IAnimationController>();
            Movement = ownerSystem.GameObject.GetComponent<IMovement>();
        }

        public virtual bool CanStart()
        {
            if (IsRunning)
            {
                return false;
            }

            AbilityTagContainer activeTags = OwnerSystem.GetActiveTags();
            if (activeTags.Tags.Count == 0)
            {
                return true;
            }

            if (activeTags.HasAnyTag(BlockingTags))
            {
                return false;
            }

            if (!activeTags.HasAllTags(RequiredTags))
            {
                return false;
            }

            return true;
        }

        public bool StartAbility(GameObject instigator)
        {
            if (IsRunning || !CanStart())
            {
                return false;
            }

            OwnerSystem.GetActiveTags().AddTags(ActivationTags);
            OwnerSystem.CancelAbilitiesWithTag(CancelWithTags, instigator);

            StartTime = Time.time;
            OnStartAbility(instigator);
            IsRunning = true;
            OnAbilityStarted?.Invoke(this);
            return true;
        }

        public bool StopAbility(GameObject instigator)
        {
            if (!IsRunning)
            {
                return false;
            }

            IsRunning = false;
            StopTime = Time.time;
            RemoveTags(ActivationTags);
            OnStopAbility(instigator);
            OnAbilityStopped?.Invoke(this);
            return true;
        }

        protected abstract void OnStartAbility(GameObject instigator);
        public abstract void UpdateAbility(float deltaTime);
        protected abstract void OnStopAbility(GameObject instigator);

        public Ability Clone()
        {
            return Instantiate(this);
        }

        private void RemoveTags(AbilityTagContainer tagsToRemove)
        {
            OwnerSystem.GetActiveTags().RemoveTags(tagsToRemove);
        }
    }
}