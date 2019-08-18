using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml;
using System.Globalization;

public enum KeyboardMouseInputType
{
    Button,
    MouseAxis, // 鼠标轴
    DigitalAxis,  // button作为轴使用
    VirtualButton, // 虚拟按钮
    VirtualAxis, // 虚拟轴
}

[Serializable]
public class KeyboardMouseInputBinding : InputBindingBase
{
    #region SerializeField
    [SerializeField]
    private KeyboardMouseInputType m_inputType;
    public KeyboardMouseInputType InputType
    {
        get { return m_inputType; }
        set { m_inputType = value; m_isDirty = true; }
    }

    [SerializeField]
    private KeyCodeModifiable m_positive;

    [SerializeField]
    private KeyCodeModifiable m_negative;

    /// <summary>
    /// -1表示不选
    /// </summary>
    [SerializeField]
    //[Range(-1, 2)]
    private IntModifiable m_axis;
    //public int axis
    //{
    //    get { return m_axis; }
    //    set
    //    {
    //        m_axis = value;
    //        m_isdirty = true;
    //    }
    //}

    public override string InputTypeString => m_inputType.ToString();


    #endregion SerializeField

    #region Variables
    private string m_rawAxisName;

    private bool m_isDirty;

    private float m_value;

    #endregion Variables

    public override void Active(int joystickIdx = -1)
    {
        ResetState();
        RegisterModifibleListener();
    }

    public override void DeActive()
    {
        UnReigsterModifibleListener();
    }

    void ResetState()
    {
        m_rawAxisName = null;
        m_value = 0.0f;

        m_isDirty = true;
    }

    void OnModifibleChange()
    {
        m_isDirty = true;
    }
    void RegisterModifibleListener()
    {
        m_positive.RegisterValueChange(OnModifibleChange);
        m_negative.RegisterValueChange(OnModifibleChange);
        m_axis.RegisterValueChange(OnModifibleChange);
    }

    void UnReigsterModifibleListener()
    {
        m_positive.UnRegisterValueChange(OnModifibleChange);
        m_negative.UnRegisterValueChange(OnModifibleChange);
        m_axis.UnRegisterValueChange(OnModifibleChange);
    }

    void InitializeRawAxisName()
    {
        if (m_inputType == KeyboardMouseInputType.MouseAxis)
        {
            if (m_axis.value >= 0 && m_axis.value < InputManager.MOUSE_AXIS_COUNT)
            {
                m_rawAxisName = string.Format("mouse_axis_{0}", m_axis.value);
            }
            else
            {
                Debug.LogWarning(m_axis + " mouse axis out of index! ");
                m_rawAxisName = "";
            }
        }
    }

    public override void Update(float deltaTime)
    {
        if(m_isDirty)
        {
            m_isDirty = false;
            InitializeRawAxisName();
        }

        if (m_inputType == KeyboardMouseInputType.DigitalAxis)
        {
            UpdateDigitalAxis(deltaTime);
        }
    }
    public void UpdateDigitalAxis(float deltaTime)
    {
        if (m_inputType != KeyboardMouseInputType.DigitalAxis)
            return;

        var positive = Input.GetKey(m_positive.value);
        var negative = Input.GetKey(m_negative.value);

        if (positive && negative) // 两个按键同时按
            return;

        if (positive) // 按了 +
        {
            if (m_value < InputManager.AXIS_ZERO && m_snap)
                m_value = InputManager.AXIS_ZERO;

            m_value += m_sensitivity * deltaTime;

        }
        else if (negative) // 按了-
        {
            if (m_value > InputManager.AXIS_ZERO && m_snap)
                m_value = InputManager.AXIS_ZERO;

            m_value -= m_sensitivity * deltaTime;

        }
        else // 都没按
        {
            if (m_value > InputManager.AXIS_ZERO)
            {
                m_value -= m_gravity * deltaTime;
                if (m_value < InputManager.AXIS_ZERO)
                    m_value = InputManager.AXIS_ZERO;

            }
            else if (m_value < InputManager.AXIS_ZERO)
            {
                m_value += m_gravity * deltaTime;
                if (m_value > InputManager.AXIS_ZERO)
                    m_value = InputManager.AXIS_ZERO;
            }
        }

        m_value = Mathf.Clamp(m_value, InputManager.AXIS_NEGATIVE, InputManager.AXIS_POSITIVE);
    }


    #region GetMethod

    public override bool? GetButton()
    {
        bool? result = false;
        switch (m_inputType)
        {
            case KeyboardMouseInputType.Button:
                result = Input.GetKey(m_positive.value);
                break;
            default:
                result = false;
                break;
        }

        if (result.HasValue && !result.Value)
            result = null;

        return result;
    }

    public override bool? GetButtonDown()
    {
        bool? result = null;

        switch (m_inputType)
        {
            case KeyboardMouseInputType.Button:
                result = Input.GetKeyDown(m_positive.value);
                break;
            default:
                result = false;
                break;
        }

        if (result.HasValue && !result.Value)
            result = null;

        return result;
    }

    public override bool? GetButtonUp()
    {
        bool? result = null;
        switch (m_inputType)
        {
            case KeyboardMouseInputType.Button:
                result = Input.GetKeyUp(m_positive.value);
                break;
            default:
                result = false;
                break;
        }

        if (result.HasValue && !result.Value)
            result = null;

        return result;
    }

