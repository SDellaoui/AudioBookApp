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
}
