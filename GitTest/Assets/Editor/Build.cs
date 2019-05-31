using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Build : Editor
{

    [MenuItem ("Tool/Build")]
    public static void ToBuild()
    {
        Debug.Log("Start to build ......jajaja");
        
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.Android);

        List<string> levels = new List<string>();
        foreach(
        EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if(scene.enabled)
            {
                levels.Add(scene.path);
            }
        }

        string apkName = "./Test.apk";
        BuildPipeline.BuildPlayer(levels.ToArray(), apkName, BuildTarget.Android, BuildOptions.None);
        AssetDatabase.Refresh();

        Debug.Log("Build Done");
    }
}
