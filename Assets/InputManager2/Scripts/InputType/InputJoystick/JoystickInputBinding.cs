using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml;
using System.Globalization;

public enum JoystickInputType
{
    Button, // 轴按键
    AnalogButton, //轴作为button使用
    DigitalAxis,  // button作为轴使用
    AnalogAxis, // 轴
}

public enum JoystickButton
{
    None = -1, 

    JoystickButton0 = 0,
    JoystickButton1,
    JoystickButton2,
    JoystickButton3,
    JoystickButton4,
    JoystickButton5,
    JoystickButton6,
    JoystickButton7,
    JoystickButton8,
    JoystickButton9,
    JoystickButton10,
    JoystickButton11,
    JoystickButton12,
    JoystickButton13,
    JoystickButton14,
    JoystickButton15,
    JoystickButton16,
    JoystickButton17,
    JoystickButton18,
    JoystickButton19,
}

public enum JoystickMatchType
{
    FromScheme, // 与当前scheme关联
    FixedIndex,  // 固定下标
}

[Serializable]
public class JoystickInputBinding : InputBindingBase
{
    #region SerializeField
    [SerializeField]
    private JoystickInputType m_inputType;
    public JoystickInputType InputType
    {
        get { return m_inputType; }
        set { m_inputType = value; m_isDirty = true; }
    }

    //默认值
    [SerializeField]
    private JoystickButtonModifiable m_positive;

    [SerializeField]
    private JoystickButtonModifiable m_negative;


    [Tooltip("匹配手柄类型")]
    [SerializeField]
    private JoystickMatchType m_joystickMatchType = JoystickMatchType.FromScheme;
    public JoystickMatchType JoystickMatchType
    {
        get { return m_joystickMatchType; }
        set { m_joystickMatchType = value; m_isDirty = true; }
    }

    // 手柄下标
    //private int m_joystickFixedIndex = -1;
    //[Range(-1, InputManager.JOYSTICK_COUNT - 1 )]
    [SerializeField]
    private IntModifiable m_joystickFixedIndex;

    /// <summary>
    /// -1表示没有无轴
    /// </summary>
    // 轴下标
    [SerializeField]
    //[Range(-1, InputManager.JOYSTICK_AXIS_COUNT - 1)]
    private IntModifiable m_axis;
    //private int m_axis = -1;
    /*
    public int Axis
    {
        get { return m_axis; }
        set {
            m_axis = value;
            m_isDirty = true;
        }
    }
    */


    #endregion SerializeField

    #region Variables
    private bool m_isDirty = false;

    private KeyCode m_joystickPositive = KeyCode.None;

    private KeyCode m_joystickNegative = KeyCode.None;

    private int m_schemeJoystickIdx;

    private string m_rawAxisName;

    private float m_value;

    private InputButtonState m_analogButtonState;

    #endregion Variables

    #region Properties
    private int JoystickIdx
    {
        get
        {
            if (m_joystickMatchType == JoystickMatchType.FixedIndex)
                return m_joystickFixedIndex.value;
            else
                return m_schemeJoystickIdx;
        }
    }

    public override string InputTypeString => m_inputType.ToString();

    #endregion Properties

    /// <summary>
    /// 手柄的下标会根据usb插口变化
    /// 所以需要动态设置
    /// </summary>
    /// <param name="joystickIdx"></param>
    public override void Active(int joystickIdx = -1)
    {
        ResetState();
        RegisterModifiableListener();

        m_schemeJoystickIdx = joystickIdx;
        m_isDirty = true;
    }

    public override void DeActive()
    {
        ResetState();
    }

    /// <summary>
    /// 重置数据
    /// </summary>
    void ResetState()
    {
        m_schemeJoystickIdx = -1;
        m_value = InputManager.AXIS_ZERO;
        m_rawAxisName = null;
        m_analogButtonState = InputButtonState.Release;
        m_joystickNegative = KeyCode.None;
        m_joystickPositive = KeyCode.None;

        UnRegisterModifibleListener();

        m_isDirty = true;
    }

