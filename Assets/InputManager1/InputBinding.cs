using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

public enum InputType
{
    Button,
    JoystickButton, // 轴按键
    MouseAxis, // 鼠标轴
    DigitalAxis,  // button作为轴使用
    AnalogButton, //轴作为button使用
    AnalogAxis, // 轴
    //RemoteButton,  // 虚拟button
    //RemoteAxis, // 虚拟轴
    //GamepadButton,
    //GamepadAnalogButton,
    //GamepadAxis,
}

public enum JoystickButton
{
    JoystickButton0,
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
    Index,
    Name,
}


[Serializable]
public class InputBinding 
{
    public enum ButtonState
    {
        JustPressed,
        Pressed,
        JustRelease,
        Release,
    }

    public const float AXIS_ZERO = 0.0f;
    const float AXIS_POSITIVE = 1.0f;
    const float AXIS_NEGATIVE = -1.0f;

    public const int MOUSE_AXIS_COUNT = 3; // 鼠标轴的数量

    public const int JOYSTICK_COUNT = 11; // 手柄数量
    public const int JOYSTICK_BUTTON_COUNT = 20; // 手柄按键数量
    public const int JOYSTICK_AXIS_COUNT = 28; // 手柄轴数量

    const float checkAxisInterval = 0.1f;

    #region SerializeField

    [SerializeField]
    private InputType m_inputType;
    public InputType InputType
    {
        get { return m_inputType; }
        set { m_inputType = value; isDirty = true; }
    }

    [SerializeField]
    private KeyCode m_positive;

    [SerializeField]
    private KeyCode m_negative;

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
    private JoystickMatchType m_joystickMatchType = JoystickMatchType.Index;
    public JoystickMatchType JoystickMatchType
    {
        get { return m_joystickMatchType; }
        set { m_joystickMatchType = value; isDirty = true; }
    }

    // 手柄下标
    [Range(0, 10)]
    [SerializeField]
    private int m_joystickIndex = 0;
    public int JoystickIndex
    {
        get { return m_joystickIndex; }
        set { m_joystickIndex = value; isDirty = true; }
    }

    [SerializeField]
    private string m_joystickName = "";
    public string JoystickName
    {
        get { return m_joystickName; }
        set { m_joystickName = value; isDirty = true; }
    }

    // 轴下标
    [SerializeField]
    [Range(0, 27)]
    private int m_axis = 0;
    public int Axis
    {
        get { return m_axis; }
        set { m_axis = value; isDirty = true; }
    }

    [SerializeField]
    private JoystickButton m_joystickBtn;
    public JoystickButton JoystickBtn
    {
        get { return m_joystickBtn; }
        set { m_joystickBtn = value; isDirty = true; }
    }


    #endregion SerializeField

    #region Variables

    [NonSerialized]
    private ButtonState m_analogButtonState;

    [NonSerialized]
    private float m_value;

    [NonSerialized]
    private string m_rawAxisName;

    [NonSerialized]
    private KeyCode m_joystickKeyCode;

    /// <summary>
    /// 改了某些数值后，需要重新计算
    /// </summary>
    [NonSerialized]
    bool isDirty = false;

    #endregion Variables

    #region Initialize
    void ResetState()
    {
        m_analogButtonState = ButtonState.Release;
        m_value = AXIS_ZERO;
        m_rawAxisName = "";

        isDirty = true;
    }

