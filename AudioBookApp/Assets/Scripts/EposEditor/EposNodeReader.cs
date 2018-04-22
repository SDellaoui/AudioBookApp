using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Net;

public class EposNodeReader : MonoBehaviour {
#if !UNITY_EDITOR
    int nbDialogs, audioClipsLoaded = 0;
    private bool isNodeTreeFileLoaded, isDialogFileLoaded, isAudioReadyToPlay, isNodeTreeReady = false;
#endif
    private static EposNodeReader _instance;

    private string storyDataPath;


    

    EposData _nodeTree;
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
#if UNITY_EDITOR
        _nodeTree.LoadNodeTree ();
        _nodes = _nodeTree.GetNodes();
        EposEventManager.Instance.InitEventManager();
#else
        StartCoroutine(GetNodeTreeFromServer("http://localhost/AudioBookApp/test_nodetree.xml"));
        StartCoroutine(GetDialogFileFromServer("http://localhost/AudioBookApp/Test_Dialog.tsv"));
#endif
    }
	
	// Update is called once per frame
	void Update () {
#if !UNITY_EDITOR
        if(isNodeTreeFileLoaded && isDialogFileLoaded && isAudioReadyToPlay && !isNodeTreeReady)
        {
            _nodes = _nodeTree.GetNodes();
            EposEventManager.Instance.InitEventManager();
            isNodeTreeReady = true;
        }
#endif
    }

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
#if !UNITY_EDITOR
    //--------------- Load Node Tree Data ------------------
    IEnumerator GetNodeTreeFromServer(string url)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();
        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            
            _nodeTree.m_nodeXmlContainer = EposXmlNodeContainer.LoadFromText(www.downloadHandler.text);
            _nodeTree.PopulateNodeTree();

            isNodeTreeFileLoaded = true;
        }
    }
    //--------------- Load Dialog File Data ------------------
    IEnumerator GetDialogFileFromServer(string url)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();
        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            if (_nodeTree.m_dialogs == null || _nodeTree.m_dialogs.Count > 0)
                _nodeTree.m_dialogs = new List<string[]>();
            
            StreamReader theReader = new StreamReader(WebRequest.Create(url).GetResponse().GetResponseStream(), Encoding.UTF8);
            _nodeTree.m_dialogs = _nodeTree.PopulateDialogFile(theReader);
            this.nbDialogs = _nodeTree.m_dialogs.Count;
            _nodeTree.LoadAudioClips();
            isDialogFileLoaded = true;
        }
    }

    //----------- Load Audioclips -----------
    public void LoadAudioClip(EposNodeData nodeData,string clipname)
    {
        StartCoroutine(GetAudioClip(nodeData,clipname));
    }
    IEnumerator GetAudioClip(EposNodeData nodeData, string clipname)
    {
        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("http://localhost/AudioBookApp/" + clipname + ".ogg", AudioType.OGGVORBIS);
        yield return www.SendWebRequest();
        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            nodeData.m_audioClip = DownloadHandlerAudioClip.GetContent(www);
            this.audioClipsLoaded++;
            if(this.audioClipsLoaded == nbDialogs)
                isAudioReadyToPlay = true;
        }
    }
#endif
}