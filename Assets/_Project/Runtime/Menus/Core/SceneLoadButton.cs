using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SceneLoadButton : MonoBehaviour
{
    [SerializeField] private bool loadAsync = true;
    [SerializeField] private string sceneName = "Game";

    private Button button;
    private string cachedSceneName;
    private bool cachedSceneExists;

    private void Awake()
    {
        button = GetComponent<Button>();
        RefreshSceneCache();
    }

    private void OnEnable()
    {
        if (button != null)
        {
            button.onClick.AddListener(LoadTargetScene);
        }
    }

    private void OnDisable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(LoadTargetScene);
        }
    }

    public void LoadTargetScene()
    {
        string targetScene = sceneName?.Trim();
        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogWarning("Target scene is empty.", this);
            return;
        }

        if (!IsSceneAvailable(targetScene))
        {
            Debug.LogWarning($"Scene '{targetScene}' is not enabled in Build Settings.", this);
            return;
        }

        Time.timeScale = 1f;

        if (loadAsync)
        {
            SceneManager.LoadSceneAsync(targetScene, LoadSceneMode.Single);
            return;
        }

        SceneManager.LoadScene(targetScene, LoadSceneMode.Single);
    }

    public void SetSceneName(string newSceneName)
    {
        sceneName = newSceneName;
        RefreshSceneCache();
    }

    private bool IsSceneAvailable(string targetScene)
    {
        if (!string.Equals(cachedSceneName, targetScene, StringComparison.Ordinal))
        {
            cachedSceneName = targetScene;
            cachedSceneExists = SceneExistsInBuild(targetScene);
        }

        return cachedSceneExists;
    }

    private void RefreshSceneCache()
    {
        cachedSceneName = sceneName?.Trim();
        cachedSceneExists = !string.IsNullOrEmpty(cachedSceneName) && SceneExistsInBuild(cachedSceneName);
    }

    private static bool SceneExistsInBuild(string targetScene)
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

