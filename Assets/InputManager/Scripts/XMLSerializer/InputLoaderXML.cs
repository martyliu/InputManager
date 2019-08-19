using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Xml;
using System.Linq;

public class InputLoaderXML : IInputLoader
{
    public string m_fileName;
    Stream m_inputStream;
    TextReader m_textReader;


    public InputLoaderXML(string file)
    {
        if (string.IsNullOrEmpty(file))
            throw new ArgumentException("filename null");

        m_fileName = file;
        m_inputStream = null;
        m_textReader = null;
    }

    public InputLoaderXML(Stream stream)
    {
        if (stream == null)
            throw new ArgumentException("stream");

        m_fileName = null;
        m_textReader = null;
        m_inputStream = stream;
    }

    public InputLoaderXML(TextReader reader)
    {
        if (reader == null)
            throw new ArgumentException("reader");

        m_fileName = null;
        m_textReader = reader;
        m_inputStream = null;

    }

    private XmlDocument CreateXmlDocument()
    {
        if(m_fileName != null && File.Exists(m_fileName))
        {
            using (StreamReader reader = File.OpenText(m_fileName))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(reader);
                return doc;
            }

        }
        else if(m_inputStream != null)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(m_inputStream);
            return doc;
        }
        else if(m_textReader != null)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(m_textReader);
            return doc;
        }

        return null;
    }

    public bool Load(ref List<JoystickControlScheme> joystickControls, ref List<KeyboardMouseControlScheme> keyboardControls)
    {
        var doc = CreateXmlDocument();
        if (doc == null)
            return false;

        var root = doc.DocumentElement;
        var nodes = SelectNodes(root, "ControlScheme");
        foreach(var node in nodes)
        {
            string type = ReadAttribute(node, "type", null);

            string name = ReadAttribute(node, "name", "Unnamed Control Scheme");
            string id = ReadAttribute(node, "id", null);
            ControlSchemeBase scheme = null;

            if (type == typeof(JoystickControlScheme).ToString())
            {
                scheme = joystickControls.Find(x => x.Name == name);
            }
            else if(type == typeof(KeyboardMouseControlScheme).ToString())
            {
                scheme = keyboardControls.Find(x => x.Name == name);
            }else
            {
                Debug.LogError("can't find type : " + type);
                continue;
            }

            scheme.DeserializeFromXml(node);

        }

        return true;

    }

    private XmlNode SelectSingleNode(XmlNode parent, string name)
    {
        return parent.SelectSingleNode(name);
    }

    private IEnumerable<XmlNode> SelectNodes(XmlNode parent, string name)
    {
        return parent.SelectNodes(name).Cast<XmlNode>();
    }

    private string ReadNode(XmlNode node, string defValue = null)
    {
        return node != null ? node.InnerText : defValue;
    }

    private static string ReadElement(XmlNode parent, string name, string defaultVal = null)
    {
        var node = parent.SelectSingleNode(name);
        return node != null ? node.InnerText : defaultVal;
    }

    public static bool ReadElement<T>(XmlNode parent, string name, out T value) where T: struct
    {
        var str = ReadElement(parent, name);
        if (string.IsNullOrEmpty(str))
        {
            value = default;
            return false;
        }
        if(typeof(T).IsEnum)
        {
            bool r = Enum.TryParse(str, out value);
            return r;

        }else
        {
            value = (T)Convert.ChangeType(str, typeof(T));
            return true;
        }
        
    }

    public static string ReadAttribute(XmlNode node, string attribute, string defValue = null)
    {
        if (node.Attributes[attribute] != null)
            return node.Attributes[attribute].InnerText;

        return defValue;
    }

}
