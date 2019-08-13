using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLiu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        InputManager.JoystickChangeHandler += Changee;
        
    }

    private void Changee(string[] arr)
    {
        Debug.Log("joystick change !");
    }

    // Update is called once per frame
    void Update()
    {
        var hor= InputManager.GetAxis("Horizontal");
        if(Mathf.Abs(hor) > 0.01f)
            Debug.Log(hor);
        var s = Input.GetAxis("joy_1_axis_0");
        //if (Mathf.Abs(s) > 0.01f)
            //Debug.Log("sss :" + s);
    }
}
