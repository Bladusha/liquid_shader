using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MenuPanelSwitchButton : MonoBehaviour
{
    [SerializeField] private MenuPanelManager panelManager;
    [SerializeField] private string targetPanelName;
    [SerializeField] private int targetPanelIndex = -1;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        ResolvePanelManager();
    }

    private void OnEnable()
    {
        if (button != null)
        {
            button.onClick.AddListener(SwitchPanel);
        }
    }

    private void OnDisable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(SwitchPanel);
        }
    }

    public void SwitchPanel()
    {
        ResolvePanelManager();
        if (panelManager == null)
        {
            Debug.LogWarning("MenuPanelManager was not found.", this);
            return;
        }

        if (!string.IsNullOrWhiteSpace(targetPanelName))
        {
            panelManager.ShowPanel(targetPanelName);
            return;
        }

        panelManager.ShowPanelByIndex(targetPanelIndex);
    }

    private void ResolvePanelManager()
    {
        if (panelManager != null)
        {
            return;
        }

        panelManager = GetComponentInParent<MenuPanelManager>();
        if (panelManager == null)
        {
            panelManager = FindAnyObjectByType<MenuPanelManager>();
        }
    }
}

