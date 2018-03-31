﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum EposNodeType {
    Begin,
    Node,
    End
};

public class EposNode{

    public Guid uuid;

    public Rect rect;
    public Rect wwiseTextFieldRect;
    public float rectWidth;
    public float rectHeight;
    public string title;
    public string wwiseEvent;
    public bool isDragged;
    public bool isSelected;
    public bool isEditingWwiseEvent;

    public EposConnectionPoint inPoint;
    public EposConnectionPoint outPoint;

    public List<Guid> inNodes;
    public List<Guid> outNodes;


    public GUIStyle style;
    public GUIStyle defaultNodeStyle;
    public GUIStyle selectedNodeStyle;

    public Action<EposNode> OnRemoveNode;

    public EposNodeType nodeType;

    public EposNode(Guid uuid, Vector2 position, EposNodeType _nodeType, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle, Action<EposConnectionPoint> OnClickInPoint, Action<EposConnectionPoint> OnClickOutPoint,List<Guid> in_nodes = null, List<Guid> out_nodes = null, Action<EposNode> OnClickRemoveNode = null)
    {
        this.uuid = uuid;

        this.inNodes = in_nodes;
        this.outNodes = out_nodes;

        this.nodeType = _nodeType;
        switch (_nodeType)
        {
            case EposNodeType.Begin:
                outPoint = new EposConnectionPoint(this, ConnectionPointType.Out, outPointStyle, OnClickOutPoint);
                rectWidth = rectHeight = 50;
                title = "BEGIN";
                break;
            case EposNodeType.End:
                inPoint = new EposConnectionPoint(this, ConnectionPointType.In, inPointStyle, OnClickInPoint);
                rectWidth = rectHeight = 50;
                title = "END";
                break;
            case EposNodeType.Node:
                inPoint = new EposConnectionPoint(this, ConnectionPointType.In, inPointStyle, OnClickInPoint);
                outPoint = new EposConnectionPoint(this, ConnectionPointType.Out, outPointStyle, OnClickOutPoint);
                rectWidth = 200;
                rectHeight = 100;
                title = "Dialog Node";
                OnRemoveNode = OnClickRemoveNode;
                break;
            default:
                break;
        }

        rect = new Rect(position.x, position.y, this.rectWidth, this.rectHeight);
        //rectHeight = height;
        //rectWidth = width;
        style = nodeStyle;
        
        defaultNodeStyle = nodeStyle;
        selectedNodeStyle = selectedStyle;
        wwiseEvent = "";

    }

    public void Drag(Vector2 delta)
    {
        rect.position += delta;
    }

    public void Draw()
    {
        

        GUI.Box(rect, "", style);
        GUI.skin.label.alignment = TextAnchor.UpperCenter;
        Rect label = new Rect(rect.x, rect.y-14, rectWidth, rectHeight);
        GUI.Label(label, title);
        switch (nodeType)
        {
            case EposNodeType.Begin:
                outPoint.Draw();
                break;
            case EposNodeType.End:
                inPoint.Draw();
                break;
            case EposNodeType.Node:
                wwiseTextFieldRect = new Rect(rect.x + 15, rect.y + 15, rectWidth - 27, 20);
                
                wwiseEvent = EditorGUI.TextField(wwiseTextFieldRect, "", wwiseEvent);
                inPoint.Draw();
                outPoint.Draw();
                break;
            default:
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
						isEditingWwiseEvent = true;
                    }
                    else
                    {
                        GUI.changed = true;
                        isSelected = false;
                        style = defaultNodeStyle;
						isEditingWwiseEvent = false;
                    }
                }
                if (e.button == 1 && isSelected && rect.Contains(e.mousePosition))
                {
                    if (nodeType == EposNodeType.Node)
                    {
                        ProcessContextMenu();
                        
                    }
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

    public void AddConnectedNode(short in_out,Guid nodeUUID)
    {
        switch (in_out)
        {
            case 0:
                if(inNodes == null)
                {
                    inNodes = new List<Guid>();
                }
                inNodes.Add(nodeUUID);
                break;
            case 1:
                if (outNodes == null)
                {
                    outNodes = new List<Guid>();
                }
                outNodes.Add(nodeUUID);
                break;
            default:
                break;
        }
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
        EposPopup popup = new EposPopup(this, EposPopupType.Rename);
        popup.ShowUtility();
    }

    private void OnClickRemoveNode()
    {
        if (OnRemoveNode != null && nodeType == EposNodeType.Node)
        {
            OnRemoveNode(this);
        }
    }
}
