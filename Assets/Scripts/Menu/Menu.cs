using Slates.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [SerializeField] TMP_InputField lobbyCodeInput;

    public void OnJoinButton()
    {
        string lobbyCode = lobbyCodeInput.text;
        if (!string.IsNullOrEmpty(lobbyCode))
        {
            Debug.Log(lobbyCode);
            Debug.Log("Client");
            SceneManager.LoadScene(1);
        }
    }

    public void OnHostButton()
    {
        Debug.Log("Host");
        SceneManager.LoadScene(1);
    }

    public void OnQuitButton()
    {
        Application.Quit();
    }
}
