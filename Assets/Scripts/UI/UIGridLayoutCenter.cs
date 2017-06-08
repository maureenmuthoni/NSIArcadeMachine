using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class UIGridLayoutCenter : MonoBehaviour
{
    private GridLayoutGroup gridLayoutGroup;
    // Use this for initialization
    void Awake()
    {
        gridLayoutGroup = GetComponent<GridLayoutGroup>();
        Vector2 halfScreen = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector2 halfCellSize = gridLayoutGroup.cellSize * 0.5f;
        int offsetLeft = (int)(halfScreen.x - halfCellSize.x);
        int offsetRight = (int)(halfScreen.x - halfCellSize.x);
        gridLayoutGroup.padding.left = offsetLeft;
        gridLayoutGroup.padding.right = offsetRight;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
