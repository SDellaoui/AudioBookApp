using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EposEventManager : MonoBehaviour {

    private List<Guid> coroutinesUUID;
    private List<IEnumerator> coroutines;

	private static EposEventManager _instance;

    public GameObject listener;
    public GameObject dialogCanvas;

    public static EposEventManager Instance
	{
		get {
			return _instance;
		}
		set{ }
	}

	void Awake()
	{
		if (_instance == null) {
			_instance = this;
			DontDestroyOnLoad (this.gameObject);
		} else
			Destroy (this.gameObject);
	}

	// Use this for initialization
	void Start () {
         
	}
    public void InitEventManager()
    {
        coroutinesUUID = new List<Guid>();
        coroutines = new List<IEnumerator>();
        foreach (EposNodeData node in EposNodeReader.Instance.GetNodes())
        {
			coroutinesUUID.Add(node.m_uuid);
			IEnumerator _event = PostEventCoroutine(node, node.m_uuid.ToString());
            coroutines.Add(_event);
        }
        EposNodeReader.Instance.BeginTree();
    }
	// Update is called once per frame
	void Update () {
		
	}
    public void PostEvent(Guid nodeID, string eventName)
    {
        for(int i=0; i<coroutinesUUID.Count; i++)
        {
            if(coroutinesUUID[i] == nodeID)
            {
                StartCoroutine(coroutines[i]);
                break;
            }
        }
    }
	IEnumerator PostEventCoroutine(EposNodeData node, string eventName)
    {
        node.PlaySound();
        yield return null;
    }
    public void StopEventCoroutine(EposNodeData _node)
    {
        for (int i = 0; i < coroutinesUUID.Count; i++)
        {
			if (coroutinesUUID[i] == _node.m_uuid)
            {
                StopCoroutine(coroutines[i]);
                break;
            }
        }
    }
}
