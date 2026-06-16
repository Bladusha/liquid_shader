using UnityEngine;
using UnityEngine.UI;

public class PdfViewerPanelController : MonoBehaviour
{
    private const string MenuId = "pdf_viewer";

    [SerializeField] private Button closeButton;
    [SerializeField] private Transform contentContainer;
    [SerializeField] private GameObject pagePrefab;
    [SerializeField] private Sprite[] pdfPages;
    [SerializeField] private KeyCode closeKey = KeyCode.Tab;

    private PdfMenuOpenButton openerButton;

    private void Awake()
    {
        LoadPages();
    }

    private void OnEnable()
    {
        MenuVisibilityCoordinator.MenuOpening += HandleOtherMenuOpening;
        MenuVisibilityCoordinator.SetMenuOpen(MenuId, true);
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseMenu);
        }
    }

    private void OnDisable()
    {
        MenuVisibilityCoordinator.MenuOpening -= HandleOtherMenuOpening;
        MenuVisibilityCoordinator.SetMenuOpen(MenuId, false);
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseMenu);
        }
    }

    private void HandleOtherMenuOpening(string menuId)
    {
        if (menuId != MenuId)
        {
            CloseMenu();
        }
    }

    private void Update()
    {
        if (InputSystemCompat.GetKeyDown(closeKey) || InputSystemCompat.GetKeyDown(KeyCode.Escape))
        {
            if (InputSystemCompat.GetKeyDown(closeKey))
            {
                MenuVisibilityCoordinator.MarkTabHandled();
            }

            CloseMenu();
        }
    }

    public void SetOpener(PdfMenuOpenButton opener)
    {
        openerButton = opener;
    }

    public void CloseMenu()
    {
        openerButton?.ReEnablePreviousMenu();
        Destroy(gameObject);
    }

    private void LoadPages()
    {
        if (contentContainer == null || pagePrefab == null)
        {
            return;
        }

        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }

        if (pdfPages == null)
        {
            return;
        }

        foreach (Sprite sprite in pdfPages)
        {
            if (sprite == null)
            {
                continue;
            }

            GameObject page = Instantiate(pagePrefab, contentContainer);
            Image image = page.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = sprite;
                image.preserveAspect = true;
            }
        }
    }
}
