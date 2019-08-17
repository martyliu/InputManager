using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class KeyboardMouseInputAction : InputActionBase
{
    [SerializeField]
    public KeyboardMouseInputBinding[] bindings;

    protected override InputBindingBase[] m_bindings => bindings;


}
