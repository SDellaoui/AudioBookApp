using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EposInspector : EposNodeEditor
{
    public Rect rect;

    private void OnSelectedItem()
    {
        Debug.Log("Selected Item");
    }
}
