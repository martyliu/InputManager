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
    private KeyCode m_positive;

    [SerializeField]
    private KeyCode m_negative;

    [SerializeField]
    [Range(0, 2)]
    private int m_axis;
    public int Axis
    {
        get { return m_axis; }
        set
        {
            if(value >= 0 && value <= 2)
            {
                m_axis = value;
                m_isDirty = true;
            }
        }
    }


    #endregion SerializeField

    #region Variables
    private string m_rawAxisName;

    private bool m_isDirty;

    private float m_value;

    KeyCode? Positive_Custom;
    KeyCode? Negative_Custom;
    int? Axis_Custom;
    #endregion Variables

    public override void Initialize(int joystickIdx = -1)
    {
        ResetState();
    }

    void ResetState()
    {
        m_rawAxisName = "";
        m_value = 0.0f;
        m_isDirty = true;
    }

    void InitializeRawAxisName()
    {
        if (m_inputType == KeyboardMouseInputType.MouseAxis)
        {
            if (m_axis >= 0 && m_axis < InputManager.MOUSE_AXIS_COUNT)
            {
                m_rawAxisName = string.Format("mouse_axis_{0}", m_axis);
            }
            else
            {
                Debug.LogError(m_axis + " mouse axis out of index! ");
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

        var positive = Input.GetKey(m_positive);
        var negative = Input.GetKey(m_negative);

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
                result = Input.GetKey(m_positive);
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
                result = Input.GetKeyDown(m_positive);
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
                result = Input.GetKeyUp(m_positive);
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
                    result = m_invert ? -result : result;
                }
                break;

            case KeyboardMouseInputType.DigitalAxis:
                result = m_invert ? -m_value : m_value;
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
                btnSetting.CurKeyCode = m_positive;

                result.Add(btnSetting);
                break;

            case KeyboardMouseInputType.MouseAxis:
                InputScanSetting mouseSetting = new InputScanSetting(InputScanType.MouseAxis, this);
                mouseSetting.CurMouseAxis = m_axis;
                mouseSetting.IsInvert = m_invert;
                result.Add(mouseSetting);
                break;

            case KeyboardMouseInputType.DigitalAxis:
                InputScanSetting positiveAxisSetting = new InputScanSetting(InputScanType.KeyboardButton, this);
                positiveAxisSetting.IsPositive = true;
                positiveAxisSetting.CurKeyCode = m_positive;

                InputScanSetting negativeAxisSetting = new InputScanSetting(InputScanType.KeyboardButton, this);
                negativeAxisSetting.IsPositive = false;
                negativeAxisSetting.CurKeyCode = m_negative;

                result.Add(positiveAxisSetting);
                result.Add(negativeAxisSetting);
                break;

            default:
                return null;
        }

        return result;
    }

    public override bool ApplyInputModify(InputScanSetting data)
    {
        if (!EnableModify())
            return false ;

        var requestInputType = data.ScanType;
        switch (requestInputType)
        {
            case InputScanType.KeyboardButton:
                if(data.IsPositive)
                    m_positive = data.CurKeyCode;
                else
                    m_negative = data.CurKeyCode;
                break;

            case InputScanType.MouseAxis:
                Axis = data.CurMouseAxis;
                break;

            default:
                return false;
        }

        return true;
    }

    /// <summary>
    /// 只将可在运行时改动的值序列化
    /// </summary>
    /// <param name="writer"></param>
    public override void SerializeToXml(XmlWriter writer)
    {
        writer.WriteStartElement("Binding");

        writer.WriteAttributeString("Typel", m_inputType.ToString();
        //writer.WriteElementString("Type", m_inputType.ToString());
        // writer.WriteElementString("Modifiable", m_modifiable.ToString().ToLower());
        //writer.WriteElementString("Sensitivity", m_sensitivity.ToString(CultureInfo.InvariantCulture));
        //writer.WriteElementString("Dead", m_dead.ToString(CultureInfo.InvariantCulture));
        //writer.WriteElementString("Gravity", m_gravity.ToString(CultureInfo.InvariantCulture));
        //writer.WriteElementString("Snap", m_snap.ToString().ToLower());
        if(Invert_Custom.HasValue)
            writer.WriteElementString("Invert", Invert_Custom.Value.ToString().ToLower());

        if(Positive_Custom.HasValue)
            writer.WriteElementString("Positive", Positive_Custom.Value.ToString());

        if(Negative_Custom.HasValue)
            writer.WriteElementString("Negative", Negative_Custom.Value.ToString());

        if(Axis_Custom.HasValue)
            writer.WriteElementString("Axis", Axis_Custom.Value.ToString());

        writer.WriteEndElement();
    }

    public override void DeserializeToXml()
    {
    }

    #endregion Modify

}
