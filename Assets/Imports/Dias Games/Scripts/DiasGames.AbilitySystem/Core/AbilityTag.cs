using System;
using UnityEngine;

namespace DiasGames.AbilitySystem.Core
{
    [Serializable]
    public class AbilityTag
    {
        [SerializeField] private string _name;

        public string Name => _name;

        public AbilityTag(string name)
        {
            _name = name;
        }

        public string[] GetSplittedTag()
        {
            return _name.Split('.', StringSplitOptions.RemoveEmptyEntries);
        }
    }
}