using System.Collections.Generic;
using UnityEngine;

namespace DiasGames.Command
{
    public class CommandInvoker : MonoBehaviour, ICommandInvoker
    {
        private readonly Queue<IActionCommand> _commandsQueue = new Queue<IActionCommand>(10);
        
        public void AddCommand(IActionCommand command)
        {
            _commandsQueue.Enqueue(command);
        }

        private void Update()
        {
            while (_commandsQueue.TryDequeue(out IActionCommand command))
            {
                command.Execute();
            }
        }
    }
}