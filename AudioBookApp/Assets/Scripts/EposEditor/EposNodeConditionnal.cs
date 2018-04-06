using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EposNodeConditionnal : EposNode
{
    
    public EposNodeConditionnal(Guid uuid, Vector2 position, EposNodeType _nodeType, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle, Action<EposConnectionPoint> OnClickInPoint, Action<EposConnectionPoint> OnClickOutPoint, string wwiseEvent = "", bool isQueued = false, Action<EposNode> OnClickRemoveNode = null, List<Guid> in_nodes = null, List<Guid> out_nodes = null) : base(uuid, position, _nodeType, nodeStyle, selectedStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, wwiseEvent, isQueued, OnClickRemoveNode, in_nodes, out_nodes)
    {
        //this.nodeData = new EposNodeData(uuid, _nodeType, 0, wwiseEvent, isQueued, in_nodes, out_nodes);
		in_Points = new List<EposConnectionPoint> ();
        this.nodeData.m_nInputs = 2;
        switch (_nodeType)
        {
			case EposNodeType.Conditionnal_OR:
	            for(int i=0; i<2; i++)
	            {
	                EposConnectionPoint pt = new EposConnectionPoint(this, ConnectionPointType.In, inPointStyle, OnClickInPoint, this.nodeData.m_nInputs, i);
					in_Points.Add (pt);
	            }
	            outPoint = new EposConnectionPoint(this, ConnectionPointType.Out, outPointStyle, OnClickOutPoint);
	            break;
	        default:
	            break;
        }
		rectWidth = 100;
        rectHeight = 100;
        title = "OR";
        OnRemoveNode = OnClickRemoveNode;
        rect = new Rect(position.x, position.y, this.rectWidth, this.rectHeight);

        style = nodeStyle;
        defaultNodeStyle = nodeStyle;
        selectedNodeStyle = selectedStyle;
    }
    public new void Draw()
    {
        
    }
}
