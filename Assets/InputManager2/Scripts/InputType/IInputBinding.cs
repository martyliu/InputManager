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

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="joystickIdx"></param>
    void Initialize(int joystickIdx = -1);

    void Update(float deltaTime);

    /// <summary>
    /// 是否允许修改
    /// </summary>
    /// <returns></returns>
    bool EnableModify();
    /// <summary>
    ///  生成改动列表
    /// </summary>
    /// <returns></returns>
    List<InputScanSetting> GenerateScanSetting();

    bool ApplyInputModify(InputScanSetting data);

    float? GetAxis();

    bool? GetButtonUp();
    bool? GetButtonDown();
    bool? GetButton();
}
