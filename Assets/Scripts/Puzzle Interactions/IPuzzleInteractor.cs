using Slates.Utility;

namespace Slates.PuzzleInteractions
{
    public interface IPuzzleInteractor : IReferencedComponent<PuzzleInteractionController>
    {
        public string Key { get; }
        public void Reset();
    }

    public enum InteractorActivationMode
    {
        // Pressing the interactor calls Activate() on the puzzle interaction controller
        Press,
        // Like press, although releasing the interactor calls Deactivate() on the puzzle interaction controller
        Hold,
    }
}
