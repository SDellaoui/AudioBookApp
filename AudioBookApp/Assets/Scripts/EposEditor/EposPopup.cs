using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum EposPopupType
{
    Rename
};

#if UNITY_EDITOR
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
#endif
