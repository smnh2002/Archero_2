using UnityEngine;

public class MenuNavigation : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject metaUpgradePanel;

    private void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        metaUpgradePanel.SetActive(false);
    }

    public void ShowUpgradePanel()
    {
        mainMenuPanel.SetActive(false);
        metaUpgradePanel.SetActive(true);
    }
}