using UnityEngine;
using UnityEngine.InputSystem;
using AYellowpaper.SerializedCollections;

public class InteractableObjectDetector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] UiController uiController;
    [SerializeField] InputActionReference interactAction;
    [SerializeField] InputActionReference closeImageAction;

    [Header("Text")]
    [SerializeField] string closeImageText = "Presiona {?} para cerrar la imagen";
    [SerializeField] string interactText = "Presiona {?} para interactuar";

    [Header("Settings")]
    [SerializeField] float detectionRange;
    [SerializeField] LayerMask interactableLayer;

    [SerializeField] SerializedDictionary<GameObject, Texture2D> interactableObjects = new();

    GameObject currentInteractable;
    bool isImageDisplayed = false;

    private void Start() {
        closeImageText = closeImageText.Replace("{?}", closeImageAction.action.GetBindingDisplayString());
        interactText = interactText.Replace("{?}", interactAction.action.GetBindingDisplayString());
    }

    void Update()
    {
        DetectInteractable();
        HandleInteraction();
        HandleEsc();
    }

    void DetectInteractable()
    {
        if (isImageDisplayed)
        {
            uiController.ShowInteractionText(closeImageText);
            return;
        }

        Ray ray = new(Camera.main.transform.position, Camera.main.transform.forward);
        if (Physics.SphereCast(ray, 0.5f, out RaycastHit hit, detectionRange, interactableLayer))
        {
            if (interactableObjects.TryGetValue(hit.collider.gameObject, out Texture2D _))
            {
                if (currentInteractable != hit.collider.gameObject)
                {
                    uiController.ShowInteractionText(interactText);
                    currentInteractable = hit.collider.gameObject;
                }
                return;
            }
        }

        currentInteractable = null;
        uiController.HideInteractionText();
    }

    void HandleInteraction()
    {
        if (currentInteractable != null && interactAction.action.WasPressedThisFrame())
        {
            if (interactableObjects.TryGetValue(currentInteractable, out Texture2D texture))
            {
                uiController.DisplayImage(texture);
                isImageDisplayed = true;
                uiController.ShowInteractionText(closeImageText);
            }
        }
    }

    void HandleEsc()
    {
        if (closeImageAction.action.WasPressedThisFrame() && isImageDisplayed)
        {
            uiController.HideImage();
            isImageDisplayed = false;
            if (currentInteractable != null)
            {
                uiController.ShowInteractionText(interactText);
            }
            else
            {
                uiController.HideInteractionText();
            }
        }
    }
}
