using UnityEngine;

namespace LiquidShader.RuntimeData
{
    public sealed class GameStateMonitor : MonoBehaviour
    {
        [SerializeField] private bool showOverlay = true;
        [SerializeField] private KeyCode toggleKey = KeyCode.F9;

        private GameStateValidationResult lastValidation;
        private GUIStyle labelStyle;

        private void OnEnable()
        {
            GameStateStore.Instance.ValidationChanged += OnValidationChanged;
            lastValidation = GameStateValidator.Validate(GameStateStore.Instance.State);
        }

        private void OnDisable()
        {
            if (GameStateStore.HasInstance)
                GameStateStore.Instance.ValidationChanged -= OnValidationChanged;
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
                showOverlay = !showOverlay;
        }

        private void OnGUI()
        {
            if (!showOverlay)
                return;

            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    wordWrap = true
                };
            }

            GameStateStore store = GameStateStore.Instance;
            GameState state = store.State;

            GUILayout.BeginArea(new Rect(12f, 12f, 460f, 260f), GUI.skin.box);
            GUILayout.Label("Game State Monitor", labelStyle);
            GUILayout.Label($"Scene: {state.activeScene}", labelStyle);
            GUILayout.Label($"Player: {state.playerPosition}", labelStyle);
            GUILayout.Label($"Values: {state.values.Count}", labelStyle);
            GUILayout.Label($"Save: {store.SavePath}", labelStyle);
            GUILayout.Space(6f);
            GUILayout.Label(lastValidation != null ? lastValidation.ToString() : "Validation not ready", labelStyle);
            GUILayout.EndArea();
        }

        private void OnValidationChanged(GameStateValidationResult validation)
        {
            lastValidation = validation;
        }
    }
}
