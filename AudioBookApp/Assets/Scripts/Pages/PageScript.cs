using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PageScript : MonoBehaviour {

	// Use this for initialization
	void Awake()
	{
		GameObject mainCanvas = GameObject.Find ("ContentCanvas");
		//GameObject scrollview = Instantiate(Resources.Load ("00_Prefabs/ScrollView", typeof(GameObject)) as GameObject);
		//scrollview.transform.parent = mainCanvas.transform;
	}
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void GoToPage(string page)
	{
		SceneManager.LoadScene (page);
	}
}
