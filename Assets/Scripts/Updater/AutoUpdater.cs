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

using LitJson;
using System.IO.Compression;

public enum UpdateState
{
    CHECKING_UPDATES,
    UPDATE_FOUND,
    DOWNLOADING_UPDATES,
    UPDATE_FINISHED,
    UPDATE_CANCELLED,
    UPDATE_SKIPPED
}

public class UpdateInfo
{
    public float downloadProgress;
    public float totalBytesRecieved;
    public float bytesRecieved;
    public UpdateState updateState;
}

public class AutoUpdater : MonoBehaviour
{
    public UIUpdatePanel updatePanel;
    private AutoUpdaterSettings settings;
    private string dataPath;
    private string persistentDataPath;
    private string tempLocation = "Downloads";
    private string arcadeDataLocation = "Arcade_Data";
    private string zipFilePath;
    private string exeToTerminate;
    private string exeToRun;
    private UpdateInfo updateInfo = new UpdateInfo();
    private WebClient checkForUpdateClient;
    private WebClient downloadClient;
    private ArcadeManager arcadeManager;
    
    void Awake()
    {
        SetupPaths();

        arcadeManager = FindObjectOfType<ArcadeManager>();
        if(arcadeManager == null)
        {
            UnityEngine.Debug.LogError("There is currently no 'ArcadeManager' in the scene");
            UnityEngine.Debug.Break();
        }

        settings = arcadeManager.autoUpdater;
    }

    void SetupPaths()
    {
        dataPath = ArcadeManager.dataPath;
        persistentDataPath = ArcadeManager.persistentDataPath;
        exeToTerminate = "NSIArcade.exe";
        exeToRun = dataPath + "/" + "NSIArcade.exe";
        arcadeDataLocation = dataPath + "/" + arcadeDataLocation;
        tempLocation = persistentDataPath + "/" + tempLocation;
        zipFilePath = tempLocation + "/NSIArcade.zip";

        if (!Directory.Exists(tempLocation))
            Directory.CreateDirectory(tempLocation);
    }
    
    // Use this for initialization
    void OnEnable()
    {
        SetupClients();
        CheckForUpdates();
        // If there is an update panel set
        if (updatePanel != null)
        {
            // Set the update info
            updatePanel.updateInfo = updateInfo;
        }
    }

    void SetupClients()
    {
        // Create a web client
        checkForUpdateClient = new WebClient();
        // Remove the callbacks for this client
        checkForUpdateClient.DownloadStringCompleted += CheckForUpdateClient_DownloadStringCompleted;
        // Add headers to allow download from anything
        checkForUpdateClient.Headers.Add("user-agent", "Anything");

        downloadClient = new WebClient();
        // Add callbacks for this client
        downloadClient.DownloadProgressChanged += DownloadProgress;
        downloadClient.DownloadFileCompleted += Completed;
        // Perform Certificates for SSL
        ServicePointManager.ServerCertificateValidationCallback += MyRemoteCertificateValidationCallback;
    }

    void OnDestroy()
    {
        Cancel();    
    }

    void OnDisable()
    {
        Cancel();
    }

    void Cancel()
    {
        checkForUpdateClient.CancelAsync();
        checkForUpdateClient.DownloadStringCompleted -= CheckForUpdateClient_DownloadStringCompleted;
        checkForUpdateClient.Headers.Clear();

        downloadClient.CancelAsync();
        downloadClient.DownloadProgressChanged -= DownloadProgress;
        downloadClient.DownloadFileCompleted -= Completed;
    }
    
    public void CheckForUpdates()
    {
        updateInfo.updateState = UpdateState.CHECKING_UPDATES;
        // Download git page to string (in json format)
        checkForUpdateClient.DownloadStringAsync(new Uri(settings.latestReleaseURL));
    }
    public void DownloadUpdates(string downloadURL)
    {
        // Download latest git release to folder
        DownloadFile(downloadURL, zipFilePath);
    }
    public void InstallUpdates()
    {
        print(arcadeDataLocation + "/install.bat");
        ProcessStartInfo info = new ProcessStartInfo();
        info.FileName = arcadeDataLocation + "/install.bat";
        info.Arguments = dataPath + " " + zipFilePath + " " + exeToTerminate + " " + exeToRun;
        info.WindowStyle = ProcessWindowStyle.Maximized;
        Process p = new Process();
        p.StartInfo = info;
        p.Start();
    }
    public void DownloadFile(string address, string location)
    {
        downloadClient.DownloadFileAsync(new Uri(address), location);
    }

    private void CheckForUpdateClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
    {
        // Download git page to string (in json format)
        string latestJson = e.Result;
        // Parse string to Json Object
        JsonData json = JsonMapper.ToObject(latestJson);
        // Store latest git release in string
        string downloadURL = json["assets"][0]["browser_download_url"].ToString();
        string version = json["tag_name"].ToString();
        // Set the latest version
        ArcadeManager.arcadeVersion.latestVersion = version;
        // Check for any updates on the project
        if (ArcadeManager.arcadeVersion.currentVersion != ArcadeManager.arcadeVersion.latestVersion)
        {
            updateInfo.updateState = UpdateState.UPDATE_FOUND;
            print("A new update has been found! Updating file.");
            DownloadUpdates(downloadURL);
        }
        else
        {
            print("No new update required.");
            updateInfo.updateState = UpdateState.UPDATE_SKIPPED;
        }
    }

    private void Completed(object sender, AsyncCompletedEventArgs e)
    {
        if (e.Cancelled)
        {
            updateInfo.updateState = UpdateState.UPDATE_CANCELLED;
            print("Download has been canceled.");
            Directory.Delete(tempLocation, true);
        }
        else
        {
            updateInfo.updateState = UpdateState.UPDATE_FINISHED;
            ArcadeManager.UpdateCurrentVersion();
            ArcadeManager.Save();
            print("Download completed! Installing Updates...");
            InstallUpdates();
        }
    }

    private void DownloadProgress(object sender, DownloadProgressChangedEventArgs e)
    {
        UnityEngine.Debug.Log(e.ProgressPercentage + "%");
        updateInfo.updateState = UpdateState.DOWNLOADING_UPDATES;
        updateInfo.downloadProgress = e.ProgressPercentage;
        updateInfo.totalBytesRecieved = e.TotalBytesToReceive;
        updateInfo.bytesRecieved = e.BytesReceived;
    }
    
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
