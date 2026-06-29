using LiquidShader.RuntimeData;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerDataMenuController : MonoBehaviour
{
    private const string SurnameKey = "Surname";
    private const string NameKey = "Name";
    private const string GroupKey = "Group";

    [Header("Input Fields")]
    [SerializeField] private TMP_InputField surnameField;
    [SerializeField] private TMP_InputField nameField;
    [SerializeField] private TMP_InputField groupField;

    [Header("Navigation")]
    [SerializeField] private MenuPanelManager panelManager;
    [SerializeField] private string nextPanelName = "MainMenu";
    [SerializeField] private string nextSceneName;
    [SerializeField] private bool loadNextScene;
    [SerializeField] private bool requireFilledFields = true;
    [SerializeField] private bool saveToPlayerPrefs = true;
    [SerializeField] private LabSelectionMenuController labSelectionController;
    [SerializeField] private Button continueButton;
    [SerializeField] private TMP_Text surnameErrorText;
    [SerializeField] private TMP_Text nameErrorText;
    [SerializeField] private TMP_Text groupErrorText;
    [SerializeField] private Color normalFieldColor = new Color(0.15f, 0.19f, 0.24f, 0.98f);
    [SerializeField] private Color errorFieldColor = new Color(0.34f, 0.08f, 0.1f, 0.98f);
    [SerializeField] private Sprite normalFieldSprite;
    [SerializeField] private Sprite errorFieldSprite;

    private void Awake()
    {
        if (panelManager == null)
        {
            panelManager = GetComponentInParent<MenuPanelManager>() ?? FindAnyObjectByType<MenuPanelManager>();
        }

        if (labSelectionController == null)
        {
            labSelectionController = GetComponentInChildren<LabSelectionMenuController>(true);
        }

        if (continueButton == null)
        {
            continueButton = GetComponent<Button>();
        }

        SetupFieldListeners();
        ClearValidationState();
    }

    private void OnEnable()
    {
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnNextButtonClick);
        }
    }

    private void OnDisable()
    {
        RemoveFieldListeners();

        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnNextButtonClick);
        }
    }

    public void OnNextButtonClick()
    {
        if (requireFilledFields && !HasRequiredData())
        {
            ApplyValidationFeedback();
            Debug.LogWarning("Fill all menu input fields before continuing.", this);
            return;
        }

        if (saveToPlayerPrefs)
        {
            SaveData();
        }

        if (loadNextScene)
        {
            string sceneName = GetSelectedTargetScene();
            if (!string.IsNullOrWhiteSpace(sceneName))
            {
                LoadScene(sceneName);
                return;
            }

            if (!string.IsNullOrWhiteSpace(nextSceneName))
            {
                LoadScene(nextSceneName.Trim());
                return;
            }

            Debug.LogWarning("No target scene configured for player data menu.", this);
            return;
        }

        if (panelManager != null && !string.IsNullOrWhiteSpace(nextPanelName))
        {
            panelManager.ShowPanel(nextPanelName);
        }
    }

    private bool HasRequiredData()
    {
        return HasText(surnameField) && HasText(nameField) && HasText(groupField);
    }

    private static bool HasText(TMP_InputField field)
    {
        return field != null && !string.IsNullOrWhiteSpace(field.text);
    }

    private void SaveData()
    {
        GameStateStore store = GameStateStore.Instance;
        store.SetValue(SurnameKey, GetTrimmedText(surnameField));
        store.SetValue(NameKey, GetTrimmedText(nameField));
        store.SetValue(GroupKey, GetTrimmedText(groupField));

        if (labSelectionController != null)
        {
            store.SetValue("SelectedLabScene", labSelectionController.GetSelectedSceneName());
        }

        store.Save();
    }

    private static void LoadScene(string sceneName)
    {
        if (!IsSceneAvailable(sceneName))
        {
            Debug.LogWarning($"Scene '{sceneName}' is not enabled in Build Settings.");
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    private static bool IsSceneAvailable(string targetScene)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (string.Equals(name, targetScene, System.StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private string GetSelectedTargetScene()
    {
        return labSelectionController != null ? labSelectionController.GetSelectedSceneName() : string.Empty;
    }

    private void SetupFieldListeners()
    {
        if (surnameField != null)
        {
            surnameField.onValueChanged.AddListener(OnSurnameChanged);
        }

        if (nameField != null)
        {
            nameField.onValueChanged.AddListener(OnNameChanged);
        }

        if (groupField != null)
        {
            groupField.onValueChanged.AddListener(OnGroupChanged);
        }
    }

    private void RemoveFieldListeners()
    {
        if (surnameField != null)
        {
            surnameField.onValueChanged.RemoveListener(OnSurnameChanged);
        }

        if (nameField != null)
        {
            nameField.onValueChanged.RemoveListener(OnNameChanged);
        }

        if (groupField != null)
        {
            groupField.onValueChanged.RemoveListener(OnGroupChanged);
        }
    }

    private void OnSurnameChanged(string value)
    {
        ClearFieldValidation(surnameField, surnameErrorText);
    }

    private void OnNameChanged(string value)
    {
        ClearFieldValidation(nameField, nameErrorText);
    }

    private void OnGroupChanged(string value)
    {
        ClearFieldValidation(groupField, groupErrorText);
    }

    private void ApplyValidationFeedback()
    {
        ApplyFieldValidation(surnameField, surnameErrorText);
        ApplyFieldValidation(nameField, nameErrorText);
        ApplyFieldValidation(groupField, groupErrorText);
    }

    private void ClearValidationState()
    {
        ClearFieldValidation(surnameField, surnameErrorText);
        ClearFieldValidation(nameField, nameErrorText);
        ClearFieldValidation(groupField, groupErrorText);
    }

    private void ApplyFieldValidation(TMP_InputField field, TMP_Text errorText)
    {
        if (field == null)
        {
            return;
        }

        bool hasText = HasText(field);
        SetFieldVisual(field, hasText ? normalFieldColor : errorFieldColor, hasText ? normalFieldSprite : errorFieldSprite);

        if (errorText != null)
        {
            errorText.text = hasText ? string.Empty : "Заполните поле";
            errorText.gameObject.SetActive(!hasText);
        }
    }

    private void ClearFieldValidation(TMP_InputField field, TMP_Text errorText)
    {
        if (field != null)
        {
            SetFieldVisual(field, normalFieldColor, normalFieldSprite);
        }

        if (errorText != null)
        {
            errorText.text = string.Empty;
            errorText.gameObject.SetActive(false);
        }
    }

    private static void SetFieldVisual(TMP_InputField field, Color color, Sprite sprite)
    {
        if (field == null)
        {
            return;
        }

        Graphic graphic = field.targetGraphic;
        if (graphic != null)
        {
            graphic.color = color;
            if (sprite != null && graphic is Image image)
            {
                image.sprite = sprite;
            }
        }
    }

    private static string GetTrimmedText(TMP_InputField field)
    {
        return field != null ? field.text.Trim() : string.Empty;
    }
}

