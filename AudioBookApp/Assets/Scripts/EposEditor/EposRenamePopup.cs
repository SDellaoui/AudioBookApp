using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EposRenamePopup : PopupWindowContent {

	string _newTitle;
	Vector2 displayPosition;

	EposNode _node;

	public EposRenamePopup(EposNode node)
	{
		_node = node;
		_newTitle = _node.title;
		//EposNodeEditor editor = EditorWindow.GetWindow(EposNodeEditor);
		EposNodeEditor window = (EposNodeEditor)EditorWindow.GetWindow(typeof(EposNodeEditor));
		Debug.Log(window.minSize);
	}

	public override Vector2 GetWindowSize()
	{
		return new Vector2(200, 150);
	}

    public override void OnGUI(Rect rect)
    {
		GUI.skin.label.alignment = TextAnchor.UpperCenter;
		GUILayout.Label("Enter a new node name", EditorStyles.boldLabel);
		_newTitle = EditorGUILayout.TextField("",_newTitle);
		if (GUILayout.Button("Popup Options", GUILayout.Width(200)))
		{
			Debug.Log("new text : "+_newTitle);
			_node.title = _newTitle;
			editorWindow.Close();
		}
    }

    public override void OnOpen()
    {
    }
    public override void OnClose()
    {
    }
}

//https://docs.unity3d.com/ScriptReference/PopupWindow.html
