using System.IO;
using UnityEditor;
using UnityEngine;

namespace Project.Editor
{
    [InitializeOnLoad]
    internal static class CodexPlayModeStarter
    {
        private const string FlagPath = "Library/CodexStartPlay.flag";

        static CodexPlayModeStarter()
        {
            EditorApplication.delayCall += TryStartPlayMode;
        }

        private static void TryStartPlayMode()
        {
            if (!File.Exists(FlagPath))
                return;

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += TryStartPlayMode;
                return;
            }

            try
            {
                File.Delete(FlagPath);
            }
            catch
            {
                // The flag is best-effort; failure should not block entering Play Mode.
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            Debug.Log("Codex starting Play Mode.");
            EditorApplication.isPlaying = true;
        }
    }
}
