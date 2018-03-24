using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public abstract class EposBaseNode : ScriptableObject {

	public Rect windowRect;
	public bool hasInputs = false;
	public string windowTitle = "";

	public virtual void DrawWindow()
	{
		windowTitle = EditorGUILayout.TextField("Title",windowTitle);
	}

	public abstract void DrawCurves();

	public virtual void SetInput(EposBaseInputNode node, Vector2 clickPos)
	{
		
	}
	public virtual void NodeDeleted(EposBaseNode node)
	{
		
	}
	public virtual EposBaseNode ClickedOnInput(Vector2 pos){
		return null;
	}
}
