using System;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;

public class EposXMLConnections
{
    [XmlElement(ElementName = "Node_OUT")]
    public EposXMLConnection node_out;

    [XmlElement(ElementName = "Node_IN")]
    public EposXMLConnection node_in;
}
public class EposXMLConnection
{
    [XmlAttribute("uuid")]
    public Guid uuid;

    [XmlAttribute("pinIndex")]
    public int pinIndex;
}
