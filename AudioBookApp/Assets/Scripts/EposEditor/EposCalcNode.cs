using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EposCalcNode : EposBaseInputNode {
	private EposBaseInputNode input1;
	private Rect input1Rect;

	private EposBaseInputNode input2;
	private Rect input2Rect;

	private CalculationType calculationType;

	public enum CalculationType
	{
		Addition,
		Substraction,
		Multiplication,
		Division
	}

	public EposCalcNode()
	{
		windowTitle = "Calculation Node";
		hasInputs = true;
	}

	public override void DrawWindow()
	{
		base.DrawCurves();

		Event e = Event.current;
		calculationType = (CalculationType)EditorGUILayout.EnumPopup("Calculation Type",calculationType);

		string input1Title = "None";

		if(input1)
		{
			input1Title = input1.getResult();
		}
		GUILayout.Label("Input 1 : "+input1Title);
		if(e.type == EventType.Repaint)
		{
			input1Rect = GUILayoutUtility.GetLastRect();
		}

		string input2Title = "None";

		if(input2)
		{
			input2Title = input2.getResult();
		}
		GUILayout.Label("Input 2 : "+input2Title);
		if(e.type == EventType.Repaint)
		{
			input2Rect = GUILayoutUtility.GetLastRect();
		}

		//video link https://www.youtube.com/watch?v=gHTJmGGH92w
		//position : 46.28
	}

}
