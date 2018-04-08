using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

[XmlRoot("EposNodeContainer")]
public class EposXmlNodeContainer
{
    [XmlElement("DialogScript")]
    public string pathToDialogScript;

    [XmlArray("EposNodes"), XmlArrayItem("EposNode")]
    public List<EposXMLNode> eposXMLNodes = new List<EposXMLNode>();

    [XmlArray("Connections"), XmlArrayItem("Connection")]
    public List<EposXMLConnections> eposXmlConnections = new List<EposXMLConnections>();

    public void AddNode(EposXMLNode node)
    {
        eposXMLNodes.Add(node);
    }

    public void Save(string path)
    {
        
        var serializer = new XmlSerializer(typeof(EposXmlNodeContainer));
        
        using (var stream = new FileStream(path, FileMode.Create))
        {
            serializer.Serialize(stream, this);
        }
    }

    public static EposXmlNodeContainer Load(string path)
    {
        var serializer = new XmlSerializer(typeof(EposXmlNodeContainer));
        using (var stream = new FileStream(path, FileMode.Open))
        {
            return serializer.Deserialize(stream) as EposXmlNodeContainer;
        }
    }

    //Loads the xml directly from the given string. Useful in combination with www.text.
    public static EposXmlNodeContainer LoadFromText(string text)
    {
        var serializer = new XmlSerializer(typeof(EposXmlNodeContainer));
        return serializer.Deserialize(new StringReader(text)) as EposXmlNodeContainer;
    }
}


//Read Data
//var monsterCollection = MonsterContainer.Load(Path.Combine(Application.dataPath, "monsters.xml"));
//var xmlData = @"<MonsterCollection><Monsters><Monster name=""a""><Health>5</Health></Monster></Monsters></MonsterCollection>";
//var monsterCollection = MonsterContainer.LoadFromText(xmlData);

//Write Data
//monsterCollection.Save(Path.Combine(Application.persistentDataPath, "monsters.xml"));