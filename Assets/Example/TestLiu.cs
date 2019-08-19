using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestLiu : MonoBehaviour
{
    public UIInputElement template;
    public Transform content;

    private void Awake()
    {
        template.gameObject.SetActive(false);
    }

    private void Start()
    {
        RefreshUI();
    }

    [ContextMenu("RefreshUI")]
    void RefreshUI()
    {
        while(content.childCount > 0)
        {
            var c = content.GetChild(0);
            c.parent = null;
            GameObject.Destroy(c.gameObject);
        }

        var firstSchemes = InputManager.Instance.pcControlSchemes[0];
        foreach(var a in firstSchemes.actions)
        {
            var settings = a.GenerateScanSettings();

            foreach(var s in settings)
            {
                var t = Instantiate(template, content);
                t.gameObject.SetActive(true);
                t.SetData(a, s);
                //SetUIData(t.GetComponentInChildren<Text>(), a, s);
            }
        }


        var joyschemes = InputManager.Instance.joystickControlSchemes[0];
        foreach (var a in joyschemes.actions)
        {
            var settings = a.GenerateScanSettings();

            foreach (var s in settings)
            {
                var t = Instantiate(template, content);
                t.gameObject.SetActive(true);
                t.SetData(a, s);
                //SetUIData(t.GetComponentInChildren<Text>(), a, s);
            }
        }

        test = new string[InputManager.JOYSTICK_COUNT * InputManager.JOYSTICK_AXIS_COUNT];
        for (int i = 0; i < InputManager.JOYSTICK_COUNT; i++)
        {
            for (int j = 0; j < InputManager.JOYSTICK_AXIS_COUNT; j++)
            {
                test[i * InputManager.JOYSTICK_AXIS_COUNT + j] = string.Concat("joy_", i, "_axis_", j);
            }
        }
    }
    string[] test;

    // Update is called once per frame
    void Update()
    {

        /*
        Debug.Log("-----------------------");
        foreach(var s in test)
        {
            var v = Input.GetAxis(s);
            var rawV = Input.GetAxisRaw(s);
            if(v != 0.0f)
            {
                Debug.Log("ttt: "   + s + "_" + v + "_" + rawV);
            }
        }

        Debug.Log("-----------------------");
        */
        //var hor = InputManager.GetAxis("Horizontal");
        //if (hor != 0)
            //Debug.Log("hor: " + hor);
        
        
    }
}
