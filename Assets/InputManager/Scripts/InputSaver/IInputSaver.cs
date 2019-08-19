using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInputSaver 
{
    void Save(List<KeyboardMouseControlScheme> keymouseSchemes, List<JoystickControlScheme> joystickSchemes);


}
