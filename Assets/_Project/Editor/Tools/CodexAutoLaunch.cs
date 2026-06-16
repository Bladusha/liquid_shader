using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Project.Editor
{
    [InitializeOnLoad]
    public static class CodexAutoLaunch
    {
        private const string EnabledPrefKey = "Project.Editor.CodexAutoLaunch.Enabled";
        private const string LaunchedSessionKey = "Project.Editor.CodexAutoLaunch.LaunchedThisSession";
        private const string CodexPathPrefKey = "Project.Editor.CodexAutoLaunch.CodexCliPath";

        static CodexAutoLaunch()
        {
            if (!EditorPrefs.GetBool(EnabledPrefKey, true))
            {
                return;
            }

            if (SessionState.GetBool(LaunchedSessionKey, false))
            {
                return;
            }

            SessionState.SetBool(LaunchedSessionKey, true);
            EditorApplication.delayCall += LaunchCodexForProject;
        }

        [MenuItem("Tools/Codex/Launch Codex For This Project")]
        public static void LaunchCodexForProject()
        {
            var codexPath = ResolveCodexCliPath();
            if (string.IsNullOrEmpty(codexPath))
            {
                Debug.LogWarning("Codex auto-launch skipped: codex.exe was not found.");
                return;
            }

            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            if (string.IsNullOrEmpty(projectRoot))
            {
                Debug.LogWarning("Codex auto-launch skipped: Unity project root was not found.");
                return;
            }

            var launchDir = Path.Combine(projectRoot, "Library", "CodexAutoLaunch");
            Directory.CreateDirectory(launchDir);

            var commandPath = Path.Combine(launchDir, "start-codex.cmd");
            File.WriteAllText(commandPath,
                "@echo off\r\n" +
                "title Codex - " + Application.productName + "\r\n" +
                "cd /d \"" + projectRoot + "\"\r\n" +
                "\"" + codexPath + "\"\r\n");

            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c start \"Codex - " + Application.productName + "\" \"" + commandPath + "\"",
                CreateNoWindow = true,
                UseShellExecute = false,
                WorkingDirectory = projectRoot
            };

            Process.Start(startInfo);
            Debug.Log("Codex launched for project: " + projectRoot);
        }

        [MenuItem("Tools/Codex/Auto Launch On Project Open")]
        public static void ToggleAutoLaunch()
        {
            var enabled = !EditorPrefs.GetBool(EnabledPrefKey, true);
            EditorPrefs.SetBool(EnabledPrefKey, enabled);
            Debug.Log("Codex auto-launch is now " + (enabled ? "enabled" : "disabled") + ".");
        }

        [MenuItem("Tools/Codex/Auto Launch On Project Open", true)]
        public static bool ToggleAutoLaunchValidate()
        {
            Menu.SetChecked("Tools/Codex/Auto Launch On Project Open", EditorPrefs.GetBool(EnabledPrefKey, true));
            return true;
        }

        [MenuItem("Tools/Codex/Print Codex CLI Path")]
        public static void PrintCodexCliPath()
        {
            Debug.Log("Codex CLI path: " + (ResolveCodexCliPath() ?? "not found"));
        }

        private static string ResolveCodexCliPath()
        {
            var configuredPath = EditorPrefs.GetString(CodexPathPrefKey, string.Empty);
            if (File.Exists(configuredPath))
            {
                return configuredPath;
            }

            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var codexBinRoot = Path.Combine(localAppData, "OpenAI", "Codex", "bin");
            if (Directory.Exists(codexBinRoot))
            {
                var newestCodex = Directory
                    .EnumerateFiles(codexBinRoot, "codex.exe", SearchOption.AllDirectories)
                    .Select(path => new FileInfo(path))
                    .OrderByDescending(file => file.LastWriteTimeUtc)
                    .FirstOrDefault();

                if (newestCodex != null)
                {
                    return newestCodex.FullName;
                }
            }

            return null;
        }
    }
}
