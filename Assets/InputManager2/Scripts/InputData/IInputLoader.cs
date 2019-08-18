using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInputLoader 
{
    //InputSaveData Load();
    ControlSchemeBase Load(string schemeName);
}
