using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    public InputActions InputActions { get; private set; }
    public InputMethod CurrentInputMethod { get; private set; }
    
    UiController uiController;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        InputActions = new InputActions();
        InputActions.Player.Enable();

        DontDestroyOnLoad(gameObject);
        InputSystem.onAnyButtonPress.Call(OnAnyInput);
    }

    void Start()
    {
        uiController = FindFirstObjectByType<UiController>(FindObjectsInactive.Include);
    }

    void OnEnable()
    {
        InputActions.Player.Enable();
    }

    void OnDisable()
    {
        InputActions.Player.Disable();
    }

    void OnAnyInput(InputControl control)
    {
        var device = control.device;

        if (device is Touchscreen)
        {
            CurrentInputMethod = InputMethod.Touchscreen;
        }
        else if (device is Gamepad)
        {
            CurrentInputMethod = InputMethod.Gamepad;
        }
        else
        {
            CurrentInputMethod = InputMethod.MouseAndKeyboard;
        }

        // uiController.mobilePanel.SetActive(CurrentInputMethod == InputMethod.Touchscreen);
        Debug.Log("Current Input Method: " + InputManager.Instance.CurrentInputMethod);
    }
}
