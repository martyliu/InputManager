﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public partial class InputManager : MonoBehaviour
{

    //public string ConfigFilePath { get { return Application.persistentDataPath + "/input_config.xml"; } }
    public string ConfigFilePath { get { return Application.dataPath + "/input_config.xml"; } }

    #region Const
    public const float AXIS_ZERO = 0.0f;
    public const float AXIS_POSITIVE = 1.0f;
    public const float AXIS_NEGATIVE = -1.0f;

    public const int MOUSE_AXIS_COUNT = 3; // 鼠标轴的数量

    public const int JOYSTICK_COUNT = 11; // 手柄数量
    public const int JOYSTICK_BUTTON_COUNT = 20; // 手柄按键数量

    /// 手柄轴数量 , 最大是28，
    /// 但是XBox的手柄一个按键同时触发两个轴，所以在此处限制它的数量
    public const int JOYSTICK_AXIS_COUNT = 8;  

    #endregion Const

    static InputManager _instance;
    public static InputManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<InputManager>();
                return _instance;
            }
            return _instance;
        }
    }

    [SerializeField]
    private bool loadConfigOnAwake = true;

    [SerializeField]
    public List<KeyboardMouseControlScheme> pcControlSchemes;

    [SerializeField]
    public List<JoystickControlScheme> joystickControlSchemes;

    [SerializeField]
    [Tooltip("检测手柄的间隔时间")]
    private float checkJoysticksDuration = 2.0f;

    #region Variables

    KeyboardMouseControlScheme curKeyboardScheme;

    JoystickControlScheme curJoystickScheme;

    private float checkJoysticksTime = 0.0f;

    private string[] joysticksNameArray;

    InputScanService scanService = new InputScanService();

    #endregion Variables


    private void Awake()
    {
        _instance = this;

        if (loadConfigOnAwake)
            Load();

        if(pcControlSchemes != null && pcControlSchemes.Count > 0)
            ChangeUsingPCSchemeInternal(pcControlSchemes[0]);
    }

    void Update()
    {
        var dt = Time.deltaTime;
        checkJoysticksTime -= dt;
        if(checkJoysticksTime <= 0 )
        {
            checkJoysticksTime = checkJoysticksDuration;
            CheckJoysticks();
        }

        scanService.Update(dt);

        if (curJoystickScheme != null)
            curJoystickScheme.Update(dt);

        if (curKeyboardScheme != null)
            curKeyboardScheme.Update(dt);
    }

    #region Joystick
    private Action<string[]> joystickChangeHandler;
    public static event Action<string[]> JoystickChangeHandler
    {
        add { if (_instance != null) _instance.joystickChangeHandler += value; }
        remove { if (_instance != null) _instance.joystickChangeHandler -= value; }
    }

    /// <summary>
    /// 检查gamepad方案
    /// </summary>
    void CheckCurrentJoystickScheme()
    {
        /// 查找匹配当前手柄的方案
        if (curJoystickScheme != null)
        {
            for(int i = 0, length = joysticksNameArray.Length;i < length; i++ )
            {
                var joy = joysticksNameArray[i];
                if (!string.IsNullOrEmpty(joy) 
                    && curJoystickScheme.IsJoystickMatch(joy) 
                    && curJoystickScheme.connectedJoystickIdx == i)
                {
                    /// 当前使用的joystick没有变化
                    return;
                }
            }

            // 查找失败，更新
            curJoystickScheme = null;
        }

        for (int i = 0 ,length = joysticksNameArray.Length; i < length; i++)
        {
            var joy = joysticksNameArray[i];
            if (!string.IsNullOrEmpty(joy))
            {
                foreach (var s in joystickControlSchemes)
                {
                    if (s.IsJoystickMatch(joy))
                    {
                        ChangeUsingJoystickSchemeInternal(s, i);
                        return;
                    }
                }
            }
        }

        /// 找了一轮都没有找到匹配的手柄，则默认使用第一个
        if(joystickControlSchemes.Count > 0)
        {
            for (int i = 0, length = joysticksNameArray.Length; i < length; i++)
            {
                var joy = joysticksNameArray[i];
                if (!string.IsNullOrEmpty(joy))
                {
                    ChangeUsingJoystickSchemeInternal(joystickControlSchemes[0], i);
                    return;
                }
            }
        }

    }

    void OnJoystickChange()
    {
        CheckCurrentJoystickScheme();
        joystickChangeHandler?.Invoke(joysticksNameArray);
    }


    /// <summary>
    /// 检测手柄是否有变化
    /// </summary>
    void CheckJoysticks()
    {
        if (joysticksNameArray == null)
        {
            joysticksNameArray = Input.GetJoystickNames();
            foreach (var a in joysticksNameArray)
            {
                if (!string.IsNullOrEmpty(a))
                {
                    OnJoystickChange();
                    break;
                }
            }
        }
        else
        {
            bool changed = false;
            var arr = Input.GetJoystickNames();
            var length = arr.Length;
            if (length != joysticksNameArray.Length)
            {
                changed = true;

            }
            else
            {
                for (int i = 0; i < length; i++)
                {
                    if (arr[i] != joysticksNameArray[i])
                    {
                        changed = true;
                        break;
                    }
                }
            }

            if (changed)
            {
                joysticksNameArray = arr;
                OnJoystickChange();
            }
        }
    }

    #endregion Joystick

    #region ScanService

    public bool StartScan(InputScanSetting setting, InputScanHandler handler, 
        float timeout = InputScanService.TIME_OUT_DURATION, 
        KeyCode cancel = InputScanService.CANCEL_KEY_CODE, 
        KeyCode clear = InputScanService.CLEAR_KEY_CODE)
    {
        return scanService.Start(setting, handler, timeout, cancel, clear);
    }

    public void StopScan()
    {
        scanService.ForceStop();
    }

    #endregion ScanService

    #region SaveAndLoad
    [ContextMenu("Load")]
    /// <summary>
    /// 加载玩家的配置
    /// </summary>
    public void Load()
    {
        if (!File.Exists(ConfigFilePath))
            return;

        var loader = new InputLoaderXML(ConfigFilePath);
        loader.Load(ref this.joystickControlSchemes, ref this.pcControlSchemes);
    }

    [ContextMenu("Save")]
    /// <summary>
    /// 保存玩家的配置
    /// </summary>
    public void Save()
    {
        var saver = new InputSaverXML(ConfigFilePath);
        Debug.Log("Save: " + ConfigFilePath);
        saver.Save(pcControlSchemes, joystickControlSchemes);
    }

    [ContextMenu("ResetKeyboard")]
    public void ResetKeyboardMouseScheme()
    {
        if(curKeyboardScheme != null)
            ResetScheme(curKeyboardScheme);
    }

    [ContextMenu("ResetJoystick")]
    public void ResetJoystickScheme()
    {
        if (curJoystickScheme != null)
            ResetScheme(curJoystickScheme);
    }

    /// <summary>
    /// 将操作方案设回为默认值
    /// </summary>
    /// <param name="scheme"></param>
    public void ResetScheme(ControlSchemeBase scheme)
    {
        scheme.ResetToDefault();
    }
    #endregion SaveAndLoad

    void ChangeUsingPCSchemeInternal(KeyboardMouseControlScheme newScheme)
    {
        if (newScheme != curKeyboardScheme)
        {
            curKeyboardScheme = newScheme;
            curKeyboardScheme.Initialize();
        }
    }

    void ChangeUsingJoystickSchemeInternal(JoystickControlScheme newScheme, int joystickIdx = -1)
    {
        if(curJoystickScheme != null && joystickIdx == -1)
        {
            int idx = curJoystickScheme.connectedJoystickIdx;
            curJoystickScheme = newScheme;
            curJoystickScheme.Initialize(idx);
        }
        else if(joystickIdx != -1)
        {
            curJoystickScheme = newScheme;
            curJoystickScheme.Initialize(joystickIdx);
        }

    }
}
