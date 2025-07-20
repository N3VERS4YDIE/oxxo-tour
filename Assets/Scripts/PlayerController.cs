using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Transform camPivot;
    public GraphicRaycaster raycaster;
    public EventSystem eventSystem;

    [Header("Settings")]
    public float movementSpeed;
    public float touchLookSensitivity = 5;
    public float mouseLookSensitivity = 5;
    public float gamepadLookSensitivity = 45;

    private readonly HashSet<int> uiTouchIds = new();
    private CharacterController characterController;
    private Camera mainCamera;
    private UiController uiController;
    private InputActions inputActions;
    private Vector2 movementInput;
    private Vector2 lookInput;
    private float totalRotationX;
    private PointerEventData pointerEventData;
    private List<RaycastResult> raycastResults = new();

    void Awake()
    {
        inputActions = InputManager.Instance.InputActions;

        characterController = GetComponent<CharacterController>();
        mainCamera = Camera.main;
        uiController = FindFirstObjectByType<UiController>(FindObjectsInactive.Include);
        pointerEventData = new(eventSystem);
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnEnable()
    {
        if (inputActions == null) return;

        EnhancedTouchSupport.Enable();

        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;
        inputActions.Player.Look.performed += OnLook;
        inputActions.Player.Look.canceled += OnLook; 
    }

    void OnDisable()
    {
        if (EnhancedTouchSupport.enabled)
        {
            EnhancedTouchSupport.Disable();
        }

        if (inputActions == null) return;

        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Move.canceled -= OnMove;
        inputActions.Player.Look.performed -= OnLook;
        inputActions.Player.Look.canceled -= OnLook;
    }

    void Update()
    {
        if (uiController != null && uiController.isOpen)
        {
            uiTouchIds.Clear();
            return;
        }

        Vector2 currentLookDelta;
        float currentSensitivity = 1.0f;

        var currentInputMethod = InputManager.Instance.CurrentInputMethod;

        if (currentInputMethod == InputMethod.Touchscreen)
        {
            currentLookDelta = ProcessTouchesForLook();
            currentSensitivity = touchLookSensitivity;
        }
        else
        {
            currentLookDelta = lookInput;
            if (currentInputMethod == InputMethod.MouseAndKeyboard)
            {
                currentSensitivity = mouseLookSensitivity;
            }
            else if (currentInputMethod == InputMethod.Gamepad)
            {
                currentSensitivity = gamepadLookSensitivity;
            }
        }

        HandleRotation(currentLookDelta, currentSensitivity);
        HandleCam();
    }

    void FixedUpdate()
    {
        if (uiController != null && uiController.isOpen)
        {
            return;
        }

        Vector3 moveDirection = movementInput.x * transform.right + movementInput.y * transform.forward;
        characterController.SimpleMove(movementSpeed * Time.fixedDeltaTime * moveDirection);
    }

    Vector2 ProcessTouchesForLook()
    {
        Vector2 cumulativeLookDelta = Vector2.zero;

        if (raycaster == null || eventSystem == null || !EnhancedTouchSupport.enabled)
        {
            if (!EnhancedTouchSupport.enabled) Debug.LogWarning("EnhancedTouch is not enabled.");
            return cumulativeLookDelta;
        }


        foreach (var touch in Touch.activeTouches)
        {
            if (touch.phase == TouchPhase.Began)
            {
                raycastResults.Clear();
                pointerEventData.position = touch.screenPosition;
                raycaster.Raycast(pointerEventData, raycastResults);

                if (raycastResults.Count > 0)
                {
                    uiTouchIds.Add(touch.finger.index);
                }
                else
                {
                    uiTouchIds.Remove(touch.finger.index);
                }
            }

            if (touch.phase == TouchPhase.Moved && !uiTouchIds.Contains(touch.finger.index))
            {
                cumulativeLookDelta += touch.delta;
            }

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                uiTouchIds.Remove(touch.finger.index);
            }
        }
        return cumulativeLookDelta;
    }

    void HandleRotation(Vector2 currentLookDelta, float sensitivity)
    {
        float lookX = currentLookDelta.x * sensitivity * Time.deltaTime;
        float lookY = currentLookDelta.y * sensitivity * Time.deltaTime;

        if (Mathf.Approximately(lookX, 0f) && Mathf.Approximately(lookY, 0f))
        {
            return;
        }

        transform.Rotate(Vector3.up, lookX, Space.World);

        totalRotationX += lookY;
        totalRotationX = Mathf.Clamp(totalRotationX, -90f, 90f);
        camPivot.localRotation = Quaternion.Euler(-totalRotationX, 0f, 0f);
    }

    void HandleCam()
    {
        if (mainCamera != null && camPivot != null)
        {
            mainCamera.transform.SetPositionAndRotation(camPivot.position, camPivot.rotation);
        }
    }

    void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    void OnLook(InputAction.CallbackContext context)
    {
        if (InputManager.Instance.CurrentInputMethod != InputMethod.Touchscreen)
        {
            lookInput = context.ReadValue<Vector2>();
        }
        else
        {
            lookInput = Vector2.zero;
        }
    }
}