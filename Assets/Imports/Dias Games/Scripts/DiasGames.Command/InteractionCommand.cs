namespace DiasGames.Command
{
    public class InteractCommand : IActionCommand
    {
        private readonly IInteractionComponent _interactionComponent;

        public InteractCommand(IInteractionComponent interactionComponent)
        {
            _interactionComponent = interactionComponent;
        }

        public void Execute()
        {
            if (_interactionComponent != null)
            {
                _interactionComponent.Interact();
            }
        }
    }
}