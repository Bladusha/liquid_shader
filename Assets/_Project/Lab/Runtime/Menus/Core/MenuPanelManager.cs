using System;
using System.Collections.Generic;
using UnityEngine;

public class MenuPanelManager : MonoBehaviour
{
    [Serializable]
    public class PanelEntry
    {
        public string panelName;
        public GameObject panelObject;
    }

    [SerializeField] private bool showFirstPanelOnStart = true;
    [SerializeField] private bool configureCursorOnAwake = true;
    [SerializeField] private CursorLockMode cursorLockMode = CursorLockMode.None;
    [SerializeField] private PanelEntry[] panels;

    private readonly Dictionary<string, int> panelIndices = new(StringComparer.Ordinal);
    private int currentPanelIndex = -1;

    public int CurrentPanelIndex => currentPanelIndex;

    private void Awake()
    {
        if (configureCursorOnAwake)
        {
            CursorStateUtility.Apply(cursorLockMode, false);
        }

        RebuildPanelLookup();
        HideAllPanels();
    }

    private void Start()
    {
        if (showFirstPanelOnStart && panels != null && panels.Length > 0)
        {
            ShowPanelByIndex(0);
        }
    }

    public void ShowPanel(string panelName)
    {
        if (string.IsNullOrWhiteSpace(panelName))
        {
            return;
        }

        if (panelIndices.TryGetValue(panelName, out int index))
        {
            ShowPanelByIndex(index);
            return;
        }

        Debug.LogWarning($"Menu panel '{panelName}' was not found.", this);
    }

    public void ShowPanelByIndex(int index)
    {
        if (panels == null || index < 0 || index >= panels.Length)
        {
            Debug.LogWarning($"Menu panel index {index} is out of range.", this);
            return;
        }

        for (int i = 0; i < panels.Length; i++)
        {
            GameObject panel = panels[i]?.panelObject;
            if (panel != null)
            {
                panel.SetActive(i == index);
            }
        }

        currentPanelIndex = index;
    }

    public void HideAllPanels()
    {
        if (panels == null)
        {
            return;
        }

        foreach (PanelEntry panel in panels)
        {
            if (panel?.panelObject != null)
            {
                panel.panelObject.SetActive(false);
            }
        }

        currentPanelIndex = -1;
    }

    private void RebuildPanelLookup()
    {
        panelIndices.Clear();
        if (panels == null)
        {
            return;
        }

        for (int i = 0; i < panels.Length; i++)
        {
            PanelEntry panel = panels[i];
            if (panel == null || string.IsNullOrWhiteSpace(panel.panelName) || panelIndices.ContainsKey(panel.panelName))
            {
                continue;
            }

            panelIndices.Add(panel.panelName, i);
        }
    }
}

