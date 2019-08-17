using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public enum InputButtonState
{
    JustPressed,
    Pressed,
    JustRelease,
    Release,
}

public abstract class InputBindingBase  : IXmlInputData
{
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
    protected bool m_invert = false;
    public bool Invert
    {
        get { return m_invert; }
        set { m_invert = value; }
    }

    public bool? Invert_Custom { get; set; } = null;


    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="joystickIdx"></param>
    public abstract void Initialize(int joystickIdx = -1);

    public abstract void Update(float deltaTime);

    public abstract float? GetAxis();
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
    public abstract bool ApplyInputModify(InputScanSetting data);

    public abstract void SerializeToXml(XmlWriter writer);
    public abstract void DeserializeToXml();


    protected float ApplyDeadZone(float value)
    {
        if (value > -m_dead && value < m_dead)
            value = InputManager.AXIS_ZERO;
        return value;
    }

}
