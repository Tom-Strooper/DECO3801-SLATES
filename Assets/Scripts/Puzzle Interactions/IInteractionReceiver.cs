using Slates.Utility;

namespace Slates.PuzzleInteractions
{
    public interface IInteractionReceiver : IReferencedComponent<PuzzleInteractionController>
    {
        public void OnInteractionReceived();
    }
}
