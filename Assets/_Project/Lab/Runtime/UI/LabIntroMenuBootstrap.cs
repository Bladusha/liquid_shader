using EasyPeasyFirstPersonController;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

public class LabIntroMenuBootstrap : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject menuPrefab;

    [Header("Video")]
    [SerializeField] private string videoFileName = "LabIntro.mp4";
    [SerializeField] private VideoClip editorVideoClip;

    [Header("Player")]
    [SerializeField] private FirstPersonController playerController;

    [Header("Runtime")]
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private bool openOnSpawn = true;

    private LabIntroMenuController instance;

    private void Awake()
    {
        if (playerController == null)
        {
            playerController = FindAnyObjectByType<FirstPersonController>();
        }
    }

    private void Start()
    {
        if (spawnOnStart)
        {
            SpawnMenu();
        }
    }

    public void SpawnMenu()
    {
        if (instance != null || menuPrefab == null)
        {
            if (menuPrefab == null)
            {
                Debug.LogError("LabIntroMenuBootstrap: menuPrefab is not assigned.", this);
            }

            return;
        }

        GameObject spawned = Instantiate(menuPrefab);
        spawned.SetActive(false);

        instance = spawned.GetComponent<LabIntroMenuController>();
        if (instance == null)
        {
            Debug.LogError("LabIntroMenuBootstrap: prefab does not contain LabIntroMenuController.", this);
            Destroy(spawned);
            return;
        }

        instance.SetFirstPersonController(playerController);
        instance.SetVideoFileName(ResolveVideoFileName(videoFileName));
        instance.SetEditorVideoClip(editorVideoClip);

        spawned.SetActive(true);

        if (openOnSpawn)
        {
            instance.OpenMenu();
        }
    }

    public void DespawnMenu()
    {
        if (instance == null)
        {
            return;
        }

        instance.CloseMenu();
        Destroy(instance.gameObject);
        instance = null;
    }

    private static string ResolveVideoFileName(string source)
    {
        string candidate = Path.GetFileName((source ?? string.Empty).Trim());
        if (string.IsNullOrWhiteSpace(candidate))
        {
            candidate = "LabIntro.mp4";
        }

        string candidatePath = Path.Combine(Application.streamingAssetsPath, candidate).Replace('\\', '/');
        if (File.Exists(candidatePath))
        {
            return candidate;
        }

        return "LabIntro.mp4";
    }
}
