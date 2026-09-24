using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    public int CurrentGold { get; private set; } // artýk PlayerPrefs'ten okunmuyor, sadece bellekte

    public event System.Action<int> OnGoldChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        CurrentGold = 0; // her zaman 0'dan baþlar
    }

    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        CurrentGold += amount;
        OnGoldChanged?.Invoke(CurrentGold);
    }

    public bool TrySpendGold(int amount)
    {
        if (amount <= 0 || CurrentGold < amount) return false;
        CurrentGold -= amount;
        OnGoldChanged?.Invoke(CurrentGold);
        return true;
    }
}