using System;
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

    public bool isQueued;
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

    public string[] dialogs;
    public int dialogIndex;

    public EposNode(Guid uuid, Vector2 position, EposNodeType _nodeType, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle, Action<EposConnectionPoint> OnClickInPoint, Action<EposConnectionPoint> OnClickOutPoint, string wwiseEvent = "", bool isQueued = false, Action<EposNode> OnClickRemoveNode = null, List<Guid> in_nodes = null, List<Guid> out_nodes = null)
    {
        this.uuid = uuid;
        this.isQueued = isQueued;
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
        this.wwiseEvent = wwiseEvent;

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
        switch (nodeType)
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

                /*
                GUI.depth = 1;
                wwiseTextFieldRect = new Rect(rect.x + 15, rect.y + 15, rectWidth - 27, 20);
                wwiseEvent = EditorGUI.TextField(wwiseTextFieldRect, "", wwiseEvent);
                */
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

                    dialogIndex = EditorGUILayout.Popup("",dialogIndex, dialogs, EditorStyles.popup);
                    wwiseEvent = "Play_"+dialogs[dialogIndex];
                    //dialogIndex = EditorGUILayout.Popup(dialogIndex, dialogs);
                    GUILayout.EndArea();
                }
                isQueued = EditorGUI.ToggleLeft(new Rect(dialogArea.x, dialogArea.y + 20, 15, 15), " Dispatch on end", isQueued, GUIStyle.none);
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
    public void RemoveConnectedNode(short in_out,Guid nodeUUID)
    {
        switch(in_out)
        {
            case 0:
                for (int i = 0; i < inNodes.Count; i++)
                {
                    Debug.Log(inNodes[i]);
                    if (inNodes[i] == nodeUUID)
                    {
                        inNodes.RemoveAt(i);
                        break;
                    }
                }
                break;
            case 1:
                for (int i = 0; i < outNodes.Count; i++)
                {
                    if (outNodes[i] == nodeUUID)
                    {
                        outNodes.RemoveAt(i);
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
        if (OnRemoveNode != null && nodeType == EposNodeType.Node)
        {
            OnRemoveNode(this);
        }
    }
	/*
    //---------------------- Wwise Events ---------------------------------

    public void Start()
    {
        Debug.Log("Receiving Input on node " + uuid);
        if (EposNodeType.Begin == nodeType)
            this.wwiseEvent = "Play_Ping_Out";
        else if (EposNodeType.End == nodeType)
            this.wwiseEvent = "Play_Ping_In";
        EposEventManager.Instance.PostEvent(uuid, wwiseEvent);

        if (!isQueued && nodeType != EposNodeType.End)
        {
            End();
        }
    }
    public void End()
    {
        Debug.Log("Sending Output from node " + uuid);
        
        foreach (Guid nextNode in outNodes)
        {
            EposNode outNode = EposNodeReader.Instance.GetNodeFromUUID(nextNode);
            if (outNode != null)
            {
                outNode.Start();
            }
        }
        EposEventManager.Instance.StopEventCoroutine(this);
    }

    public void PlaySound()
    {
        if (wwiseEvent == "" || nodeType != EposNodeType.Node)
            return;
        if (isQueued)
            AkSoundEngine.PostEvent(wwiseEvent, EposEventManager.Instance.listener, (uint)0x0009, WwiseCallback, this);
        else
            AkSoundEngine.PostEvent(wwiseEvent, EposEventManager.Instance.listener);
        EposEventManager.Instance.dialogCanvas.GetComponent<ContentController>().DisplayNewCharacterDialog(EposNodeReader.Instance.GetDialogLine(dialogIndex));
    }

    void WwiseCallback(object in_cookie, AkCallbackType in_type, object in_info)
    {
        if(in_type == AkCallbackType.AK_Duration)
        {
            //Debug.Log("Event Started");
        }
        if (in_type == AkCallbackType.AK_EndOfEvent)
        {
            Debug.Log("[WWISE] reached end of event : " + wwiseEvent);
            End();
        }
    }
	*/
    //------------------- Dialog Dropdown list ----------------
    public void SetDialogIndex(int index)
    {
        this.dialogIndex = index;
        //wwiseEvent = "Play_" + dialogs[this.dialogIndex];
        //Debug.Log(wwiseEvent);
    }
    public void SetDialogList(string dialogFileName, List<string[]> dialogs)
    {
        if (nodeType != EposNodeType.Node)
            return;
        this.dialogs = new string[dialogs.Count];
        for(int i=0; i<dialogs.Count; i++)
        {
            this.dialogs[i] = "VO_"+dialogFileName+"_"+dialogs[i][0] + "_" + i;
        }
    }
}


public class EposNodeData
{
	private Guid m_uuid;
	private string m_wwiseEvent;

	private bool m_isQueued;

	private List<Guid> m_inNodes;
	private List<Guid> m_outNodes;

	private EposNodeType m_nodeType;

	private string[] m_dialogs;
	private int m_dialogIndex;

	public EposNodeData(Guid uuid, EposNodeType nodeType, int dialogIndex = 0, string wwiseEvent = "", bool isQueued = false, List<Guid> in_nodes = null, List<Guid> out_nodes = null)
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
	}

	public Guid GetUUID()
	{
		return this.m_uuid;
	}
	public EposNodeType GetNodeType()
	{
		return this.m_nodeType;
	}

	//---------------------- Wwise Events ---------------------------------

	public void Start()
	{
		Debug.Log("Receiving Input on node " + m_uuid);
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
		Debug.Log("Sending Output from node " + m_uuid);

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
			Debug.Log("[WWISE] reached end of event : " + m_wwiseEvent);
			End();
		}
	}
}