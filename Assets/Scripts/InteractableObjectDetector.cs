using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities; // Required for ReadOnlyArray
using AYellowpaper.SerializedCollections;

// Assuming InputManager exists and provides:
// - InputManager.Instance
// - InputManager.Instance.CurrentInputMethod (enum InputMethod)
// - enum InputMethod { Touchscreen, MouseKeyboard, Gamepad } // Example enum

// Assuming UiController exists and provides:
// - uiController.ShowInteractionText(string text / bool show)
// - uiController.ShowImage(Texture2D texture / bool show)
// - uiController.interactButton (GameObject)
// - uiController.closeButton (GameObject)
// - uiController.movementStick (GameObject)

public class InteractableObjectDetector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UiController uiController;
    [SerializeField] private InputActionReference interactAction;
    [SerializeField] private InputActionReference closeImageAction;
    // Assumes InputManager.Instance is available

    [Header("Text Templates")]
    [SerializeField] private string interactTextTemplate = "Presiona {binding} para interactuar";
    [SerializeField] private string closeImageTextTemplate = "Presiona {binding} para cerrar la imagen";

    [Header("Interaction Settings")]
    [SerializeField] private float detectionRange = 3f;
    [SerializeField] private LayerMask interactableLayer = ~0;

    [Header("Interactable Data")]
    [SerializeField] private SerializedDictionary<GameObject, Texture2D> interactableObjects = new();

    private GameObject currentInteractable = null;
    private bool isImageDisplayed = false;
    private Camera mainCamera;

    // Optional: Cache Input Method to detect changes if needed for specific events
    // private InputMethod previousInputMethod;

    // --- Control Scheme Names (MUST MATCH NAMES IN YOUR INPUT ACTIONS ASSET) ---
    private const string KeyboardMouseScheme = "Keyboard&Mouse"; // Adjust if your scheme name is different
    private const string GamepadScheme = "Gamepad";             // Adjust if your scheme name is different
    private const string TouchScheme = "Touch";                 // Adjust if your scheme name is different


    void Awake()
    {
        // Null checks for essential components
        if (uiController == null) { Debug.LogError("UiController reference missing!", this); enabled = false; return; }
        if (interactAction == null || interactAction.action == null) { Debug.LogError("Interact Action missing!", this); enabled = false; return; }
        if (closeImageAction == null || closeImageAction.action == null) { Debug.LogError("Close Image Action missing!", this); enabled = false; return; }
        if (InputManager.Instance == null) { Debug.LogError("InputManager instance missing!", this); enabled = false; return; }

        mainCamera = Camera.main;
        if (mainCamera == null) { Debug.LogError("Main Camera not found!", this); enabled = false; }
    }

    void OnEnable()
    {
        // Perform initial UI update based on starting state
        UpdateInteractionUI();
    }

    void OnDisable()
    {
        // Cleanup UI when disabled
        if (uiController != null)
        {
            uiController.ShowInteractionText(false);
            uiController.ShowImage(false);
            if (uiController.interactButton != null) uiController.interactButton.SetActive(false);
            if (uiController.closeButton != null) uiController.closeButton.SetActive(false);
            if (uiController.movementStick != null) uiController.movementStick.SetActive(true); // Ensure stick is visible
        }
        currentInteractable = null;
        isImageDisplayed = false;
    }


    void Update()
    {
        if (mainCamera == null || uiController == null || InputManager.Instance == null) return;

        DetectInteractable();
        HandleInteractionInput();
        HandleCloseInput();

        // Always update UI based on current state and input method
        UpdateInteractionUI();
    }

    void DetectInteractable()
    {
        if (isImageDisplayed)
        {
            // If image is shown, ensure interactable is cleared so UI updates correctly when closed
            if (currentInteractable != null)
            {
                currentInteractable = null;
            }
            return;
        }

        GameObject detectedObject = null;
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);

        if (Physics.SphereCast(ray, 0.5f, out RaycastHit hit, detectionRange, interactableLayer))
        {
            if (interactableObjects.ContainsKey(hit.collider.gameObject))
            {
                detectedObject = hit.collider.gameObject;
            }
        }

        if (currentInteractable != detectedObject)
        {
            currentInteractable = detectedObject;
        }
    }

    void HandleInteractionInput()
    {
        if (!isImageDisplayed && currentInteractable != null && interactAction.action.WasPressedThisFrame())
        {
            if (interactableObjects.TryGetValue(currentInteractable, out Texture2D texture))
            {
                uiController.ShowImage(texture);
                isImageDisplayed = true;
                // UI is updated in UpdateInteractionUI
            }
        }
    }

    void HandleCloseInput()
    {
        if (isImageDisplayed && closeImageAction.action.WasPressedThisFrame())
        {
            uiController.ShowImage(false);
            isImageDisplayed = false;
            // Force re-detection in the same frame to update UI immediately
            DetectInteractable();
            // UI is updated in UpdateInteractionUI
        }
    }

    // Gets the control scheme name string based on the InputManager's enum
    string GetCurrentControlSchemeName(InputMethod method)
    {
        switch (method)
        {
            case InputMethod.MouseAndKeyboard:
                return KeyboardMouseScheme;
            case InputMethod.Gamepad:
                return GamepadScheme;
            case InputMethod.Touchscreen:
                return TouchScheme; // Or null/empty if touch doesn't have a named scheme
            default:
                return null; // No specific scheme or unknown method
        }
    }

    // Gets the binding display string for a specific control scheme
    string GetBindingStringForScheme(InputAction action, string schemeName)
    {
        if (action == null || string.IsNullOrEmpty(schemeName))
        {
            return "?"; // Return placeholder if action or scheme is invalid
        }

        // Find the binding index for the given control scheme
        int bindingIndex = action.GetBindingIndex(InputBinding.MaskByGroup(schemeName));

        if (bindingIndex >= 0)
        {
            // Use the specific overload to get the display string for *only* this binding
            return action.GetBindingDisplayString(bindingIndex, InputBinding.DisplayStringOptions.DontIncludeInteractions);
        }
        else
        {
            // Fallback: If no specific binding found for the scheme, try the general one (might show multiple)
            // Or return a placeholder indicating no binding found for the current scheme
            // return action.GetBindingDisplayString();
            Debug.LogWarning($"No specific binding found for action '{action.name}' and control scheme '{schemeName}'.");
            return "?"; // Placeholder
        }

    }


    void UpdateInteractionUI()
    {
        if (uiController == null || InputManager.Instance == null) return;

        var currentMethod = InputManager.Instance.CurrentInputMethod;
        string currentSchemeName = GetCurrentControlSchemeName(currentMethod); // Get scheme name

        // --- Determine State ---
        bool canInteract = !isImageDisplayed && currentInteractable != null;
        bool showImage = isImageDisplayed;

        // --- Default UI States ---
        string textToShow = null;
        bool interactButtonActive = false;
        bool closeButtonActive = false;
        bool movementStickActive = true; // Usually visible

        // --- Apply Logic based on Input Method ---
        if (currentMethod == InputMethod.Touchscreen)
        {
            // Mobile Touch UI Logic
            if (showImage)
            {
                // Image is displayed
                closeButtonActive = true;
                interactButtonActive = false; // Cannot interact while image is shown
                movementStickActive = false; // Hide stick when image is shown
            }
            else if (canInteract)
            {
                // Looking at interactable, no image shown
                interactButtonActive = true;
                closeButtonActive = false;
                movementStickActive = true; // Show stick for movement
            }
            else
            {
                // Not looking at interactable, no image shown
                interactButtonActive = false;
                closeButtonActive = false;
                movementStickActive = true; // Show stick for movement
            }
            // No text prompts for touch
            textToShow = null;
        }
        else // Keyboard/Mouse or Gamepad
        {
            // Non-Touch UI Logic (Text Prompts)
            movementStickActive = true; // Stick GameObject doesn't apply here, but keep logic consistent if needed elsewhere
            interactButtonActive = false; // Mobile buttons not used
            closeButtonActive = false;    // Mobile buttons not used

            if (showImage)
            {
                // Show close prompt
                string binding = GetBindingStringForScheme(closeImageAction.action, currentSchemeName);
                textToShow = closeImageTextTemplate.Replace("{binding}", binding);
            }
            else if (canInteract)
            {
                // Show interact prompt
                string binding = GetBindingStringForScheme(interactAction.action, currentSchemeName);
                textToShow = interactTextTemplate.Replace("{binding}", binding);
            }
            else
            {
                // No prompt needed
                textToShow = null;
            }
        }

        // --- Update UI Controller ---
        uiController.ShowInteractionText(textToShow);
        // uiController.ShowImage is handled by HandleInteractionInput/HandleCloseInput

        // Update mobile buttons (check references first)
        if (uiController.interactButton != null)
            uiController.interactButton.SetActive(interactButtonActive);

        if (uiController.closeButton != null)
            uiController.closeButton.SetActive(closeButtonActive);

        if (uiController.movementStick != null)
            uiController.movementStick.SetActive(movementStickActive);
    }
}