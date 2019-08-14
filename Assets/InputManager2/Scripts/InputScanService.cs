using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct InputScanResult
{
    public InputScanFlags ScanFlags;
    public KeyCode Key;
    public JoystickButton JoystickBtn;
    public int Joystick;
    public int JoystickAxis;
    public float JoystickAxisValue;
    public int MouseAxis;
    public object UserData;

}

public struct InputScanSettings
{
    public InputScanFlags ScanFlags;
    public int? Joystick;
    public float Timeout;
    public KeyCode CancelScanKey;
    public object UserData;

}

public enum InputScanFlags
{
    None = 0,
    Key = 1 << 1,
    JoystickButton = 1 << 2,
    JoystickAxis = 1 << 3,
    MouseAxis = 1 << 4
}


public delegate bool InputScanHandler(InputScanResult result);

/// <summary>
/// 监测改键
/// </summary>
public class InputScanService 
{

    private InputScanHandler m_scanHandler;
    private InputScanResult m_scanResult;
    private InputScanFlags m_scanFlags;
    private KeyCode m_cancelScanKey;
    private float m_scanStartTime;
    private float m_scanTimeout;
    private int? m_scanJoystick;
    private object m_scanUserData;

    private KeyCode[] m_keys;
    private string[] m_rawMouseAxes;
    private string[] m_rawJoystickAxes;

    public float GameTime { get; set; }
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

    public void Start(InputScanSettings settings, InputScanHandler scanHandler)
    {
        if (settings.Joystick.HasValue && (settings.Joystick < 0 || settings.Joystick >= InputManager.JOYSTICK_COUNT))
        {
            Debug.LogError("Joystick is out of range. Cannot start scan.");
            return;
        }

        if (IsScanning)
            Stop();

        m_scanTimeout = settings.Timeout;
        m_scanFlags = settings.ScanFlags;
        m_scanStartTime = GameTime;
        m_cancelScanKey = settings.CancelScanKey;
        m_scanJoystick = settings.Joystick;
        m_scanUserData = settings.UserData;
        m_scanHandler = scanHandler;
        IsScanning = true;

    }

    public void Stop()
    {
        if (IsScanning)
        {
            IsScanning = false;

            m_scanResult.ScanFlags = InputScanFlags.None;
            m_scanResult.Key = KeyCode.None;
            m_scanResult.Joystick = -1;
            m_scanResult.JoystickAxis = -1;
            m_scanResult.JoystickAxisValue = 0.0f;
            m_scanResult.MouseAxis = -1;
            m_scanResult.UserData = m_scanUserData;

            if (m_scanHandler != null)
                m_scanHandler(m_scanResult);

            m_scanJoystick = null;
            m_scanHandler = null;
            m_scanResult.UserData = null;
            m_scanFlags = InputScanFlags.None;
        }
    }


    public void Update()
    {
        float timeout = GameTime - m_scanStartTime;
        if (Input.GetKeyDown(m_cancelScanKey) || timeout >= m_scanTimeout)
        {
            Stop();
            return;
        }

        bool success = false;
        if (IsScanning && HasFlag(InputScanFlags.Key))
        {
            success = ScanKey();
        }
        if (IsScanning && !success && HasFlag(InputScanFlags.JoystickButton))
        {
            success = ScanJoystickButton();
        }
        if (IsScanning && !success && HasFlag(InputScanFlags.JoystickAxis))
        {
            success = ScanJoystickAxis();
        }
        if (IsScanning && !success && HasFlag(InputScanFlags.MouseAxis))
        {
            success = ScanMouseAxis();
        }

        IsScanning = IsScanning && !success;
    }

    private bool ScanKey()
    {
        int length = m_keys.Length;
        for (int i = 0; i < length; i++)
        {
            if ((int)m_keys[i] >= (int)KeyCode.JoystickButton0)
                break;

            if (Input.GetKeyDown(m_keys[i]))
            {
                m_scanResult.ScanFlags = InputScanFlags.Key;
                m_scanResult.Key = m_keys[i];

                m_scanResult.Joystick = -1;
                m_scanResult.JoystickAxis = -1;
                m_scanResult.JoystickAxisValue = 0.0f;
                m_scanResult.MouseAxis = -1;
                m_scanResult.UserData = m_scanUserData;

                if (m_scanHandler(m_scanResult))
                {
                    m_scanHandler = null;
                    m_scanResult.UserData = null;
                    m_scanFlags = InputScanFlags.None;
                    return true;
                }
            }
        }

        return false;
    }

