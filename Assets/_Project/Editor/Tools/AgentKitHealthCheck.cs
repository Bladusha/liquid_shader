using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace Project.Editor
{
    public static class AgentKitHealthCheck
    {
        private static ListRequest packageListRequest;

        [MenuItem("Tools/Unity AI Agent Kit/Run Health Check")]
        public static void Run()
        {
            Debug.Log("Unity AI Agent Kit health check started.");
            packageListRequest = Client.List(true);
            EditorApplication.update -= CompleteWhenReady;
            EditorApplication.update += CompleteWhenReady;
        }

        private static void CompleteWhenReady()
        {
            if (packageListRequest == null || !packageListRequest.IsCompleted)
            {
                return;
            }

            EditorApplication.update -= CompleteWhenReady;
            var requiredPackages = new[]
            {
                "com.unity.inputsystem",
                "com.unity.ml-agents",
                "com.unity.sentis"
            };

            if (packageListRequest.Status == StatusCode.Success)
            {
                foreach (var packageName in requiredPackages)
                {
                    var installed = packageListRequest.Result.Any(package => package.name == packageName);
                    Debug.Log(installed
                        ? $"[OK] Package installed: {packageName}"
                        : $"[Missing] Package not installed: {packageName}");
                }
            }
            else
            {
                Debug.LogWarning($"Package list failed: {packageListRequest.Error?.message}");
            }

            packageListRequest = null;
            Debug.Log("Unity AI Agent Kit health check finished.");
        }
    }
}