    public override float? GetAxis()
    {
        float? result = null;

        switch (m_inputType)
        {
            case KeyboardMouseInputType.MouseAxis:
                if (!string.IsNullOrEmpty(m_rawAxisName))
                {
                    result = Input.GetAxis(m_rawAxisName);
                    result = ApplyDeadZone(result.Value);
                    result = m_invert.value ? -result : result;
                }
                break;

            case KeyboardMouseInputType.DigitalAxis:
                result = m_invert.value ? -m_value : m_value;
                break;

            default:
                result = InputManager.AXIS_ZERO;
                break;
        }

        if (result.HasValue && result.Value == InputManager.AXIS_ZERO)
            result = null;

        return result;
    }

    #endregion GetMethod

    #region Modify
    public override bool EnableModify()
    {
        return m_modifiable
            && m_inputType != KeyboardMouseInputType.VirtualAxis
            && m_inputType != KeyboardMouseInputType.VirtualButton;
    }

    public override List<InputScanSetting> GenerateScanSetting()
    {
        if (!EnableModify())
            return null;

        List<InputScanSetting> result = new List<InputScanSetting>();

        switch (m_inputType)
        {
            case KeyboardMouseInputType.Button:
                InputScanSetting btnSetting = new InputScanSetting( InputScanType.KeyboardButton, this);
                btnSetting.IsPositive = true;
                btnSetting.CurKeyCode = m_positive.value;

                result.Add(btnSetting);
                break;

            case KeyboardMouseInputType.MouseAxis:
                InputScanSetting mouseSetting = new InputScanSetting(InputScanType.MouseAxis, this);
                mouseSetting.CurMouseAxis = m_axis.value;
                mouseSetting.IsInvert = m_invert.value;
                result.Add(mouseSetting);
                break;

            case KeyboardMouseInputType.DigitalAxis:
                InputScanSetting positiveAxisSetting = new InputScanSetting(InputScanType.KeyboardButton, this);
                positiveAxisSetting.IsPositive = true;
                positiveAxisSetting.CurKeyCode = m_positive.value;

                InputScanSetting negativeAxisSetting = new InputScanSetting(InputScanType.KeyboardButton, this);
                negativeAxisSetting.IsPositive = false;
                negativeAxisSetting.CurKeyCode = m_negative.value;

                result.Add(positiveAxisSetting);
                result.Add(negativeAxisSetting);
                break;

            default:
                return null;
        }

        return result;
    }

    public override bool ApplyInputModify(InputScanResult data)
    {
        if (!EnableModify())
            return false ;

        var requestInputType = data.ScanType;
        switch (requestInputType)
        {
            case InputScanType.KeyboardButton:
                if(data.IsPositive)
                {
                    m_positive.SetCustom(data.KeyCode);
                }
                else
                {
                    m_negative.SetCustom(data.KeyCode);
                }
                break;

            case InputScanType.MouseAxis:
                m_axis.SetCustom(data.Axis);
                break;

            default:
                return false;
        }

        return true;
    }

    public override bool NeedSerialize()
    {
        if (!EnableModify())
            return false;

        if ( m_invert.HasCustom ||
            m_positive.HasCustom ||
            m_negative.HasCustom ||
            m_axis.HasCustom)
            return true;

        return false;
    }
    /// <summary>
    /// 只将可在运行时改动的值序列化
    /// </summary>
    /// <param name="writer"></param>
    public override void SerializeToXml(XmlWriter writer)
    {
        if (!NeedSerialize())
            return;

        writer.WriteStartElement("Binding");

        writer.WriteAttributeString("type", m_inputType.ToString());

        //writer.WriteElementString("Type", m_inputType.ToString());
        // writer.WriteElementString("Modifiable", m_modifiable.ToString().ToLower());
        //writer.WriteElementString("Sensitivity", m_sensitivity.ToString(CultureInfo.InvariantCulture));
        //writer.WriteElementString("Dead", m_dead.ToString(CultureInfo.InvariantCulture));
        //writer.WriteElementString("Gravity", m_gravity.ToString(CultureInfo.InvariantCulture));
        //writer.WriteElementString("Snap", m_snap.ToString().ToLower());
        if (m_invert.HasCustom)
            writer.WriteElementString("Invert", m_invert.value.ToString());

        if(m_positive.HasCustom)
            writer.WriteElementString("Positive", m_positive.value.ToString());

        if(m_negative.HasCustom)
            writer.WriteElementString("Negative", m_negative.value.ToString());

        if(m_axis.HasCustom)
            writer.WriteElementString("Axis", m_axis.value.ToString());

        writer.WriteEndElement();
    }

    public override void DeserializeToXml(XmlNode node)
    {
        if (!EnableModify())
            return;

        var t = node.Attributes["type"].InnerText;
        if (string.IsNullOrEmpty(t) || t != InputTypeString)
            return;

        if (InputLoaderXML.ReadElement(node, "Invert", out bool v))
        {
            m_invert.SetCustom(v);
        }

        if (InputLoaderXML.ReadElement(node, "Positive", out KeyCode p))
        {
            m_positive.SetCustom(p);
        }

        if (InputLoaderXML.ReadElement(node, "Negative", out KeyCode n))
        {
            m_negative.SetCustom(n);
        }

        if (InputLoaderXML.ReadElement(node, "Axis", out int a))
        {
            m_axis.SetCustom(a);
        }
    }

    public override void Clear()
    {
        m_positive.SetCustom(KeyCode.None);
        m_negative.SetCustom(KeyCode.None);
        m_axis.SetCustom(-1);
    }

    public override void Reset()
    {
    }

    #endregion Modify

}
