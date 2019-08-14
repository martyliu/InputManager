using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InputButtonState
{
    JustPressed,
    Pressed,
    JustRelease,
    Release,
}

public interface IInputBinding 
{

    void Initialize(int joystickIdx = -1);

    void Update(float deltaTime);

    float? GetAxis();

    bool? GetButtonUp();
    bool? GetButtonDown();
    bool? GetButton();
}
