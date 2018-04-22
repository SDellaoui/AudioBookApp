using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class EposConnection
{
    public EposConnectionPoint inPoint;
    public EposConnectionPoint outPoint;
    public Action<EposConnection> OnClickRemoveConnection;

    public EposConnection(EposConnectionPoint inPoint, EposConnectionPoint outPoint, Action<EposConnection> OnClickRemoveConnection)
    {
        this.inPoint = inPoint;
        this.outPoint = outPoint;
        this.OnClickRemoveConnection = OnClickRemoveConnection;
    }
    #if UNITY_EDITOR
    public void Draw()
    {
        Handles.DrawBezier(
            inPoint.rect.center,
            outPoint.rect.center,
            inPoint.rect.center + Vector2.left * 50f,
            outPoint.rect.center - Vector2.left * 50f,
            Color.white,
            null,
            2f
        );

        if (Handles.Button((inPoint.rect.center + outPoint.rect.center) * 0.5f, Quaternion.identity, 4, 8, Handles.RectangleCap))
        {
            if (OnClickRemoveConnection != null)
            {
                OnClickRemoveConnection(this);
            }
        }
    }
    #endif
}