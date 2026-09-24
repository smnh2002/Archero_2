using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Debug amacli: E tusuna basinca +100 coin verir.
/// RuntimeInitializeOnLoadMethod ile otomatik olarak her sahnede baslar.
/// </summary>
public class DebugCoinCheat : MonoBehaviour
{
    [SerializeField] private int coinAmount = 100;

    private static DebugCoinCheat _instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        if (_instance != null) return;

        GameObject go = new GameObject("[DebugCoinCheat]");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<DebugCoinCheat>();
        Debug.Log("<color=yellow>[DebugCoinCheat] Aktif - E tusuna basinca +100 coin!</color>");
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            GiveCoins();
    }

    private void GiveCoins()
    {
        if (CurrencyManager.Instance == null)
        {
            Debug.LogWarning("[DebugCoinCheat] CurrencyManager bulunamadi!");
            return;
        }

        CurrencyManager.Instance.AddGold(coinAmount);
        Debug.Log($"<color=yellow>[DEBUG] +{coinAmount} coin verildi! Toplam: {CurrencyManager.Instance.CurrentGold}</color>");
    }
}