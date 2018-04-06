using System;
using UnityEngine;

public enum ConnectionPointType { In, Out }

public class EposConnectionPoint
{
    public Rect rect;

    public ConnectionPointType type;

    public EposNode node;

    public GUIStyle style;

    public Action<EposConnectionPoint> OnClickConnectionPoint;

    public int nPoints;
    public int ptIndex;

    public EposConnectionPoint(EposNode node, ConnectionPointType type, GUIStyle style, Action<EposConnectionPoint> OnClickConnectionPoint, int nPoints = 1, int ptIndex = 0)
    {
        this.node = node;
        this.type = type;
        this.style = style;
        this.OnClickConnectionPoint = OnClickConnectionPoint;
        rect = new Rect(0, 0, 10f, 20f);
        //this.overrideY = overrideY;
        this.nPoints = nPoints;
        this.ptIndex = ptIndex;
    }

    public void Draw()
    {
        if (nPoints > 1)
        {
            rect.y = node.rect.y + (((node.rect.height / (nPoints+1)) * (ptIndex+1)) - rect.height * 0.5f);
            //rect.y = node.rect.y + (node.rect.height * (1 / nPoints + 1)) - rect.height * (1 / nPoints + 1);
        }
        else
            rect.y = node.rect.y + (node.rect.height * 0.5f) - rect.height * 0.5f;

        //rect.y += overrideY;

        switch (type)
        {
            case ConnectionPointType.In:
                rect.x = node.rect.x - rect.width + 8f;
                break;

            case ConnectionPointType.Out:
                rect.x = node.rect.x + node.rect.width - 8f;
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
                rect.x = node.rect.x - rect.width + 8f;
                break;

            case ConnectionPointType.Out:
                rect.x = node.rect.x + node.rect.width - 8f;
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
    public EposNode GetNodeConnected()
    {
        return node;
    }
}
