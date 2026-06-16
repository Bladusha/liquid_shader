using UnityEngine;

public class InteractionFeedback : MonoBehaviour
{
    [Header("Outline")]
    public bool useOutline = true;
    public Transform outlineTarget;
    public Color outlineColor = Color.yellow;
    [Range(0f, 10f)] public float outlineWidth = 5f;

    [Header("Active Outline")]
    public bool changeOutlineOnActive = true;
    public Color activeOutlineColor = Color.green;
    [Range(0f, 10f)] public float activeOutlineWidth = 3f;
    public bool disableOutlineOnActive = false;

    [Header("Notifications")]
    [TextArea(2, 4)] public string hoverMessage = "Press E to interact";
    [TextArea(2, 4)] public string activeMessage = "";

    private Outline runtimeOutline;
    private bool isActive;

    public string HoverMessage => isActive && !string.IsNullOrEmpty(activeMessage) ? activeMessage : hoverMessage;
    public string ActiveMessage => activeMessage;
    public bool IsActive => isActive;

    public void SetHoverState(bool active)
    {
        if (!useOutline || isActive)
        {
            return;
        }

        Outline outline = GetOrCreateOutline();
        if (outline == null)
        {
            return;
        }

        outline.OutlineMode = Outline.Mode.OutlineAll;
        outline.OutlineColor = outlineColor;
        outline.OutlineWidth = outlineWidth;
        outline.enabled = active;
    }

    public void SetActiveState(bool active)
    {
        isActive = active;

        if (!useOutline)
        {
            return;
        }

        Outline outline = GetOrCreateOutline();
        if (outline == null)
        {
            return;
        }

        if (active && disableOutlineOnActive)
        {
            outline.enabled = false;
            return;
        }

        outline.OutlineMode = Outline.Mode.OutlineAll;

        if (active && changeOutlineOnActive)
        {
            outline.OutlineColor = activeOutlineColor;
            outline.OutlineWidth = activeOutlineWidth;
        }
        else
        {
            outline.OutlineColor = outlineColor;
            outline.OutlineWidth = outlineWidth;
        }

        outline.enabled = true;
    }

    public void ClearHoverState()
    {
        if (isActive)
        {
            return;
        }

        if (runtimeOutline != null)
        {
            runtimeOutline.enabled = false;
        }
    }

    public void ClearAll()
    {
        isActive = false;

        if (runtimeOutline != null)
        {
            runtimeOutline.enabled = false;
        }
    }

    private Outline GetOrCreateOutline()
    {
        Transform target = outlineTarget != null ? outlineTarget : transform;
        if (target == null)
        {
            return null;
        }

        if (runtimeOutline == null)
        {
            runtimeOutline = target.GetComponent<Outline>();
            if (runtimeOutline == null)
            {
                runtimeOutline = target.gameObject.AddComponent<Outline>();
            }
        }

        return runtimeOutline;
    }
}
