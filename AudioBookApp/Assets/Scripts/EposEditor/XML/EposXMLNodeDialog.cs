using System;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;

public class EposXMLNodeDialog : EposXMLNode
{

    [XmlAttribute("dispatchOnEnd")]
    public bool isQueued;

    [XmlAttribute("dialogIndex")]
    public int dialogIndex;

    [XmlAttribute("wwiseEvent")]
    public string wwiseEvent;
}
