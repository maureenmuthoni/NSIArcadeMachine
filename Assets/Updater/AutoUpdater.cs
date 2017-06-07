using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

using SimpleJSON;
using System.IO.Compression;

public class Version
{
    public string currentVersion;
    public string latestVersion;
}

public class AutoUpdater : MonoBehaviour
{
    public GameObject updatePanel;
    public string gitReleaseURL = "https://api.github.com/repos/Mannilie/NSIArcadeMachine/releases/latest";
    public string proxyAddress = "proxy.det.nsw.edu.au";
    public int proxyPort = 80;
    public bool useProxy = false;

    private string dataPath;
    private string persistentDataPath;
    private string tempLocation = "Downloads";
    private string arcadeUILocation = "ArcadeUI";
    private string versionFilePath;
    private string zipFilePath;
    private string exeToTerminate;
    private Version version = new Version();
    private Slider progressBar;
    private float progress = 0;
    private WebClient client;

    /*
    @echo off
curl --cacert "cacert.pem" https://api.github.com/repos/Mannilie/NSIArcadeMachine/releases/latest | jq -r ".assets[] | .browser_download_url" > latest_version.txt
set /p current_version =< current_version.txt
set /p latest_version =< latest_version.txt

echo %current_version%
echo %latest_version%

if %current_version%==%latest_version% (
	echo "Already latest version"
) else (
	curl --cacert "cacert.pem" -o NSIArcade.zip -LOk %latest_version%

    echo %latest_version% > current_version.txt
    echo "Downloaded latest version"

    PowerShell Expand-Archive -Path ".\NSIArcade.zip" -DestinationPath ".\bin"
)

CALL bin\NSIArcade.exe

pause
        */

    void Awake()
    {
        progressBar = updatePanel.GetComponentInChildren<Slider>();
    }

    // Use this for initialization
    void Start()
    {
        Screen.fullScreen = true;

        if (Application.isEditor)
            dataPath = Application.dataPath;
        else
            dataPath = Application.dataPath + "/..";
        persistentDataPath = Application.persistentDataPath;

        exeToTerminate = "NSIArcade.exe";
        arcadeUILocation = dataPath + "/" + arcadeUILocation;
        tempLocation = persistentDataPath + "/" + tempLocation;

        if (!Directory.Exists(tempLocation))
            Directory.CreateDirectory(tempLocation);

        versionFilePath = tempLocation + "/version.json";
        zipFilePath = tempLocation + "/NSIArcade.zip";
                
        if (File.Exists(versionFilePath))
        {
            version = Load(versionFilePath);
        }

        CheckForUpdates();

        // Check for any updates on the project
        if (version.currentVersion != version.latestVersion)
        {
            print("A new update has been found! Updating file.");
            DownloadUpdates();
        } else { 
            print("No new update required.");
        }
    }

    void Update()
    {
        progressBar.value = progress / 100f;
    }
    
    Version Load(string path)
    {
        string lel = File.ReadAllText(path);
        JSONNode jsonNode = JSONNode.Parse(lel);
        Version version = new Version();
        version.currentVersion = jsonNode["currentVersion"];
        version.latestVersion = jsonNode["latestVersion"];
        return version;
    }

    void Save(string path)
    {
        JSONNode jsonVersion = JSON.Parse(JsonUtility.ToJson(version));
        File.WriteAllText(path, jsonVersion.ToString());
    }

    public void CheckForUpdates()
    {
        // Perform Certificates for SSL
        ServicePointManager.ServerCertificateValidationCallback += MyRemoteCertificateValidationCallback;
        // Create a web client
        client = new WebClient();
        // Remove the callbacks for this client
        client.DownloadProgressChanged -= DownloadProgress;
        client.DownloadFileCompleted -= Completed; 
        // Add headers to allow download from anything
        client.Headers.Add("user-agent", "Anything");
        // Download git page to string (in json format)
        string latestJson = client.DownloadString(gitReleaseURL);
        // Parse string to Json Object
        JSONNode json = JSONNode.Parse(latestJson);
        // Store latest git release in string
        string latestVersion = json["assets"][0]["browser_download_url"].ToString();
        latestVersion = latestVersion.Substring(latestVersion.IndexOf("http"), latestVersion.Length - 2);
        // Set the latest version
        version.latestVersion = latestVersion;
    }

