using TMPro;
using UnityEngine;

public class UseKeyboard : MonoBehaviour
{
    private TMP_InputField inputField;
    private TouchScreenKeyboard keyboard;

    private void Start()
    {
        inputField = GetComponent<TMP_InputField>();
        inputField.onSelect.AddListener(ShowKeyboard);
    }

    public void ShowKeyboard(string message)
    {
        keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);
    }
}
