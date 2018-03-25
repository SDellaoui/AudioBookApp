using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public enum EposPopupType
{
    Rename
};


public class EposPopup : EditorWindow {

    

	string _newTitle;
	Vector2 displayPosition;

    EposNodeEditor mainWindow;
    EposPopup popupWindow;
	EposNode _node;

	public EposPopup(EposNode node, EposPopupType popupType)
	{
        switch(popupType)
        {
            case EposPopupType.Rename:
                this.titleContent = new GUIContent("Rename node");
                break;
        }
        _node = node;
		_newTitle = _node.title;
        

		//EposNodeEditor editor = EditorWindow.GetWindow(EposNodeEditor);
		mainWindow = (EposNodeEditor)EditorWindow.GetWindow(typeof(EposNodeEditor));
        this.minSize = mainWindow.minSize;
        this.position = mainWindow.position;
    }

    void OnGUI()
    {
        GUIStyle style = GUI.skin.box;
        GUI.skin.label.alignment = TextAnchor.UpperCenter;
        //GUILayout.Label(titleContent, EditorStyles.boldLabel);
		//GUILayout.Label("Enter a new node name", EditorStyles.boldLabel);
		_newTitle = EditorGUILayout.TextField("",_newTitle);
		if (GUILayout.Button("Close", GUILayout.Width(200)))
		{
			Debug.Log("new text : "+_newTitle);
			_node.title = _newTitle;
            mainWindow.Repaint();
            Close();
		}
        // Compute how large the button needs to be.
        //Vector2 size = style.CalcSize(popupWindow.titleContent);
    }
    void OnInspectorUpdate()
    {
        Repaint();
    }

}

//https://docs.unity3d.com/ScriptReference/PopupWindow.html
