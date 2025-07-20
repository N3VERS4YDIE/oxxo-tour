using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UiController : MonoBehaviour
{
    [HideInInspector] public bool isOpen;
    public TMP_Text text;
    public RawImage image;
    public GameObject mobilePanel;
    public GameObject movementStick;
    public GameObject interactButton;
    public GameObject closeButton;

    void Awake()
    {
        image.gameObject.SetActive(false);
        text.gameObject.SetActive(false);
        // mobilePanel.SetActive(false);
        interactButton.SetActive(false);
        closeButton.SetActive(false);
    }

    public void ShowInteractionText(bool show = true)
    {
        if (show)
        {
            text.gameObject.SetActive(true);
        }
        else
        {
            text.gameObject.SetActive(false);
        }
    }

    public void ShowInteractionText(string message)
    {
        text.text = message;
        ShowInteractionText(true);
    }

    public void ShowImage(bool show = true)
    {
        if (show) {
            isOpen = true;
            image.gameObject.SetActive(true);
        }
        else
        {
            isOpen = false;
            image.gameObject.SetActive(false);
        }
    }

    public void ShowImage(Texture2D texture)
    {
        image.texture = texture;
        ShowImage(true);
    }
}
