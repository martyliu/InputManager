using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputSaveData 
{
    public List<KeyboardMouseControlScheme> KeyboardMouseControlSchemes;
    public List<JoystickControlScheme> JoystickControlSchemes;
    //public string CurrentPCScheme;

    public InputSaveData()
    {
        KeyboardMouseControlSchemes = new List<KeyboardMouseControlScheme>();
        JoystickControlSchemes = new List<JoystickControlScheme>();
        //CurrentPCScheme = null;
    }
}
