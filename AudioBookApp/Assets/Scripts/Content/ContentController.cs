using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContentController : MonoBehaviour {

	public GameObject contentPanel;
	public GameObject itemPrefab;
	// Use this for initialization
	void Start () {

		for (int i = 0; i < 10; i++) {
			GameObject item = Instantiate (itemPrefab) as GameObject;
			item.transform.SetParent(contentPanel.transform, false);
			//item.transform.parent = contentPanel.transform;
		}

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
