using System.Collections.Generic;
using NUnit.Framework;
using DiasGames.AbilitySystem.Core;
using DiasGames.AbilitySystem.Editor;

namespace DiasGames.EditModeTests
{
    public class TagSettingsTests
    {
        [Test]
        public void Ensure_UniqueNames_ForTags()
        {
            var settings = AbilityTagSettings.GetOrCreateSettings();
            List<string> names = new List<string>(20);

            bool hasDuplicates = false;
            foreach (var tag in settings.Tags)
            {
                if (names.Contains(tag.Name))
                {
                    hasDuplicates = true;
                    break;
                }
                
                names.Add(tag.Name);
            }

            Assert.IsFalse(hasDuplicates, "There are duplicated tags");
        }
        
        [Test]
        public void When_CreatingNewTag_DontAllowDuplicates()
        {
            AbilityTagSettingsProvider provider = (AbilityTagSettingsProvider)AbilityTagSettingsProvider.CreateAbilityTagSettingsProvider();
            provider.OnActivate("", null);
            string tag = System.Guid.NewGuid().ToString();
            bool firstAttempt = provider.CreateNewTag(tag);
            bool secondAttempt = provider.CreateNewTag(tag);

            Assert.IsTrue(firstAttempt, $"Couldn't create {tag}");
            Assert.IsFalse(secondAttempt, $"Duplicated tag was created: {tag}");

            bool deleted = provider.DeleteTag(tag);
            Assert.IsTrue(deleted, "Couldn't delete created tags");
        }
        
        [Test]
        public void Ensure_Tag_Deletion()
        {
            AbilityTagSettingsProvider provider = (AbilityTagSettingsProvider)AbilityTagSettingsProvider.CreateAbilityTagSettingsProvider();
            provider.OnActivate("", null);
            string tag = System.Guid.NewGuid().ToString();
            bool created = provider.CreateNewTag(tag);

            Assert.IsTrue(created, $"Couldn't create {tag}");

            bool deleted = provider.DeleteTag(tag);
            Assert.IsTrue(deleted, "Couldn't delete created tag");
        }
        
        [Test]
        public void DontDelete_NonExistentTags()
        {
            AbilityTagSettingsProvider provider = (AbilityTagSettingsProvider)AbilityTagSettingsProvider.CreateAbilityTagSettingsProvider();
            provider.OnActivate("", null);
            string tag = System.Guid.NewGuid().ToString();

            bool deleted = provider.DeleteTag(tag);
            Assert.IsFalse(deleted, "Deleted tag that doesn't exist");
        }
    }
}