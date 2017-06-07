using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

using UnityEditor;
using UnityEditor.Build;
public class ArcadeBuild : Editor, IPreprocessBuild
{
    public const string arcadeUIDir = "ArcadeUI";
    public int callbackOrder
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    public void OnPreprocessBuild(BuildTarget target, string path)
    {
        string buildLocation = path.Substring(0, path.LastIndexOf("/"));

        string sourcePath = Application.dataPath + "/" + arcadeUIDir;
        string destinationPath = buildLocation + "/" + arcadeUIDir;

        if (!Directory.Exists(destinationPath))
            Directory.CreateDirectory(destinationPath);

        //Copy all the files & Replaces any files with the same name
        foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            File.Copy(newPath, newPath.Replace(sourcePath, destinationPath), true);

        //File.Copy(Application.dataPath + "/" + arcadeUIDir, buildLocation + "/" + arcadeUIDir, true);
        //FileUtil.CopyFileOrDirectory(Application.dataPath + "/" + arcadeUIDir, buildLocation + "/" + arcadeUIDir);
        CleanTempFiles(buildLocation);
    }
    
    //[MenuItem("Arcade/Build (x86)")]
    //public static void BuildToArcadeMachinex86()
    //{
    //    BuildPlayerOptions buildOptions = new BuildPlayerOptions();
    //    buildOptions.target = BuildTarget.StandaloneWindows;
    //    buildOptions.locationPathName = EditorUtility.OpenFolderPanel("Select Build Location", "../" + Application.dataPath, "Builds");
    //    BuildPipeline.BuildPlayer(buildOptions);
    //}

    void CleanTempFiles(string path)
    {
        DirectoryInfo di = new DirectoryInfo(path);
        foreach (FileInfo file in di.GetFiles("*", SearchOption.AllDirectories))
        {
            if (file.Extension == ".meta")
            {
                file.Delete();
            }
        }
    }
}