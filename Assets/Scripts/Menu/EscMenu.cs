using UnityEngine;
using UnityEngine.UI;

public class EscMenu : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        this.GetComponent<Canvas>().enabled = false;
    }
    public void OnConfirmButton()
    {
        Application.Quit();
    }

    public void OnCancelButton()
    {
        this.Disappear();
    }

    public void Disappear()
    {
        this.GetComponent<Canvas>().enabled = false;
    }

    public void Appear()
    {
        this.GetComponent<Canvas>().enabled = true;
    }
}
