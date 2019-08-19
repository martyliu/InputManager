using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct InputScanSetting
{
    public InputScanType ScanType; // 类型

    public JoystickButton CurJoystickButton; // 当前的按键
    public KeyCode CurKeyCode; // 当前按键

    public int CurJoystickIndex; // 手柄下标
    public int CurJoystickAxis; // 手柄的轴下标

    public int CurMouseAxis;  // 鼠标的轴下标

    public bool IsInvert;

    public bool IsPositive ; // 是否正方向

    public InputBindingBase InputBinding;

    public InputScanSetting(InputScanType t, InputBindingBase bindings)
    {
        ScanType = t;
        InputBinding = bindings;

        CurJoystickButton =  JoystickButton.None;
        CurKeyCode =  KeyCode.None;

        CurJoystickIndex = -1;
        CurJoystickAxis = -1;

        CurMouseAxis = -1;

        IsPositive = true;
        IsInvert = false;
    }
}

public struct InputScanResult
{
    public InputResultType ResultType;

    public KeyCode KeyCode;
    public JoystickButton JoystickButton;
    public int JoystickIndex;
    public int Axis;

    InputScanSetting setting;

    public InputScanType ScanType { get { return setting.ScanType; } }
    public bool IsPositive { get { return setting.IsPositive; } }
    public InputBindingBase InputBinding { get { return setting.InputBinding; } }

    public InputScanResult(InputScanSetting st, InputResultType rt)
    {
        ResultType = rt;

        setting = st;
        KeyCode = KeyCode.None;
        JoystickButton = JoystickButton.None;
        JoystickIndex = -1;
        Axis = -1;
    }

}

public enum InputScanType
{
    None,

    KeyboardButton, // 键盘key
    MouseAxis, // 鼠标轴

    JoystickButton, // 手柄key
    JoystickAxis, // 手柄轴
}

public enum InputResultType
{
    Success,
    Cancel,
    Clear,
}

public delegate bool InputScanHandler(InputScanResult result);

/// <summary>
/// 监测改键
/// 有些手柄的轴默认值并不是0，可能是-1，或者1，所以应该监听按键的变化来设置改键
/// </summary>
public class InputScanService 
{
    public const float TIME_OUT_DURATION = 10.0f;
    public const KeyCode CANCEL_KEY_CODE = KeyCode.Escape;
    public const KeyCode CLEAR_KEY_CODE = KeyCode.Backspace;

    private InputScanType m_scanningType = InputScanType.None;
    private InputScanHandler m_scanHandler;

    private KeyCode m_cancelScanKey;
    private KeyCode m_clearScanKey;
    private float m_scanTimeout = float.PositiveInfinity;

    private KeyCode[] m_keys;
    private string[] m_rawMouseAxes;
    private string[] m_rawJoystickAxes;

    /// <summary>
    /// 用于检测axes的变化
    /// </summary>
    Dictionary<string, float> axesToValueMap = new Dictionary<string, float>();

    float m_leftTime = float.PositiveInfinity;

    InputScanSetting m_curScaningSetting;

    public bool IsScanning { get; private set; }

    public InputScanService()
    {
        m_rawMouseAxes = new string[InputManager.MOUSE_AXIS_COUNT];
        for (int i = 0; i < m_rawMouseAxes.Length; i++)
        {
            m_rawMouseAxes[i] = string.Concat("mouse_axis_", i);
        }

        m_rawJoystickAxes = new string[InputManager.JOYSTICK_COUNT * InputManager.JOYSTICK_AXIS_COUNT];
        for (int i = 0; i < InputManager.JOYSTICK_COUNT; i++)
        {
            for (int j = 0; j < InputManager.JOYSTICK_AXIS_COUNT; j++)
            {
                m_rawJoystickAxes[i * InputManager.JOYSTICK_AXIS_COUNT + j] = string.Concat("joy_", i, "_axis_", j);
            }
        }

        m_keys = (KeyCode[])System.Enum.GetValues(typeof(KeyCode));
        IsScanning = false;

    }

    public bool Start(InputScanSetting setting, InputScanHandler handler, float timeout, KeyCode cancel, KeyCode clear)
    {
        if (IsScanning)
            ForceStop();

        m_curScaningSetting = setting;
        m_scanningType = setting.ScanType;

        m_leftTime = timeout;
        m_cancelScanKey = cancel;
        m_clearScanKey = clear;
        m_scanHandler = handler;

        IsScanning = true;

        axesToValueMap.Clear();

        return true;
    }

