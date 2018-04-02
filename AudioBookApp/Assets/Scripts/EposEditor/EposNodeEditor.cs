using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;

public class EposNodeEditor : EditorWindow {

    private List<EposNode> nodes;
    private List<EposConnection> connections;

    private string dialogScriptPath;
    private string[] dialogFileFilters = { "TSV sheets","tsv" };
    private List<string[]> dialogs;
    private string dialogFileName;

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

        dialogScriptPath = null;
        dialogs = new List<string[]>();
    }

    private void OnGUI()
    {
        DrawGrid(20, 0.2f, Color.gray);
        DrawGrid(100, 0.4f, Color.gray);

        DrawInterface();

        DrawBeginEndNodes();
        DrawNodes();
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

    private void DrawInterface()
    {
        GUI.depth = 100;
        if (GUILayout.Button("Save", GUILayout.Width(50)))
        {
            SaveNodeTree();
        }
        if (GUILayout.Button("Load", GUILayout.Width(50)))
        {
            LoadNodeTree();
        }
        if (GUILayout.Button("Reset", GUILayout.Width(50)))
        {
            ResetWindow();
        }

        
        Rect dialogNodeLoad = new Rect(60,2, 100, 18);
        GUILayout.BeginArea(dialogNodeLoad);

        if (GUILayout.Button("Load Dialog file"))
        {
            string path = EditorUtility.OpenFilePanelWithFilters("Overwrite with tsv", Path.GetFullPath(Application.dataPath + "/../../StoryData"),dialogFileFilters);
            if (path.Length != 0)
            {
                ReadDialogFile(path);
            }
        }
        GUILayout.EndArea();
        Rect dialogNode = new Rect(dialogNodeLoad.xMax+10,4, 150,30);
        EditorGUI.LabelField(dialogNode, dialogScriptPath, GUIStyle.none);
    }

    private void DrawNodes()
    {
        if (nodes != null)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].SetDialogList(dialogFileName, dialogs);
                nodes[i].Draw();
            }
        }
    }

    private void DrawBeginEndNodes()
    {
        if (nodes == null)
        {
            nodes = new List<EposNode>();
            nodes.Add(new EposNode(Guid.NewGuid(), new Vector2(60, 60), EposNodeType.Begin, beginEndStyle, selectedNodeStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint));
            nodes.Add(new EposNode(Guid.NewGuid(), new Vector2(this.position.width - 60, this.position.height - 60), EposNodeType.End, beginEndStyle, selectedNodeStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint));
        }
        
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
        EposNode newNode = new EposNode(Guid.NewGuid(), mousePosition, EposNodeType.Node, nodeStyle, selectedNodeStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, "", false, OnClickRemoveNode, null,null);
        //newNode.SetDialogList(dialogs);
        nodes.Add(newNode);
    }

    private void OnClickInPoint(EposConnectionPoint inPoint)
    {
        selectedInPoint = inPoint;
        Debug.Log("Selected In point : " + selectedInPoint.GetNodeConnected());

        if (selectedOutPoint != null)
        {
            if ((selectedOutPoint.node != selectedInPoint.node))
            {
                CreateConnection();
            }
            ClearConnectionSelection();
        }
    }

    private void OnClickOutPoint(EposConnectionPoint outPoint)
    {
        selectedOutPoint = outPoint;

        if (selectedInPoint != null)
        {
            Debug.Log(selectedInPoint.node);
            Debug.Log(selectedOutPoint.node);
            if ((selectedOutPoint.node != selectedInPoint.node))
            {
                CreateConnection();
            }
            ClearConnectionSelection();
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
        Debug.Log("Create connection between : in_"+ selectedInPoint.GetNodeConnected().title+" and  out "+ selectedOutPoint.GetNodeConnected().title);
        EposNode selectedInNode = selectedInPoint.GetNodeConnected();
        EposNode selectedOutNode = selectedOutPoint.GetNodeConnected();

        bool inNodeFound = false;
        bool outNodeFound = false;
        for (int i = 0; i < nodes.Count; i++)
        {
            if(nodes[i].uuid == selectedInNode.uuid)
            {
                nodes[i].AddConnectedNode(0, selectedOutNode.uuid);
                inNodeFound = true;
            }
            else if (nodes[i].uuid == selectedOutNode.uuid)
            {
                nodes[i].AddConnectedNode(1, selectedInNode.uuid);
                outNodeFound = true;
            }
            if (inNodeFound && outNodeFound)
                break;
        }
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
        //remove node from other nodes
        for(int i=0; i<nodes.Count; i++)
        {
            if (node.uuid == nodes[i].uuid)
                continue;
            for (int j=0; j<nodes[i].inNodes.Count; j++)
            {
                if(node.uuid == nodes[i].inNodes[j])
                {
                    nodes[i].inNodes.RemoveAt(j);
                }
            }
            for (int j = 0; j < nodes[i].outNodes.Count; j++)
            {
                if (node.uuid == nodes[i].outNodes[j])
                {
                    nodes[i].outNodes.RemoveAt(j);
                }
            }
        }
        nodes.Remove(node);
        // remove node connection entry
    }

    //------------ Reset Window ------------------

    private void ResetWindow()
    {
        nodes = null;
        connections = null;
        dialogScriptPath = null;
        GUI.changed = true;
    }

    //------------ Save Node tree ----------------

    private void SaveNodeTree()
    {
        string path = "Assets/test_nodetree.xml";
        EposXmlNodeContainer nodeXMLContainer = new EposXmlNodeContainer();

        nodeXMLContainer.pathToDialogScript = dialogScriptPath;

        if (nodes != null)
        {
            foreach (EposNode node in nodes)
            {
                EposXMLNode XMLNode = new EposXMLNode
                {
                    uuid = node.uuid,
                    title = node.title,
                    nodeType = node.nodeType,
                    posX = node.rect.position.x,
                    posY = node.rect.position.y,
                    wwiseEvent = node.wwiseEvent,
                    isQueued = node.isQueued,
                    in_nodes = node.inNodes,
                    out_nodes = node.outNodes,
                    dialogIndex = node.dialogIndex
                };
                nodeXMLContainer.eposXMLNodes.Add(XMLNode);
            }
        }

        //Save xml file
        nodeXMLContainer.Save(path);
    }

    private void LoadNodeTree()
    {
        nodes = new List<EposNode>();
        connections = new List<EposConnection>();
        EposXmlNodeContainer nodeXmlContainer = EposXmlNodeContainer.Load(Path.Combine(Application.dataPath, "test_nodetree.xml"));
        dialogScriptPath = nodeXmlContainer.pathToDialogScript;
        ReadDialogFile(dialogScriptPath);
        foreach(EposXMLNode xmlNode in nodeXmlContainer.eposXMLNodes)
        {
            switch(xmlNode.nodeType)
            {
                case EposNodeType.Begin:
                case EposNodeType.End:
                    nodes.Add(new EposNode(xmlNode.uuid, new Vector2(xmlNode.posX, xmlNode.posY), xmlNode.nodeType, beginEndStyle, selectedNodeStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint,"",false,null,xmlNode.in_nodes,xmlNode.out_nodes));
                    
                    break;
                case EposNodeType.Node:
                    EposNode newNode = new EposNode(xmlNode.uuid, new Vector2(xmlNode.posX, xmlNode.posY), xmlNode.nodeType, nodeStyle, selectedNodeStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint,xmlNode.wwiseEvent,xmlNode.isQueued, OnClickRemoveNode, xmlNode.in_nodes, xmlNode.out_nodes);
                    newNode.SetDialogIndex(xmlNode.dialogIndex);                 
                    nodes.Add(newNode);
                    break;
                default:
                    break;
            }
        }
        LoadNodesConnections();

    }

    private void LoadNodesConnections()
    {
        connections = new List<EposConnection>();
        for(int i=0; i<nodes.Count; i++)
        {
            foreach(Guid uuid in nodes[i].inNodes)
            {
                for(int j=0; j<nodes.Count; j++)
                {
                    if (i == j)
                        continue;
                    if (nodes[j].uuid == uuid)
                    {
                        if (!FindExistingConnection(nodes[i].inPoint, nodes[j].outPoint, connections))
                            connections.Add(new EposConnection(nodes[i].inPoint, nodes[j].outPoint, OnClickRemoveConnection));
                    }
                }
            }
            foreach (Guid uuid in nodes[i].outNodes)
            {
                for (int j = 0; j < nodes.Count; j++)
                {
                    if (i == j)
                        continue;
                    if (nodes[j].uuid == uuid)
                    {
                        if(!FindExistingConnection(nodes[j].inPoint, nodes[i].outPoint,connections))
                            connections.Add(new EposConnection(nodes[j].inPoint, nodes[i].outPoint, OnClickRemoveConnection));
                    }
                }
            }
        }
    }

    private bool FindExistingConnection(EposConnectionPoint inPoint, EposConnectionPoint outPoint, List<EposConnection> _connections)
    {
        foreach(EposConnection connection in _connections)
        {
            if (connection.inPoint == inPoint && connection.outPoint == outPoint)
                return true;
        }
        return false;
    }
    public void LoadFile()
    {
        this.LoadNodeTree();
    }
    public List<EposNode> GetNodes()
    {
        return nodes;
    }

    //---------------- Script Dialog Management -------------------------

    private void ReadDialogFile(string path)
    {
        dialogFileName = Path.GetFileNameWithoutExtension(path);
        dialogScriptPath = path;
        StreamReader theReader = new StreamReader(dialogScriptPath, Encoding.UTF8);
        string line;
        int index = 0;
        using (theReader)
        {
            do
            {
                
                line = theReader.ReadLine();
                if (index == 0)
                {
                    index++;
                    continue;
                }
                if (line != null)
                {
                    string[] split = line.Split('\t');
                    dialogs.Add(split);

                    index++;
                }
            }
            while (line != null);
            theReader.Close();
            GetWindow<EposNodeEditor>().titleContent = new GUIContent("Epos Editor - "+dialogFileName);
            GUI.changed = true;
        }
    }
    public string ReadDialogLine(int index)
    {
        return dialogs[index][1];
    }
}
