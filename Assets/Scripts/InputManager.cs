using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public InputActions InputActions { get; private set; }

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
    }

    void OnDisable()
    {
        InputActions.Player.Disable();
    }
}
