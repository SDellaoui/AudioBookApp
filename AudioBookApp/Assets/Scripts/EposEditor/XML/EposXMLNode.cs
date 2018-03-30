using System;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;

public class EposXMLNode
{
    [XmlAttribute("uuid")]
    public Guid uuid;

    [XmlAttribute("nodeType")]
    public string nodeType;

    [XmlAttribute("posX")]
    public float posX;

    [XmlAttribute("posY")]
    public float posY;


    [XmlArray("ConnectedNodes"), XmlArrayItem("Node")]
    public List<int> nodes = new List<int>();
}