    void OnModifibleValueChange()
    {
        m_isDirty = true;
    }

    void RegisterModifiableListener()
    {
        m_positive.RegisterValueChange(OnModifibleValueChange);
        m_negative.RegisterValueChange(OnModifibleValueChange);
        m_axis.RegisterValueChange(OnModifibleValueChange);
        m_joystickFixedIndex.RegisterValueChange(OnModifibleValueChange);
    }

    void UnRegisterModifibleListener()
    {
        m_positive.UnRegisterValueChange(OnModifibleValueChange);
        m_negative.UnRegisterValueChange(OnModifibleValueChange);
        m_axis.UnRegisterValueChange(OnModifibleValueChange);
        m_joystickFixedIndex.UnRegisterValueChange(OnModifibleValueChange);
    }

    public override void Update(float deltaTime)
    {
        if(m_isDirty)
        {
            m_isDirty = false;
            InitializeButtonAndRawAxisName();
        }

        if (m_inputType == JoystickInputType.DigitalAxis)
        {
            UpdateDigitalAxis(deltaTime);
        }
        else if (m_inputType == JoystickInputType.AnalogButton)
        {
            UpdateAnalogButton();
        }

    }

    public void UpdateDigitalAxis(float deltaTime)
    {
        if (m_inputType != JoystickInputType.DigitalAxis)
            return;

        var positive = Input.GetKey(m_joystickPositive);
        var negative = Input.GetKey(m_joystickNegative);

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

    public void UpdateAnalogButton()
    {
        if (m_inputType != JoystickInputType.AnalogButton )
            return;

        if (string.IsNullOrEmpty(m_rawAxisName))
            return;

        float val = Input.GetAxis(m_rawAxisName);

        val = m_invert.value ? -val : val;

        if (val > m_dead)
        {
            if (m_analogButtonState == InputButtonState.JustPressed)
            {
                m_analogButtonState = InputButtonState.Pressed;
            }
            else if (m_analogButtonState == InputButtonState.Release || m_analogButtonState == InputButtonState.JustRelease)
            {
                m_analogButtonState = InputButtonState.JustPressed;
            }
        }
        else
        {
            if (m_analogButtonState == InputButtonState.JustRelease)
            {
                m_analogButtonState = InputButtonState.Release;
            }
            else if (m_analogButtonState == InputButtonState.Pressed || m_analogButtonState == InputButtonState.JustPressed)
            {
                m_analogButtonState = InputButtonState.JustRelease;
            }
        }
    }


    /// <summary>
    /// 根据手柄下标，设置对应的button或axis
    /// </summary>
    void InitializeButtonAndRawAxisName()
    {
        if(m_inputType == JoystickInputType.Button || m_inputType == JoystickInputType.DigitalAxis)
        {
            var idx = JoystickIdx;
            if (idx >= 0 && idx < InputManager.JOYSTICK_COUNT)
            {
                if (m_positive.value == JoystickButton.None)
                {
                    m_joystickPositive = KeyCode.None;
                }
                else
                {
                    int v = (int)KeyCode.Joystick1Button0 + idx * InputManager.JOYSTICK_BUTTON_COUNT + (int)m_positive.value;
                    m_joystickPositive = (KeyCode)v;
                }

                if (m_negative.value == JoystickButton.None)
                {
                    m_joystickNegative = KeyCode.None;
                }
                else
                {
                    int v = (int)KeyCode.Joystick1Button0 + idx * InputManager.JOYSTICK_BUTTON_COUNT + (int)m_negative.value;
                    m_joystickNegative = (KeyCode)v;
                }
            }
            else
            {
                m_joystickPositive = KeyCode.None;
                m_joystickNegative = KeyCode.None;
            }
        }
        else if (m_inputType == JoystickInputType.AnalogAxis || m_inputType == JoystickInputType.AnalogButton)
        {
            var idx = JoystickIdx;
            if (idx >= 0 && idx < InputManager.JOYSTICK_COUNT)
            {
                if (m_axis.value >= 0 && m_axis.value < InputManager.JOYSTICK_AXIS_COUNT)
                {
                    m_rawAxisName = string.Format("joy_" + idx.ToString() + "_axis_" + m_axis.value.ToString());
                }
                else
                {
                    Debug.LogError(idx.ToString() + " joystick " + m_axis.value.ToString() + " axis out of index! ");
                    m_rawAxisName = "";
                }

            }
            else
            {
                m_rawAxisName = "";
            }
        }
    }

    #region GetMethod
    public override bool? GetButton()
    {
        bool? result = false;
        switch (m_inputType)
        {
            case JoystickInputType.Button:
                result = Input.GetKey(m_joystickPositive);
                break;
            case JoystickInputType.AnalogButton:
                result = m_analogButtonState == InputButtonState.Pressed || m_analogButtonState == InputButtonState.JustPressed;
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
            case JoystickInputType.Button:
                result = Input.GetKeyDown(m_joystickPositive);
                break;
            case JoystickInputType.AnalogButton:
                result = m_analogButtonState == InputButtonState.JustPressed;
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
            case JoystickInputType.Button:
                result = Input.GetKeyUp(m_joystickPositive);
                break;
            case JoystickInputType.AnalogButton:
                result = m_analogButtonState == InputButtonState.JustRelease;
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
            case JoystickInputType.AnalogAxis:
                if (!string.IsNullOrEmpty(m_rawAxisName))
                {
                    result = Input.GetAxis(m_rawAxisName);
                    result = ApplyDeadZone(result.Value);
                    result = m_invert.value ? -result : result;
                }
                break;
            case JoystickInputType.DigitalAxis:
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

    /// <summary>
    /// 对于digitalAxis有正和负，所以返回一个列表
    /// </summary>
    /// <returns></returns>
    public override List<InputScanSetting> GenerateScanSetting()
    {
        if (!EnableModify())
            return null;

        List<InputScanSetting> result = new List<InputScanSetting>();
        switch (m_inputType)
        {
            case JoystickInputType.Button:
                InputScanSetting btnSetting = new InputScanSetting( InputScanType.JoystickButton, this);
                btnSetting.CurJoystickIndex = JoystickIdx;
                btnSetting.CurJoystickButton = m_positive.value;
                btnSetting.IsPositive = true;

                result.Add(btnSetting);
                break;

             case JoystickInputType.DigitalAxis:
                InputScanSetting positiveAxisSetting = new InputScanSetting(InputScanType.JoystickButton, this);
                positiveAxisSetting.IsPositive = true;
                positiveAxisSetting.CurJoystickIndex = JoystickIdx;
                positiveAxisSetting.CurJoystickButton = m_positive.value;

                InputScanSetting negativeAxisSetting = new InputScanSetting(InputScanType.JoystickButton, this);
                negativeAxisSetting.IsPositive = false;
                negativeAxisSetting.CurJoystickIndex = JoystickIdx;
                negativeAxisSetting.CurJoystickButton = m_negative.value;

                result.Add(positiveAxisSetting);
                result.Add(negativeAxisSetting);

                break;

            case JoystickInputType.AnalogAxis:
            case JoystickInputType.AnalogButton:
                InputScanSetting analogAxisSetting = new InputScanSetting(InputScanType.JoystickAxis, this);
                analogAxisSetting.CurJoystickIndex = JoystickIdx;
                analogAxisSetting.CurJoystickAxis = m_axis.value;
                analogAxisSetting.IsInvert = m_invert.value;

                result.Add(analogAxisSetting);
                break;
        }

        return result;
    }

    #endregion Modify

    #region UtilityMethod

    public override bool ApplyInputModify(InputScanResult data)
    {
        if(!EnableModify())
            return false;

        switch (data.ScanType)
        {
            case InputScanType.JoystickButton:

                if (m_joystickMatchType == JoystickMatchType.FixedIndex)
                    m_joystickFixedIndex.SetCustom(data.JoystickIndex);

                if (data.IsPositive)
                    m_positive.SetCustom(data.JoystickButton);
                else
                    m_negative.SetCustom(data.JoystickButton);

                break;

            case InputScanType.JoystickAxis:
                if (m_joystickMatchType == JoystickMatchType.FixedIndex)
                    m_joystickFixedIndex.SetCustom(data.JoystickIndex);
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

        if (m_invert.HasCustom || 
            m_positive.HasCustom || 
            m_negative.HasCustom || 
            m_axis.HasCustom || 
            m_joystickFixedIndex.HasCustom 
            )
            return true;

        return false;
    }

    public override void SerializeToXml(XmlWriter writer)
    {
        if (!NeedSerialize())
            return;

        writer.WriteStartElement("Binding");
        writer.WriteAttributeString("type", m_inputType.ToString());
        //writer.WriteElementString("Type", m_inputType.ToString());

        //writer.WriteElementString("Modifiable", m_modifiable.ToString().ToLower());
        //writer.WriteElementString("Sensitivity", m_sensitivity.ToString(CultureInfo.InvariantCulture));
        //writer.WriteElementString("Dead", m_dead.ToString(CultureInfo.InvariantCulture));
        //writer.WriteElementString("Gravity", m_gravity.ToString(CultureInfo.InvariantCulture));
        //writer.WriteElementString("Snap", m_snap.ToString().ToLower());
        if(m_invert.HasCustom)
            writer.WriteElementString("Invert", m_invert.value.ToString());


        if (m_positive.HasCustom)
            writer.WriteElementString("Positive", m_positive.value.ToString());
        //if(Positive_Custom.HasValue)
            //writer.WriteElementString("Positive", Positive_Custom.Value.ToString());

        if(m_negative.HasCustom)
            writer.WriteElementString("Negative", m_negative.value.ToString());

        if(m_axis.HasCustom)
            writer.WriteElementString("Axis", m_axis.value.ToString());

        if(m_joystickFixedIndex.HasCustom)
            writer.WriteElementString("JoystickFixedIndex", m_joystickFixedIndex.value.ToString());

        writer.WriteEndElement();
    }

    public override void DeserializeToXml(XmlNode node)
    {
        if (!EnableModify())
            return;

        var t = node.Attributes["type"].InnerText;
        if (string.IsNullOrEmpty(t) || t != InputTypeString)
            return;

        if(InputLoaderXML.ReadElement(node, "Invert", out bool v))
        {
            m_invert.SetCustom(v);
        }

        if(InputLoaderXML.ReadElement(node, "Positive", out JoystickButton p))
        {
            m_positive.SetCustom(p);
        }

        if(InputLoaderXML.ReadElement(node, "Negative", out JoystickButton n))
        {
            m_negative.SetCustom(n);
        }

        if(InputLoaderXML.ReadElement(node, "Axis", out int a))
        {
            m_axis.SetCustom(a);
        }

        if(InputLoaderXML.ReadElement(node, "JoystickFixedIndex", out int i))
        {
            m_joystickFixedIndex.SetCustom(i);
        }
    }

    public override void Clear()
    {
        m_positive.SetCustom(JoystickButton.None);
        m_negative.SetCustom(JoystickButton.None);
        m_axis.SetCustom(-1);
        m_joystickFixedIndex.SetCustom(-1);
    }

    public override void Reset()
    {
        m_positive.ResetToDefault();
        m_negative.ResetToDefault();
        m_axis.ResetToDefault();
        m_joystickFixedIndex.ResetToDefault();
    }


    #endregion UtilityMethod
}

