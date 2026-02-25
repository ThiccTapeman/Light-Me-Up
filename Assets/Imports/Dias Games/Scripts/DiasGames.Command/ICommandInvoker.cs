namespace DiasGames.Command
{
    public interface ICommandInvoker
    {
        void AddCommand(IActionCommand command);
    }
}