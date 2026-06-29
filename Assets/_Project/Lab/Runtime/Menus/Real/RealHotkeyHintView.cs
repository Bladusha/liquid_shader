using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RealHotkeyHintView : MonoBehaviour
{
    [SerializeField] private GameObject promptRoot;
    [SerializeField] private TMP_Text promptLabel;
    [SerializeField] private GameObject detailsRoot;
    [SerializeField] private TMP_Text detailsLabel;
    [SerializeField, Min(0.01f)] private float animationDuration = 0.18f;
    [SerializeField] private Vector2 collapsedDetailsOffset = new Vector2(-10f, -16f);

    private CanvasGroup promptGroup;
    private CanvasGroup detailsGroup;
    private RectTransform detailsRect;
    private Graphic[] promptGraphics;
    private Graphic[] detailsGraphics;
    private Vector2 detailsExpandedPosition;
    private Coroutine animationRoutine;
    private bool isExpanded;
    private bool hasAppliedState;

    public void Configure(TMP_Text prompt, GameObject detailsPanel, TMP_Text detailsText)
    {
        promptRoot = prompt != null ? prompt.transform.parent.gameObject : promptRoot;
        promptLabel = prompt;
        detailsRoot = detailsPanel;
        detailsLabel = detailsText;
        CacheAnimationTargets();
        SetExpanded(false);
    }

    public void Configure(GameObject promptPanel, GameObject detailsPanel)
    {
        promptRoot = promptPanel;
        detailsRoot = detailsPanel;
        CacheAnimationTargets();
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
        CacheAnimationTargets();
        isExpanded = expanded;

        if (!isActiveAndEnabled || !hasAppliedState)
        {
            ApplyInstant(expanded);
            hasAppliedState = true;
            return;
        }

        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
        }

        animationRoutine = StartCoroutine(AnimateExpanded(expanded));
    }

    public void SetExpandedInstant(bool expanded)
    {
        CacheAnimationTargets();
        isExpanded = expanded;
        ApplyInstant(expanded);
        hasAppliedState = true;
    }

    private void CacheAnimationTargets()
    {
        if (promptRoot == null && promptLabel != null)
        {
            promptRoot = promptLabel.transform.parent.gameObject;
        }

        if (promptRoot != null && promptGroup == null)
        {
            promptGroup = GetOrAddCanvasGroup(promptRoot);
            promptGraphics = promptRoot.GetComponentsInChildren<Graphic>(true);
        }

        if (detailsRoot != null)
        {
            if (detailsGroup == null)
            {
                detailsGroup = GetOrAddCanvasGroup(detailsRoot);
                detailsGraphics = detailsRoot.GetComponentsInChildren<Graphic>(true);
            }

            if (detailsRect == null)
            {
                detailsRect = detailsRoot.transform as RectTransform;
                if (detailsRect != null)
                {
                    detailsExpandedPosition = detailsRect.anchoredPosition;
                }
            }
        }
    }

    private IEnumerator AnimateExpanded(bool expanded)
    {
        if (detailsRoot != null)
        {
            detailsRoot.SetActive(true);
        }

        SetGraphicAlpha(promptGraphics, 1f);
        SetGraphicAlpha(detailsGraphics, 1f);

        float startPromptAlpha = promptGroup != null ? promptGroup.alpha : expanded ? 1f : 0f;
        float targetPromptAlpha = expanded ? 0f : 1f;
        float startDetailsAlpha = detailsGroup != null ? detailsGroup.alpha : expanded ? 0f : 1f;
        float targetDetailsAlpha = expanded ? 1f : 0f;
        Vector3 startDetailsScale = detailsRect != null ? detailsRect.localScale : Vector3.one;
        Vector3 targetDetailsScale = expanded ? Vector3.one : new Vector3(0.97f, 0.97f, 1f);
        Vector2 startDetailsPosition = detailsRect != null ? detailsRect.anchoredPosition : Vector2.zero;
        Vector2 targetDetailsPosition = expanded ? detailsExpandedPosition : detailsExpandedPosition + collapsedDetailsOffset;

        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / animationDuration);
            float eased = EaseOutCubic(t);

            if (promptGroup != null)
            {
                promptGroup.alpha = Mathf.Lerp(startPromptAlpha, targetPromptAlpha, eased);
            }

            if (detailsGroup != null)
            {
                detailsGroup.alpha = Mathf.Lerp(startDetailsAlpha, targetDetailsAlpha, eased);
            }

            if (detailsRect != null)
            {
                detailsRect.localScale = Vector3.LerpUnclamped(startDetailsScale, targetDetailsScale, eased);
                detailsRect.anchoredPosition = Vector2.LerpUnclamped(startDetailsPosition, targetDetailsPosition, eased);
            }

            yield return null;
        }

        ApplyInstant(expanded);
        animationRoutine = null;
    }

    private void ApplyInstant(bool expanded)
    {
        if (promptGroup != null)
        {
            SetGraphicAlpha(promptGraphics, 1f);
            promptGroup.alpha = expanded ? 0f : 1f;
            promptGroup.interactable = false;
            promptGroup.blocksRaycasts = false;
        }

        if (detailsRoot != null)
        {
            detailsRoot.SetActive(expanded);
        }

        if (detailsGroup != null)
        {
            SetGraphicAlpha(detailsGraphics, 1f);
            detailsGroup.alpha = expanded ? 1f : 0f;
            detailsGroup.interactable = false;
            detailsGroup.blocksRaycasts = false;
        }

        if (detailsRect != null)
        {
            detailsRect.localScale = expanded ? Vector3.one : new Vector3(0.97f, 0.97f, 1f);
            detailsRect.anchoredPosition = expanded ? detailsExpandedPosition : detailsExpandedPosition + collapsedDetailsOffset;
        }
    }

    private static CanvasGroup GetOrAddCanvasGroup(GameObject target)
    {
        CanvasGroup group = target.GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = target.AddComponent<CanvasGroup>();
        }

        return group;
    }

    private static void SetGraphicAlpha(Graphic[] graphics, float alpha)
    {
        if (graphics == null)
        {
            return;
        }

        foreach (Graphic graphic in graphics)
        {
            if (graphic == null)
            {
                continue;
            }

            Color color = graphic.color;
            color.a = alpha;
            graphic.color = color;
        }
    }

    private static float EaseOutCubic(float t)
    {
        float p = 1f - t;
        return 1f - p * p * p;
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
