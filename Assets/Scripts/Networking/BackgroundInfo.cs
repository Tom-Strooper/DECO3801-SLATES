using Fusion;

namespace Slates.Networking
{
    public class BackgroundInfo
    {
        public string LobbyCode { get; set; }
        public GameMode PlayerMode { get; set; }

        public void ResetLobbyCode() => LobbyCode = null;
    }
}