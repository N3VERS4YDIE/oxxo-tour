using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UiController : MonoBehaviour
{
    [HideInInspector] public bool isOpen;
    public TMP_Text text;
    public RawImage image;

    void Start()
    {
        image.gameObject.SetActive(false);
        text.gameObject.SetActive(false);
    }

    public void ShowInteractionText(string message)
    {
        text.text = message;
        text.gameObject.SetActive(true);
    }

    public void HideInteractionText()
    {
        text.gameObject.SetActive(false);
    }

    public void DisplayImage(Texture2D texture)
    {
        isOpen = true;
        image.texture = texture;
        image.gameObject.SetActive(true);
    }

    public void HideImage()
    {
        isOpen = false;
        image.gameObject.SetActive(false);
    }
}
