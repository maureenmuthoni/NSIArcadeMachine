using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class UIUpdatePanel : MonoBehaviour
{
    public Text title;
    public Slider slider;
    [Header("UpdateInfo")]
    public Text totalMB;
    public Text progressMB;
    public GameObject downloadInfoPanel;
    public float deactivateDelay = 2.0f;

    [HideInInspector] public UpdateInfo updateInfo;
   
    // Use this for initialization
    void Start()
    {

    }

    void OnEnable()
    {
        downloadInfoPanel.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        ApplyUI();
    }
    
    void ApplyUI()
    {
        switch (updateInfo.updateState)
        {
            case UpdateState.CHECKING_UPDATES:
                title.text = "Checking for Updates...";
                break;
            case UpdateState.UPDATE_FOUND:
                title.text = "Update Found!";
                break;
            case UpdateState.DOWNLOADING_UPDATES:
                title.text = "Downloading Update...";
                float totalBytes = updateInfo.totalBytesRecieved / 1024 / 1024;
                float bytes = updateInfo.bytesRecieved / 1024 / 1024;
                totalMB.text = string.Format("{0:0.0}", totalBytes) + "MB";
                progressMB.text = string.Format("{0:0.0}", bytes);
                downloadInfoPanel.gameObject.SetActive(true);
                
                slider.value = updateInfo.downloadProgress / 100f;
                break;
            case UpdateState.UPDATE_FINISHED:
                title.text = "Update Complete, now installing...";
                break;
            case UpdateState.UPDATE_SKIPPED:
                title.text = "Update not required.";
                StartCoroutine(Deactivate());
                break;
            case UpdateState.UPDATE_CANCELLED:
                downloadInfoPanel.gameObject.SetActive(false);
                break;
            default:
                break;
        }
    }

    IEnumerator Deactivate()
    {
        yield return new WaitForSeconds(deactivateDelay);
        gameObject.SetActive(false);
    }
}
