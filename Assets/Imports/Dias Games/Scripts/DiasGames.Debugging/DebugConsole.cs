using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DiasGames.Debugging
{
    public class DebugConsole : MonoBehaviour
    {
        [SerializeField] private InputField _consoleInput;
        [SerializeField] private Text _output;
        [SerializeField] private CanvasGroup _consoleCanvasGroup;
        [SerializeField] private CanvasGroup _openMessageCanvasGroup;

        private bool _showConsole = false;
        private string _input;
        private Vector2 _scroll;

        private static DebugConsole _controller;
        private static readonly List<object> _commands = new List<object>(20);

        public static void AddConsoleCommand(DebugCommand command)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if(_commands.Exists(x => ((DebugCommandBase)x).Id == command.Id))
            {
                Debug.LogWarning($"A command with Id {command.Id} already exist");
                return;
            }

            _commands.Add(command);
#endif
        }
        
        public static void AddConsoleCommand<T>(DebugCommand<T> commandParam)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if(_commands.Exists(x => ((DebugCommandBase)x).Id == commandParam.Id))
            {
                Debug.LogWarning($"A command with Id {commandParam.Id} already exist");
                return;
            }

            _commands.Add(commandParam);
#endif
        }

        public static void RemoveConsoleCommand(string commandId)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            int index = _commands.FindIndex(x => ((DebugCommandBase)x).Id == commandId);
            if (index > -1)
            {
                _commands.RemoveAt(index);
            }
#endif
        }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        
        private void Awake()
        {
            if (_controller != null)
            {
                Destroy(gameObject);
                return;
            }

            _controller = this;
            _consoleInput.onSubmit.AddListener(HandleCommandSubmit);
            SetVisible(false);
            DontDestroyOnLoad(gameObject);
            InitializeCommands();
        }

        private void InitializeCommands()
        {
            DebugCommand helpCommand = new DebugCommand("help", "show all commands",
                "help", ShowHelp);

            AddConsoleCommand(helpCommand);
        }

        private void HandleCommandSubmit(string consoleCommand)
        {
            if (consoleCommand != string.Empty)
            {
                _input = consoleCommand;
                HandleInput();
                _consoleInput.text = string.Empty;
            }
        }

        private void Update()
        {
            if (Keyboard.current.backquoteKey.wasPressedThisFrame)
            {
                ToggleConsole();
            }
        }

        private void ToggleConsole()
        {
            _showConsole = !_showConsole;
            SetVisible(_showConsole);
            foreach (PlayerInput input in FindObjectsByType<PlayerInput>(FindObjectsSortMode.None))
            {
                if (_showConsole)
                {
                    input.DeactivateInput();
                }
                else
                {
                    input.ActivateInput();
                }
            }
        }

        private void SetVisible(bool visible)
        {
            _consoleCanvasGroup.interactable = visible;
            _consoleCanvasGroup.blocksRaycasts = visible;
            _consoleCanvasGroup.alpha = visible ? 1 : 0;
            _openMessageCanvasGroup.alpha = visible ? 0 : 1;

            ILevelController levelController =  GameObject.FindWithTag("GameController").GetComponent<ILevelController>();
            levelController.SetCursorLocked(!visible);
        }

        private void ShowHelp()
        {
            string output = "Use this console to debug your system (Commands are case sensitive). See all commands below:\n";
            _commands.Sort((x,y) => ((DebugCommandBase)x).Id.CompareTo(((DebugCommandBase)y).Id));
            for (int i = 0; i < _commands.Count; i++)
            {
                DebugCommandBase commandBase = _commands[i] as DebugCommandBase;
                if (commandBase == null)
                {
                    continue;
                }

                output += $"<b>{commandBase.Format}</b> - {commandBase.Description}\n";
            }

            _output.text = output;
        }
        private void HandleInput()
        {
            string[] properties = _input.Split(' ');
            _output.text = string.Empty;
            for (int i = 0; i < _commands.Count; i++)
            {
                if (_commands[i] is DebugCommandBase commandBase && _input.Contains(commandBase.Id))
                {
                    if (!commandBase.IsValid(_input))
                    {
                        _output.text =
                            $"Can't execute command {commandBase.Id} because the command doesn't match the format: {commandBase.Format}";
                        return;
                    }
                    
                    if (_commands[i] is DebugCommand command)
                    {
                        command.Execute();
                        return;
                    }
                    
                    if(_commands[i] is DebugCommandBool commandBool)
                    {
                        commandBool.Execute(bool.Parse(properties[1]));
                        return;
                    }
                    
                    if(_commands[i] is DebugCommandFloat commandFloat)
                    {
                        commandFloat.Execute(float.Parse(properties[1]));
                        return;
                    }
                    
                    if(_commands[i] is DebugCommandInt commandInt)
                    {
                        commandInt.Execute(int.Parse(properties[1]));
                        return;
                    }
                    
                    if(_commands[i] is DebugCommandString commandString)
                    {
                        string parameter = string.Empty;
                        for (int j = 1; j < properties.Length; j++)
                        {
                            parameter += properties[j];
                        }
                        commandString.Execute(parameter);
                        return;
                    }
                }
            }

            _output.text = $"Command <b>{properties[0]}</b> doesn't exist!";
        }
#endif
    }
}