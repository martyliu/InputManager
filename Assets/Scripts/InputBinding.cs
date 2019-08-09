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
    private InputType inputType;

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

    // 手柄下标
    [SerializeField]
    private int m_joystick = 0;

    // 轴下标
    [SerializeField]
    private int m_axis = 0;

    [SerializeField]
    private JoystickButton m_joystickBtn;

    #endregion SerializeField

    private ButtonState m_analogButtonState;

    private float m_value;

    private string m_rawAxisName;

    public void Initialize()
    {
        
        if(inputType == InputType.MouseAxis)
        {
            if(m_axis >= 0 && m_axis < MOUSE_AXIS_COUNT)
            {
                m_rawAxisName = string.Format("mouse_axis_{0}", m_axis);
            }
            else
            {
                Debug.LogError(m_axis + " mouse axis out of index! " );
                m_rawAxisName = "";
            }
        }else if(inputType == InputType.JoystickButton)
        {
            if (m_joystick >= 0 && m_joystick < JOYSTICK_COUNT)
            {
                int v = (int)KeyCode.Joystick1Button0 + m_joystick * JOYSTICK_BUTTON_COUNT + (int)m_joystickBtn;
                m_positive = (KeyCode)v;

            }else
            {
                Debug.LogError(m_joystick + " joystick out of index! " );
                m_positive = KeyCode.None;
            }

        }else if(inputType == InputType.AnalogAxis || inputType == InputType.AnalogButton)
        {
            if (m_joystick >= 0 && m_joystick < JOYSTICK_COUNT)
            {
                if(m_axis >= 0 && m_axis < JOYSTICK_AXIS_COUNT)
                {
                    m_rawAxisName = string.Format("joy_" + m_joystick.ToString() + "_axis_" + m_axis.ToString());

                }else
                {
                    Debug.LogError(m_joystick.ToString() + " joystick " + m_axis.ToString() + " axis out of index! ");
                    m_rawAxisName = "";
                }

            }
            else
            {
                Debug.LogError(m_joystick + " joystick out of index! ");
                m_rawAxisName = "";
            }
        }
    }

    public bool? GetButton()
    {
        bool? result = false;
        switch (inputType)
        {
            case InputType.Button:
            case InputType.JoystickButton:
                result = Input.GetKey(m_positive);
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

        switch (inputType)
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
        switch (inputType)
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

    float ApplyDeadZone(float value)
    {
        if (value > -m_dead && value < m_dead)
            value = AXIS_ZERO;
        return value;
    }

    public float? GetAxis()
    {
        float? result = null;

        switch (inputType)
        {

            case InputType.MouseAxis:
                result = Input.GetAxis(m_rawAxisName);
                result = ApplyDeadZone(result.Value);
                result = m_invert ? -result : result;

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

    public void Update(float deltaTime)
    {
        if (inputType == InputType.DigitalAxis)
        {
            UpdateDigitalAxis(deltaTime);
        }
        else if (inputType == InputType.AnalogButton)
        {
            UpdateAnalogButton();
        }
    }
    public void UpdateDigitalAxis(float deltaTime)
    {
        if (inputType != InputType.DigitalAxis)
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
        if (inputType != InputType.AnalogButton)
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
}
