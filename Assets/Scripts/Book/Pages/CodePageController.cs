using Slates.Networking;
using TMPro;
using UnityEngine;

namespace Slates.Book.Pages
{
    public class CodePageController : PageController
    {
        [SerializeField] private TMP_Text _code;

        public override void Initialise()
        {
            base.Initialise();
            _code.text = FindAnyObjectByType<NetworkSpawner>()?.Info?.LobbyCode ?? "######";
        }
    }
}