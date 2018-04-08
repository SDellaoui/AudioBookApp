using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EposNodeReader : MonoBehaviour {

    private static EposNodeReader _instance;

    private string storyDataPath;

    //EposNodeEditor _nodeTree;
	EposData _nodeTree;
    //List<EposNode> _nodes;
	List<EposNodeData> _nodes;

    public static EposNodeReader Instance {
        get {
            return _instance;
        }
        set { }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
            Destroy(this.gameObject);
    }
    // Use this for initialization
    void Start () {
		_nodeTree = new EposData ();
		_nodeTree.LoadNodeTree ();
		_nodes = _nodeTree.GetNodes ();
		EposEventManager.Instance.InitEventManager();
        //InitNodeTree();

    }
	
	// Update is called once per frame
	void Update () {
		
	}
	/*
    void InitNodeTree()
    {
        _nodeTree = ScriptableObject.CreateInstance<EposNodeEditor>();

        _nodeTree.LoadFile();
        _nodes = _nodeTree.GetNodes();
        EposEventManager.Instance.InitEventManager();
        _nodeTree.Close();
    }
	*/
    public string GetDialogLine(int lineIndex)
    {
        return _nodeTree.ReadDialogLine(lineIndex);
    }
    public void BeginTree() {
        foreach(EposNodeData node in _nodes)
        {
			if(node.m_nodeType == EposNodeType.Begin)
            {
                node.Start();
                break;
            }
        }

    }
    public void ExecuteNodeEvent(string node)
    {
        Debug.Log(node);
    }

    public EposNodeData GetNodeFromUUID(Guid uuid)
    {
        if (_nodes != null)
        {
            foreach (EposNodeData node in _nodes)
            {
				if (node.m_uuid == uuid)
                    return node;
            }
        }
        return null;
    }
    public EposData GetNodeTree()
    {
        return this._nodeTree;
    }
    public List<EposNodeData> GetNodes()
    {
        return _nodes;
    }
}
