using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using System;
using System.Xml;

public class InputSaverXML : IInputSaver
{

    private string m_fileName;
    private Stream m_outputStream;
    private StringBuilder m_output;


    public InputSaverXML(string file)
    {
        if (file == null)
            throw new ArgumentException("filename");

        m_fileName = file;
        m_outputStream = null;
        m_output = null;
    }


    public InputSaverXML(Stream stream)
    {
        if (stream == null)
            throw new ArgumentNullException("stream");

        m_fileName = null;
        m_output = null;
        m_outputStream = stream;
    }

    public InputSaverXML(StringBuilder output)
    {
        if (output == null)
            throw new ArgumentNullException("output");

        m_fileName = null;
        m_outputStream = null;
        m_output = output;
    }

    private XmlWriter CreateXmlWriter(XmlWriterSettings settings)
    {
        if (m_fileName != null)
        {
            return XmlWriter.Create(m_fileName, settings);
        }
        else if (m_outputStream != null)
        {
            return XmlWriter.Create(m_outputStream, settings);
        }
        else if (m_output != null)
        {
            return XmlWriter.Create(m_output, settings);
        }

        return null;
    }


    public void Save(InputSaveData data)
    {
        XmlWriterSettings xmlSettings = new XmlWriterSettings();

        xmlSettings.Encoding = Encoding.UTF8;
        xmlSettings.Indent = true;

        using (XmlWriter writer = CreateXmlWriter(xmlSettings))
        {
            writer.WriteStartDocument(true);
            writer.WriteStartElement("Input");
            foreach(var scheme in data.KeyboardMouseControlSchemes)
                scheme.SerializeToXml(writer);
            foreach(var scheme in data.JoystickControlSchemes)
                scheme.SerializeToXml(writer);

            writer.WriteEndElement();
            writer.WriteEndDocument();

        }

    }
}
