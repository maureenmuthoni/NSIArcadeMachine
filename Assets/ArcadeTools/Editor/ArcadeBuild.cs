using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

using UnityEditor;
using UnityEditor.Build;
public class ArcadeBuild : Editor, IPreprocessBuild
{
    public const string arcadeDataFileName = "Arcade_Data";
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

        string sourcePath = Application.dataPath + "/" + arcadeDataFileName;
        string destinationPath = buildLocation + "/" + arcadeDataFileName;

        if (!Directory.Exists(destinationPath))
            Directory.CreateDirectory(destinationPath);

        //Copy all the files & Replaces any files with the same name
        DirectoryCopy(sourcePath, destinationPath, true);

        //File.Copy(Application.dataPath + "/" + arcadeUIDir, buildLocation + "/" + arcadeUIDir, true);
        //FileUtil.CopyFileOrDirectory(Application.dataPath + "/" + arcadeUIDir, buildLocation + "/" + arcadeUIDir);
        CleanTempFiles(buildLocation);
    }

    private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
    {
        // Get the subdirectories for the specified directory.
        DirectoryInfo dir = new DirectoryInfo(sourceDirName);

        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException(
                "Source directory does not exist or could not be found: "
                + sourceDirName);
        }

        DirectoryInfo[] dirs = dir.GetDirectories();
        // If the destination directory doesn't exist, create it.
        if (!Directory.Exists(destDirName))
        {
            Directory.CreateDirectory(destDirName);
        }

        // Get the files in the directory and copy them to the new location.
        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            string temppath = Path.Combine(destDirName, file.Name);
            file.CopyTo(temppath, true);
        }

        // If copying subdirectories, copy them and their contents to new location.
        if (copySubDirs)
        {
            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, temppath, copySubDirs);
            }
        }
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