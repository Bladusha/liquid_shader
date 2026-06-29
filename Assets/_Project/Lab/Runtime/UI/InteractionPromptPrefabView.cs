using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractionPromptPrefabView : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TMP_Text tmpText;
    [SerializeField] private Text legacyText;

    [Header("Visibility")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private bool hideGameObject = true;

    [Header("State Visuals")]
    [SerializeField] private GameObject activeStateRoot;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Color normalBackground = new Color(0f, 0f, 0f, 0.75f);
    [SerializeField] private Color activeBackground = new Color(0.1f, 0.45f, 0.15f, 0.85f);

    private RectTransform rectTransform;

    private void Awake()
    {
        CacheReferences();
        Hide();
    }

    private void Reset()
    {
        CacheReferences();
    }

    public void Show(bool active)
    {
        CacheReferences();

        if (hideGameObject && !gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        if (activeStateRoot != null)
        {
            activeStateRoot.SetActive(active);
        }

        if (backgroundImage != null)
        {
            backgroundImage.color = active ? activeBackground : normalBackground;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void Show(string prompt, bool active)
    {
        CacheReferences();

        if (!string.IsNullOrWhiteSpace(prompt))
        {
            if (tmpText != null)
            {
                tmpText.text = prompt;
            }

            if (legacyText != null)
            {
                legacyText.text = prompt;
            }
        }

        Show(active);
    }

    public void Hide()
    {
        CacheReferences();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (hideGameObject && gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }

    public void SetScreenPosition(Vector2 screenPosition)
    {
        CacheReferences();
        if (rectTransform == null)
        {
            return;
        }

        rectTransform.position = screenPosition;
    }

    private void CacheReferences()
    {
        if (rectTransform == null)
        {
            rectTransform = transform as RectTransform;
        }

        if (tmpText == null)
        {
            tmpText = GetComponentInChildren<TMP_Text>(true);
        }

        if (legacyText == null)
        {
            legacyText = GetComponentInChildren<Text>(true);
        }

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
        }
    }
}
