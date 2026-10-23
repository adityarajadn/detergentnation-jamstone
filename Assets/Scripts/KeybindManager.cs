using System;
using UnityEngine;

public static class KeybindManager
{
    public static KeyCode MoveLeft { get => GetKey("Key_MoveLeft", KeyCode.A); set => SetKey("Key_MoveLeft", value); }
    public static KeyCode MoveRight { get => GetKey("Key_MoveRight", KeyCode.D); set => SetKey("Key_MoveRight", value); }
    public static KeyCode MoveUp { get => GetKey("Key_MoveUp", KeyCode.W); set => SetKey("Key_MoveUp", value); }
    public static KeyCode MoveDown { get => GetKey("Key_MoveDown", KeyCode.S); set => SetKey("Key_MoveDown", value); }
    public static KeyCode Jump { get => GetKey("Key_Jump", KeyCode.Space); set => SetKey("Key_Jump", value); }
    public static KeyCode UseItem1 { get => GetKey("Key_UseItem1", KeyCode.Q); set => SetKey("Key_UseItem1", value); }
    public static KeyCode UseItem2 { get => GetKey("Key_UseItem2", KeyCode.E); set => SetKey("Key_UseItem2", value); }
    public static KeyCode UseItem { get => UseItem1; set => UseItem1 = value; }
    public static KeyCode Interact { get => GetKey("Key_Interact", KeyCode.F); set => SetKey("Key_Interact", value); }
    public static KeyCode Reload { get => GetKey("Key_Reload", KeyCode.R); set => SetKey("Key_Reload", value); }

    public static event Action OnKeybindsChanged;

    public static KeyCode GetKey(string key, KeyCode defaultKey)
    {
        return (KeyCode)PlayerPrefs.GetInt(key, (int)defaultKey);
    }

    public static void SetKey(string key, KeyCode code)
    {
        PlayerPrefs.SetInt(key, (int)code);
        PlayerPrefs.Save();
        OnKeybindsChanged?.Invoke();
    }

    public static void ResetDefaults()
    {
        PlayerPrefs.DeleteKey("Key_MoveLeft");
        PlayerPrefs.DeleteKey("Key_MoveRight");
        PlayerPrefs.DeleteKey("Key_MoveUp");
        PlayerPrefs.DeleteKey("Key_MoveDown");
        PlayerPrefs.DeleteKey("Key_Jump");
        PlayerPrefs.DeleteKey("Key_UseItem1");
        PlayerPrefs.DeleteKey("Key_UseItem2");
        PlayerPrefs.DeleteKey("Key_UseItem");
        PlayerPrefs.DeleteKey("Key_Interact");
        PlayerPrefs.DeleteKey("Key_Reload");
        PlayerPrefs.Save();
        OnKeybindsChanged?.Invoke();
    }
}
