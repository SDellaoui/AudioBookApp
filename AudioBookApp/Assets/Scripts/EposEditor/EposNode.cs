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
    public Rect wwiseTextFieldRect;
    public float rectWidth;
    public float rectHeight;
    public string title;
    
    public bool isDragged;
    public bool isSelected;

    public GUIStyle style;
    public GUIStyle defaultNodeStyle;
    public GUIStyle selectedNodeStyle;

    public Texture2D defaultBackground;
    public Texture2D selectedBackground;

    public Action<EposNode> OnRemoveNode;

    public string[] dialogs;

    public EposNode(Guid uuid, Vector2 position, EposNodeType _nodeType, Action<EposConnectionPoint> OnClickInPoint, Action<EposConnectionPoint> OnClickOutPoint, Action<EposNode> OnClickRemoveNode = null)
    {
        this.nodeData = new EposNodeData(uuid, _nodeType, OnClickInPoint, OnClickOutPoint);

        switch (_nodeType)
        {
            case EposNodeType.Begin:
                rectWidth = rectHeight = 50;
                title = "BEGIN";
                break;
            case EposNodeType.End:
                rectWidth = rectHeight = 50;
                title = "END";
                break;
            case EposNodeType.Node:
                rectWidth = 200;
                rectHeight = 100;
                title = "Dialog Node";
                OnRemoveNode = OnClickRemoveNode;
                break;
            default:
                break;
        }

        this.nodeData.rect = new Rect(position.x, position.y, this.rectWidth, this.rectHeight);

        InitNodeStyles();
        OnRemoveNode = OnClickRemoveNode;
    }

    public void InitNodeStyles()
    {
        this.style = new GUIStyle();
        this.defaultBackground = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
        this.selectedBackground = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D;
        this.style.normal.background = this.defaultBackground;
        switch(this.nodeData.m_nodeType)
        {
            case EposNodeType.Begin:
            case EposNodeType.End:
                this.style.border = new RectOffset(25, 25, 25, 25);
                break;
            default:
                this.style.border = new RectOffset(12, 12, 12, 12);
                break;
        }
    }

    public void Drag(Vector2 delta)
    {
        this.nodeData.rect.position += delta;
    }

    public void Draw()
    {
        //Manage backgrounds
        this.style.normal.background = (isSelected)?this.selectedBackground:this.defaultBackground;


        GUI.depth = 0;
        GUI.Box(this.nodeData.rect, "", style);
        GUI.skin.label.alignment = TextAnchor.UpperCenter;
        Rect label = new Rect(this.nodeData.rect.x, this.nodeData.rect.y-14, rectWidth, rectHeight);
        GUI.Label(label, title);
        foreach (EposConnectionPoint pt in this.nodeData.in_Points)
        {
            pt.Draw();
        }
        foreach (EposConnectionPoint pt in this.nodeData.out_Points)
        {
            pt.Draw();
        }
        switch (this.nodeData.m_nodeType)
        {
            case EposNodeType.Node:

                GUI.depth = 2;
                Rect dialogTitle = new Rect(this.nodeData.rect.x + 14, this.nodeData.rect.y + 15, this.nodeData.rect.width - 27, 20);
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
                    if (this.nodeData.rect.Contains(e.mousePosition))
                    {
                        isDragged = true;
                        isSelected = true;
                        GUI.changed = true;
                        //style = selectedNodeStyle;
                    }
                    else
                    {
                        isSelected = false;
                        GUI.changed = true;
                        //style = defaultNodeStyle;
                    }
                }
                if (e.button == 1 && isSelected && this.nodeData.rect.Contains(e.mousePosition))
                {
                    if ((this.nodeData.m_nodeType != EposNodeType.Begin || this.nodeData.m_nodeType != EposNodeType.End))
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
        if (OnRemoveNode != null)
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
    private bool dispatched = false;
    private int m_nInputsReceived;

    public Rect rect;
    public Guid m_uuid;
	public string m_wwiseEvent;

	public bool m_isQueued;

    public List<EposConnectionPoint> in_Points;
    public List<EposConnectionPoint> out_Points;

    public Action<EposConnectionPoint> _OnClickInPoint;
    public Action<EposConnectionPoint> _OnClickOutPoint;

    public EposNodeType m_nodeType;

    public string[] m_dialogs;
    public int m_dialogIndex;

    public int m_nInputs;
    public int m_nOutputs;
    
    

	public EposNodeData(Guid uuid, EposNodeType nodeType, Action<EposConnectionPoint> OnClickInPoint = null, Action<EposConnectionPoint> OnClickOutPoint = null)
	{
		this.m_uuid = uuid;
		this.m_nodeType = nodeType;
        
        this._OnClickInPoint = OnClickInPoint;
        this._OnClickOutPoint = OnClickOutPoint;

        this.m_nInputs = this.m_nOutputs = 1;

        this.m_dialogs = new string[0];
        this.m_wwiseEvent = "";
        this.m_isQueued = false;
        this.m_nInputsReceived = 0;

        SetConnectionPoints();
	}
    public void SetConnectionPoints()
    {
        this.in_Points = new List<EposConnectionPoint>();
        this.out_Points = new List<EposConnectionPoint>();

        EposConnectionPoint inPoint = (this._OnClickInPoint == null) ? new EposConnectionPoint(this, ConnectionPointType.In,this.m_nInputs) : new EposConnectionPoint(this, ConnectionPointType.In, this._OnClickInPoint, this.m_nInputs);
        EposConnectionPoint outPoint = (this._OnClickOutPoint == null) ? new EposConnectionPoint(this, ConnectionPointType.Out, this.m_nOutputs) : new EposConnectionPoint(this, ConnectionPointType.Out, this._OnClickOutPoint, this.m_nOutputs);

        switch (this.m_nodeType)
        {
            case EposNodeType.Begin:
                this.out_Points.Add(outPoint);
                break;
            case EposNodeType.End:
                this.in_Points.Add(inPoint);
                break;
            case EposNodeType.Node:
                this.in_Points.Add(inPoint);
                this.out_Points.Add(outPoint);
                break;
            case EposNodeType.Conditionnal_OR:
            case EposNodeType.Conditionnal_AND:
                this.m_nInputs = 2;
                for (int i = 0; i < this.m_nInputs; i++)
                {
                    inPoint = (this._OnClickInPoint == null) ? new EposConnectionPoint(this, ConnectionPointType.In, this.m_nInputs,i) : new EposConnectionPoint(this, ConnectionPointType.In, this._OnClickInPoint, this.m_nInputs,i);
                    this.in_Points.Add(inPoint);
                }
                this.out_Points.Add(outPoint);
                break;
            default:
                break;
        }
    }
	//---------------------- Wwise Events ---------------------------------

	public void Start()
	{
        //Debug.Log("Start node " + m_uuid);
        m_nInputsReceived++;
        if (EposNodeType.Begin == m_nodeType || EposNodeType.End == m_nodeType || EposNodeType.Conditionnal_OR == m_nodeType)
            End();
        else if (EposNodeType.Conditionnal_AND == m_nodeType && m_nInputsReceived == m_nInputs)
        { 
            End();
        }
        else if (EposNodeType.Node == m_nodeType)
        {
            EposEventManager.Instance.PostEvent(m_uuid, m_wwiseEvent);

            if (!m_isQueued)
            {
                End();
            }
        }
	}
	public void End()
	{
        if (dispatched)
            return;
        //Debug.Log("Nb Inputs received "+m_nInputsReceived+" for node "+m_nodeType.ToString());
        List<EposNodeData> nextNodes = EposNodeReader.Instance.GetNodeTree().GetNextNodes(this);
        
        foreach(EposNodeData nextN in nextNodes)
        {
            nextN.Start();
        }
        dispatched = true;
		EposEventManager.Instance.StopEventCoroutine(this);
	}

	public float PlaySound()
	{
        if (m_wwiseEvent == "" || m_nodeType != EposNodeType.Node)
			return 0f;

        Debug.Log("Test playing audioclip");
        AudioSource dialogSource = EposEventManager.Instance.gameObject.transform.Find("Sound/DialogNode").GetComponent<AudioSource>();
        dialogSource.clip = Resources.Load<AudioClip>("02_Sounds/Test/script_test_bloc_01");
        dialogSource.Play();

        return dialogSource.clip.length;
	}
}