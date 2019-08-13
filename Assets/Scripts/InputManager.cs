using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public partial class InputManager : MonoBehaviour
{
    static InputManager _instance;
    public static InputManager Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = FindObjectOfType<InputManager>();
                return _instance;
            }
            return _instance;
        }
    }

    [SerializeField]
    private List<ControlScheme> schemes;

    [SerializeField]
    [Tooltip("默认的输入方案")]
    private string defaultScheme;


    [SerializeField]
    [Tooltip("检测手柄的间隔时间")]
    private float checkJoysticksDuration = 2.0f;

    private ControlScheme curUsingScheme = null;// 当前正在使用的模式

    private ControlScheme gamepadScheme = null; // 当前手柄使用的模式，支持同时存在两种模式

    private InputScanService scanService = new InputScanService();

    float checkJoysticksTime = 0.0f;

    string[] joysticksNameArray;

    private Action<string[]> joystickChangeHandler;
    public static event Action<string[]> JoystickChangeHandler
    {
        add { if(_instance != null) _instance.joystickChangeHandler += value; }
        remove { if(_instance != null) _instance.joystickChangeHandler -= value; }
    }

    public List<ControlScheme> Schemes
    {
        get { return schemes; }
        set { schemes = value; }
    }

    private void Awake()
    {
        _instance = this;

        foreach(var s in schemes)
        {
            if(s.Name == defaultScheme)
            {
                ChangeUsingSchemeInternal(s);
                break;
            }
        }
    }

    private void Update()
    {
        var deltaTime = Time.deltaTime;
        if(curUsingScheme != null)
            curUsingScheme.Update(deltaTime);

        if (gamepadScheme != null)
            gamepadScheme.Update(deltaTime);

        checkJoysticksTime -= deltaTime;
        if(checkJoysticksTime <= 0)
        {
            CheckJoysticks();
            checkJoysticksTime = checkJoysticksDuration;
        }
    }

    #region GetMethod
    public bool GetButton(int schemeIndex, string buttonName)
    {
        var scheme = schemes[schemeIndex];
        if (scheme != null)
            return scheme.GetButton(buttonName);
        else
        {
            Debug.Log(schemeIndex + " scheme has not exist ");
            return false;
        }
    }


    public bool GetButtonUp(int schemeIndex, string buttonName)
    {
        var scheme = Instance.schemes[schemeIndex];
        if (scheme != null)
            return scheme.GetButtonUp(buttonName);
        else
        {
            Debug.Log(schemeIndex + " scheme has not exist ");
            return false;
        }
    }

    public bool GetButtonDown(int schemeIndex, string buttonName)
    {
        var scheme = Instance.schemes[schemeIndex];
        if (scheme != null)
        {
            return scheme.GetButtonDown(buttonName);
        }
        else
        {
            Debug.Log(schemeIndex + " scheme has not exist ");
            return false;
        }
    }


    public float GetAxis(int schemeIndex, string axisName)
    {
        var scheme = Instance.schemes[schemeIndex];
        if (scheme != null)
            return scheme.GetAxis(axisName);
        else
        {
            Debug.Log(schemeIndex + " scheme has not exist ");
            return 0.0f;
        }
    }
    #endregion GetMethod


    #region Method

    /// <summary>
    /// 检查gamepad方案
    /// </summary>
    void CheckGamepadScheme()
    {
        if(gamepadScheme == null)
        { 
            foreach(var s in schemes)
            {

            }
        }

    }

    void OnJoystickChange()
    {
        CheckGamepadScheme();
        joystickChangeHandler?.Invoke(joysticksNameArray);
    }


    /// <summary>
    /// 检测手柄是否有变化
    /// </summary>
    void CheckJoysticks()
    {
        if(joysticksNameArray == null)
        {
            joysticksNameArray = Input.GetJoystickNames();
            foreach(var a in joysticksNameArray)
            {
                if(!string.IsNullOrEmpty(a))
                {
                    OnJoystickChange();
                    break;
                }
            }
        }else
        {
            bool changed = false;
            var arr = Input.GetJoystickNames();
            var length = arr.Length;
            if(length != joysticksNameArray.Length)
            {
                changed = true;

            }else
            {
                for(int i = 0; i < length; i++)
                {
                    if (arr[i] != joysticksNameArray[i])
                    {
                        changed = true;
                        break;
                    }
                }
            }

            if(changed)
            {
                joysticksNameArray = arr;
                OnJoystickChange();
            }
        }
    }



    void ChangeUsingSchemeInternal(ControlScheme newScheme)
    {
        if(newScheme != curUsingScheme)
        {
            curUsingScheme = newScheme;
            curUsingScheme.DoActive();
        }
    }

    #endregion Method
}
