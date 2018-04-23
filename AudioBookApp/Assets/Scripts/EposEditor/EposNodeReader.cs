using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Net;

public class EposNodeReader : MonoBehaviour {

    int nbDialogs, audioClipsLoaded = 0;
    private bool isNodeTreeFileLoaded, isDialogFileLoaded, isAudioReadyToPlay, isNodeTreeReady = false;

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
        StartCoroutine(GetNodeTreeFromServer("http://127.0.0.1/AudioBookApp/test_nodetree.xml"));
        /*
#if UNITY_EDITOR
        _nodeTree.LoadNodeTree ();
        _nodes = _nodeTree.GetNodes();
        EposEventManager.Instance.InitEventManager();
#else
        StartCoroutine(GetNodeTreeFromServer("http://127.0.0.1/AudioBookApp/test_nodetree.xml"));
        
#endif
*/
    }

    // Update is called once per frame
    void Update () {
        if(isNodeTreeFileLoaded && isDialogFileLoaded && isAudioReadyToPlay && !isNodeTreeReady)
        {
            _nodes = _nodeTree.GetNodes();
            EposEventManager.Instance.InitEventManager();
            isNodeTreeReady = true;
        }
    }

    IEnumerator GetAudioClip(string clipname)
    {
        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("http://127.0.0.1/AudioBookApp/" + clipname + ".mp3", AudioType.MPEG);
        yield return www.SendWebRequest();
        /*
        while (!www.isDone)
        {
            Debug.Log(www.downloadProgress);
            
        }
        yield return null;
        */
        Debug.Log("Play");
        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            AudioSource Source = gameObject.AddComponent<AudioSource>();
            Source.spatialBlend = 0;
            AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
            Source.clip = clip;
            Source.Play();
        }
        //yield return StartCoroutine(ShowDownloadProgress(www));
        /*
        yield return www.SendWebRequest();
        /*
        Debug.Log("Play");
        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            AudioSource Source = gameObject.AddComponent<AudioSource>();
            Source.spatialBlend = 0;
            AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
            Source.clip = clip;
            Source.Play();
        }
        */
    }
    IEnumerator ShowDownloadProgress(UnityWebRequest www)
    {
        while(!www.isDone)
        {
            Debug.Log(www.downloadProgress);
            yield return null;
        }
        Debug.Log("Play");
        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            AudioSource Source = gameObject.AddComponent<AudioSource>();
            Source.spatialBlend = 0;
            AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
            Source.clip = clip;
            Source.Play();
        }
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
            isAudioReadyToPlay = true;
            StartCoroutine(GetDialogFileFromServer("http://127.0.0.1/AudioBookApp/Test_Dialog.tsv"));
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
        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("http://127.0.0.1/AudioBookApp/" + clipname + ".wav", AudioType.WAV);
        yield return www.SendWebRequest();
        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            nodeData.m_audioClip = DownloadHandlerAudioClip.GetContent(www);
            this.audioClipsLoaded++;
            Debug.Log("Nombre de dialogues "+nbDialogs+" Audioclips chargés : "+this.audioClipsLoaded);
            if(this.audioClipsLoaded == nbDialogs)
                isAudioReadyToPlay = true;
        }
    }
}