using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RealHotkeyHintView : MonoBehaviour
{
    [SerializeField] private TMP_Text promptLabel;
    [SerializeField] private GameObject detailsRoot;
    [SerializeField] private TMP_Text detailsLabel;

    public void Configure(TMP_Text prompt, GameObject detailsPanel, TMP_Text detailsText)
    {
        promptLabel = prompt;
        detailsRoot = detailsPanel;
        detailsLabel = detailsText;
        SetExpanded(false);
    }

    public void SetPrompt(string text)
    {
        if (promptLabel != null)
        {
            promptLabel.text = text;
        }
    }

    public void SetExpanded(bool expanded)
    {
        if (detailsRoot != null)
        {
            detailsRoot.SetActive(expanded);
        }
    }

    public void SetHotkeyLines(IReadOnlyList<string> lines)
    {
        if (detailsLabel == null)
        {
            return;
        }

        if (lines == null || lines.Count == 0)
        {
            detailsLabel.text = string.Empty;
            return;
        }

        detailsLabel.text = string.Join("\n", lines);
    }
}
