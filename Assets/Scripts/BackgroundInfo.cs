using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

// going to use this class to store stuff like the lobby code
// will use DontDestroyOnLoad to let it persist throughout scenes
public class BackgroundInfo : MonoBehaviour
{
    private string _lobbyCode;

    private GameMode _playerMode;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        SceneManager.LoadScene("Scenes/Menu Scenes/KBM Main Menu");
    }

    public void SetPlayerMode(GameMode mode)
    {
        _playerMode = mode;
    }

    public GameMode GetPlayerMode()
    {
        return _playerMode;
    }

    public void SetLobbyCode(string newCode)
    {
        _lobbyCode = newCode;
    }

    public string GetLobbyCode()
    {
        return _lobbyCode;
    }

    public void ResetLobbyCode()
    {
        _lobbyCode = null;
    }
}
