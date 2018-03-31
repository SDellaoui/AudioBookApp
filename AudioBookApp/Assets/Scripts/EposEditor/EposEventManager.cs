using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EposEventManager : MonoBehaviour {

	EposNodeEditor _nodeEditor;

	private static EposEventManager _instance;
	public static EposEventManager Instance
	{
		get {
			return Instance;
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
		StartEpos ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void StartEpos()
	{
		_nodeEditor = UnityEditor.EditorWindow.GetWindow<EposNodeEditor> ();
	}
}
