using UnityEngine;

namespace LiquidShader.RuntimeData
{
    public static class GameStateBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            _ = GameStateStore.Instance;
        }
    }
}
