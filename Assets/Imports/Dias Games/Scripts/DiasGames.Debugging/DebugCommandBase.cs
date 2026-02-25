using System;

namespace DiasGames.Debugging
{
    public abstract class DebugCommandBase
    {
        public string Id { get; private set; }
        public string Description { get; private set; }
        public string Format { get; private set; }

        public DebugCommandBase(string commandId, string commandDescription, string commandFormat)
        {
            Id = commandId;
            Description = commandDescription;
            Format = commandFormat;
        }

        public abstract bool IsValid(string command);
    }

    public class DebugCommand : DebugCommandBase
    {
        private Action _command;

        public DebugCommand(string commandId, string commandDescription, string commandFormat, Action command)
            : base(commandId, commandDescription, commandFormat)
        {
            _command = command;
        }

        public void Execute()
        {
            _command?.Invoke();
        }

        public override bool IsValid(string command)
        {
            string[] properties = command.Split(' ');
            if (properties.Length > 1)
            {
                return false;
            }

            return true;
        }
    }
    
    public abstract class DebugCommand<T> : DebugCommandBase
    {
        private Action<T> _command;

        public DebugCommand(string commandId, string commandDescription, string commandFormat, Action<T> command)
            : base(commandId, commandDescription, commandFormat)
        {
            _command = command;
        }

        public void Execute(T value)
        {
            _command?.Invoke(value);
        }
        
        public override bool IsValid(string command)
        {
            string[] properties = command.Split(' ');
            if (properties.Length <= 1)
            {
                return false;
            }

            return true;
        }
    }

    public class DebugCommandBool : DebugCommand<bool>
    {
        public DebugCommandBool(string commandId, 
            string commandDescription, string commandFormat, 
            Action<bool> command) : base(commandId, commandDescription, commandFormat, command)
        {
            
        }
        
        public override bool IsValid(string command)
        {
            if (!base.IsValid(command))
            {
                return false;
            }
            
            string[] properties = command.Split(' ');
            return bool.TryParse(properties[1], out bool result);
        }
    }
    
    public class DebugCommandFloat : DebugCommand<float>
    {
        public DebugCommandFloat(string commandId, 
            string commandDescription, string commandFormat, 
            Action<float> command) : base(commandId, commandDescription, commandFormat, command)
        {
            
        }

        public override bool IsValid(string command)
        {
            if (!base.IsValid(command))
            {
                return false;
            }
            
            string[] properties = command.Split(' ');
            return float.TryParse(properties[1], out float result);
        }
    }
    
    public class DebugCommandInt : DebugCommand<int>
    {
        public DebugCommandInt(string commandId, 
            string commandDescription, string commandFormat, 
            Action<int> command) : base(commandId, commandDescription, commandFormat, command)
        {
            
        }

        public override bool IsValid(string command)
        {
            if (!base.IsValid(command))
            {
                return false;
            }

            string[] properties = command.Split(' ');
            return int.TryParse(properties[1], out int result);
        }
    }
    
    public class DebugCommandString : DebugCommand<string>
    {
        public DebugCommandString(string commandId, 
            string commandDescription, string commandFormat, 
            Action<string> command) : base(commandId, commandDescription, commandFormat, command)
        {
            
        }
    }
}