using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CoinUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI coinText;

    private void Start()
    {
        // Subscribe to the gold changed event
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnGoldChanged += UpdateCoinText;
            
            // Initialize with current gold
            UpdateCoinText(CurrencyManager.Instance.CurrentGold);
        }
        else
        {
            Debug.LogWarning("CurrencyManager instance not found!");
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnGoldChanged -= UpdateCoinText;
        }
    }

    private void UpdateCoinText(int currentGold)
    {
        if (coinText != null)
        {
            // Format for thousands separators (e.g. 1,000)
            coinText.text = currentGold.ToString("N0");
        }
    }
}
