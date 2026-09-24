using UnityEngine;
using UnityEngine.InputSystem;

public class LevelUpDebugInput : MonoBehaviour
{
    private void Update()
    {
        if (Keyboard.current == null) return;

        // GEÇÝCÝ: Her tuþa basýþta durumu görelim
        if (Keyboard.current.digit1Key.wasPressedThisFrame ||
            Keyboard.current.digit2Key.wasPressedThisFrame ||
            Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            Debug.Log($"<color=lime>Tuþa basýldý. GameManager var mý: {GameManager.Instance != null}, State: {GameManager.Instance?.CurrentState}</color>");
        }

        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.LevelUp) return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame) UpgradeManager.Instance.ChooseUpgrade(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) UpgradeManager.Instance.ChooseUpgrade(1);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) UpgradeManager.Instance.ChooseUpgrade(2);
    }
}

