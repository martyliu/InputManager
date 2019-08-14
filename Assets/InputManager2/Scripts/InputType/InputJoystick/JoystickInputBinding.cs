using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
public class JoystickInputBinding : IInputBinding
{
    #region SerializeField
    [SerializeField]
    private JoystickInputType m_inputType;
    public JoystickInputType InputType
    {
        get { return m_inputType; }
        set { m_inputType = value; m_isDirty = true; }
    }

    [SerializeField]
    private JoystickButton m_positive = JoystickButton.None;
    public JoystickButton Positive
    {
        get { return m_positive; }
        set { m_positive = value; m_isDirty = true; }
    }

    [SerializeField]
    private JoystickButton m_negative = JoystickButton.None;
    public JoystickButton Negative
    {
        get { return m_negative; }
        set { m_negative = value; m_isDirty = true; }
    }

    [SerializeField]
    private float m_sensitivity = 1f;

    [SerializeField]
    private float m_dead = 0.01f;

    [SerializeField]
    private float m_gravity = 1.0f;

    [SerializeField]
    private bool m_snap = false;

    [SerializeField]
    private bool m_invert = false;

    [Tooltip("匹配手柄类型")]
    [SerializeField]
    private JoystickMatchType m_joystickMatchType = JoystickMatchType.FromScheme;
    public JoystickMatchType JoystickMatchType
    {
        get { return m_joystickMatchType; }
        set { m_joystickMatchType = value; m_isDirty = true; }
    }

    // 手柄下标
    [Range(0, 10)]
    [SerializeField]
    private int m_joystickFixedIndex = 0;
    public int JoystickIndex
    {
        get { return m_joystickFixedIndex; }
        set { m_joystickFixedIndex = value; m_isDirty = true; }
    }

    // 轴下标
    [SerializeField]
    [Range(0, 27)]
    private int m_axis = 0;
    public int Axis
    {
        get { return m_axis; }
        set { m_axis = value; m_isDirty = true; }
    }
    #endregion SerializeField

    private bool m_isDirty = false;

    private KeyCode m_joystickPositive = KeyCode.None;

    private KeyCode m_joystickNegative = KeyCode.None;

    private int m_joystickIdx;

    private string m_rawAxisName;

    private float m_value;

    private InputButtonState m_analogButtonState;

    /// <summary>
    /// 手柄的下标会根据usb插口变化
    /// 所以需要动态设置
    /// </summary>
    /// <param name="joystickIdx"></param>
    public void Initialize(int joystickIdx = -1)
    {
        ResetState();

        m_joystickIdx = joystickIdx;
    }

    void ResetState()
    {
        m_joystickIdx = -1;
        m_isDirty = true;
        m_value = InputManager.AXIS_ZERO;
    }

    public void Update(float deltaTime)
    {
        if(m_isDirty)
        {
            m_isDirty = false;
            InitializeButtonAndRawAxisName();
            Debug.Log(m_rawAxisName);
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
        if (m_inputType != JoystickInputType.AnalogButton)
            return;

        float val = Input.GetAxis(m_rawAxisName);

        val = m_invert ? -val : val;

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
            var idx = m_joystickIdx;
            if (idx >= 0 && idx < InputManager.JOYSTICK_COUNT)
            {
                if (m_positive == JoystickButton.None)
                {
                    m_joystickPositive = KeyCode.None;
                }
                else
                {
                    int v = (int)KeyCode.Joystick1Button0 + idx * InputManager.JOYSTICK_BUTTON_COUNT + (int)m_positive;
                    m_joystickPositive = (KeyCode)v;
                }

                if (m_negative == JoystickButton.None)
                {
                    m_joystickNegative = KeyCode.None;
                }
                else
                {
                    int v = (int)KeyCode.Joystick1Button0 + idx * InputManager.JOYSTICK_BUTTON_COUNT + (int)m_negative;
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
            var idx = m_joystickIdx;
            if (idx >= 0 && idx < InputManager.JOYSTICK_COUNT)
            {
                if (m_axis >= 0 && m_axis < InputManager.JOYSTICK_AXIS_COUNT)
                {
                    m_rawAxisName = string.Format("joy_" + idx.ToString() + "_axis_" + m_axis.ToString());
                }
                else
                {
                    Debug.LogError(idx.ToString() + " joystick " + m_axis.ToString() + " axis out of index! ");
                    m_rawAxisName = "";
                }

            }
            else
            {
                m_rawAxisName = "";
            }
        }
    }

    public bool? GetButton()
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

    public bool? GetButtonDown()
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

    public bool? GetButtonUp()
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

    public float? GetAxis()
    {
        float? result = null;

        switch (m_inputType)
        {
            case JoystickInputType.AnalogAxis:
                if (!string.IsNullOrEmpty(m_rawAxisName))
                {
                    result = Input.GetAxis(m_rawAxisName);
                    result = ApplyDeadZone(result.Value);
                    result = m_invert ? -result : result;
                }
                break;
            case JoystickInputType.DigitalAxis:
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

    float ApplyDeadZone(float value)
    {
        if (value > -m_dead && value < m_dead)
            value = InputManager.AXIS_ZERO;
        return value;
    }
}
