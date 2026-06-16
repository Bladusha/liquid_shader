using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PanelPrefabOpenButton : MonoBehaviour
{
    [SerializeField] private GameObject panelPrefabToOpen;
    [SerializeField] private Transform parentForNewPanel;
    [SerializeField] private bool hideCurrentPanel = true;

    private Button button;
    private GameObject currentPanel;
    private GameObject spawnedPanelInstance;

    private void Awake()
    {
        button = GetComponent<Button>();
        ResolveCurrentPanel();
    }

    private void OnEnable()
    {
        if (button != null)
        {
            button.onClick.AddListener(OpenPanelPrefab);
        }
    }

    private void OnDisable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OpenPanelPrefab);
        }
    }

    public void OpenPanelPrefab()
    {
        if (panelPrefabToOpen == null)
        {
            Debug.LogError("Panel prefab is not assigned.", this);
            return;
        }

        ResolveCurrentPanel();
        Transform targetParent = parentForNewPanel != null ? parentForNewPanel : currentPanel?.transform.parent;
        if (targetParent == null)
        {
            Debug.LogWarning("Unable to resolve parent for a new panel instance.", this);
            return;
        }

        if (hideCurrentPanel && currentPanel != null)
        {
            currentPanel.SetActive(false);
        }

        if (spawnedPanelInstance != null)
        {
            Destroy(spawnedPanelInstance);
        }

        spawnedPanelInstance = Instantiate(panelPrefabToOpen, targetParent, false);
        spawnedPanelInstance.SetActive(true);
    }

    public void SetCurrentPanel(GameObject panel)
    {
        currentPanel = panel;
    }

    public void SetPanelPrefabToOpen(GameObject prefab)
    {
        panelPrefabToOpen = prefab;
    }

    private void ResolveCurrentPanel()
    {
        if (currentPanel != null)
        {
            return;
        }

        Transform parent = transform.parent;
        while (parent != null)
        {
            if (parent.GetComponent<RectTransform>() != null &&
                parent.name.Contains("panel", StringComparison.OrdinalIgnoreCase))
            {
                currentPanel = parent.gameObject;
                return;
            }

            parent = parent.parent;
        }

        if (transform.parent != null)
        {
            currentPanel = transform.parent.gameObject;
        }
    }
}
