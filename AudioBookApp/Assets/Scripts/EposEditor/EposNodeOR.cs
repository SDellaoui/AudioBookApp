using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EposNodeConditionnal : EposNode
{
    public List<EposConnectionPoint> in_Points;

    public EposNodeConditionnal(Guid uuid, Vector2 position, EposNodeType _nodeType, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle, Action<EposConnectionPoint> OnClickInPoint, Action<EposConnectionPoint> OnClickOutPoint, string wwiseEvent = "", bool isQueued = false, Action<EposNode> OnClickRemoveNode = null, List<Guid> in_nodes = null, List<Guid> out_nodes = null) : base(uuid, position, _nodeType, nodeStyle, selectedStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, wwiseEvent, isQueued, OnClickRemoveNode, in_nodes, out_nodes)
    {
        //this.nodeData = new EposNodeData(uuid, _nodeType, 0, wwiseEvent, isQueued, in_nodes, out_nodes);

        switch (_nodeType)
        {

            case EposNodeType.Conditionnal_OR:
                Rect inRect = inPoint.rect;
                for(int i=0; i<2; i++)
                {
                    EposConnectionPoint pt = new EposConnectionPoint(this, ConnectionPointType.In, inPointStyle, OnClickInPoint);
                    pt.rect = inRect;
                    in_Points.Add(pt);
                    inRect.yMin += inRect.height + 5;
                }
                //inPoint = new EposConnectionPoint(this, ConnectionPointType.In, inPointStyle, OnClickInPoint);
                //outPoint = new EposConnectionPoint(this, ConnectionPointType.Out, outPointStyle, OnClickOutPoint);
                rectWidth = 200;
                rectHeight = 200;
                title = "OR";
                OnRemoveNode = OnClickRemoveNode;
                break;
            default:
                break;
        }

        rect = new Rect(position.x, position.y, this.rectWidth, this.rectHeight);

        style = nodeStyle;
        defaultNodeStyle = nodeStyle;
        selectedNodeStyle = selectedStyle;
    }

    public new void Draw()
    {
        GUI.depth = 0;
        GUI.Box(rect, "", style);
        GUI.skin.label.alignment = TextAnchor.UpperCenter;
        Rect label = new Rect(rect.x, rect.y - 14, rectWidth, rectHeight);
        GUI.Label(label, title);
    }
}
