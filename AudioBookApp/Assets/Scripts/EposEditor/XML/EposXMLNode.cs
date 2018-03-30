using System.Xml;
using System.Xml.Serialization;

public class EposXMLNode
{
    [XmlAttribute("id")]
    public int id;

    [XmlAttribute("nodeType")]
    public string nodeType;

    [XmlAttribute("posX")]
    public float posX;

    [XmlAttribute("posY")]
    public float posY;
}
