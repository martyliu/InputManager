using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using System;


public abstract class InputBindingBase  : IXmlInputData
{
    protected enum InputButtonState
    {
        JustPressed,
        Pressed,
        JustRelease,
        Release,
    }

    [Tooltip("是否允许修改")]
    [SerializeField]
    protected bool m_modifiable = false;
    public bool Modifiable
    {
        get { return m_modifiable; }
        set { m_modifiable = value; }
    }

    [SerializeField]
    protected float m_sensitivity = 1f;

    [SerializeField]
    protected float m_dead = 0.01f;

    [SerializeField]
    protected float m_gravity = 1.0f;

    [SerializeField]
    protected bool m_snap = false;

    [SerializeField]
    protected BoolModifiable m_invert;

    public abstract string InputTypeString { get; }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="joystickIdx"></param>
    public abstract void Active(int joystickIdx = -1);
    public abstract void DeActive();

    public abstract void Update(float deltaTime);

    public abstract float? GetAxis();
    public abstract float? GetAxisRaw();
    public abstract bool? GetButtonUp();
    public abstract bool? GetButtonDown();
    public abstract bool? GetButton();

    /// <summary>
    /// 是否允许修改
    /// </summary>
    /// <returns></returns>
    public virtual bool EnableModify()
    {
        return m_modifiable;
    }
    /// <summary>
    ///  生成改动列表
    /// </summary>
    /// <returns></returns>
    public abstract List<InputScanSetting> GenerateScanSetting();
    /// <summary>
    /// 修改键位
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public abstract bool ApplyInputModify(InputScanResult data);
    /// <summary>
    /// 清空键位
    /// </summary>
    public abstract void Clear();
    /// <summary>
    /// 重置键位
    /// </summary>
    public abstract void ResetToDefault();

    public abstract bool NeedSerialize();
    public abstract void SerializeToXml(XmlWriter writer);
    public abstract void DeserializeFromXml(XmlNode node);


    protected float ApplyDeadZone(float value)
    {
        if (value > -m_dead && value < m_dead)
            value = InputManager.AXIS_ZERO;
        return value;
    }

}
