using System;
using UnityEngine;

public enum ConnectionPointType { In, Out }

public class EposConnectionPoint
{
    public Rect rect;

    public ConnectionPointType type;

    public EposNode node;
    public EposBeginEndNode beginEndNode;

    public GUIStyle style;

    public Action<EposConnectionPoint> OnClickConnectionPoint;

    public EposConnectionPoint(EposNode node, ConnectionPointType type, GUIStyle style, Action<EposConnectionPoint> OnClickConnectionPoint)
    {
        this.node = node;
        this.type = type;
        this.style = style;
        this.OnClickConnectionPoint = OnClickConnectionPoint;
        rect = new Rect(0, 0, 10f, 20f);
    }

    public EposConnectionPoint(EposBeginEndNode node, ConnectionPointType type, GUIStyle style, Action<EposConnectionPoint> OnClickConnectionPoint)
    {
        this.beginEndNode = node;
        this.type = type;
        this.style = style;
        this.OnClickConnectionPoint = OnClickConnectionPoint;
        rect = new Rect(0, 0, 10f, 20f);
    }

    public void Draw()
    {
        rect.y = node.rect.y + (node.rect.height * 0.5f) - rect.height * 0.5f;

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
    public void Draw(EposNodeType nodeType)
    {
        rect.y = beginEndNode.rect.y + (beginEndNode.rect.height * 0.5f) - rect.height * 0.5f;

        switch (nodeType)
        {
            case EposNodeType.End:
                rect.x = beginEndNode.rect.x - rect.width + 8f;
                break;

            case EposNodeType.Begin:
                rect.x = beginEndNode.rect.x + beginEndNode.rect.width - 8f;
                break;
            case EposNodeType.Node:
                switch (type)
                {
                    case ConnectionPointType.In:
                        rect.x = node.rect.x - rect.width + 8f;
                        break;

                    case ConnectionPointType.Out:
                        rect.x = node.rect.x + node.rect.width - 8f;
                        break;
                }
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
    public string GetNodeConnected()
    {
        return (beginEndNode == null) ? node.GetType().ToString() : beginEndNode.GetType().ToString();
    }
}
