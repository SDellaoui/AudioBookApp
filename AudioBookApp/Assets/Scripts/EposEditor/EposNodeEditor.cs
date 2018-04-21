using System;
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

        GUI.depth = 100;
        if (GUILayout.Button("Save", GUILayout.Width(50)))
        {
            SaveNodeTree();
        }
        if (GUILayout.Button("Load", GUILayout.Width(50)))
        {
            this.m_eposData = new EposData();
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
            this.m_eposData.m_nodes.Add(new EposNode(Guid.NewGuid(), new Vector2(60, 60), EposNodeType.Begin, OnClickInPoint, OnClickOutPoint));
            this.m_eposData.m_nodes.Add(new EposNode(Guid.NewGuid(), new Vector2(this.position.width - 60, this.position.height - 60), EposNodeType.End, OnClickInPoint, OnClickOutPoint));
        }
        
    }

    private void DrawConnections()
    {
        if (this.m_eposData.m_connections != null)
        {
            for (int i = 0; i < this.m_eposData.m_connections.Count; i++)
            {
                this.m_eposData.m_connections[i].Draw();
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
        genericMenu.AddItem(new GUIContent("Add node"), false, () => OnClickAddNode(mousePosition, EposNodeType.Node));
        genericMenu.AddItem(new GUIContent("Add 'OR' node"), false, () => OnClickAddNode(mousePosition, EposNodeType.Conditionnal_OR));
        genericMenu.AddItem(new GUIContent("Add 'AND' node"), false, () => OnClickAddNode(mousePosition, EposNodeType.Conditionnal_AND));
        genericMenu.ShowAsContext();
    }

    private void OnClickAddNode(Vector2 mousePosition, EposNodeType nodeType)
    {
        if (this.m_eposData.m_nodes == null)
        {
            this.m_eposData.m_nodes = new List<EposNode>();
        }
        if (nodeType == EposNodeType.Node)
        {
            EposNode newNode = new EposNode(Guid.NewGuid(), mousePosition, EposNodeType.Node, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode);
            this.m_eposData.m_nodes.Add(newNode);
        }
        else
        {
            EposNodeConditionnal newNode = new EposNodeConditionnal(Guid.NewGuid(), mousePosition, nodeType, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode);
            this.m_eposData.m_nodes.Add(newNode);
        }
    }

    private void OnClickInPoint(EposConnectionPoint inPoint)
    {
        selectedInPoint = inPoint;

        if (selectedOutPoint != null)
        {
            if ((selectedOutPoint.nodeData != selectedInPoint.nodeData))
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
            if ((selectedOutPoint.nodeData != selectedInPoint.nodeData))
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

		EposNodeData nodeIn = connection.inPoint.nodeData;
		EposNodeData nodeOut = connection.outPoint.nodeData;

        for (int i=0; i< this.m_eposData.m_nodes.Count; i++)
        {
			//EposNodeType nodeType = this.m_eposData.m_nodes[i].nodeData.m_nodeType;
            if(this.m_eposData.m_nodes[i].nodeData.m_uuid == nodeIn.m_uuid)
            {
                inNodeFound = true;
            }
            if (this.m_eposData.m_nodes[i].nodeData.m_uuid == nodeOut.m_uuid)
            {
                outNodeFound = true;
            }
            if (inNodeFound && outNodeFound)
            {
                this.m_eposData.m_connections.Remove(connection);
                break;
            }
        }
        
    }

    private void CreateConnection()
    {
        if (this.m_eposData.m_connections == null)
        {
            this.m_eposData.m_connections = new List<EposConnection>();
        }

		EposNodeData selectedInNode = selectedInPoint.GetNodeConnected();
		EposNodeData selectedOutNode = selectedOutPoint.GetNodeConnected();

        bool inNodeFound = false;
        bool outNodeFound = false;
        for (int i = 0; i < this.m_eposData.m_nodes.Count; i++)
        {
            if(this.m_eposData.m_nodes[i].nodeData.m_uuid == selectedInNode.m_uuid)
            {
                inNodeFound = true;
            }
            else if (this.m_eposData.m_nodes[i].nodeData.m_uuid == selectedOutNode.m_uuid)
            {
                outNodeFound = true;
            }
            if (inNodeFound && outNodeFound)
            {
                if (!this.m_eposData.FindExistingConnection(selectedInPoint, selectedOutPoint, this.m_eposData.m_connections))
                {
                    this.m_eposData.m_connections.Add(new EposConnection(selectedInPoint, selectedOutPoint, OnClickRemoveConnection));
                }
                break;
            }
        }
    }

    private void ClearConnectionSelection()
    {
        selectedInPoint = null;
        selectedOutPoint = null;
    }

    public void OnClickRemoveNode(EposNode node)
    {
        if (this.m_eposData.m_connections != null)
        {
            List<EposConnection> connectionsToRemove = new List<EposConnection>();

            for (int i = 0; i < this.m_eposData.m_connections.Count; i++)
            {
                if(node.nodeData.in_Points.Contains(this.m_eposData.m_connections[i].inPoint) || node.nodeData.out_Points.Contains(this.m_eposData.m_connections[i].outPoint))
                {
                    connectionsToRemove.Add(this.m_eposData.m_connections[i]);
                }
            }

            for (int i = 0; i < connectionsToRemove.Count; i++)
            {
                this.m_eposData.m_connections.Remove(connectionsToRemove[i]);
            }

            connectionsToRemove = null;
        }
        
        this.m_eposData.m_nodes.Remove(node);
        this.m_eposData.m_nodesData.Remove(node.nodeData);
    }
    //------------ Reset Window ------------------

    private void ResetWindow()
    {
        this.m_eposData = new EposData();
        GUI.changed = true;
    }

    //------------ Save Node tree ----------------

    private void SaveNodeTree()
    {
        string path = "Assets/test_nodetree.xml";
        EposXmlNodeContainer nodeXMLContainer = new EposXmlNodeContainer();

        nodeXMLContainer.pathToDialogScript = this.m_eposData.m_dialogScriptPath;


        
        foreach(EposConnection connect in this.m_eposData.m_connections)
        {
            EposXMLConnection connection_IN = new EposXMLConnection
            {
                uuid = connect.inPoint.nodeData.m_uuid,
                pinIndex = connect.inPoint.ptIndex
            };
            EposXMLConnection connection_OUT = new EposXMLConnection
            {
                uuid = connect.outPoint.nodeData.m_uuid,
                pinIndex = connect.outPoint.ptIndex
            };
            EposXMLConnections connections = new EposXMLConnections
            {
                node_in = connection_IN,
                node_out = connection_OUT
            };
            nodeXMLContainer.eposXmlConnections.Add(connections);
        }

        if (this.m_eposData.m_nodes != null)
        {
            EposXMLNode _xmlNode;
            foreach (EposNode node in this.m_eposData.m_nodes)
            {
                _xmlNode = new EposXMLNode
                {
                    uuid = node.nodeData.m_uuid,
                    title = node.title,
                    nodeType = node.nodeData.m_nodeType,
                    posX = node.nodeData.rect.position.x,
                    posY = node.nodeData.rect.position.y
                };
                switch (node.nodeData.m_nodeType)
				{
					case EposNodeType.Begin:
					case EposNodeType.End:
                    case EposNodeType.Conditionnal_AND:
                    case EposNodeType.Conditionnal_OR:
                        nodeXMLContainer.eposXMLNodes.Add(_xmlNode);
                        break;
					case EposNodeType.Node:
                        EposXMLNodeDialog xmlNodeDialog = new EposXMLNodeDialog
                        {
                            uuid = node.nodeData.m_uuid,
                            title = node.title,
                            nodeType = node.nodeData.m_nodeType,
                            posX = node.nodeData.rect.position.x,
                            posY = node.nodeData.rect.position.y,
                            wwiseEvent = node.nodeData.m_audioClip.name,//node.nodeData.m_wwiseEvent,
                            isQueued = node.nodeData.m_isQueued,
                            dialogIndex = node.nodeData.m_dialogIndex
                        };
		                nodeXMLContainer.eposXMLNodeDialogs.Add(xmlNodeDialog);
						break;
				}
            }
        }
        //Save xml file
        nodeXMLContainer.Save(path);
    }

    private void LoadNodeTree()
    {
        this.m_eposData.m_nodes = new List<EposNode>();
        this.m_eposData.m_connections = new List<EposConnection>();
        EposXmlNodeContainer nodeXmlContainer = EposXmlNodeContainer.Load(Path.Combine(Application.dataPath, "test_nodetree.xml"));
        this.m_eposData.m_dialogScriptPath = nodeXmlContainer.pathToDialogScript;
        this.m_eposData.ReadDialogFile(Path.Combine(Application.dataPath, this.m_eposData.m_dialogScriptPath));
        
        foreach(EposXMLNode xmlNode in nodeXmlContainer.eposXMLNodes)
        {
            switch(xmlNode.nodeType)
            {
                case EposNodeType.Begin:
                case EposNodeType.End:
                    this.m_eposData.m_nodes.Add(new EposNode(xmlNode.uuid, new Vector2(xmlNode.posX, xmlNode.posY), xmlNode.nodeType, OnClickInPoint, OnClickOutPoint));
                    break;
                case EposNodeType.Conditionnal_OR:
                case EposNodeType.Conditionnal_AND:
					EposNodeConditionnal nc = new EposNodeConditionnal(xmlNode.uuid, new Vector2(xmlNode.posX, xmlNode.posY),xmlNode.nodeType, OnClickInPoint, OnClickOutPoint,OnClickRemoveNode);
					this.m_eposData.m_nodes.Add(nc);
                    break;
				default:
                    break;
            }
            
        }
        foreach (EposXMLNodeDialog xmlNodeDialog in nodeXmlContainer.eposXMLNodeDialogs)
        {
            EposNode newNode = new EposNode(xmlNodeDialog.uuid, new Vector2(xmlNodeDialog.posX, xmlNodeDialog.posY), xmlNodeDialog.nodeType, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode);
            //newNode.nodeData.m_audioClip.name = xmlNodeDialog.wwiseEvent;
            newNode.nodeData.m_audioClip = Resources.Load<AudioClip>("02_Sounds/Test/" + xmlNodeDialog.wwiseEvent);
            newNode.nodeData.m_isQueued = xmlNodeDialog.isQueued;
            newNode.SetDialogIndex(xmlNodeDialog.dialogIndex);
            this.m_eposData.m_nodes.Add(newNode);
        }
        foreach (EposNode node in this.m_eposData.m_nodes)
        {
            this.m_eposData.m_nodesData.Add(node.nodeData);
        }
        this.m_eposData.LoadConnections(nodeXmlContainer.eposXmlConnections, OnClickRemoveConnection);
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
    public List<EposConnection> m_connections;

    public string m_nodePath;
    public string m_dialogScriptPath;
    public string[] m_dialogFileFilters = { "TSV sheets","tsv" };
    public List<string[]> m_dialogs;
    public string m_dialogFileName;

	public List<string[]> m_eventsList;

	public EposData()
	{
        m_nodes = new List<EposNode>();
        m_nodesData = new List<EposNodeData>();
        m_connections = new List<EposConnection>();
		m_dialogs = new List<string[]> ();
		GetWwiseEvents ();
	}

	public void GetWwiseEvents()
	{
        /*
		XmlDocument document = new XmlDocument ();
		document.Load(Application.dataPath+"/../../WwiseProject/GeneratedSoundBanks/Windows/SoundbanksInfo.xml");
		XmlNode root = document.DocumentElement;
		XmlNodeList list = document.GetElementsByTagName ("Event");
        */
        this.m_eventsList = new List<string[]>();
        /*
		foreach (XmlNode xn in list) {
            string[] wwiseEvt = new string[] {xn.Attributes["Id"].Value,xn.Attributes["Name"].Value };
            this.m_eventsList.Add(wwiseEvt);
		}
        */
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
            case EposNodeType.Conditionnal_OR:
            case EposNodeType.Conditionnal_AND:
                m_nodesData.Add(new EposNodeData(xmlNode.uuid, xmlNode.nodeType));
				break;
			default:
				break;
			}
		}
        foreach (EposXMLNodeDialog xmlNode in nodeXmlContainer.eposXMLNodeDialogs)
        {
            EposNodeData newNode = new EposNodeData(xmlNode.uuid, xmlNode.nodeType)
            {
                m_dialogIndex = xmlNode.dialogIndex,
                m_isQueued = xmlNode.isQueued
            };
            newNode.m_audioClip = Resources.Load<AudioClip>("02_Sounds/Test/" + xmlNode.wwiseEvent);
            m_nodesData.Add(newNode);
        }
        LoadConnections(nodeXmlContainer.eposXmlConnections);
	}
    public void LoadConnections(List<EposXMLConnections> xmlConnections, Action<EposConnection> OnClickRemoveConnection = null)
    {
        this.m_connections = new List<EposConnection>();
        foreach (EposXMLConnections connection in xmlConnections)
        {
            EposXMLConnection node_in = connection.node_in;
            EposXMLConnection node_out = connection.node_out;

            int nodeIn_Index = 0;
            int nodeOut_Index = 0;
            bool foundIn = false;
            bool foundOut = false;

            for (int i = 0; i < this.m_nodesData.Count; i++)
            {
                EposNodeData node = this.m_nodesData[i];
                if (node.m_uuid == node_in.uuid)
                {
                    nodeIn_Index = i;
                    foundIn = true;
                }
                else if (node.m_uuid == node_out.uuid)
                {
                    nodeOut_Index = i;
                    foundOut = true;
                }
                if (foundIn && foundOut)
                {
                    EposConnectionPoint inPoint = this.m_nodesData[nodeIn_Index].in_Points[node_in.pinIndex];
                    EposConnectionPoint outPoint = this.m_nodesData[nodeOut_Index].out_Points[node_out.pinIndex];
                    if (!FindExistingConnection(inPoint, outPoint, this.m_connections))
                    {
                        this.m_connections.Add(new EposConnection(inPoint, outPoint, OnClickRemoveConnection));
                    }
                    break;
                }
            }
        }
    }
    public bool FindExistingConnection(EposConnectionPoint inPoint, EposConnectionPoint outPoint, List<EposConnection> _connections)
    {
        foreach (EposConnection connection in _connections)
        {
            if (connection.inPoint == inPoint && connection.outPoint == outPoint)
                return true;
        }
        return false;
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

    public List<EposNodeData> GetNextNodes(EposNodeData currentNode)
    {
        List<EposNodeData> nodes = new List<EposNodeData>();

        foreach (EposConnection co in this.m_connections)
        {
            if (co.outPoint.nodeData.m_uuid == currentNode.m_uuid)
            {
                nodes.Add(co.inPoint.nodeData);
            }
        }
        return nodes;
    }
}
