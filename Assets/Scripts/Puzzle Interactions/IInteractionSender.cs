using Slates.Utility;

namespace Slates.PuzzleInteractions
{
    public interface IInteractionSender : IReferencedComponent<PuzzleInteractionController>
    {
        public InteractionHandler OnInteractionSent { get; set; }
    }
}
