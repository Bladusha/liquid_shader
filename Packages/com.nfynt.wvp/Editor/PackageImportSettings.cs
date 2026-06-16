using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

namespace Nfynt.Editor
{
    [InitializeOnLoad]
    public class PackageImportSettings
    {
        static PackageImportSettings()
        {
            RefreshPackage();
        }

        public static void RefreshPackage()
        {
            if (Application.isPlaying) return;

            //Copy video resource to streaming assets for test
            string srcDir = "Packages/com.nfynt.wvp/Samples~/Resource";
            string destDir = Application.streamingAssetsPath;
            DirectoryCopy(srcDir, destDir, false);

            //Always include Nfynt/TextureBlit shader
            //AddAlwaysIncludedShader("");
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                //Debug.Log("Source directory does not exist or could not be found: " + sourceDirName);
                return;
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                if (file.Extension == ".meta")
                    continue;

                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, true);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }

		public static void AddAlwaysIncludedShader(string shaderName)
		{
			Shader shader = Shader.Find(shaderName);
			if (shader == null)
				return;

			GraphicsSettings gSettings = AssetDatabase.LoadAssetAtPath<GraphicsSettings>("ProjectSettings/GraphicsSettings.asset");
			SerializedObject so = new SerializedObject(gSettings);
			SerializedProperty arrayProp = so.FindProperty("m_AlwaysIncludedShaders");
			bool hasShader = false;
			for (int i = 0; i < arrayProp.arraySize; ++i)
			{
				SerializedProperty arrayElem = arrayProp.GetArrayElementAtIndex(i);
				if (shader == arrayElem.objectReferenceValue)
				{
					hasShader = true;
					break;
				}
			}

			if (!hasShader)
			{
				int arrayIndex = arrayProp.arraySize;
				arrayProp.InsertArrayElementAtIndex(arrayIndex);
				SerializedProperty arrayElem = arrayProp.GetArrayElementAtIndex(arrayIndex);
				arrayElem.objectReferenceValue = shader;

				so.ApplyModifiedProperties();

				AssetDatabase.SaveAssets();
			}
		}
	}
}