    void InitializeRawAxisName()
    {
        if (m_inputType == InputType.MouseAxis)
        {
            if (m_axis >= 0 && m_axis < MOUSE_AXIS_COUNT)
            {
                m_rawAxisName = string.Format("mouse_axis_{0}", m_axis);
            }
            else
            {
                Debug.LogError(m_axis + " mouse axis out of index! ");
                m_rawAxisName = "";
            }
        }
        else if (m_inputType == InputType.AnalogAxis || m_inputType == InputType.AnalogButton)
        {
            var idx = GetJoystickIndex();
            if (idx >= 0 && idx < JOYSTICK_COUNT)
            {
                if (m_axis >= 0 && m_axis < JOYSTICK_AXIS_COUNT)
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

    void InitializeJoystickButton()
    {
        if (m_inputType == InputType.JoystickButton)
        {
            var idx = GetJoystickIndex();
            if (idx >= 0 && idx < JOYSTICK_COUNT)
            {
                int v = (int)KeyCode.Joystick1Button0 + idx * JOYSTICK_BUTTON_COUNT + (int)m_joystickBtn;
                m_joystickKeyCode = (KeyCode)v;
            }
            else
            {
                m_joystickKeyCode = KeyCode.None;
            }
        }
    }

    public void Initialize()
    {
        ResetState();
    }

    #endregion Initialize

    #region GetMethod
    public bool? GetButton()
    {
        bool? result = false;
        switch (m_inputType)
        {
            case InputType.Button:
                result = Input.GetKey(m_positive);
                break;
            case InputType.JoystickButton:
                result = Input.GetKey(m_joystickKeyCode);
                break;
            case InputType.AnalogButton:
                result = m_analogButtonState == ButtonState.Pressed || m_analogButtonState == ButtonState.JustPressed;
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
            case InputType.Button:
            case InputType.JoystickButton:
                result = Input.GetKeyDown(m_positive);
                break;
            case InputType.AnalogButton:
                result = m_analogButtonState == ButtonState.JustPressed;
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
            case InputType.Button:
            case InputType.JoystickButton:
                result = Input.GetKeyUp(m_positive);
                break;
            case InputType.AnalogButton:
                result = m_analogButtonState == ButtonState.JustRelease;
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
            case InputType.MouseAxis:
            case InputType.AnalogAxis:
                if(!string.IsNullOrEmpty(m_rawAxisName))
                {
                    result = Input.GetAxis(m_rawAxisName);
                    result = ApplyDeadZone(result.Value);
                    result = m_invert ? -result : result;
                }
                break;
            case InputType.DigitalAxis:
                result = m_invert ? -m_value : m_value;

                break;
            default:
                result = AXIS_ZERO;
                break;
        }

        if (result.HasValue && result.Value == AXIS_ZERO)
            result = null;
        
        return result;
    }

    #endregion GetMethod

    public void Update(float deltaTime)
    {
        CheckAndUpdateDirty();

        if (m_inputType == InputType.DigitalAxis)
        {
            UpdateDigitalAxis(deltaTime);
        }
        else if (m_inputType == InputType.AnalogButton)
        {
            UpdateAnalogButton();
        }
    }

    void CheckAndUpdateDirty()
    {
        if (isDirty)
        {
            isDirty = false;

            InitializeRawAxisName();
            InitializeJoystickButton();
        }
    }

    public void UpdateDigitalAxis(float deltaTime)
    {
        if (m_inputType != InputType.DigitalAxis)
            return;

        var positive = Input.GetKey(m_positive);
        var negative = Input.GetKey(m_negative);
        
        if(positive  && negative ) // 两个按键同时按
            return;

        if(positive ) // 按了 +
        {
            if(m_value < AXIS_ZERO && m_snap)
                m_value = AXIS_ZERO;

            m_value +=  m_sensitivity * deltaTime;

        }else if(negative ) // 按了-
        {
            if(m_value > AXIS_ZERO && m_snap)
                m_value = AXIS_ZERO;

            m_value -=  m_sensitivity * deltaTime;

        }else // 都没按
        {
            if(m_value > AXIS_ZERO)
            {
                m_value -= m_gravity * deltaTime;
                if (m_value < AXIS_ZERO)
                    m_value = AXIS_ZERO;

            }else if(m_value < AXIS_ZERO)
            {
                m_value += m_gravity * deltaTime;
                if (m_value > AXIS_ZERO)
                    m_value = AXIS_ZERO;
            }
        }

        m_value = Mathf.Clamp(m_value, AXIS_NEGATIVE, AXIS_POSITIVE);
    }

    public void UpdateAnalogButton( )
    {
        if (m_inputType != InputType.AnalogButton)
            return;

        float val = Input.GetAxis(m_rawAxisName);

        val = m_invert ? -val : val;

        if(val > m_dead)
        {
            if (m_analogButtonState == ButtonState.JustPressed)
            {
                m_analogButtonState = ButtonState.Pressed;
            }
            else if(m_analogButtonState == ButtonState.Release || m_analogButtonState == ButtonState.JustRelease)
            {
                m_analogButtonState = ButtonState.JustPressed;
            }

        }else
        {
            if (m_analogButtonState == ButtonState.JustRelease)
            {
                m_analogButtonState = ButtonState.Release;
            }else if(m_analogButtonState == ButtonState.Pressed || m_analogButtonState == ButtonState.JustPressed)
            {
                m_analogButtonState = ButtonState.JustRelease;
            }
        }
    }


    #region Utility

    int GetJoystickIndex()
    {
        switch (m_joystickMatchType)
        {
            case JoystickMatchType.Index:
                return m_joystickIndex;

            case JoystickMatchType.Name:
                var allJoysName = Input.GetJoystickNames();
                for(int i = 0, count = allJoysName.Length; i< count; i ++)
                {
                    if(allJoysName[i].Contains(m_joystickName))
                        return i;
                }
                Debug.LogWarning("can't match joystick! " + m_joystickName);
                return -1;

            default:
                return -1;
        }
    }

    float ApplyDeadZone(float value)
    {
        if (value > -m_dead && value < m_dead)
            value = AXIS_ZERO;
        return value;
    }

    #endregion Utility
}
