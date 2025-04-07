using UnityEngine.InputSystem;
using System.Linq;

public static class InputUtils
{
    public static string GetActiveBindingDisplay(InputAction action, PlayerInput playerInput)
    {
        string controlScheme = playerInput.currentControlScheme;

        var binding = action.bindings
            .FirstOrDefault(b => b.groups.Contains(controlScheme) && !b.isPartOfComposite);

        return binding.ToDisplayString() ?? "??";
    }
}
