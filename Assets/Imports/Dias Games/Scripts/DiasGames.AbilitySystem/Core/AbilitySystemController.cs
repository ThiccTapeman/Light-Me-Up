using System;
using System.Collections.Generic;
using DiasGames.Debugging;
using JetBrains.Annotations;
using UnityEngine;

namespace DiasGames.AbilitySystem.Core
{
    public class AbilitySystemController : MonoBehaviour, IAbilitySystem
    {
        public event Action<Ability> OnAbilityStarted;
        public event Action<Ability> OnAbilityStopped;
        
        public GameObject GameObject => gameObject;

        [SerializeField] private UpdateMode _updateMode = UpdateMode.Update;
        [SerializeField] private List<Ability> _defaultAbilities = new List<Ability>();

        private readonly List<Ability> _abilitiesAdded = new List<Ability>(50);
        private readonly AbilityTagContainer _activeTagContainer = new AbilityTagContainer(new List<AbilityTag>(10));

        private bool _debugAbilitySystem;
        private const string _debugAbilityCommandId = "debugAbilitySystem";

        private void Start()
        {
            for (var i = 0; i < _defaultAbilities.Count; i++)
            {
                Ability ability = _defaultAbilities[i];
                if (AddAbility(ability, gameObject))
                {
                    _defaultAbilities[i] = _abilitiesAdded[i];
                }
            }

            var showAbilitiesCommand = new DebugCommandBool(_debugAbilityCommandId,
                "Show which abilities are running on character and the tags in the system",
                $"{_debugAbilityCommandId} <true/false>", active => _debugAbilitySystem = active);
            
            DebugConsole.AddConsoleCommand(showAbilitiesCommand);
        }

        private void OnDestroy()
        {
            DebugConsole.RemoveConsoleCommand(_debugAbilityCommandId);
        }

        private void Update()
        {
            if (_updateMode == UpdateMode.Update)
            {
                SystemUpdate(Time.deltaTime);
            }
        }

        private void FixedUpdate()
        {
            if (_updateMode == UpdateMode.FixedUpdate)
            {
                SystemUpdate(Time.fixedDeltaTime);
            }
        }

        private void SystemUpdate(float deltaTime)
        {
            if (Mathf.Approximately(Time.timeScale, 0.0f))
            {
                return;
            }

            for (int i=0; i < _abilitiesAdded.Count; i++)
            {
                Ability ability = _abilitiesAdded[i];
                if (ability.IsRunning)
                {
                    ability.UpdateAbility(deltaTime);
                }
                else if (ability.AutoStart)
                {
                    ability.StartAbility(gameObject);
                }
            }
        }

        public Ability AddAbility(Ability newAbility, 
            [NotNull] GameObject instigator)
        {
            if (_abilitiesAdded.Exists(x => x.AbilityName == newAbility.AbilityName))
            {
                Debug.LogWarning($"[{instigator.name}] Trying to add {newAbility}, but this ability is already in the system");
                return null;
            }
            
            Ability ability = newAbility.Clone();
            _abilitiesAdded.Add(ability);
            ability.Setup(this);
            ability.OnAbilityStarted += HandleAbilityStarted;
            ability.OnAbilityStopped += HandleAbilityStopped;
            return ability;
        }

        public bool RemoveAbilityByName(string abilityName, GameObject instigator)
        {
            Ability ability = _abilitiesAdded.Find(x => x.AbilityName == abilityName);
            if (ability == null)
            {
                return false;
            }

            ability.StopAbility(instigator);
            ability.OnAbilityStarted -= HandleAbilityStarted;
            ability.OnAbilityStopped -= HandleAbilityStopped;
            _abilitiesAdded.Remove(ability);
            return true;
        }

        public bool StartAbilityByName(string abilityName, GameObject instigator)
        {
            Ability ability = _abilitiesAdded.Find(x => x.AbilityName == abilityName);
            if (ability == null)
            {
                return false;
            }

            return ability.StartAbility(instigator);
        }

        public bool StopAbilityByName(string abilityName, GameObject instigator)
        {
            Ability ability = _abilitiesAdded.Find(x => x.AbilityName == abilityName);
            if (ability == null)
            {
                return false;
            }

            return ability.StopAbility(instigator);
        }

        public AbilityTagContainer GetActiveTags()
        {
            return _activeTagContainer;
        }

        public TAbility GetAbility<TAbility>() where TAbility : Ability
        {
            foreach (Ability currentAbility in _abilitiesAdded)
            {
                if (currentAbility is TAbility ability)
                {
                    return ability;
                }
            }

            return null;
        }

        public void CancelAbilitiesWithTag(AbilityTagContainer cancelTags, GameObject instigator)
        {
            if (cancelTags == null || cancelTags.IsEmpty())
            {
                return;
            }

            IEnumerable<Ability> activeAbilities = GetActiveAbilities();
            foreach (Ability ability in activeAbilities)
            {
                if (ability.ActivationTags.HasAnyTag(cancelTags))
                {
                    ability.StopAbility(instigator);
                }
            }
        }

        private IEnumerable<Ability> GetActiveAbilities()
        {
            foreach (Ability ability in _abilitiesAdded)
            {
                if (ability.IsRunning)
                {
                    yield return ability;
                }
            }
        }

        private void HandleAbilityStarted(Ability ability)
        {
            OnAbilityStarted?.Invoke(ability);
        }

        private void HandleAbilityStopped(Ability ability)
        {
            OnAbilityStopped?.Invoke(ability);
        }

        private void OnGUI()
        {
            if (!_debugAbilitySystem)
            {
                return;
            }

            float width = Screen.width / 3.0f;
            float height = 150.0f;
            float y = Screen.height - height - 10.0f;
            GUI.Box(new Rect(0,y, width, height), string.Empty);
            GUI.backgroundColor = new Color(0, 0, 0, 0);

            List<string> debugLabels = new List<string>
            {
                $"<b>ABILITY SYSTEM:</b> {gameObject.name}\n",
                $"Running abilities: <b>{GetActiveAbilitiesNames()}</b>",
                $"Active Tags: <b>{GetActiveTagsNames()}</b>",
            };

            string finalLable = string.Empty;
            foreach (string label in debugLabels)
            {
                finalLable += $"{label}\n";
            }
            GUI.Label(new Rect(new Vector2(10, y + 10), new Vector2(width - 20, height - 20)), finalLable);
        }

        private string GetActiveAbilitiesNames()
        {
            string activeAbilities = string.Empty;
            foreach (Ability ability in GetActiveAbilities())
            {
                activeAbilities += $"{ability.AbilityName}, ";
            }

            return activeAbilities;
        }

        private string GetActiveTagsNames()
        {
            string activeTags = string.Empty;
            foreach (AbilityTag abilityTag in _activeTagContainer.Tags)
            {
                activeTags += $"{abilityTag.Name}, ";
            }

            return activeTags;
        }
    }
}