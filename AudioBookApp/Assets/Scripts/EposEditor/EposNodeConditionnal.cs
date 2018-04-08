using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EposNodeConditionnal : EposNode
{
    
	public EposNodeConditionnal(Guid uuid, Vector2 position, EposNodeType _nodeType, GUIStyle nodeStyle, GUIStyle selectedStyle, Action<EposConnectionPoint> OnClickInPoint, Action<EposConnectionPoint> OnClickOutPoint, string wwiseEvent = "", bool isQueued = false, Action<EposNode> OnClickRemoveNode = null) : base(uuid, position, _nodeType, nodeStyle, selectedStyle, OnClickInPoint, OnClickOutPoint, wwiseEvent, isQueued, OnClickRemoveNode)
    {
        this.nodeData = new EposNodeData(uuid, _nodeType, 0, wwiseEvent, isQueued, OnClickInPoint, OnClickOutPoint);
        
		rectWidth = 100;
        rectHeight = 100;
        
        title = (_nodeType == EposNodeType.Conditionnal_OR)? "OR": "AND";
        OnRemoveNode = OnClickRemoveNode;
        this.nodeData.rect = new Rect(position.x, position.y, this.rectWidth, this.rectHeight);

        style = nodeStyle;
        defaultNodeStyle = nodeStyle;
        selectedNodeStyle = selectedStyle;
    }
}
