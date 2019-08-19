using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInputLoader 
{
    bool Load(ref List<JoystickControlScheme> joystickControls, ref List<KeyboardMouseControlScheme> keyboardControls);
}
