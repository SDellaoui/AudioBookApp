using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EposNodeEditor : EditorWindow {

    private EposBeginEndNode beginNode;
    private EposBeginEndNode endNode;
    private List<EposNode> nodes;
    private List<EposConnection> connections;

    private GUIStyle beginEndStyle;
    private GUIStyle nodeStyle;
    private GUIStyle selectedNodeStyle;
    private GUIStyle inPointStyle;
    private GUIStyle outPointStyle;

    private EposConnectionPoint selectedInPoint;
    private EposConnectionPoint selectedOutPoint;

    private Vector2 offset;
    private Vector2 drag;

    private bool isDrawing = false;

    //private Vector2 windowSize;


    [MenuItem("Window/Node Based Editor")]
    private static void OpenWindow()
    {
        EposNodeEditor window = GetWindow<EposNodeEditor>();
        window = GetWindow<EposNodeEditor>();
        window.titleContent = new GUIContent("Epos Editor");
        //windowSize = window.minSize;
        window.autoRepaintOnSceneChange = true;
        
    }

    private void OnEnable()
    {
        nodeStyle = new GUIStyle();
        nodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
        nodeStyle.border = new RectOffset(12, 12, 12, 12);

        beginEndStyle = new GUIStyle();
        beginEndStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
        beginEndStyle.border = new RectOffset(25, 25, 25, 25);

        selectedNodeStyle = new GUIStyle();
        selectedNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D;
        selectedNodeStyle.border = new RectOffset(12, 12, 12, 12);

        inPointStyle = new GUIStyle();
        inPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left.png") as Texture2D;
        inPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left on.png") as Texture2D;
        inPointStyle.border = new RectOffset(4, 4, 12, 12);

        outPointStyle = new GUIStyle();
        outPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right.png") as Texture2D;
        outPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right on.png") as Texture2D;
        outPointStyle.border = new RectOffset(4, 4, 12, 12);
    }

    private void OnGUI()
    {
        DrawGrid(20, 0.2f, Color.gray);
        DrawGrid(100, 0.4f, Color.gray);

        DrawNodes();
        DrawBeginEndNodes();
        DrawConnections();

        DrawConnectionLine(Event.current);

        ProcessNodeEvents(Event.current);
        ProcessEvents(Event.current);

        if (GUI.changed) Repaint();
    }

    private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
    {
        int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
        int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

        Handles.BeginGUI();
        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        offset += drag * 0.5f;
        Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);

        for (int i = 0; i < widthDivs; i++)
        {
            Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
        }

        for (int j = 0; j < heightDivs; j++)
        {
            Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
        }

        Handles.color = Color.white;
        Handles.EndGUI();
    }

    private void DrawNodes()
    {
        if (nodes != null)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].Draw();
            }
        }
    }

    private void DrawBeginEndNodes()
    {
        EposNodeEditor window = GetWindow<EposNodeEditor>();
        if (beginNode == null && endNode == null)
        {
            beginNode = new EposBeginEndNode(new Vector2(10,10), 50, 50, EposNodeType.Begin, beginEndStyle, selectedNodeStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint);
            endNode = new EposBeginEndNode(new Vector2(window.position.width -60, window.position.height - 60), 50, 50, EposNodeType.End, beginEndStyle, selectedNodeStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint);
        }
        beginNode.Draw();
        endNode.Draw();
    }

    private void DrawConnections()
    {
        if (connections != null)
        {
            for (int i = 0; i < connections.Count; i++)
            {
                connections[i].Draw();
            }
        }
    }

    private void DrawConnectionLine(Event e)
    {
        if(e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
        {
            if ((selectedInPoint != null && selectedOutPoint == null)
                || (selectedOutPoint != null && selectedInPoint == null))
            {
                selectedInPoint = null;
                selectedOutPoint = null;
            }
        }
        if (selectedInPoint != null && selectedOutPoint == null)
        {
            Handles.DrawBezier(
                selectedInPoint.rect.center,
                e.mousePosition,
                selectedInPoint.rect.center + Vector2.left * 50f,
                e.mousePosition - Vector2.left * 50f,
                Color.white,
                null,
                2f
            );
            GUI.changed = true;
        }

        if (selectedOutPoint != null && selectedInPoint == null)
        {
            Handles.DrawBezier(
                selectedOutPoint.rect.center,
                e.mousePosition,
                selectedOutPoint.rect.center - Vector2.left * 50f,
                e.mousePosition + Vector2.left * 50f,
                Color.white,
                null,
                2f
            );
            GUI.changed = true;
        }
    }

    private void ProcessEvents(Event e)
    {
        drag = Vector2.zero;
        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 1)
                {
                    ProcessContextMenu(e.mousePosition);
                }
                break;
            case EventType.MouseDrag:
                if (e.button == 0)
                {
                    OnDrag(e.delta);
                }
                break;
        }
    }
    private void OnDrag(Vector2 delta)
    {
        drag = delta;

        if (nodes != null)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].Drag(delta);
            }
        }
        beginNode.Drag(delta);
        endNode.Drag(delta);
        GUI.changed = true;
    }
    private void ProcessNodeEvents(Event e)
    {
        if (nodes != null)
        {
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                bool guiChanged = nodes[i].ProcessEvents(e);

                if (guiChanged)
                {
                    GUI.changed = true;
                }
            }
        }

        if(beginNode != null || endNode != null)
        {
            bool guiChanged_begin = beginNode.ProcessEvents(e);
            bool guiChanged_end = endNode.ProcessEvents(e);
            if (guiChanged_begin || guiChanged_end)
            {
                GUI.changed = true;
            }
        }
    }


    private void ProcessContextMenu(Vector2 mousePosition)
    {
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("Add node"), false, () => OnClickAddNode(mousePosition));
        genericMenu.ShowAsContext();
    }

    private void OnClickAddNode(Vector2 mousePosition)
    {
        if (nodes == null)
        {
            nodes = new List<EposNode>();
        }

        nodes.Add(new EposNode(mousePosition, 200, 100, nodeStyle, selectedNodeStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode));
    }

    private void OnClickInPoint(EposConnectionPoint inPoint)
    {
        selectedInPoint = inPoint;
        Debug.Log("Selected In point : " + selectedInPoint.GetNodeConnected());

        if (selectedOutPoint != null)
        {
            if (selectedOutPoint.GetNodeConnected() == "EposBeginEndNode" && selectedInPoint.GetNodeConnected() == "EposBeginEndNode")
            {
                if ((selectedOutPoint.beginEndNode != selectedInPoint.beginEndNode))
                {
                    CreateConnection();
                }
                ClearConnectionSelection();
            }
            else
            {
                if ((selectedInPoint.GetNodeConnected() == "EposBeginEndNode" && selectedOutPoint.GetNodeConnected() == "EposNode")
                    || selectedOutPoint.node != selectedInPoint.node)
                {
                    CreateConnection();
                }
                ClearConnectionSelection();
            }
        }
    }

    private void OnClickOutPoint(EposConnectionPoint outPoint)
    {
        selectedOutPoint = outPoint;
        Debug.Log("Selected Out point : " + selectedOutPoint.GetNodeConnected());

        if (selectedInPoint != null)
        {
            Debug.Log(selectedInPoint.node);
            Debug.Log(selectedOutPoint.node);

            if (selectedOutPoint.GetNodeConnected() == "EposBeginEndNode" && selectedInPoint.GetNodeConnected() == "EposBeginEndNode")
            {
                if((selectedOutPoint.beginEndNode != selectedInPoint.beginEndNode))
                {
                    CreateConnection();
                }
                ClearConnectionSelection();
            }
            else
            {
                Debug.Log(selectedInPoint.GetNodeConnected());
                if ((selectedOutPoint.GetNodeConnected() == "EposBeginEndNode" && selectedInPoint.GetNodeConnected() == "EposNode")
                    || (selectedOutPoint.node != selectedInPoint.node))
                    CreateConnection();

                ClearConnectionSelection();
            }
        }
    }

    private void OnClickRemoveConnection(EposConnection connection)
    {
        connections.Remove(connection);
    }

    private void CreateConnection()
    {
        if (connections == null)
        {
            connections = new List<EposConnection>();
        }
        Debug.Log("Create connection between : "+ selectedInPoint.GetNodeConnected()+" and "+ selectedOutPoint.GetNodeConnected());
        connections.Add(new EposConnection(selectedInPoint, selectedOutPoint, OnClickRemoveConnection));
    }

    private void ClearConnectionSelection()
    {
        selectedInPoint = null;
        selectedOutPoint = null;
    }

    private void OnClickRemoveNode(EposNode node)
    {
        if (connections != null)
        {
            List<EposConnection> connectionsToRemove = new List<EposConnection>();

            for (int i = 0; i < connections.Count; i++)
            {
                if (connections[i].inPoint == node.inPoint || connections[i].outPoint == node.outPoint)
                {
                    connectionsToRemove.Add(connections[i]);
                }
            }

            for (int i = 0; i < connectionsToRemove.Count; i++)
            {
                connections.Remove(connectionsToRemove[i]);
            }

            connectionsToRemove = null;
        }

        nodes.Remove(node);
    }
}
