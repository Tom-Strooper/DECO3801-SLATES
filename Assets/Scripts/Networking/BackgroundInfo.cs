using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Slates.Networking
{

    // going to use this class to store stuff like the lobby code
    // will use DontDestroyOnLoad to let it persist throughout scenes
    public class BackgroundInfo : MonoBehaviour
    {
        public string LobbyCode { get; set; } = null;
        public GameMode PlayerMode { get; set; }

        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
            SceneManager.LoadScene("Scenes/Menu Scenes/KBM Main Menu");
        }

        public void ResetLobbyCode() => LobbyCode = null;
    }
}