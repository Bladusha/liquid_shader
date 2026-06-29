using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LabSelectionMenuController : MonoBehaviour
{
    [Serializable]
    public class LabOption
    {
        [SerializeField] private string displayName = "Test 1";
        [SerializeField] private string sceneName = "real";

        public string DisplayName => displayName;
        public string SceneName => sceneName;
    }

    [SerializeField] private TMP_Dropdown labDropdown;
    [SerializeField] private LabOption[] labOptions = { new LabOption() };
    [SerializeField] private string selectedLabPrefKey = "SelectedLabIndex";
    [SerializeField] private bool saveSelectionToPlayerPrefs = true;

    private Button continueButton;

    private void Awake()
    {
        if (labDropdown == null)
        {
            labDropdown = GetComponentInChildren<TMP_Dropdown>(true);
        }

        if (continueButton == null)
        {
            continueButton = GetComponent<Button>();
        }

        SyncOptions();
        RestoreSelection();
        ApplySelection();
    }

    private void OnEnable()
    {
        if (labDropdown != null)
        {
            labDropdown.onValueChanged.AddListener(OnSelectionChanged);
        }

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueButtonClick);
        }
    }

    private void OnDisable()
    {
        if (labDropdown != null)
        {
            labDropdown.onValueChanged.RemoveListener(OnSelectionChanged);
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinueButtonClick);
        }
    }

    public void OnContinueButtonClick()
    {
        ApplySelection();
        LoadSelectedScene();
    }

    public void OnSelectionChanged(int index)
    {
        ApplySelection();
    }

    public string GetSelectedSceneName()
    {
        return ResolveSceneName(GetCurrentIndex());
    }

    private void ApplySelection()
    {
        if (!saveSelectionToPlayerPrefs)
        {
            return;
        }

        PlayerPrefs.SetInt(selectedLabPrefKey, GetCurrentIndex());
        PlayerPrefs.Save();
    }

    private void RestoreSelection()
    {
        if (labDropdown == null)
        {
            return;
        }

        int savedIndex = PlayerPrefs.GetInt(selectedLabPrefKey, 0);
        labDropdown.SetValueWithoutNotify(Mathf.Clamp(savedIndex, 0, Mathf.Max(0, labDropdown.options.Count - 1)));
        labDropdown.RefreshShownValue();
    }

    private void SyncOptions()
    {
        if (labDropdown == null)
        {
            return;
        }

        EnsureDefaultOption();

        var options = new System.Collections.Generic.List<string>(labOptions.Length);
        foreach (LabOption option in labOptions)
        {
            options.Add(string.IsNullOrWhiteSpace(option?.DisplayName) ? "Laboratory" : option.DisplayName);
        }

        labDropdown.ClearOptions();
        labDropdown.AddOptions(options);
        labDropdown.RefreshShownValue();
    }

    private void EnsureDefaultOption()
    {
        if (labOptions != null && labOptions.Length > 0)
        {
            return;
        }

        labOptions = new[] { new LabOption() };
    }

    private int GetCurrentIndex()
    {
        if (labDropdown == null || labDropdown.options == null || labDropdown.options.Count == 0)
        {
            return 0;
        }

        return Mathf.Clamp(labDropdown.value, 0, labDropdown.options.Count - 1);
    }

    private string ResolveSceneName(int index)
    {
        if (labOptions == null || labOptions.Length == 0)
        {
            return string.Empty;
        }

        index = Mathf.Clamp(index, 0, labOptions.Length - 1);
        return labOptions[index] != null ? labOptions[index].SceneName?.Trim() ?? string.Empty : string.Empty;
    }

    private void LoadSelectedScene()
    {
        string sceneName = GetSelectedSceneName();
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("Selected laboratory scene name is empty.", this);
            return;
        }

        if (!IsSceneAvailable(sceneName))
        {
            Debug.LogWarning($"Scene '{sceneName}' is not enabled in Build Settings.", this);
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
            string name = Path.GetFileNameWithoutExtension(path);
            if (string.Equals(name, targetScene, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
