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

            // TODO - Integrate w/ actual code
            _code.text = "######";
        }
    }
}