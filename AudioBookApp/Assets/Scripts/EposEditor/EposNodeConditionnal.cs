using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
public class EposNodeConditionnal : EposNode
{
    
	public EposNodeConditionnal(Guid uuid, Vector2 position, EposNodeType _nodeType, Action<EposConnectionPoint> OnClickInPoint, Action<EposConnectionPoint> OnClickOutPoint, Action<EposNode> OnClickRemoveNode = null) : base(uuid, position, _nodeType, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode)
    {
        this.nodeData = new EposNodeData(uuid, _nodeType, OnClickInPoint, OnClickOutPoint);
        
		rectWidth = 100;
        rectHeight = 100;
        
        title = (_nodeType == EposNodeType.Conditionnal_OR)? "OR": "AND";
        OnRemoveNode = OnClickRemoveNode;
        this.nodeData.rect = new Rect(position.x, position.y, this.rectWidth, this.rectHeight);
    }
}
#endif
