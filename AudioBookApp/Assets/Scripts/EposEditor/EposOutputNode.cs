using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EposOutputNode : EposBaseNode {

	private string result = "";

	private EposBaseInputNode inputNode;
	private Rect inputNodeRect;

	public EposOutputNode()
	{
		windowTitle = "Output Node";
		hasInputs = true;
	}

	public override void DrawWindow()
	{
		base.DrawWindow();

		Event e = Event.current;

		string input1Title = "None";
		if(inputNode)
		{
			input1Title = inputNode.getResult();
			GUILayout.Label("Input 1 : "+input1Title);

			if(e.type == EventType.Repaint)
			{
				inputNodeRect = GUILayoutUtility.GetLastRect();
			}
			GUILayout.Label("Result : "+ result);
		}
	}
	public override void DrawCurves()
	{
		if(inputNode)
		{
			Rect rect = windowRect;
			rect.x += inputNodeRect.x;
			rect.y = inputNodeRect.y + inputNodeRect.height/2;
			rect.width = 1;
			rect.height = 1;

			EposNodeEditor.DrawNodeCurve(inputNode.windowRect, rect);
		}
	}

	public override void NodeDeleted(EposBaseNode node)
	{
		if(node.Equals(inputNode))
		{
			inputNode = null;
		}
	}

	public override EposBaseInputNode ClickedOnInput(Vector2 pos)
	{
		EposBaseInputNode retVal = null;
		pos.x = windowRect.x;
		pos.y = windowRect.y;

		if(inputNodeRect.Contains(pos))
		{
			retVal = inputNode;
			inputNode = null;
		}
		return retVal;
	}
	public override void SetInput(EposBaseInputNode input, Vector2 clickPos)
	{
		clickPos.x = windowRect.x;
		clickPos.y = windowRect.y; 

		if(inputNodeRect.Contains(clickPos))
		{
			inputNode = input;
		}
	}

}
