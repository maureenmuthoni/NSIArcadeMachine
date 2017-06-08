using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFishEyeView : MonoBehaviour
{
    public Vector2 minSize = Vector2.zero;
    public float downScale = 0.35f;
    public float length = 1f;

    private Vector2 maxSize;
    private Vector2 halfScreen;
    private RectTransform rect;

    // Use this for initialization
    void Start()
    {
        rect = GetComponent<RectTransform>();
        maxSize = rect.sizeDelta;
        halfScreen = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);     
    }

    // Update is called once per frame
    void Update()
    {
        float distX = Mathf.Abs((rect.position.x - halfScreen.x) * length);
        //distX = -Mathf.Sqrt(distX * minSize.x) + maxSize.x;
        distX = maxSize.x - distX * downScale;
        distX = Mathf.Clamp(distX, minSize.x, maxSize.x);
        Vector2 sizeDelta = rect.sizeDelta;
        sizeDelta.x = distX * downScale;
        sizeDelta.y = distX * downScale;
        rect.sizeDelta = sizeDelta;
    }
}
