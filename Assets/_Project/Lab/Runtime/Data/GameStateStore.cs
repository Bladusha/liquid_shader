using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LiquidShader.RuntimeData
{
    public sealed class GameStateStore : MonoBehaviour
    {
        private const string SaveKey = "liquid_shader_game_state";
        private static GameStateStore instance;

        [SerializeField] private bool loadOnAwake = true;
        [SerializeField] private bool saveOnSceneChange = true;
        [SerializeField] private bool validateInRealtime = true;
        [SerializeField] private float validationInterval = 0.25f;

        private float nextValidationTime;

        public static GameStateStore Instance
        {
            get
            {
                if (instance != null)
                    return instance;

                GameObject storeObject = new GameObject(nameof(GameStateStore));
                instance = storeObject.AddComponent<GameStateStore>();
                DontDestroyOnLoad(storeObject);
                return instance;
            }
        }

        public static bool HasInstance => instance != null;

        public GameState State { get; private set; } = new GameState();
        public string SavePath => $"PlayerPrefs:{SaveKey}";

        public event Action<GameState> StateChanged;
        public event Action<GameStateValidationResult> ValidationChanged;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            if (loadOnAwake)
                Load();

            UpdateSceneName(SceneManager.GetActiveScene());
        }

        private void OnEnable()
        {
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        private void OnDisable()
        {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        }

        private void Update()
        {
            if (!validateInRealtime || Time.unscaledTime < nextValidationTime)
                return;

            nextValidationTime = Time.unscaledTime + Mathf.Max(0.02f, validationInterval);
            ValidationChanged?.Invoke(GameStateValidator.Validate(State));
        }

        public void SetValue(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("State key cannot be empty.", nameof(key));

            State.Set(key, value);
            StateChanged?.Invoke(State);
        }

        public string GetValue(string key, string fallback = "")
        {
            return State.Get(key, fallback);
        }

        public void SetPlayerPosition(Vector3 position)
        {
            State.playerPosition = position;
            StateChanged?.Invoke(State);
        }

        public void Load()
        {
            if (!PlayerPrefs.HasKey(SaveKey))
            {
                State = new GameState();
                StateChanged?.Invoke(State);
                ValidationChanged?.Invoke(GameStateValidator.Validate(State));
                return;
            }

            string json = PlayerPrefs.GetString(SaveKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json))
            {
                State = new GameState();
                StateChanged?.Invoke(State);
                ValidationChanged?.Invoke(GameStateValidator.Validate(State));
                return;
            }

            State = JsonUtility.FromJson<GameState>(json) ?? new GameState();
            StateChanged?.Invoke(State);
            ValidationChanged?.Invoke(GameStateValidator.Validate(State));
        }

        public void Save()
        {
            string json = JsonUtility.ToJson(State, true);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }

        public void ResetState(bool deleteSaveFile)
        {
            State = new GameState();
            UpdateSceneName(SceneManager.GetActiveScene());

            if (deleteSaveFile)
            {
                PlayerPrefs.DeleteKey(SaveKey);
                PlayerPrefs.Save();
            }

            StateChanged?.Invoke(State);
            ValidationChanged?.Invoke(GameStateValidator.Validate(State));
        }

        private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
        {
            UpdateSceneName(newScene);

            if (saveOnSceneChange)
                Save();
        }

        private void UpdateSceneName(Scene scene)
        {
            State.activeScene = scene.name;
            StateChanged?.Invoke(State);
        }
    }
}
