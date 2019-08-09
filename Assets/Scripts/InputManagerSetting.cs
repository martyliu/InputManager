using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Client
{

    /// <summary>
    /// 输入配置表
    /// </summary>
    [CreateAssetMenu(fileName = "InputSetting.asset", menuName = "MechaWar/Create InputSetting")]
    public class InputManagerSetting : ScriptableObject
    {
        public List<ControlScheme> schemes;
    }
}