    public void ForceStop()
    {
        if(IsScanning)
        {
            IsScanning = false;

            if (m_scanHandler != null)
            {
                var result = new InputScanResult(m_curScaningSetting, InputResultType.Cancel);
                m_scanHandler(result);
            }
        }
    }

    void ClearInput()
    {
        if (IsScanning)
        {
            IsScanning = false;

            if (m_scanHandler != null)
            {
                var result = new InputScanResult(m_curScaningSetting, InputResultType.Clear);
                m_scanHandler(result);
            }
        }
    }

    public void Update(float dt)
    {
        if (!IsScanning)
            return;

        m_leftTime -= dt;
        if(m_leftTime <= 0.0f || Input.GetKeyDown(m_cancelScanKey))
        {
            ForceStop();
            return;
        }

        if(Input.GetKeyDown(m_clearScanKey))
        {
            ClearInput();
            return;
        }

        bool success = false;
        switch (m_curScaningSetting.ScanType)
        {
            case InputScanType.KeyboardButton:
                success = ScanKeyboardButton();
                break;
            case InputScanType.MouseAxis:
                success = ScanMouseAxis();
                break;
            case InputScanType.JoystickButton:
                success = ScanJoystickButton();
                break;
            case InputScanType.JoystickAxis:
                success = ScanJoystickAxis();
                break;
        }

        IsScanning = IsScanning && !success;
    }

    /// <summary>
    /// 监测键盘输入
    /// </summary>
    /// <returns></returns>
    bool ScanKeyboardButton()
    {
        for(int i = 0, length = m_keys.Length; i < length; i++ )
        {
            if ((int)m_keys[i] >= (int)KeyCode.JoystickButton0)
                break;

            if (Input.GetKeyDown(m_keys[i]) && m_scanHandler != null)
            {
                var result = new InputScanResult(m_curScaningSetting, InputResultType.Success);
                result.KeyCode = m_keys[i];

                if (m_scanHandler(result))
                {
                    m_scanHandler = null;
                    return true;
                }
            }
        }

        return false;
    }

    bool ScanMouseAxis()
    {
        for (int i = 0; i < m_rawMouseAxes.Length; i++)
        {
            if(IsAxisChange(m_rawMouseAxes[i]) && m_scanHandler != null)
            {
                var result = new InputScanResult(m_curScaningSetting, InputResultType.Success);
                result.Axis = i;

                if (m_scanHandler(result))
                {
                    m_scanHandler = null;
                    return true;
                }
            }

        }
        return false;
    }

    bool ScanJoystickButton()
    {
        int start = (int)KeyCode.Joystick1Button0;
        int end = (int)KeyCode.Joystick8Button19;

        for(int i = start; i <= end; i++)
        {
            var curKey = (KeyCode)i;
            if(Input.GetKeyDown(curKey) && m_scanHandler != null)
            {
                var result = new InputScanResult( m_curScaningSetting, InputResultType.Success);
                result.JoystickIndex = (i - start) / InputManager.JOYSTICK_BUTTON_COUNT;
                result.JoystickButton = ((JoystickButton)((i - start) % InputManager.JOYSTICK_BUTTON_COUNT));

                if (m_scanHandler(result))
                {
                    m_scanHandler = null;
                    return true;
                }
            }
        }

        return false;
    }

    bool ScanJoystickAxis()
    {
        for (int i = 0; i < m_rawJoystickAxes.Length; i++)
        {
            if(IsAxisChange(m_rawJoystickAxes[i]) && m_scanHandler != null)
            {
                var result = new InputScanResult(m_curScaningSetting, InputResultType.Success);
                result.JoystickIndex = i / InputManager.JOYSTICK_AXIS_COUNT;
                result.Axis = i % InputManager.JOYSTICK_AXIS_COUNT;

                if (m_scanHandler(result))
                {
                    m_scanHandler = null;
                    return true;
                }
            }
        }
        return false;
    }

    bool IsAxisChange(string axisName)
    {
        var axisValue = Input.GetAxis(axisName);
        if (!axesToValueMap.ContainsKey(axisName))
            axesToValueMap.Add(axisName, axisValue);
        return Mathf.Abs(axisValue - axesToValueMap[axisName]) > 0.1f;
    }

}




