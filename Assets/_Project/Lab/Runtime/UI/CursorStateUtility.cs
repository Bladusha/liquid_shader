using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public static class CursorStateUtility
{
    private static CursorStateApplier applier;
    private static bool isShuttingDown;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        isShuttingDown = false;
        EnsureApplier();
    }

    public static void Apply(CursorLockMode lockState, bool visible, int reapplyFrames = 3)
    {
        if (isShuttingDown)
        {
            Cursor.lockState = lockState;
            Cursor.visible = visible;
            return;
        }

        EnsureApplier();
        if (applier == null)
        {
            Cursor.lockState = lockState;
            Cursor.visible = visible;
            return;
        }

        applier.Apply(lockState, visible, reapplyFrames);
    }

    private static void EnsureApplier()
    {
        if (isShuttingDown)
        {
            return;
        }

        if (applier != null)
        {
            return;
        }

        applier = Object.FindAnyObjectByType<CursorStateApplier>();
        if (applier != null)
        {
            return;
        }

        GameObject root = new GameObject("CursorStateUtility");
        Object.DontDestroyOnLoad(root);
        applier = root.AddComponent<CursorStateApplier>();
    }

    private sealed class CursorStateApplier : MonoBehaviour
    {
        private Coroutine applyRoutine;
        private CursorLockMode targetLockState;
        private bool targetVisible;

        public void Apply(CursorLockMode lockState, bool visible, int reapplyFrames)
        {
            targetLockState = lockState;
            targetVisible = visible;
            SetCursorState();

            if (applyRoutine != null)
            {
                StopCoroutine(applyRoutine);
            }

            applyRoutine = StartCoroutine(ApplyForFrames(Mathf.Max(1, reapplyFrames)));
        }

        private IEnumerator ApplyForFrames(int frameCount)
        {
            for (int i = 0; i < frameCount; i++)
            {
                yield return null;
                SetCursorState();
            }

            applyRoutine = null;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                SetCursorState();
            }
        }

        private void OnDestroy()
        {
            if (applier == this)
            {
                applier = null;
            }

            isShuttingDown = true;
        }

        private void SetCursorState()
        {
            Cursor.lockState = targetLockState;
            Cursor.visible = targetVisible;

            if (targetLockState == CursorLockMode.Locked)
            {
                CenterMousePosition();
                FocusGameViewInEditor();
            }
        }

        private static void CenterMousePosition()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null)
            {
                return;
            }

            mouse.WarpCursorPosition(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
            InputState.Change(mouse.delta, Vector2.zero);
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private static void FocusGameViewInEditor()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += FocusGameViewDelayed;
#endif
        }

#if UNITY_EDITOR
        private static void FocusGameViewDelayed()
        {
            System.Type gameViewType = System.Type.GetType("UnityEditor.GameView,UnityEditor");
            UnityEditor.EditorWindow window = gameViewType != null
                ? UnityEditor.EditorWindow.GetWindow(gameViewType)
                : null;
            window?.Focus();
        }
#endif
    }
}
