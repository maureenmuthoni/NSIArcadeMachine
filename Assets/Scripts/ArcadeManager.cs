using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using System.IO;
using LitJson;

[System.Serializable]
public class AutoUpdaterSettings
{
    public string latestReleaseURL = "https://api.github.com/repos/Mannilie/NSIArcadeMachine/releases/latest";
    public string proxyAddress = "proxy.det.nsw.edu.au";
    public int proxyPort = 80;
    public bool useProxy = false;
}

[System.Serializable]
public class ArcadeVersion
{
    public string currentVersion;
    public string latestVersion;
}

public class ArcadeManager : MonoBehaviour
{
    public string currentVersion = "0.1";
    public string arcadeInfoFileName = "arcade-info";
    [Header("Settings")]
    public Text version;
    public AutoUpdaterSettings autoUpdater;

    public static ArcadeVersion arcadeVersion = new ArcadeVersion();
    public static string dataPath;
    public static string persistentDataPath;
    public static ArcadeManager Instance;

    private string fullArcadeVersionPath;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        
        if (Application.isEditor)
            dataPath = Application.dataPath + "/../Arcade-Temp";
        else
            dataPath = Application.dataPath + "/..";

        persistentDataPath = Application.persistentDataPath;

        fullArcadeVersionPath = persistentDataPath + "/Arcade_Data/" + arcadeInfoFileName + ".json";
        if (File.Exists(fullArcadeVersionPath))
        {
            arcadeVersion = Load(fullArcadeVersionPath);
            version.text = arcadeVersion.currentVersion.ToString();
        }
        else
        {
            Save();
        }
    }
    
    // Updates the current version value to latest version
    public static void UpdateCurrentVersion()
    {
        arcadeVersion.currentVersion = arcadeVersion.latestVersion;
    }
    ArcadeVersion Load(string path)
    {
        JsonData data = JsonMapper.ToObject(File.ReadAllText(path));
        ArcadeVersion arcadeVersion = new ArcadeVersion();
        arcadeVersion.currentVersion = (string) data["currentVersion"];
        arcadeVersion.latestVersion = (string) data["latestVersion"];
        currentVersion = arcadeVersion.currentVersion;
        return arcadeVersion;
    }

    public static void Save()
    {
        string path = Instance.fullArcadeVersionPath;
        string json = JsonMapper.ToJson(arcadeVersion);
        FileInfo file = new FileInfo(path);
        file.Directory.Create(); // If the directory already exists, this method does nothing.
        File.WriteAllText(path, json);
    }
}
