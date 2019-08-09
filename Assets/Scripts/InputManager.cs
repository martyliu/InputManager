using Client;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InputManager : MonoBehaviour
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

    public InputManagerSetting setting;

    [Tooltip("默认的输入方案")]
    public string defaultScheme;

    ControlScheme curUsingScheme;// 当前正在使用的模式

    public List<ControlScheme> schemes;


    private void Awake()
    {
        _instance = this;

        foreach(var s in schemes)
        {
            if(s.Name == defaultScheme)
            {
                curUsingScheme = s;
                curUsingScheme.DoActive();
                break;
            }
        }

        curUsingScheme.Actions[0].Name = "change";
    }

    private void Update()
    {
        if(curUsingScheme != null)
            curUsingScheme.Update(Time.deltaTime);
    }

    public static bool GetButton(int schemeIndex, string buttonName)
    {
        var scheme = Instance.schemes[schemeIndex];
        if(scheme != null)
            return scheme.GetButton(buttonName);
        else
        {
            Debug.Log(schemeIndex + " scheme has not exist ");
            return false;
        }
    }


    public static bool GetButtonUp(int schemeIndex, string buttonName)
    {
        var scheme = Instance.schemes[schemeIndex];

        if(scheme != null)
            return scheme.GetButtonUp(buttonName);
        else
        {
            Debug.Log(schemeIndex + " scheme has not exist ");
            return false;
        }
    }

    public static bool GetButtonDown(int schemeIndex, string buttonName)
    {
        var scheme = Instance.schemes[schemeIndex];
        if(scheme != null)
        {
            return scheme.GetButtonDown(buttonName);
        }else
        {
            Debug.Log(schemeIndex + " scheme has not exist ");
            return false;
        }
    }


    public static float GetAxis(int schemeIndex, string axisName)
    {
        var scheme = Instance.schemes[schemeIndex];
        if(scheme != null)
            return scheme.GetAxis(axisName);
        else
        {
            Debug.Log(schemeIndex + " scheme has not exist ");
            return 0.0f;
        }
    }

}
