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

    public bool IsPositive ; // 是否正方向

    public IInputBinding InputBinding;

    public bool IsInvert;

    public InputScanSetting(InputScanType t, IInputBinding bindings)
    {
        ScanType = t;
        InputBinding = bindings;

        CurJoystickButton = JoystickButton.None;
        CurKeyCode = KeyCode.None;

        CurJoystickIndex = -1;
        CurJoystickAxis = -1;

        CurMouseAxis = -1;

        IsPositive = true;

        IsInvert = false;
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

public delegate bool InputScanHandler(InputScanSetting result);

/// <summary>
/// 监测改键
/// 有些手柄的轴默认值并不是0，可能是-1，或者1，所以应该监听按键的变化来设置改键
/// </summary>
public class InputScanService 
{
    public const float TIME_OUT_DURATION = 10.0f;
    public const KeyCode CANCEL_KEY_CODE = KeyCode.Escape;

    private InputScanHandler m_scanHandler;

    private KeyCode m_cancelScanKey;
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

    public bool Start(InputScanSetting setting, InputScanHandler handler, float timeout, KeyCode cancel)
    {
        if (IsScanning)
            ForceStop();

        m_curScaningSetting = setting;

        m_leftTime = timeout;
        m_cancelScanKey = cancel;
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

            m_curScaningSetting.ScanType = InputScanType.None;
            if (m_scanHandler != null)
                m_scanHandler(m_curScaningSetting);
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

            if (Input.GetKeyDown(m_keys[i]))
            {
                InputScanSetting result = m_curScaningSetting;
                result.CurKeyCode = m_keys[i];

                if (m_scanHandler(result))
                {
                    m_scanHandler = null;
                    m_curScaningSetting.ScanType = InputScanType.None;
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
            if(IsAxisChange(m_rawMouseAxes[i]))
            {
                var result = m_curScaningSetting;
                result.CurMouseAxis = i;

                if (m_scanHandler(result))
                {
                    m_scanHandler = null;
                    m_curScaningSetting.ScanType = InputScanType.None;
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
            if(Input.GetKeyDown(curKey))
            {
                var result = m_curScaningSetting;
                result.CurJoystickIndex = (i - start) / InputManager.JOYSTICK_BUTTON_COUNT;
                result.CurJoystickButton = (JoystickButton)((i - start) % InputManager.JOYSTICK_BUTTON_COUNT);

                if (m_scanHandler(result))
                {
                    m_scanHandler = null;
                    m_curScaningSetting.ScanType = InputScanType.None;
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
            if(IsAxisChange(m_rawJoystickAxes[i]))
            {
                var result = m_curScaningSetting;
                result.CurJoystickIndex = i / InputManager.JOYSTICK_AXIS_COUNT;
                result.CurJoystickAxis = i % InputManager.JOYSTICK_AXIS_COUNT;

                if (m_scanHandler(result))
                {
                    m_scanHandler = null;
                    m_curScaningSetting.ScanType = InputScanType.None;
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




