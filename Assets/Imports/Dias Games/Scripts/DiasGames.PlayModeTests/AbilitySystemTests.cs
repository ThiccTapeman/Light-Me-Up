using System;
using System.Collections;
using System.Collections.Generic;
using DiasGames.AbilitySystem.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DiasGames.PlayModeTests
{
    public class AbilitySystemTests
    {
        private GameObject _character;
        private AbilitySystemController _abilitySystemController;

        internal class AbilityMockup : Ability
        {
            [SerializeField] private bool _myAutoStart = false;
            public override bool AutoStart => _myAutoStart;

            public void SetName(string newName)
            {
                _abilityName = newName;
            }

            public void SetAutoStart(bool autoStart)
            {
                _myAutoStart = autoStart;
            }

            protected override void OnStartAbility(GameObject instigator)
            { }

            public override void UpdateAbility(float deltaTime)
            { }

            protected override void OnStopAbility(GameObject instigator)
            { }
        }
        
        internal class EffectMockup : GameplayEffect
        {
            public int ExecutedTimes { get; private set; }
            public void SetName(string newName)
            {
                _abilityName = newName;
            }

            protected override void ExecuteEffect(float deltaTime, GameObject instigator)
            {
                ExecutedTimes++;
            }
        }

        private AbilityTagContainer CreateTags(string[] tagNames)
        {
            List<AbilityTag> abilityTags = new List<AbilityTag>(tagNames.Length);
            foreach (string tagName in tagNames)
            {
                AbilityTag tag = new AbilityTag(tagName);
                abilityTags.Add(tag);
            }

            return new AbilityTagContainer(abilityTags);
        }

        private TAbility CreateAbility<TAbility>(string abilityName, string[] tags) 
            where TAbility : AbilityMockup
        {
            AbilityTagContainer tagContainer = CreateTags(tags);
            TAbility mockupA =  ScriptableObject.CreateInstance<TAbility>();
            mockupA.ActivationTags.AddTags(tagContainer);
            mockupA.SetName(abilityName);

            return mockupA;
        }
        
        private TAbility CreateEffect<TAbility>(string effectName, string[] tags) 
            where TAbility : EffectMockup
        {
            AbilityTagContainer tagContainer = CreateTags(tags);
            TAbility mockupA =  ScriptableObject.CreateInstance<TAbility>();
            mockupA.ActivationTags.AddTags(tagContainer);
            mockupA.SetName(effectName);

            return mockupA;
        }

        [SetUp]
        public void CreateInstancesForTest()
        {
            _character = new GameObject("Character for Test");
            _abilitySystemController =  _character.AddComponent<AbilitySystemController>();
        }

        [Test]
        public void When_ComparingTags_EnsureTagsWithExactNames()
        {
            string targetName = "Test.Subclass.Third";
            AbilityTag tagA = new AbilityTag(targetName);

            AbilityTagContainer tagContainerA = new AbilityTagContainer(new List<AbilityTag>{tagA});
            AbilityTagContainer tagContainerB = new AbilityTagContainer(new List<AbilityTag>{tagA});

            bool hasTag = tagContainerA.HasAnyTag(tagContainerB);
            Assert.IsTrue(hasTag, "Tag Comparison, in AbilityTagContainer, is broken");
        }

        [Test]
        public void When_ComparingTags_EnsureTagsWithParentNames()
        {
            string childName = "Test.Subclass.Third";
            string parentName = "Test.Subclass";

            AbilityTag childTag = new AbilityTag(childName);
            AbilityTag parentTag = new AbilityTag(parentName);

            AbilityTagContainer childContainer = new AbilityTagContainer(new List<AbilityTag>{childTag});
            AbilityTagContainer parentContainer = new AbilityTagContainer(new List<AbilityTag>{parentTag});

            bool hasTag = childContainer.HasAnyTag(parentContainer);
            Assert.IsTrue(hasTag, "Tag Parent Comparison is broken");
        }

        [Test]
        public void When_AddingAbility_CheckIfWasAddedCorrectly()
        {
            const string abilityName = "mockup ability";
            AbilityMockup mockupA =
                CreateAbility<AbilityMockup>(abilityName, new[] { "Grounded", "Rolling", "Crouched" });

            _abilitySystemController.AddAbility(mockupA, _character);
            Assert.NotNull(_abilitySystemController.GetAbility<AbilityMockup>(),$"Ability was not added correctly");

            _abilitySystemController.RemoveAbilityByName(abilityName, _character);
        }

        [Test]
        public void When_AddingAbility_AvoidDuplicates()
        {
            const string abilityName = "mockup ability";
            AbilityMockup mockupA =
                CreateAbility<AbilityMockup>(abilityName, new[] { "Grounded", "Rolling", "Crouched" });

            _abilitySystemController.AddAbility(mockupA, _character);
            bool duplicate = _abilitySystemController.AddAbility(mockupA, _character);
            Assert.False(duplicate,$"Duplicated ability was added");

            _abilitySystemController.RemoveAbilityByName(abilityName, _character);
        }

        [Test]
        public void StartAbility_When_TheresNoActiveTags()
        {
            const string abilityName = "mockup ability";
            string[] tags = new[] { "Grounded", "Rolling", "Crouched" };
            AbilityMockup mockupA =
                CreateAbility<AbilityMockup>(abilityName, tags);
            _abilitySystemController.AddAbility(mockupA, _character);

            Assert.IsTrue(_abilitySystemController.GetActiveTags().Tags.Count == 0, "There is active tags running");

            _abilitySystemController.StartAbilityByName(abilityName, _character);
            
            Assert.IsTrue(_abilitySystemController.GetAbility<AbilityMockup>().IsRunning, $"Ability is not running!");

            bool hasAllTags = true;
            foreach (AbilityTag tag in _abilitySystemController.GetActiveTags().Tags)
            {
                bool foundTag = false;
                foreach (string tagName in tags)
                {
                    if (tagName.Equals(tag.Name))
                    {
                        foundTag = true;
                        break;
                    }
                }

                if (!foundTag)
                {
                    hasAllTags = false;
                    break;
                }
            }

            Assert.IsTrue(hasAllTags, $"Tags were not added correctly");

            _abilitySystemController.RemoveAbilityByName(abilityName, _character);
        }
        
        [Test]
        public void When_StoppingAbilitiesWithSameTag_DontRemoveSharedTagAfterOneStops()
        {
            const string abilityNameA = "mockup ability A";
            const string abilityNameB = "mockup ability B";
            string[] tagsA = new[] { "Grounded", "Rolling", "Crouched" };
            string[] tagsB = new[] { "Grounded", "Equipped" };
            AbilityTag sharedTag = new AbilityTag("Shared");

            AbilityMockup mockupA =
                CreateAbility<AbilityMockup>(abilityNameA, tagsA);
            mockupA.ActivationTags.Tags.Add(sharedTag);
            
            AbilityMockup mockupB =
                CreateAbility<AbilityMockup>(abilityNameB, tagsB);
            mockupB.ActivationTags.Tags.Add(sharedTag);
            
            _abilitySystemController.AddAbility(mockupA, _character);
            _abilitySystemController.AddAbility(mockupB, _character);

            _abilitySystemController.StartAbilityByName(abilityNameA, _character);
            _abilitySystemController.StartAbilityByName(abilityNameB, _character);

            _abilitySystemController.RemoveAbilityByName(abilityNameA, _character);

            bool containsTag = _abilitySystemController.GetActiveTags().Tags.Exists(x => x.Name == "Grounded");
            bool containsSharedTag = _abilitySystemController.GetActiveTags().Tags.Exists(x => x.Name == "Shared");
            Assert.IsTrue(containsTag && containsSharedTag, "Tag was removed incorrectly");
            
            _abilitySystemController.RemoveAbilityByName(abilityNameB, _character);
        }
        
        [Test]
        public void DontStartAbility_WithBlockingTagsActive()
        {
            const string abilityNameA = "mockup ability A";
            const string abilityNameB = "mockup ability B";
            
            string[] tagsA = new[] { "Grounded", "Rolling", "Crouched" };
            const string blocking1 = "Grounded";
            const string blocking2 = "Running";
            
            AbilityMockup mockupA =
                CreateAbility<AbilityMockup>(abilityNameA, tagsA);
            
            AbilityMockup mockupB =
                CreateAbility<AbilityMockup>(abilityNameB, tagsA);
            mockupB.BlockingTags.Tags.Add(new AbilityTag(blocking1));
            mockupB.BlockingTags.Tags.Add(new AbilityTag(blocking2));
            
            _abilitySystemController.AddAbility(mockupA, _character);
            _abilitySystemController.AddAbility(mockupB, _character);

            _abilitySystemController.StartAbilityByName(abilityNameA, _character);
            bool startedB = _abilitySystemController.StartAbilityByName(abilityNameB, _character);
            Assert.IsFalse(startedB);
            
            _abilitySystemController.RemoveAbilityByName(abilityNameA, _character);
            _abilitySystemController.RemoveAbilityByName(abilityNameB, _character);
        }
        
        [Test]
        public void When_RequiringTags_OnlyStartIfAllTagsArePresent() 
        {
            const string abilityNameA = "mockup ability A";
            const string abilityNameB = "mockup ability B";
            const string abilityNameC = "mockup ability C";

            string[] tagsA = new[] { "Grounded", "Rolling", "Crouched", "FastSpeed" };
            string[] requiredTagA = new[] { "Grounded", "Running"};
            string[] requiredTagB = new[] { "Grounded", "Crouched" };

            AbilityMockup mockupA =
                CreateAbility<AbilityMockup>(abilityNameA, tagsA);

            AbilityMockup mockupB =
                CreateAbility<AbilityMockup>(abilityNameB, tagsA);

            AbilityMockup mockupC =
                CreateAbility<AbilityMockup>(abilityNameC, tagsA);

            mockupB.RequiredTags.AddTags(CreateTags(requiredTagA));
            mockupC.RequiredTags.AddTags(CreateTags(requiredTagB));

            _abilitySystemController.AddAbility(mockupA, _character);
            _abilitySystemController.AddAbility(mockupB, _character);
            _abilitySystemController.AddAbility(mockupC, _character);

            _abilitySystemController.StartAbilityByName(abilityNameA, _character);
            bool mockupBStarted = _abilitySystemController.StartAbilityByName(abilityNameB, _character);
            bool mockupCStarted = _abilitySystemController.StartAbilityByName(abilityNameC, _character);

            Assert.IsFalse(mockupBStarted);
            Assert.IsTrue(mockupCStarted);

            _abilitySystemController.RemoveAbilityByName(abilityNameA, _character);
            _abilitySystemController.RemoveAbilityByName(abilityNameB, _character);
            _abilitySystemController.RemoveAbilityByName(abilityNameC, _character);
        }

        [Test]
        public void When_StartAbilityWithCancelTags_CancelActiveAbilitiesWithAnyOfCancelTags()
        {
            Ability activeAbility = null;
            _abilitySystemController.OnAbilityStarted += ability => activeAbility = ability;
            
            const string abilityNameA = "mockup ability A";
            const string abilityNameB = "mockup ability B";

            string[] tagsA = new[] { "Grounded.Rolling", "Crouching"};
            string[] tagsB = new[] { "Air.Jump" };

            AbilityMockup mockupA =
                CreateAbility<AbilityMockup>(abilityNameA, tagsA);
 
            AbilityMockup mockupB =
                CreateAbility<AbilityMockup>(abilityNameB, tagsB);

            mockupB.CancelWithTags.Tags.Add(new AbilityTag("Grounded"));
            mockupA.CancelWithTags.Tags.Add(new AbilityTag("Air.Jump"));

            _abilitySystemController.AddAbility(mockupA, _character);
            _abilitySystemController.AddAbility(mockupB, _character);

            _abilitySystemController.StartAbilityByName(abilityNameA, _character);
            Assert.AreEqual(mockupA.AbilityName, activeAbility.AbilityName);
            Assert.IsTrue(activeAbility.IsRunning);

            Ability previousAbility = activeAbility;
            _abilitySystemController.StartAbilityByName(abilityNameB, _character);
            Assert.IsFalse(previousAbility.IsRunning);
            
            previousAbility = activeAbility;
            _abilitySystemController.StartAbilityByName(abilityNameA, _character);
            Assert.IsFalse(previousAbility.IsRunning);
            
            _abilitySystemController.RemoveAbilityByName(abilityNameA, _character);
            _abilitySystemController.RemoveAbilityByName(abilityNameB, _character);
        }
        
        [UnityTest]
        public IEnumerator When_AddingAutoStartAbility_DontRunStartTwice()
        {
            int startedAmount = 0;
            _abilitySystemController.OnAbilityStarted += ability => startedAmount++;
            
            const string abilityNameA = "mockup ability A";

            string[] tagsA = { "Grounded.Rolling", "Crouching"};

            AbilityMockup mockupA =
                CreateAbility<AbilityMockup>(abilityNameA, tagsA);

            mockupA.SetAutoStart(true);

            _abilitySystemController.AddAbility(mockupA, _character);

            yield return new WaitForSeconds(0.5f);

            Assert.AreEqual(1, startedAmount);
        }
        
        [UnityTest]
        public IEnumerator When_AddingInstantEffect_ExecuteEffectOnce()
        {
            const string effectName = "effect A";

            string[] tagsA = { "Effect A"};

            EffectMockup mockupA =
                CreateEffect<EffectMockup>(effectName, tagsA);

            EffectMockup addedEffect = (EffectMockup)_abilitySystemController.AddAbility(mockupA, _character);

            yield return new WaitForSeconds(0.5f);

            Assert.NotNull(addedEffect);
            Assert.AreEqual(1, addedEffect.ExecutedTimes);
        }
        
        [UnityTest]
        public IEnumerator When_AddingEffect_RemoveIfBlocked()
        {
            const string effectName = "effect A";

            string[] tagsA = { "Effect A"};

            AbilityMockup ability = CreateAbility<AbilityMockup>("mock ability", tagsA);
            EffectMockup mockupA =
                CreateEffect<EffectMockup>(effectName, tagsA);
            mockupA.BlockingTags.Tags.Add(new AbilityTag(tagsA[0]));

            _abilitySystemController.AddAbility(ability, _character);
            _abilitySystemController.StartAbilityByName(ability.AbilityName, _character);

            EffectMockup addedEffect = (EffectMockup)_abilitySystemController.AddAbility(mockupA, _character);

            yield return new WaitForSeconds(0.5f);

            Assert.NotNull(addedEffect);
            Assert.AreEqual(0, addedEffect.ExecutedTimes);
            Assert.IsNull(_abilitySystemController.GetAbility<EffectMockup>());
        }
    }
}