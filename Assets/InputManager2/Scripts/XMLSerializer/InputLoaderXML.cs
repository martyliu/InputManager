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
        if(m_fileName != null)
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

    public InputSaveData Load()
    {
        var doc = CreateXmlDocument();
        if(doc != null)
        {
            return LoadXml(doc);
        }
        return null;
    }

    public ControlSchemeBase Load(string schemeName)
    {
        return null;
    }


    InputSaveData LoadXml(XmlDocument doc)
    {
        InputSaveData data = new InputSaveData();
        var root = doc.DocumentElement;


        //data.CurrentPCScheme = ReadNode( SelectSingleNode(root, "CurrentPCScheme" ));
        SelectNodes(root, "ControlScheme");


        return data;

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

}
