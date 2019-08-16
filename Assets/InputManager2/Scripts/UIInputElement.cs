using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIInputElement : MonoBehaviour
{
    public Text Text_input;
    public Button Btn_modify;

    InputActionBase action;
    InputScanSetting setting;

    public void SetData(InputActionBase ac, InputScanSetting s)
    {
        action = ac;
        setting = s;


        Text_input.text = GetUIStr();
    }

    string GetUIStr()
    {
        var s = setting;
        var a = action;

        string showStr = a.Name + "_" + s.ScanType.ToString();
        switch (s.ScanType)
        {
            case InputScanType.KeyboardButton:
                if (s.IsPositive)
                    showStr += "_positive_" + s.CurKeyCode.ToString();
                else
                    showStr += "_negative_" + s.CurKeyCode.ToString();
                break;

            case InputScanType.MouseAxis:
                if (s.IsInvert)
                    showStr += "_MouseAxis" + s.CurMouseAxis + "_invert";
                else
                    showStr += "_MouseAxis" + s.CurMouseAxis;
                break;

            case InputScanType.JoystickButton:

                if (s.IsPositive)
                    showStr += "_positive" + "_Joy" + s.CurJoystickIndex + "_" + s.CurJoystickButton.ToString();
                else
                    showStr += "_negative" + "_Joy" + s.CurJoystickIndex + "_" + s.CurJoystickButton.ToString();
                break;
            case InputScanType.JoystickAxis:
                showStr += "_Joy" + s.CurJoystickIndex + "_Axis" + s.CurJoystickAxis.ToString();
                if (s.IsInvert)
                    showStr += "_invert";
                break;
        }
        return showStr;

    }


    private void Awake()
    {
        Btn_modify.onClick.AddListener(OnClickBtn);
    }

    private void OnDestroy()
    {
        if(Btn_modify)
            Btn_modify.onClick.RemoveListener(OnClickBtn);
    }

    void OnClickBtn()
    {
        if(InputManager.Instance.StartScan(setting, FinishScan))
        {
            Text_input.text = "...";
            Debug.Log("scanning " + GetUIStr());
        }
    }

    bool FinishScan(InputScanSetting result)
    {
        if (result.ScanType == InputScanType.None)
        {
            Text_input.text = GetUIStr();
            Debug.Log("scan end failed " + GetUIStr());
            return false;
        }


        result.InputBinding.ApplyInputModify(result);
        setting = result;
        Text_input.text = GetUIStr();
        Debug.Log("scan end success " + GetUIStr());

        return true;
    }


}
