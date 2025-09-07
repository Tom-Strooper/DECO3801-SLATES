using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [SerializeField] TMP_InputField lobbyCodeInput;
    private BackgroundInfo _backgroundInfo;

    private void Awake()
    {
        _backgroundInfo = GameObject.Find("Background Info").GetComponent<BackgroundInfo>();
    }

    public void OnJoinButton()
    {
        string lobbyCode = lobbyCodeInput.text;
        if (!string.IsNullOrEmpty(lobbyCode))
        {
            _backgroundInfo.SetLobbyCode(lobbyCode);
            _backgroundInfo.SetPlayerMode(Fusion.GameMode.Client);
            SceneManager.LoadScene(2);
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
        Application.Quit();
    }
}
