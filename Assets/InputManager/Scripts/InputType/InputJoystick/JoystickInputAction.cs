using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class JoystickInputAction : InputActionBase
{
    [SerializeField]
    public JoystickInputBinding[] bindings;

    protected override InputBindingBase[] m_bindings => bindings;

 


}
