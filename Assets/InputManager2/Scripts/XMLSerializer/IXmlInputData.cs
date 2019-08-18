using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public interface IXmlInputData
{
    bool NeedSerialize();
    void SerializeToXml(XmlWriter writer);
    void DeserializeToXml(XmlNode node);
}