    public void DownloadUpdates()
    {
        // Download latest git release to folder
        DownloadFile(version.latestVersion, zipFilePath);
        version.currentVersion = version.latestVersion;
        Save(versionFilePath);
    }

    public void InstallUpdates()
    {
        ProcessStartInfo info = new ProcessStartInfo();
        info.FileName = arcadeUILocation + "/install.bat";
        info.Arguments = dataPath + " " + zipFilePath + " " + exeToTerminate;
        info.WindowStyle = ProcessWindowStyle.Maximized;
        Process p = new Process();
        p.StartInfo = info;
        p.Start();
    }

    public void DownloadFile(string address, string location)
    {
        WebClient client = new WebClient();
        // Add callbacks for this client
        client.DownloadProgressChanged += DownloadProgress;
        client.DownloadFileCompleted += Completed;
        client.DownloadFileAsync(new Uri(address), location);
        

        //WebClient client = new WebClient();
        //Uri Uri = new Uri(address);
        //
        ////_completed = false;
        //
        //client.DownloadDataCompleted += new DownloadDataCompletedEventHandler(Completed);
        //
        //client.DownloadFileAsync(Uri, "NSIArcade.zip");
    }

    private void Completed(object sender, AsyncCompletedEventArgs e)
    {
        if (e.Cancelled)
        {
            print("Download has been canceled.");
        }
        else
        {
            print("Download completed! Installing Updates...");
            InstallUpdates();
        }
    }

    private void DownloadProgress(object sender, DownloadProgressChangedEventArgs e)
    {
        UnityEngine.Debug.Log(e.ProgressPercentage + "%");
        progress = e.ProgressPercentage;
    }

    /*
    bool IsUpdateRequired()
    {
        tempLocation.Insert(0, Application.dataPath + "/");
        string updaterLocation = Application.dataPath + "/Updater";
        string proxy = proxyAddress + ":" + proxyPort.ToString();
        
        string cd = "cd " + updaterLocation;
        string setProxy = "set https_proxy=" + proxy;
        string runCurl = "curl --cacert \"cacert.pem\" https://api.github.com/repos/Mannilie/NSIArcadeMachine/releases/latest | jq -r \".assets[] | .browser_download_url\"";
        string runPause = updaterLocation + "/pause.bat";
        string arguments = cd + " && " + setProxy + " && " + runCurl + " && " + runPause;
        
        ProcessStartInfo cmd = new ProcessStartInfo();
        cmd.FileName ="cmd.exe";  // Specify exe name.
        cmd.Arguments = "\"" + Regex.Replace(arguments, @"(\\+)$", @"$1$1") + "\"";
        cmd.UseShellExecute = false;
        cmd.RedirectStandardOutput = true;
        cmd.RedirectStandardError = true;
        
        Process p = Process.Start(cmd);
        string result = p.StandardOutput.ReadToEnd();
        string error = p.StandardError.ReadToEnd();
        print(result);
        print(error);
        p.WaitForExit();

        return false;
    }
    */

    public bool MyRemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        bool isOk = true;
        // If there are errors in the certificate chain, look at each error to determine the cause.
        if (sslPolicyErrors != SslPolicyErrors.None)
        {
            for (int i = 0; i < chain.ChainStatus.Length; i++)
            {
                if (chain.ChainStatus[i].Status != X509ChainStatusFlags.RevocationStatusUnknown)
                {
                    chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                    chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                    chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                    bool chainIsValid = chain.Build((X509Certificate2)certificate);
                    if (!chainIsValid)
                    {
                        isOk = false;
                    }
                }
            }
        }
        return isOk;
    }
}
