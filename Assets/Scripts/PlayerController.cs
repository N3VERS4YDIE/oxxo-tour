using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Transform camPivot;

    [Header("Settings")]
    public float movementSpeed;
    public float mouseSpeed;
    public float controllerLookSpeed;

    public InputActions InputActions => inputActions;

    CharacterController characterController;
    Camera mainCamera;
    UiController uiController;
    InputActions inputActions;
    Vector2 movementInput;
    Vector2 lookInput;
    float totalRotationX;

    void Awake()
    {
        inputActions = new InputActions();
    }

    void Start()
    {
        mainCamera = Camera.main;
        characterController = GetComponent<CharacterController>();
        uiController = FindFirstObjectByType<UiController>(FindObjectsInactive.Include);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnEnable()
    {
        inputActions.Player.Enable();

        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;
        inputActions.Player.Look.performed += OnLook;
        inputActions.Player.Look.canceled += OnLook;
    }

    void OnDisable()
    {
        inputActions.Player.Disable();

        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Move.canceled -= OnMove;
        inputActions.Player.Look.performed -= OnLook;
        inputActions.Player.Look.canceled -= OnLook;
    }

    void Update()
    {
        if (uiController.isOpen)
        {
            return;
        }

        HandleCam();
        HandleRotation();
    }

    void FixedUpdate()
    {
        if (uiController.isOpen)
        {
            return;
        }

        Vector3 moveDirection = movementInput.x * transform.right + movementInput.y * transform.forward;
        characterController.SimpleMove(movementSpeed * moveDirection);
    }

    void HandleRotation()
    {
        transform.Rotate(Vector3.up, lookInput.x * mouseSpeed);

        totalRotationX += lookInput.y * mouseSpeed;
        totalRotationX = Mathf.Clamp(totalRotationX, -90, 90);
        camPivot.localRotation = Quaternion.Euler(-totalRotationX, 0, 0);
    }

    void HandleCam()
    {
        mainCamera.transform.SetPositionAndRotation(camPivot.position, camPivot.rotation);
    }

    void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }
}