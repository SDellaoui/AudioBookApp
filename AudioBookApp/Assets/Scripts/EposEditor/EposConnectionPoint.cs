using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum ConnectionPointType { In, Out }

public class EposConnectionPoint
{
    public Rect rect;

    public ConnectionPointType type;

    public EposNode node;
    public EposNodeData nodeData;
    public GUIStyle style;

    public Action<EposConnectionPoint> OnClickConnectionPoint;

    public int nPoints;
    public int ptIndex;

    #if UNITY_EDITOR
    public EposConnectionPoint(EposNodeData nodeData, ConnectionPointType type, Action<EposConnectionPoint> OnClickConnectionPoint, int nPoints = 1, int ptIndex = 0)
    {
        this.nodeData = nodeData;
        this.type = type;
        this.OnClickConnectionPoint = OnClickConnectionPoint;
        rect = new Rect(0, 0, 10f, 20f);
        this.nPoints = nPoints;
        this.ptIndex = ptIndex;

        this.style = new GUIStyle();
        if (type == ConnectionPointType.In)
        {
            this.style.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left.png") as Texture2D;
            this.style.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left on.png") as Texture2D;
        }
        else
        {
            this.style.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right.png") as Texture2D;
            this.style.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right on.png") as Texture2D;
        }
        this.style.border = new RectOffset(4, 4, 12, 12);
    }
    #endif
    public EposConnectionPoint(EposNodeData nodeData, ConnectionPointType type, int nPoints = 1, int ptIndex = 0)
    {
        this.nodeData = nodeData;
        this.type = type;
        this.nPoints = nPoints;
        this.ptIndex = ptIndex;
    }

    #if UNITY_EDITOR
    public void Draw()
    {
        if (nPoints > 1)
        {
            rect.y = this.nodeData.rect.y + (((this.nodeData.rect.height / (nPoints+1)) * (ptIndex+1)) - rect.height * 0.5f);
            //rect.y = node.rect.y + (node.rect.height * (1 / nPoints + 1)) - rect.height * (1 / nPoints + 1);
        }
        else
            rect.y = this.nodeData.rect.y + (this.nodeData.rect.height * 0.5f) - rect.height * 0.5f;

        //rect.y += overrideY;

        switch (type)
        {
            case ConnectionPointType.In:
                rect.x = this.nodeData.rect.x - rect.width + 8f;
                break;

            case ConnectionPointType.Out:
                rect.x = this.nodeData.rect.x + this.nodeData.rect.width - 8f;
                break;
        }

        if (GUI.Button(rect, "", style))
        {
            if (OnClickConnectionPoint != null)
            {
                OnClickConnectionPoint(this);
            }
        }
    }
    public void SetY(float y)
    {
        //overrideY = y;
    }
    public void Draw(EposNodeType nodeType)
    {
        switch (type)
        {
            case ConnectionPointType.In:
                rect.x = this.nodeData.rect.x - rect.width + 8f;
                break;

            case ConnectionPointType.Out:
                rect.x = this.nodeData.rect.x + this.nodeData.rect.width - 8f;
                break;
        }

        if (GUI.Button(rect, "", style))
        {
            if (OnClickConnectionPoint != null)
            {
                OnClickConnectionPoint(this);
            }
        }
    }
    #endif
    public EposNodeData GetNodeConnected()
    {
        return nodeData;
    }
}
