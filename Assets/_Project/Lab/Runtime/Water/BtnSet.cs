using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

[MovedFrom(true, null, null, "WaterButton")]
public class BtnSet : MonoBehaviour, WorkzoneSelectionController.IInteractable
{
    [Header("Связи")]
    public WaterController controller;
    public Transform buttonVisual;

    [Header("Настройки воды (Передаются в контроллер)")]
    [Range(0f, 1f)] public float fillLevel = 0f;
    [Range(0f, 5f)] public float flowSpeed = 1f;
    [Range(0f, 1f)] public float temperature = 0.35f;
    [Range(0f, 1f)] public float surfaceOscillation = 0.3f;
    [Range(0.5f, 8f)] public float bubbleSize = 1.25f;
    [Range(0f, 1f)] public float bubbleAmount = 0.35f;
    [Range(-1f, 1f)] public float frontTilt = 0.12f;
    [Range(0.001f, 0.2f)] public float frontFade = 0.06f;
    [Range(0f, 1f)] public float alpha = 0.82f;

    [Header("Анимация")]
    public Vector3 pressedOffset = new Vector3(0, -0.05f, 0);
    public float animSpeed = 1f;

    private Vector3 startPos;
    private bool isActivated = false;

    private void Start()
    {
        if (buttonVisual != null)
            startPos = buttonVisual.localPosition;
    }

    private void Update()
    {
        if (buttonVisual != null)
        {
            Vector3 target = isActivated ? (startPos + pressedOffset) : startPos;
            buttonVisual.localPosition = Vector3.Lerp(buttonVisual.localPosition, target, Time.deltaTime * animSpeed);
        }
    }

    public void Press()
    {
        isActivated = true;
        SceneActivityLog.Add("Взаимодействие", $"BtnSet {name}");

        if (controller != null)
        {
            controller.SelectMode(this);
        }
    }

    public void Interact()
    {
        if (MenuVisibilityCoordinator.AnyMenuOpen)
        {
            return;
        }

        Press();
    }

    public void Release()
    {
        isActivated = false;
    }

    public void SetPressedVisual(bool pressed)
    {
        isActivated = pressed;
    }
}
