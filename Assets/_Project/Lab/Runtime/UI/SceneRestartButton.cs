using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SceneRestartButton : MonoBehaviour
{
    [SerializeField] private float resetDelay = 0.2f;
    [SerializeField] private bool playClickSound = true;
    [SerializeField] private PanelInputAutoSave panelToClean;
    [SerializeField] private bool hardResetAllData;

    private Button button;
    private AudioSource audioSource;

    private void Awake()
    {
        button = GetComponent<Button>();
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        if (button != null)
        {
            button.onClick.AddListener(RestartScene);
        }
    }

    private void OnDisable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(RestartScene);
        }
    }

    public void RestartScene()
    {
        if (button != null)
        {
            button.interactable = false;
        }

        PerformCleanup();
        PlaySoundIfAvailable();
        Invoke(nameof(ReloadScene), resetDelay);
    }

    private void PerformCleanup()
    {
        if (hardResetAllData)
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            return;
        }

        PanelInputAutoSave targetPanel = panelToClean != null
            ? panelToClean
            : FindAnyObjectByType<PanelInputAutoSave>(FindObjectsInactive.Include);

        targetPanel?.ClearSavedData();
    }

    private void PlaySoundIfAvailable()
    {
        if (!playClickSound)
        {
            return;
        }

        audioSource ??= GetComponent<AudioSource>();
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }
    }

    private void ReloadScene()
    {
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(sceneIndex);
            return;
        }

        Debug.LogError("Current scene is not present in Build Settings.", this);
        if (button != null)
        {
            button.interactable = true;
        }
    }
}
