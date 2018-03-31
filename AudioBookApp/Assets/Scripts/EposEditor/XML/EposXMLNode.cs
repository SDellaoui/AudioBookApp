using System;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;

public class EposXMLNode
{
    [XmlAttribute("uuid")]
    public Guid uuid;

    [XmlAttribute("title")]
    public string title;

    [XmlAttribute("nodeType")]
    public EposNodeType nodeType;

    [XmlAttribute("posX")]
    public float posX;

    [XmlAttribute("posY")]
    public float posY;


    [XmlArray("ConnectedNodesInput"), XmlArrayItem("Node_In")]
    public List<Guid> in_nodes = new List<Guid>();

    [XmlArray("ConnectedNodesOutput"), XmlArrayItem("Node_Out")]
    public List<Guid> out_nodes = new List<Guid>();
}
