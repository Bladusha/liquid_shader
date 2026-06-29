using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PdfMenuOpenButton : MonoBehaviour
{
    [SerializeField] private GameObject pdfMenuPrefab;
    [SerializeField] private GameObject menuToHideOnOpen;

    private Button button;
    private Canvas rootCanvas;
    private GameObject currentPdfMenuInstance;

    private void Awake()
    {
        button = GetComponent<Button>();
        rootCanvas = GetComponentInParent<Canvas>()?.rootCanvas;
        ResolveMenuToHide();
    }

    private void OnEnable()
    {
        if (button != null)
        {
            button.onClick.AddListener(OpenMenu);
        }
    }

    private void OnDisable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OpenMenu);
        }
    }

    public void OpenMenu()
    {
        if (pdfMenuPrefab == null)
        {
            Debug.LogError("PDF menu prefab is not assigned.", this);
            return;
        }

        if (currentPdfMenuInstance != null)
        {
            return;
        }

        rootCanvas ??= GetComponentInParent<Canvas>()?.rootCanvas;
        if (rootCanvas == null)
        {
            Debug.LogError("PDF menu button is not inside a Canvas.", this);
            return;
        }

        if (menuToHideOnOpen != null && menuToHideOnOpen.activeSelf)
        {
            menuToHideOnOpen.SetActive(false);
        }

        currentPdfMenuInstance = Instantiate(pdfMenuPrefab, rootCanvas.transform);
        currentPdfMenuInstance.transform.SetAsLastSibling();

        RectTransform rectTransform = currentPdfMenuInstance.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.localScale = Vector3.one;
        }

        PdfViewerPanelController viewer = currentPdfMenuInstance.GetComponent<PdfViewerPanelController>();
        if (viewer != null)
        {
            viewer.SetOpener(this);
        }
    }

    public void ReEnablePreviousMenu()
    {
        if (menuToHideOnOpen != null && !menuToHideOnOpen.activeSelf)
        {
            menuToHideOnOpen.SetActive(true);
        }

        currentPdfMenuInstance = null;
    }

    private void ResolveMenuToHide()
    {
        if (menuToHideOnOpen != null)
        {
            return;
        }

        Transform parent = transform.parent;
        while (parent != null)
        {
            if (parent.gameObject.activeInHierarchy &&
                (parent.name.Contains("Panel") || parent.name.Contains("Canvas")))
            {
                menuToHideOnOpen = parent.gameObject;
                return;
            }

            parent = parent.parent;
        }

        if (transform.parent != null)
        {
            menuToHideOnOpen = transform.parent.gameObject;
        }
    }
}
