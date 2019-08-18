using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml;
using System.Linq;

[Serializable]
public abstract class ControlSchemeBase : IXmlInputData
{
    public abstract InputActionBase[] m_actions { get; }

    [SerializeField]
    private string name;
    public string Name
    {
        get { return name; }
        set { name = value; }
    }

    [SerializeField]
    private string description;
    public string Description
    {
        get { return description; }
        set { description = value; }
    }

    //[SerializeField]
    //private string m_uniqueID;
    //public string UniqueID
    //{
        //get { return m_uniqueID; }
        //set { m_uniqueID = value; }
    //}

    public ControlSchemeBase()
    {
        //m_uniqueID = GenerateUniqueID();
    }


    /// <summary>
    /// Editor使用，是否展开
    /// </summary>
    [HideInInspector]
    [SerializeField]
    private bool isExpanded = false;
    public bool IsExpaned
    {
        get { return isExpanded; }
        set { isExpanded = value; }
    }


    public virtual void Update(float dt)
    {
        foreach (var a in m_actions)
            a.Update(dt);
    }

    public InputActionBase GetAction(string actionName)
    {
        return Array.Find(m_actions, a => a.Name == actionName);
    }

    public bool GetButton(string buttonName)
    {
        var action = GetAction(buttonName);

        if (action == null)
        {
            Debug.LogError(buttonName + " action not exist ! ");
            return false;
        }
        return action.GetButton();
    }


    public bool GetButtonUp(string buttonName)
    {
        var action = GetAction(buttonName);

        if (action == null)
        {
            Debug.LogError(buttonName + " action not exist ! ");
            return false;
        }
        return action.GetButtonUp();
    }

    public bool GetButtonDown(string buttonName)
    {
        var action = GetAction(buttonName);

        if (action == null)
        {
            Debug.LogError(buttonName + " action not exist ! ");
            return false;
        }

        return action.GetButtonDown();
    }

    public float GetAxis(string axisName)
    {
        var action = GetAction(axisName);
        if (action == null)
        {
            Debug.LogError(axisName + " action not exist ! ");
            return 0.0f;

        }
        else
            return action.GetAxis();
    }

    public static string GenerateUniqueID()
    {
        return Guid.NewGuid().ToString("N");
    }

    #region Serialize
    public virtual bool NeedSerialize()
    {
        foreach(var a in m_actions)
        {
            if (a.NeedSerialize())
                return true;
        }
        return false;
    }

    public virtual void SerializeToXml(XmlWriter writer)
    {
        if (!NeedSerialize())
            return;

        writer.WriteStartElement("ControlScheme");
        writer.WriteAttributeString("name", name);
        //writer.WriteAttributeString("id", m_uniqueID);
        writer.WriteAttributeString("type", this.GetType().ToString());
        foreach(var action in m_actions)
            action.SerializeToXml(writer);
        writer.WriteEndElement();
    }

    public virtual void DeserializeToXml(XmlNode node)
    {
        var nodeName = node.Attributes["name"].InnerText;
        if (string.IsNullOrEmpty(nodeName) || nodeName != name)
            return;

        var actions = node.SelectNodes("Action").Cast<XmlNode>();
        foreach(var a in actions)
        {
            var actionName = a.Attributes["name"].InnerText;
            if (string.IsNullOrEmpty(actionName))
                continue;

            var action = Array.Find(m_actions, x => x.Name == actionName);
            if (action == null)
                continue;

            action.DeserializeToXml(a);
        }

    }

    #endregion Serialize
}
