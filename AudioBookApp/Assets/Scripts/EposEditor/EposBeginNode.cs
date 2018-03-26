using System;
using UnityEngine;
using UnityEditor;

public class EposBeginEndNode{

    public Rect rect;
    public float rectWidth;
    public float rectHeight;
    public bool isDragged;
    public bool isSelected;


    public EposConnectionPoint inPoint;
    public EposConnectionPoint outPoint;

    public GUIStyle style;
    public GUIStyle defaultNodeStyle;
    public GUIStyle selectedNodeStyle;

    public EposNodeType nodeType;

    public EposBeginEndNode(Vector2 position, float width, float height, EposNodeType _nodeType, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle, Action<EposConnectionPoint> OnClickInPoint, Action<EposConnectionPoint> OnClickOutPoint)
    {
        rect = new Rect(position.x, position.y, width, height);
        rectHeight = height;
        rectWidth = width;
        style = nodeStyle;
        inPoint = new EposConnectionPoint(this, ConnectionPointType.In, inPointStyle, OnClickInPoint);
        outPoint = new EposConnectionPoint(this, ConnectionPointType.Out, outPointStyle, OnClickOutPoint);
        defaultNodeStyle = nodeStyle;
        selectedNodeStyle = selectedStyle;
        nodeType = _nodeType;
    }

    public void Drag(Vector2 delta)
    {
        rect.position += delta;
    }

    public void Draw()
    {
        
        GUI.Box(rect, "", style);
        GUI.skin.label.alignment = TextAnchor.UpperCenter;
        Rect label = new Rect(rect.x, rect.y - 14, rectWidth, rectHeight);
        switch (nodeType)
        {
            case EposNodeType.Begin:
                inPoint.Draw(nodeType);
                GUI.Label(label, "BEGIN");
                break;
            case EposNodeType.End:
                outPoint.Draw(nodeType);
                GUI.Label(label, "END");
                break;
        }
        
    }

    public bool ProcessEvents(Event e)
    {
        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0)
                {
                    if (rect.Contains(e.mousePosition))
                    {
                        isDragged = true;
                        GUI.changed = true;
                        isSelected = true;
                        style = selectedNodeStyle;
                    }
                    else
                    {
                        GUI.changed = true;
                        isSelected = false;
                        style = defaultNodeStyle;
                    }
                }
                if (e.button == 1 && isSelected && rect.Contains(e.mousePosition))
                {
                    //ProcessContextMenu();
                    e.Use();
                }
                break;

            case EventType.MouseUp:
                isDragged = false;
                break;
            case EventType.MouseDrag:
                if (e.button == 0 && isDragged)
                {
                    Drag(e.delta);
                    e.Use();
                    return true;
                }
                break;
        }
        return false;
    }
}
