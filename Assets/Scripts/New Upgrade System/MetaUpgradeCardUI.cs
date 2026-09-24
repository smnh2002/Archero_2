using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MetaUpgradeCardUI : MonoBehaviour
{
    [SerializeField] private MetaUpgradeData upgradeData;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Image ımage;


    private void Start ()
    {
        Refresh();
    }

    public void Refresh()
    {
        int level = MetaProgressionManager.Instance.GetLevel(upgradeData.upgradeId);
        bool isMax = MetaProgressionManager.Instance.IsMaxLevel(upgradeData);

        nameText.text = upgradeData.upgradeName;
        levelText.text = $"Lv. {level}/{upgradeData.maxLevel}";
        costText.text = isMax ? "MAX" : MetaProgressionManager.Instance.GetCost(upgradeData).ToString();
        

        upgradeButton.interactable = !isMax;
    }

    public void OnUpgradeButtonPressed()
    {
        if (MetaProgressionManager.Instance.TryUpgrade(upgradeData))
        {
            Refresh();

            // Upgrade satın alındığında PlayerStats'ı yeniden hesapla
            // (meta bonusları dahil ederek MaxHealth vb. güncellensin)
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.ResetStats();
            }
        }
    }
}