using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Slates.UI
{
    public class Menu : MonoBehaviour
    {
        [SerializeField] TMP_InputField lobbyCodeInput;

        private BackgroundInfo _backgroundInfo;
        private EscMenu _escMenu;

        private void Awake()
        {
            _backgroundInfo = GameObject.Find("Background Info").GetComponent<BackgroundInfo>();
            _escMenu = GameObject.Find("Esc Menu Canvas").GetComponent<EscMenu>();
        }

        public void OnJoinButton()
        {
            string lobbyCode = lobbyCodeInput.text;
            if (!string.IsNullOrEmpty(lobbyCode))
            {
                _backgroundInfo.SetLobbyCode(lobbyCode);
                _backgroundInfo.SetPlayerMode(Fusion.GameMode.Client);
                SceneManager.LoadScene("Scenes/Puzzles Test Scene");
            }
        }

        public void OnHostButton()
        {
            System.Random r = new System.Random();
            _backgroundInfo.SetLobbyCode(r.Next(1000000).ToString("D6"));
            _backgroundInfo.SetPlayerMode(Fusion.GameMode.Host);
            SceneManager.LoadScene(2);
        }

        public void OnQuitButton()
        {
            _escMenu.GetComponent<Canvas>().enabled = true;
        }
    }
}
