﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;
using System.Xml;
using UnityEngine;
using UnityEditor;

public class EposNodeEditor : EditorWindow {

    private EposData m_eposData;

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


    [MenuItem("Window/Node Based Editor")]
    private static void OpenWindow()
    {
        EposNodeEditor window = GetWindow<EposNodeEditor>();
        window = GetWindow<EposNodeEditor>();
        window.titleContent = new GUIContent("Epos Editor");
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


        m_eposData = new EposData();
    }

    private void OnGUI()
    {
        DrawGrid(20, 0.2f, Color.gray);
        DrawGrid(100, 0.4f, Color.gray);

        

        DrawBeginEndNodes();
        DrawNodes();
        DrawInterface();
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
		int toolBarInt;
		string[] toolStrings = new string[]{"tool1", "tool2", "tool3"};
		GUILayout.Toolbar (0, toolStrings);
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
			string folder = Application.dataPath + "/Resources/01_StoryData";
            string path = EditorUtility.OpenFilePanelWithFilters("Overwrite with tsv", Path.GetFullPath(folder),this.m_eposData.m_dialogFileFilters);
            if (path.Length != 0)
            {
				string relativepath = "";
				if (path.StartsWith(Application.dataPath)) {
					relativepath =  path.Substring(Application.dataPath.Length);
					relativepath = relativepath.Substring (1, relativepath.Length - 1);
				}
                this.m_eposData.m_dialogScriptPath = relativepath;
                this.m_eposData.ReadDialogFile(path);
            }
        }
        GUILayout.EndArea();
        Rect dialogNode = new Rect(dialogNodeLoad.xMax+10,4, 150,30);
        EditorGUI.LabelField(dialogNode, this.m_eposData.m_dialogScriptPath, GUIStyle.none);
    }

    private void DrawNodes()
    {
        if (this.m_eposData.m_nodes != null)
        {
            for (int i = 0; i < this.m_eposData.m_nodes.Count; i++)
            {
                this.m_eposData.m_nodes[i].SetDialogList(this.m_eposData.m_dialogFileName, this.m_eposData.m_dialogs);
                this.m_eposData.m_nodes[i].Draw();
            }
        }
    }

    private void DrawBeginEndNodes()
    {
        if (this.m_eposData.m_nodes == null)
        {
            this.m_eposData.m_nodes = new List<EposNode>();
        }
        if(this.m_eposData.m_nodes.Count == 0)
        {
            this.m_eposData.m_nodes.Add(new EposNode(Guid.NewGuid(), new Vector2(60, 60), EposNodeType.Begin, beginEndStyle, selectedNodeStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint));
            this.m_eposData.m_nodes.Add(new EposNode(Guid.NewGuid(), new Vector2(this.position.width - 60, this.position.height - 60), EposNodeType.End, beginEndStyle, selectedNodeStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint));
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

        if (this.m_eposData.m_nodes != null)
        {
            for (int i = 0; i < this.m_eposData.m_nodes.Count; i++)
            {
                this.m_eposData.m_nodes[i].Drag(delta);
            }
        }
        GUI.changed = true;
    }
    private void ProcessNodeEvents(Event e)
    {
        if (this.m_eposData.m_nodes != null)
        {
            for (int i = this.m_eposData.m_nodes.Count - 1; i >= 0; i--)
            {
                bool guiChanged = this.m_eposData.m_nodes[i].ProcessEvents(e);

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
        if (this.m_eposData.m_nodes == null)
        {
            this.m_eposData.m_nodes = new List<EposNode>();
        }
        EposNode newNode = new EposNode(Guid.NewGuid(), mousePosition, EposNodeType.Node, nodeStyle, selectedNodeStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, "", false, OnClickRemoveNode, null,null);
        this.m_eposData.m_nodes.Add(newNode);
    }

    private void OnClickInPoint(EposConnectionPoint inPoint)
    {
        selectedInPoint = inPoint;
        //Debug.Log("Selected In point : " + selectedInPoint.GetNodeConnected());

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
            //Debug.Log(selectedInPoint.node);
            //Debug.Log(selectedOutPoint.node);
            if ((selectedOutPoint.node != selectedInPoint.node))
            {
                CreateConnection();
            }
            ClearConnectionSelection();
        }
    }

    private void OnClickRemoveConnection(EposConnection connection)
    {
        bool inNodeFound = false;
        bool outNodeFound = false;
        for (int i=0; i< this.m_eposData.m_nodes.Count; i++)
        {
            if(this.m_eposData.m_nodes[i].nodeData.m_uuid == connection.inPoint.node.nodeData.m_uuid)
            {
                this.m_eposData.m_nodes[i].RemoveConnectedNode(0, connection.outPoint.node.nodeData.m_uuid);
                inNodeFound = true;
            }
            if (this.m_eposData.m_nodes[i].nodeData.m_uuid == connection.outPoint.node.nodeData.m_uuid)
            {
                this.m_eposData.m_nodes[i].RemoveConnectedNode(1, connection.inPoint.node.nodeData.m_uuid);
                outNodeFound = true;
            }
            if (inNodeFound && outNodeFound)
                break;
        }
        connections.Remove(connection);
    }

    private void CreateConnection()
    {
        if (connections == null)
        {
            connections = new List<EposConnection>();
        }
        //Debug.Log("Create connection between : in_"+ selectedInPoint.GetNodeConnected().title+" and  out "+ selectedOutPoint.GetNodeConnected().title);
        EposNode selectedInNode = selectedInPoint.GetNodeConnected();
        EposNode selectedOutNode = selectedOutPoint.GetNodeConnected();

        bool inNodeFound = false;
        bool outNodeFound = false;
        for (int i = 0; i < this.m_eposData.m_nodes.Count; i++)
        {
            if(this.m_eposData.m_nodes[i].nodeData.m_uuid == selectedInNode.nodeData.m_uuid)
            {
                this.m_eposData.m_nodes[i].AddConnectedNode(0, selectedOutNode.nodeData.m_uuid);
                inNodeFound = true;
            }
            else if (this.m_eposData.m_nodes[i].nodeData.m_uuid == selectedOutNode.nodeData.m_uuid)
            {
                this.m_eposData.m_nodes[i].AddConnectedNode(1, selectedInNode.nodeData.m_uuid);
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
        for(int i=0; i< this.m_eposData.m_nodes.Count; i++)
        {
            if (node.nodeData.m_uuid == this.m_eposData.m_nodes[i].nodeData.m_uuid)
                continue;
            for (int j=0; j< this.m_eposData.m_nodes[i].nodeData.m_inNodes.Count; j++)
            {
                if(node.nodeData.m_uuid == this.m_eposData.m_nodes[i].nodeData.m_inNodes[j])
                {
                    this.m_eposData.m_nodes[i].nodeData.m_inNodes.RemoveAt(j);
                }
            }
            for (int j = 0; j < this.m_eposData.m_nodes[i].nodeData.m_outNodes.Count; j++)
            {
                if (node.nodeData.m_uuid == this.m_eposData.m_nodes[i].nodeData.m_outNodes[j])
                {
                    this.m_eposData.m_nodes[i].nodeData.m_outNodes.RemoveAt(j);
                }
            }
        }
        this.m_eposData.m_nodes.Remove(node);
        // remove node connection entry
    }

    //------------ Reset Window ------------------

    private void ResetWindow()
    {
        this.m_eposData = new EposData();
        connections = null;

        GUI.changed = true;
    }

    //------------ Save Node tree ----------------

    private void SaveNodeTree()
    {
        string path = "Assets/test_nodetree.xml";
        EposXmlNodeContainer nodeXMLContainer = new EposXmlNodeContainer();

        nodeXMLContainer.pathToDialogScript = this.m_eposData.m_dialogScriptPath;


        if (this.m_eposData.m_nodes != null)
        {
            foreach (EposNode node in this.m_eposData.m_nodes)
            {
                EposXMLNode XMLNode = new EposXMLNode
                {
                    uuid = node.nodeData.m_uuid,
                    title = node.title,
                    nodeType = node.nodeData.m_nodeType,
                    posX = node.rect.position.x,
                    posY = node.rect.position.y,
                    wwiseEvent = node.nodeData.m_wwiseEvent,
                    isQueued = node.nodeData.m_isQueued,
                    in_nodes = node.nodeData.m_inNodes,
                    out_nodes = node.nodeData.m_outNodes,
                    dialogIndex = node.nodeData.m_dialogIndex
                };
                nodeXMLContainer.eposXMLNodes.Add(XMLNode);
            }
        }

        //Save xml file
        nodeXMLContainer.Save(path);
    }

    private void LoadNodeTree()
    {
        this.m_eposData.m_nodes = new List<EposNode>();
        connections = new List<EposConnection>();
        EposXmlNodeContainer nodeXmlContainer = EposXmlNodeContainer.Load(Path.Combine(Application.dataPath, "test_nodetree.xml"));
        this.m_eposData.m_dialogScriptPath = nodeXmlContainer.pathToDialogScript;
        this.m_eposData.ReadDialogFile(Path.Combine(Application.dataPath, this.m_eposData.m_dialogScriptPath));
        foreach(EposXMLNode xmlNode in nodeXmlContainer.eposXMLNodes)
        {
            switch(xmlNode.nodeType)
            {
                case EposNodeType.Begin:
                case EposNodeType.End:
                    this.m_eposData.m_nodes.Add(new EposNode(xmlNode.uuid, new Vector2(xmlNode.posX, xmlNode.posY), xmlNode.nodeType, beginEndStyle, selectedNodeStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint,"",false,null,xmlNode.in_nodes,xmlNode.out_nodes));
                    
                    break;
                case EposNodeType.Node:
                    EposNode newNode = new EposNode(xmlNode.uuid, new Vector2(xmlNode.posX, xmlNode.posY), xmlNode.nodeType, nodeStyle, selectedNodeStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint,xmlNode.wwiseEvent,xmlNode.isQueued, OnClickRemoveNode, xmlNode.in_nodes, xmlNode.out_nodes);
                    newNode.SetDialogIndex(xmlNode.dialogIndex);
                    this.m_eposData.m_nodes.Add(newNode);
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
        for(int i=0; i< this.m_eposData.m_nodes.Count; i++)
        {
            foreach(Guid uuid in this.m_eposData.m_nodes[i].nodeData.m_inNodes)
            {
                for(int j=0; j< this.m_eposData.m_nodes.Count; j++)
                {
                    if (i == j)
                        continue;
                    if (this.m_eposData.m_nodes[j].nodeData.m_uuid == uuid)
                    {
                        if (!FindExistingConnection(this.m_eposData.m_nodes[i].inPoint, this.m_eposData.m_nodes[j].outPoint, connections))
                            connections.Add(new EposConnection(this.m_eposData.m_nodes[i].inPoint, this.m_eposData.m_nodes[j].outPoint, OnClickRemoveConnection));
                    }
                }
            }
            foreach (Guid uuid in this.m_eposData.m_nodes[i].nodeData.m_outNodes)
            {
                for (int j = 0; j < this.m_eposData.m_nodes.Count; j++)
                {
                    if (i == j)
                        continue;
                    if (this.m_eposData.m_nodes[j].nodeData.m_uuid == uuid)
                    {
                        if(!FindExistingConnection(this.m_eposData.m_nodes[j].inPoint, this.m_eposData.m_nodes[i].outPoint,connections))
                            connections.Add(new EposConnection(this.m_eposData.m_nodes[j].inPoint, this.m_eposData.m_nodes[i].outPoint, OnClickRemoveConnection));
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
        return this.m_eposData.m_nodes;
    }

    //---------------- Script Dialog Management -------------------------
    public string ReadDialogLine(int index)
    {
        return this.m_eposData.m_dialogs[index][1];
    }
}


// Class containing node editor data. used both on editor and runtime.
public class EposData
{
    public List<EposNode> m_nodes;
    public List<EposNodeData> m_nodesData;

    public string m_nodePath;
    public string m_dialogScriptPath;
    public string[] m_dialogFileFilters = { "TSV sheets","tsv" };
    public List<string[]> m_dialogs;
    public string m_dialogFileName;

	public List<string> m_eventsList;

	public EposData()
	{
        m_nodes = new List<EposNode>();
        m_nodesData = new List<EposNodeData>();
		m_dialogs = new List<string[]> ();
		GetWwiseEvents ();
	}

	public void GetWwiseEvents()
	{
		XmlDocument document = new XmlDocument ();
		document.Load(Application.dataPath+"/../../WwiseProject/GeneratedSoundBanks/Windows/SoundbanksInfo.xml");
		XmlNode root = document.DocumentElement;
		XmlNodeList list = document.GetElementsByTagName ("Event");
		foreach (XmlNode xn in list) {
			Debug.Log (xn.Attributes["Name"].Value);
		}
	}
	public void LoadNodeTree()
	{
		this.m_nodePath = Path.Combine (Application.dataPath, "test_nodetree.xml");

		EposXmlNodeContainer nodeXmlContainer = EposXmlNodeContainer.Load(this.m_nodePath);
		m_dialogScriptPath = nodeXmlContainer.pathToDialogScript;
		ReadDialogFile (m_dialogScriptPath);

		foreach(EposXMLNode xmlNode in nodeXmlContainer.eposXMLNodes)
		{
			switch(xmlNode.nodeType)
			{
			case EposNodeType.Begin:
			case EposNodeType.End:
                    m_nodesData.Add(new EposNodeData(xmlNode.uuid, xmlNode.nodeType, xmlNode.dialogIndex, xmlNode.wwiseEvent, xmlNode.isQueued, xmlNode.in_nodes, xmlNode.out_nodes));
				break;
			case EposNodeType.Node:
                    m_nodesData.Add(new EposNodeData(xmlNode.uuid, xmlNode.nodeType, xmlNode.dialogIndex, xmlNode.wwiseEvent, xmlNode.isQueued, xmlNode.in_nodes, xmlNode.out_nodes));                
				break;
			default:
				break;
			}
		}
	}
	public void ReadDialogFile(string path)
	{
        if (this.m_dialogs == null || this.m_dialogs.Count > 0)
            this.m_dialogs = new List<string[]>();
		m_dialogFileName = Path.GetFileNameWithoutExtension(path);
		StreamReader theReader = new StreamReader(Path.Combine(Application.dataPath,path), Encoding.UTF8);
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
					this.m_dialogs.Add(split);

					index++;
				}
			}
			while (line != null);
			theReader.Close();
		}
	}
	public string ReadDialogLine(int index)
	{
		return this.m_dialogs[index][1];
	}
	public List<EposNodeData> GetNodes()
	{
		return this.m_nodesData;
	}
}
