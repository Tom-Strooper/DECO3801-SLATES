using Slates.Networking;
using TMPro;
using UnityEngine;

namespace Slates.UI
{
    public class Menu : MonoBehaviour
    {
        [SerializeField] TMP_InputField lobbyCodeInput;

        public void OnJoinButton()
        {
            string lobbyCode = lobbyCodeInput.text;

            if (!string.IsNullOrEmpty(lobbyCode))
            {
                NetworkGameManager.Instance.Info.LobbyCode = lobbyCode;
                NetworkGameManager.Instance.Info.PlayerMode = Fusion.GameMode.Client;

                NetworkGameManager.Instance.StartGame();
            }
        }

        public void OnHostButton()
        {
            System.Random r = new System.Random();

            NetworkGameManager.Instance.Info.LobbyCode = r.Next(1000000).ToString("D6");
            NetworkGameManager.Instance.Info.PlayerMode = Fusion.GameMode.Host;

            NetworkGameManager.Instance.StartGame();
        }

        public void OnQuitButton()
        {
            NetworkGameManager.Instance.QuitGame();
        }
    }
}
