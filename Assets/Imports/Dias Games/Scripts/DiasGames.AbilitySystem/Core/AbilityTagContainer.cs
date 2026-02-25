using System;
using System.Collections.Generic;
using UnityEngine;

namespace DiasGames.AbilitySystem.Core
{
    [Serializable]
    public class AbilityTagContainer
    {
        [SerializeField] private List<AbilityTag> _tags = new List<AbilityTag>();

        public List<AbilityTag> Tags => _tags;
        public int Count => _tags.Count;

        public AbilityTagContainer(List<AbilityTag> tags)
        {
            _tags = tags;
        }

        public bool HasTag(AbilityTag tagToCompare, bool ignoreParent=false)
        {
            if (ignoreParent)
            {
                return HasExactTag(tagToCompare);
            }
            
            foreach (AbilityTag tag in _tags)
            {
                string[] splittedTag = GetSplittedTag(tag.Name);
                string tagBuiltBySplit = string.Empty;
                for (int i = 0; i < splittedTag.Length; i++)
                {
                    tagBuiltBySplit += splittedTag[i];
                    if (string.Equals(tagToCompare.Name,
                            tagBuiltBySplit,
                            StringComparison.Ordinal))
                    {
                        return true;
                    }

                    tagBuiltBySplit += '.';
                }
            }

            return false;
        }

        private bool HasExactTag(AbilityTag tagToCompare)
        {
            foreach (AbilityTag tag in _tags)
            {
                if (string.Equals(tagToCompare.Name,
                        tag.Name,
                        StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasAnyTag(AbilityTagContainer container)
        {
            foreach (AbilityTag tagToCompare in container.Tags)
            {
                if (HasTag(tagToCompare))
                {
                    return true;
                }
            }

            return false;
        }
        
        public bool HasAllTags(AbilityTagContainer container)
        { 
            foreach (AbilityTag tagToCompare in container.Tags)
            {
                if (!HasTag(tagToCompare))
                {
                    return false;
                }
            }

            return true;
        }

        public void AddTags(AbilityTagContainer tagsToAdd)
        {
            _tags.AddRange(tagsToAdd.Tags);
        }

        public void RemoveTags(AbilityTagContainer tagsToRemove)
        {
            foreach (AbilityTag tag in tagsToRemove.Tags)
            {
                _tags.Remove(tag);
            }
        }

        public bool IsEmpty()
        {
            return _tags.Count == 0;
        }

        public string[] GetSplittedTag(string tagName)
        {
            return tagName.Split('.');
        }
    }
}