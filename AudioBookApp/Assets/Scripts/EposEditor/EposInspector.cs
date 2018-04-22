using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
public class EposInspector : EposNodeEditor
{
    public Rect rect;

    private void OnSelectedItem()
    {
        Debug.Log("Selected Item");
    }
}
#endif
