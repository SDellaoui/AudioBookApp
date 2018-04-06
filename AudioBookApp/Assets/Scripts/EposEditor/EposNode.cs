using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum EposNodeType {
    Begin,
    End,
    Node,
    Conditionnal_OR,
    Conditionnal_AND
};

public class EposNode{

    public EposNodeData nodeData;

    public Rect rect;
    public Rect wwiseTextFieldRect;
    public float rectWidth;
    public float rectHeight;
    public string title;
    
    public bool isDragged;
    public bool isSelected;

    public EposConnectionPoint inPoint;
    public EposConnectionPoint outPoint;

	public List<EposConnectionPoint> in_Points;

    public GUIStyle style;
    public GUIStyle defaultNodeStyle;
    public GUIStyle selectedNodeStyle;

    public Action<EposNode> OnRemoveNode;

    public string[] dialogs;

    public EposNode(Guid uuid, Vector2 position, EposNodeType _nodeType, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle, Action<EposConnectionPoint> OnClickInPoint, Action<EposConnectionPoint> OnClickOutPoint, string wwiseEvent = "", bool isQueued = false, Action<EposNode> OnClickRemoveNode = null, List<Guid> in_nodes = null, List<Guid> out_nodes = null)
    {
        this.nodeData = new EposNodeData(uuid, _nodeType, 0, wwiseEvent, isQueued, in_nodes, out_nodes);

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

        style = nodeStyle;
        defaultNodeStyle = nodeStyle;
        selectedNodeStyle = selectedStyle;

    }

    public void Drag(Vector2 delta)
    {
        rect.position += delta;
    }

    public void Draw()
    {

        GUI.depth = 0;
        GUI.Box(rect, "", style);
        GUI.skin.label.alignment = TextAnchor.UpperCenter;
        Rect label = new Rect(rect.x, rect.y-14, rectWidth, rectHeight);
        GUI.Label(label, title);
        switch (this.nodeData.m_nodeType)
        {
            case EposNodeType.Begin:
                outPoint.Draw();
                break;
            case EposNodeType.End:
                inPoint.Draw();
                break;
            case EposNodeType.Node:
                inPoint.Draw();
                outPoint.Draw();

                GUI.depth = 2;
                Rect dialogTitle = new Rect(rect.x + 14, rect.y + 15, rect.width - 27, 20);
                GUI.skin.label.fontStyle = FontStyle.Bold;
                GUI.Label(dialogTitle, "Dialog line");
                Rect dialogArea = dialogTitle;
                dialogArea.y += 25;

                if (dialogs != null)
                {
                    Rect dialogNodeLoad = wwiseTextFieldRect;
                    dialogNodeLoad.y = wwiseTextFieldRect.y + 50;
                    GUILayout.BeginArea(dialogArea);
                    this.nodeData.m_dialogIndex = EditorGUILayout.Popup("", this.nodeData.m_dialogIndex,dialogs, EditorStyles.popup);
					if(dialogs.Length > 0)
                    	this.nodeData.m_wwiseEvent = "Play_"+ dialogs[this.nodeData.m_dialogIndex];
                    GUILayout.EndArea();
                }
                this.nodeData.m_isQueued = EditorGUI.ToggleLeft(new Rect(dialogArea.x, dialogArea.y + 20, 15, 15), " Dispatch on end", this.nodeData.m_isQueued, GUIStyle.none);
                break;
			case EposNodeType.Conditionnal_OR:
				foreach (EposConnectionPoint pt in in_Points) {
					pt.Draw ();
					pt.rect.y = pt.node.rect.y + pt.rect.y;// + (this.rect.height * 0.5f) - pt.rect.height * 0.5f;
				}
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
                    if (this.nodeData.m_nodeType == EposNodeType.Node)
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
                if(this.nodeData.m_inNodes == null)
                {
                    this.nodeData.m_inNodes = new List<Guid>();
                }
                this.nodeData.m_inNodes.Add(nodeUUID);
                break;
            case 1:
                if (this.nodeData.m_outNodes == null)
                {
                    this.nodeData.m_outNodes = new List<Guid>();
                }
                this.nodeData.m_outNodes.Add(nodeUUID);
                break;
            default:
                break;
        }
    }
    public void RemoveConnectedNode(short in_out,Guid nodeUUID)
    {
        switch(in_out)
        {
            case 0:
                for (int i = 0; i < this.nodeData.m_inNodes.Count; i++)
                {
                    Debug.Log(this.nodeData.m_inNodes[i]);
                    if (this.nodeData.m_inNodes[i] == nodeUUID)
                    {
                        this.nodeData.m_inNodes.RemoveAt(i);
                        break;
                    }
                }
                break;
            case 1:
                for (int i = 0; i < this.nodeData.m_outNodes.Count; i++)
                {
                    if (this.nodeData.m_outNodes[i] == nodeUUID)
                    {
                        this.nodeData.m_outNodes.RemoveAt(i);
                        break;
                    }
                }
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
        if (OnRemoveNode != null && this.nodeData.m_nodeType == EposNodeType.Node)
        {
            OnRemoveNode(this);
        }
    }

    //------------------- Dialog Dropdown list ----------------
    public void SetDialogIndex(int index)
    {
        this.nodeData.m_dialogIndex = index;
    }
    public void SetDialogList(string dialogFileName, List<string[]> dialogs)
    {
        if (this.nodeData.m_nodeType != EposNodeType.Node)
            return;
        this.dialogs = new string[dialogs.Count];
        for(int i=0; i<dialogs.Count; i++)
        {
            this.dialogs[i] = "VO_"+dialogFileName+"_"+dialogs[i][0] + "_" + i;
        }
    }
}

// Class containing node data. used both on editor and runtime.
public class EposNodeData
{
	public Guid m_uuid;
	public string m_wwiseEvent;

