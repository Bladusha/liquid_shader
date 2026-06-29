using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class DropdownClosedVisualHider : MonoBehaviour, IPointerClickHandler, ISubmitHandler, ICancelHandler
{
    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private Graphic[] closedVisuals;

    private Coroutine watchRoutine;

    private void Awake()
    {
        if (dropdown == null)
        {
            dropdown = GetComponent<TMP_Dropdown>();
        }

        if (closedVisuals == null || closedVisuals.Length == 0)
        {
            closedVisuals = FindClosedVisuals();
        }
    }

    private void OnEnable()
    {
        if (dropdown != null)
        {
            dropdown.onValueChanged.AddListener(OnValueChanged);
        }

        SetClosedVisualsVisible(true);
    }

    private void OnDisable()
    {
        if (dropdown != null)
        {
            dropdown.onValueChanged.RemoveListener(OnValueChanged);
        }

        StopWatching();
        SetClosedVisualsVisible(true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        BeginOpenWatch();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        BeginOpenWatch();
    }

    public void OnCancel(BaseEventData eventData)
    {
        RestoreClosedVisuals();
    }

    private void OnValueChanged(int value)
    {
        RestoreClosedVisuals();
    }

    private void BeginOpenWatch()
    {
        StopWatching();
        watchRoutine = StartCoroutine(WatchDropdownList());
    }

    private IEnumerator WatchDropdownList()
    {
        yield return null;

        bool sawOpenList = false;
        while (isActiveAndEnabled)
        {
            bool hasOpenList = HasOpenDropdownList();
            if (hasOpenList)
            {
                sawOpenList = true;
                SetClosedVisualsVisible(false);
            }
            else if (sawOpenList)
            {
                RestoreClosedVisuals();
                yield break;
            }

            yield return null;
        }
    }

    private bool HasOpenDropdownList()
    {
        Transform root = dropdown != null ? dropdown.transform : transform;
        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            if (child.gameObject.activeInHierarchy && child.name.StartsWith("Dropdown List"))
            {
                return true;
            }
        }

        return false;
    }

    private void RestoreClosedVisuals()
    {
        StopWatching();
        SetClosedVisualsVisible(true);
    }

    private void StopWatching()
    {
        if (watchRoutine != null)
        {
            StopCoroutine(watchRoutine);
            watchRoutine = null;
        }
    }

    private void SetClosedVisualsVisible(bool visible)
    {
        if (closedVisuals == null)
        {
            return;
        }

        float alpha = visible ? 1f : 0f;
        for (int i = 0; i < closedVisuals.Length; i++)
        {
            Graphic graphic = closedVisuals[i];
            if (graphic == null)
            {
                continue;
            }

            Color color = graphic.color;
            color.a = alpha;
            graphic.color = color;
        }
    }

    private Graphic[] FindClosedVisuals()
    {
        Transform template = dropdown != null && dropdown.template != null ? dropdown.template : null;
        var graphics = GetComponentsInChildren<Graphic>(true);
        var result = new System.Collections.Generic.List<Graphic>(graphics.Length);

        foreach (Graphic graphic in graphics)
        {
            if (graphic == null)
            {
                continue;
            }

            if (template != null && graphic.transform.IsChildOf(template))
            {
                continue;
            }

            result.Add(graphic);
        }

        return result.ToArray();
    }
}