    private bool ScanJoystickButton()
    {
        int scanStart = (int)KeyCode.Joystick1Button0;
        int scanEnd = (int)KeyCode.Joystick8Button19;

        if (m_scanJoystick.HasValue)
        {
            scanStart = (int)KeyCode.Joystick1Button0 + m_scanJoystick.Value * InputManager.JOYSTICK_BUTTON_COUNT;
            scanEnd = scanStart + InputManager.JOYSTICK_BUTTON_COUNT;
        }

        for (int key = scanStart; key < scanEnd; key++)
        {
            if (Input.GetKeyDown((KeyCode)key))
            {
                if (m_scanJoystick.HasValue)
                {
                    m_scanResult.Key = (KeyCode)key;
                    m_scanResult.Joystick = m_scanJoystick.Value;
                }
                else
                {
                    m_scanResult.Key = (KeyCode)((int)KeyCode.JoystickButton0 + (key - (int)KeyCode.Joystick1Button0) % 20);
                    m_scanResult.Joystick = ((key - (int)KeyCode.Joystick1Button0) / 20);
                }

                m_scanResult.JoystickAxis = -1;
                m_scanResult.JoystickAxisValue = 0.0f;
                m_scanResult.MouseAxis = -1;
                m_scanResult.UserData = m_scanUserData;
                m_scanResult.ScanFlags = InputScanFlags.JoystickButton;

                if (m_scanHandler(m_scanResult))
                {
                    m_scanHandler = null;
                    m_scanResult.UserData = null;
                    m_scanFlags = InputScanFlags.None;
                    return true;
                }
            }
        }

        return false;
    }

    private bool ScanJoystickAxis()
    {
        int scanStart = 0, scanEnd = m_rawJoystickAxes.Length;
        float axisRaw = 0.0f;

        if (m_scanJoystick.HasValue)
        {
            scanStart = m_scanJoystick.Value * InputManager.JOYSTICK_COUNT;
            scanEnd = scanStart + InputManager.JOYSTICK_AXIS_COUNT;
        }

        for (int i = scanStart; i < scanEnd; i++)
        {
            axisRaw = Input.GetAxisRaw(m_rawJoystickAxes[i]);
            if (Mathf.Abs(axisRaw) >= 1.0f)
            {
                m_scanResult.ScanFlags = InputScanFlags.JoystickAxis;
                m_scanResult.Joystick = i / InputManager.JOYSTICK_COUNT;
                m_scanResult.JoystickAxis = i % InputManager.JOYSTICK_COUNT;
                m_scanResult.JoystickAxisValue = axisRaw;

                m_scanResult.Key = KeyCode.None;
                m_scanResult.MouseAxis = -1;
                m_scanResult.UserData = m_scanUserData;

                if (m_scanHandler(m_scanResult))
                {
                    m_scanHandler = null;
                    m_scanResult.UserData = null;
                    m_scanFlags = InputScanFlags.None;
                    return true;
                }
            }
        }

        return false;
    }

    private bool ScanMouseAxis()
    {
        for (int i = 0; i < m_rawMouseAxes.Length; i++)
        {
            if (Mathf.Abs(Input.GetAxis(m_rawMouseAxes[i])) > 0.0f)
            {
                m_scanResult.ScanFlags = InputScanFlags.MouseAxis;
                m_scanResult.MouseAxis = i;
                m_scanResult.UserData = m_scanUserData;

                m_scanResult.Key = KeyCode.None;
                m_scanResult.Joystick = -1;
                m_scanResult.JoystickAxis = -1;
                m_scanResult.JoystickAxisValue = 0.0f;

                if (m_scanHandler(m_scanResult))
                {
                    m_scanHandler = null;
                    m_scanResult.UserData = null;
                    m_scanFlags = InputScanFlags.None;
                    return true;
                }
            }
        }

        return false;
    }


    private bool HasFlag(InputScanFlags flag)
    {
        return ((int)m_scanFlags & (int)flag) != 0;
    }

}




