using Slates.Networking;
using UnityEngine;

namespace Slates.UI
{
    public class PauseMenu : MonoBehaviour
    {
        public void OnConfirmButton() => NetworkGameManager.Instance.QuitGame();
        public void OnCancelButton() => NetworkGameManager.Instance.UnpauseGame();
    }

}