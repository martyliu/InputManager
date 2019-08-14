using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class InputManager : MonoBehaviour
{
    #region Const
    public const float AXIS_ZERO = 0.0f;
    public const float AXIS_POSITIVE = 1.0f;
    public const float AXIS_NEGATIVE = -1.0f;

    public const int MOUSE_AXIS_COUNT = 3; // 鼠标轴的数量

    public const int JOYSTICK_COUNT = 11; // 手柄数量
    public const int JOYSTICK_BUTTON_COUNT = 20; // 手柄按键数量
    public const int JOYSTICK_AXIS_COUNT = 28; // 手柄轴数量

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
    private string defaultPCScheme;

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

    #endregion Variables


    private void Awake()
    {
        _instance = this;

        foreach (var s in pcControlSchemes)
        {
            if (s.Name == defaultPCScheme)
            {
                ChangeUsingPCSchemeInternal(s);
                break;
            }
        }
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
        curJoystickScheme = newScheme;
        curJoystickScheme.Initialize(joystickIdx);
    }
}
