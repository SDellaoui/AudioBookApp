using System;
using UnityEditor;
using UnityEngine;

public class EposNode{

    public Rect rect;
    public float rectWidth;
    public float rectHeight;
    public string title;
    public bool isDragged;
    public bool isSelected;

    public EposConnectionPoint inPoint;
    public EposConnectionPoint outPoint;

    public GUIStyle style;
    public GUIStyle defaultNodeStyle;
    public GUIStyle selectedNodeStyle;

    public Action<EposNode> OnRemoveNode;

    public EposNode(Vector2 position, float width, float height, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle, Action<EposConnectionPoint> OnClickInPoint, Action<EposConnectionPoint> OnClickOutPoint, Action<EposNode> OnClickRemoveNode)
    {
        rect = new Rect(position.x, position.y, width, height);
        rectHeight = height;
        rectWidth = width;
        style = nodeStyle;
        inPoint = new EposConnectionPoint(this, ConnectionPointType.In, inPointStyle, OnClickInPoint);
        outPoint = new EposConnectionPoint(this, ConnectionPointType.Out, outPointStyle, OnClickOutPoint);
        defaultNodeStyle = nodeStyle;
        selectedNodeStyle = selectedStyle;
        OnRemoveNode = OnClickRemoveNode;
    }

    public void Drag(Vector2 delta)
    {
        rect.position += delta;
    }

    public void Draw()
    {
        inPoint.Draw();
        outPoint.Draw();
        GUI.Box(rect, title, style);
        GUI.skin.label.alignment = TextAnchor.UpperCenter;
        Rect label = new Rect(rect.x, rect.y-14, rectWidth, rectHeight);
        GUI.Label(label, "Coucou");
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
                    ProcessContextMenu();
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
    private void ProcessContextMenu()
    {
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("Rename"), false, OnClickRenameNode);
        genericMenu.AddItem(new GUIContent("Remove"), false, OnClickRemoveNode);
        genericMenu.ShowAsContext();
    }
        
    private void OnClickRenameNode()
    {
        Event e = Event.current;
        Debug.Log("Rename node");
    }

    private void OnClickRemoveNode()
    {
        if (OnRemoveNode != null)
        {
            OnRemoveNode(this);
        }
    }
}
