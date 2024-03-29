﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml;
using System.Linq;

[Serializable]
public abstract class InputActionBase : IXmlInputData
{
    protected abstract InputBindingBase[] m_bindings { get; }

    [SerializeField]
    private string m_name;
    public string Name
    {
        get { return m_name; }
        set { m_name = value; }
    }

    [SerializeField]
    private string m_description;


    public void Initialize(int joystickIdx = -1)
    {
        foreach (var b in m_bindings)
            b.Active(joystickIdx);
    }

    public void Update(float dt)
    {
        foreach (var b in m_bindings)
            b.Update(dt);
    }

    public void ResetToDefault()
    {
        foreach (var b in m_bindings)
            b.ResetToDefault();
    }

    #region GetMethod
    public bool GetButton()
    {
        foreach (var b in m_bindings)
        {
            var val = b.GetButton();
            if (val.HasValue)
                return val.Value;
        }
        return false;
    }

    public bool GetButtonUp()
    {
        foreach (var b in m_bindings)
        {
            var val = b.GetButtonUp();
            if (val.HasValue)
                return val.Value;
        }
        return false;
    }

    public bool GetButtonDown()
    {
        foreach (var b in m_bindings)
        {
            var val = b.GetButtonDown();
            if (val.HasValue)
                return val.Value;
        }
        return false;
    }

    public float GetAxis()
    {
        foreach (var b in m_bindings)
        {
            var val = b.GetAxis();
            if (val.HasValue)
                return val.Value;
        }
        return InputManager.AXIS_ZERO;
    }

    public float GetAxisRaw()
    {
        foreach(var b in m_bindings)
        {
            var val = b.GetAxis();
            if (val.HasValue)
                return val.Value;
        }
        return InputManager.AXIS_ZERO;
    }
    #endregion GetMethod

    #region Modify
    /// <summary>
    /// 该action是否允许修改，只要有一个binding允许，则返回true
    /// </summary>
    /// <returns></returns>
    public bool EnableModify()
    {
        foreach(var b in m_bindings)
        {
            if (b.EnableModify())
                return true;
        }
        return false;
    }

    public List<InputScanSetting> GenerateScanSettings()
    {
        List<InputScanSetting> result = new List<InputScanSetting>();
        foreach(var b in m_bindings)
        {
            var bS = b.GenerateScanSetting();
            if (bS != null)
                result.AddRange(bS);
        }
        return result;
    }

    public void ApplyScanResult()
    {

    }

    #endregion Modify


    #region Serialize
    public virtual bool NeedSerialize()
    {
        foreach(var b in m_bindings)
        {
            if (b.NeedSerialize())
                return true;
        }
        return false;
    }

    public virtual void SerializeToXml(XmlWriter writer)
    {
        if (!NeedSerialize())
            return;

        writer.WriteStartElement("Action");

        writer.WriteAttributeString("name", m_name);
        //writer.WriteElementString("Description", m_description);

        foreach (var b in m_bindings)
        {
            b.SerializeToXml(writer);
        }

        writer.WriteEndElement();
    }

    public virtual void DeserializeFromXml(XmlNode node)
    {
        var nodeName = node.Attributes["name"].InnerText;
        if (string.IsNullOrEmpty(nodeName) || nodeName != m_name)
            return;

        var allBindings = node.SelectNodes("Binding").Cast<XmlNode>();
        foreach (var b in allBindings)
        {
            var bindingType = b.Attributes["type"].InnerText;
            var binding = Array.Find(m_bindings, x => x.InputTypeString == bindingType);
            if (binding == null)
                continue;

            binding.DeserializeFromXml(b);
        }
    }

    #endregion Serialize

}