	public bool m_isQueued;

    public List<Guid> m_inNodes;
    public List<Guid> m_outNodes;

    public EposNodeType m_nodeType;

    public string[] m_dialogs;
    public int m_dialogIndex;

    public Action<string,string> Coucou;

	public EposNodeData(Guid uuid, EposNodeType nodeType, int dialogIndex = 0, string wwiseEvent = "", bool isQueued = false, List<Guid> in_nodes = null, List<Guid> out_nodes = null, Action<string,string> Coucou = null)
	{
		this.m_uuid = uuid;
		this.m_nodeType = nodeType;
		this.m_wwiseEvent = wwiseEvent;
		this.m_isQueued = isQueued;
		this.m_inNodes = in_nodes;
		this.m_outNodes = out_nodes;

		this.m_dialogs = new string[0];
		this.m_dialogIndex = dialogIndex;
		this.m_wwiseEvent = wwiseEvent;

        this.Coucou = Coucou;
	}

	//---------------------- Wwise Events ---------------------------------

	public void Start()
	{
		//Debug.Log("Receiving Input on node " + m_uuid);
		if (EposNodeType.Begin == m_nodeType)
			this.m_wwiseEvent = "Play_Ping_Out";
		else if (EposNodeType.End == m_nodeType)
			this.m_wwiseEvent = "Play_Ping_In";
		EposEventManager.Instance.PostEvent(m_uuid, m_wwiseEvent);

		if (!m_isQueued && m_nodeType != EposNodeType.End)
		{
			End();
		}
	}
	public void End()
	{
		//Debug.Log("Sending Output from node " + m_uuid);

		foreach (Guid nextNode in m_outNodes)
		{
			EposNodeData outNode = EposNodeReader.Instance.GetNodeFromUUID(nextNode);
			if (outNode != null)
			{
				outNode.Start();
			}
		}
		EposEventManager.Instance.StopEventCoroutine(this);
	}

	public void PlaySound()
	{
        if(Coucou != null)
            Coucou("Si tu lis ça c'est que t'es un gros débile","prout");
		if (m_wwiseEvent == "" || m_nodeType != EposNodeType.Node)
			return;
		if (m_isQueued)
			AkSoundEngine.PostEvent(m_wwiseEvent, EposEventManager.Instance.listener, (uint)0x0009, WwiseCallback, this);
		else
			AkSoundEngine.PostEvent(m_wwiseEvent, EposEventManager.Instance.listener);
		EposEventManager.Instance.dialogCanvas.GetComponent<ContentController>().DisplayNewCharacterDialog(EposNodeReader.Instance.GetDialogLine(m_dialogIndex));
	}

	void WwiseCallback(object in_cookie, AkCallbackType in_type, object in_info)
	{
		if(in_type == AkCallbackType.AK_Duration)
		{

		}
		if (in_type == AkCallbackType.AK_EndOfEvent)
		{
			//Debug.Log("[WWISE] reached end of event : " + m_wwiseEvent);
			End();
		}
	}
}