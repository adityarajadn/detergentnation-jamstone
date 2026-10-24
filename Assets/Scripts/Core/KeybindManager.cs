using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum KeyAction
{
    MoveLeft,
    MoveRight,
    Jump,
    DropPlatform,
    UseConsumable,
    Slot1,
    Slot2,
    Interact,
    Attack,
    MeleeAttack,
    Reload
}

public enum InputDeviceType
{
    Keyboard,
    Mouse
}

[Serializable]
public struct ActionBinding
{
    public InputDeviceType device;
    public Key key;
    public int mouse_button; // 0 = Left, 1 = Right, 2 = Middle

    public ActionBinding(Key k)
    {
        device = InputDeviceType.Keyboard;
        key = k;
        mouse_button = 0;
    }

    public ActionBinding(int mouseBtn)
    {
        device = InputDeviceType.Mouse;
        key = Key.None;
        mouse_button = mouseBtn;
    }

    public override string ToString()
    {
        if (device == InputDeviceType.Mouse)
        {
            if (mouse_button == 0) return "LMB";
            if (mouse_button == 1) return "RMB";
            if (mouse_button == 2) return "MMB";
            return "Mouse" + mouse_button;
        }

        switch (key)
        {
            case Key.Space: return "Space";
            case Key.LeftArrow: return "Left";
            case Key.RightArrow: return "Right";
            case Key.DownArrow: return "Down";
            case Key.UpArrow: return "Up";
            case Key.Digit1: return "1";
            case Key.Digit2: return "2";
            case Key.Digit3: return "3";
            case Key.Digit4: return "4";
            case Key.LeftShift: return "LShift";
            case Key.RightShift: return "RShift";
            case Key.LeftCtrl: return "LCtrl";
            default: return key.ToString();
        }
    }
}

public static class KeybindManager
{
    public static event Action on_keybinds_changed;

    public static ActionBinding GetDefault(KeyAction action)
    {
        switch (action)
        {
            case KeyAction.MoveLeft: return new ActionBinding(Key.A);
            case KeyAction.MoveRight: return new ActionBinding(Key.D);
            case KeyAction.Jump: return new ActionBinding(Key.Space);
            case KeyAction.DropPlatform: return new ActionBinding(Key.S);
            case KeyAction.UseConsumable: return new ActionBinding(Key.Q);
            case KeyAction.Slot1: return new ActionBinding(Key.Digit1);
            case KeyAction.Slot2: return new ActionBinding(Key.Digit2);
            case KeyAction.Interact: return new ActionBinding(Key.F);
            case KeyAction.Attack: return new ActionBinding(0); // LMB
            case KeyAction.MeleeAttack: return new ActionBinding(1); // RMB
            case KeyAction.Reload: return new ActionBinding(Key.R);
            default: return new ActionBinding(Key.None);
        }
    }

    public static ActionBinding GetBinding(KeyAction action)
    {
        string prefKey = "Binding_" + action;
        if (!PlayerPrefs.HasKey(prefKey))
        {
            return GetDefault(action);
        }

        string raw = PlayerPrefs.GetString(prefKey, "");
        if (string.IsNullOrEmpty(raw)) return GetDefault(action);

        string[] parts = raw.Split(':');
        if (parts.Length != 2) return GetDefault(action);

        if (parts[0] == "Mouse" && int.TryParse(parts[1], out int mouseBtn))
        {
            return new ActionBinding(mouseBtn);
        }
        else if (parts[0] == "Key" && Enum.TryParse(parts[1], out Key k))
        {
            return new ActionBinding(k);
        }

        return GetDefault(action);
    }

    public static void SetBinding(KeyAction action, ActionBinding binding)
    {
        string prefKey = "Binding_" + action;
        string serialized = binding.device == InputDeviceType.Mouse
            ? "Mouse:" + binding.mouse_button
            : "Key:" + binding.key;

        PlayerPrefs.SetString(prefKey, serialized);
        PlayerPrefs.Save();
        on_keybinds_changed?.Invoke();
    }

    public static void ResetToDefaults()
    {
        foreach (KeyAction act in Enum.GetValues(typeof(KeyAction)))
        {
            PlayerPrefs.DeleteKey("Binding_" + act);
        }
        PlayerPrefs.Save();
        on_keybinds_changed?.Invoke();
    }

    public static bool IsPressed(KeyAction action)
    {
        ActionBinding b = GetBinding(action);
        if (b.device == InputDeviceType.Mouse)
        {
            Mouse m = Mouse.current;
            if (m == null) return false;
            if (b.mouse_button == 0) return m.leftButton.isPressed;
            if (b.mouse_button == 1) return m.rightButton.isPressed;
            if (b.mouse_button == 2) return m.middleButton.isPressed;
            return false;
        }
        else
        {
            Keyboard kb = Keyboard.current;
            if (kb == null || b.key == Key.None) return false;
            try
            {
                return kb[b.key].isPressed;
            }
            catch
            {
                return false;
            }
        }
    }

    public static bool WasPressedThisFrame(KeyAction action)
    {
        ActionBinding b = GetBinding(action);
        if (b.device == InputDeviceType.Mouse)
        {
            Mouse m = Mouse.current;
            if (m == null) return false;
            if (b.mouse_button == 0) return m.leftButton.wasPressedThisFrame;
            if (b.mouse_button == 1) return m.rightButton.wasPressedThisFrame;
            if (b.mouse_button == 2) return m.middleButton.wasPressedThisFrame;
            return false;
        }
        else
        {
            Keyboard kb = Keyboard.current;
            if (kb == null || b.key == Key.None) return false;
            try
            {
                return kb[b.key].wasPressedThisFrame;
            }
            catch
            {
                return false;
            }
        }
    }
}
