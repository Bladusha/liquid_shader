using UnityEngine;

namespace LiquidShader.RuntimeData
{
    public sealed class SceneStateBridge : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private bool loadPlayerPositionOnStart = true;
        [SerializeField] private bool savePlayerPositionInRealtime = true;
        [SerializeField] private float saveInterval = 0.2f;

        private float nextSaveTime;

        private void Start()
        {
            if (player == null)
                player = transform;

            if (loadPlayerPositionOnStart)
                player.position = GameStateStore.Instance.State.playerPosition;
        }

        private void Update()
        {
            if (!savePlayerPositionInRealtime || Time.unscaledTime < nextSaveTime)
                return;

            nextSaveTime = Time.unscaledTime + Mathf.Max(0.02f, saveInterval);
            GameStateStore.Instance.SetPlayerPosition(player.position);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
                GameStateStore.Instance.Save();
        }

        private void OnApplicationQuit()
        {
            GameStateStore.Instance.Save();
        }
    }
}
