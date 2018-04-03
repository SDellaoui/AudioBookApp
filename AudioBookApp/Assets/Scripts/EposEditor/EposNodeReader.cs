using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EposNodeReader : MonoBehaviour {

    private static EposNodeReader _instance;

    private string storyDataPath;

    EposNodeEditor _nodeTree;
    List<EposNode> _nodes;

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
		EposData data = new EposData ();
		data.TestStaticClass ();
        InitNodeTree();

    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void InitNodeTree()
    {
        _nodeTree = ScriptableObject.CreateInstance<EposNodeEditor>();

        _nodeTree.LoadFile();
        _nodes = _nodeTree.GetNodes();
        EposEventManager.Instance.InitEventManager();
        _nodeTree.Close();
    }
    public string GetDialogLine(int lineIndex)
    {
        return _nodeTree.ReadDialogLine(lineIndex);
    }
    public void BeginTree() { 

        foreach(EposNode node in _nodes)
        {
            if(node.nodeType == EposNodeType.Begin)
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

    public EposNode GetNodeFromUUID(Guid uuid)
    {
        if (_nodes != null)
        {
            foreach (EposNode node in _nodes)
            {
                if (node.uuid == uuid)
                    return node;
            }
        }
        return null;
    }
    public List<EposNode> GetNodes()
    {
        return _nodes;
    }
}
