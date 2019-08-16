using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public partial class InputSettingWindow
{

    [Serializable]
    private class LeftPanelSelection
    {
        public const int NONE = -1;

        public int ControlScheme;
        public int Action;

        public bool IsEmpty
        {
            get { return ControlScheme == NONE && Action == NONE; }
        }

        public bool IsSelectingControlScheme
        {
            get { return ControlScheme != NONE && Action == NONE; }
        }

        public bool IsSelectingAction
        {
            get { return Action != NONE; }
        }

        public LeftPanelSelection()
        {
            ControlScheme = Action = NONE;
        }

        public void Reset()
        {
            ControlScheme = Action = NONE;
        }

        public void SelectOneScheme(int index)
        {
            ControlScheme = index;
            Action = NONE;
        }

        public void SelectOneAction(int sIdx, int aIdx)
        {
            ControlScheme = sIdx;
            Action = aIdx;
        }
     


    }

    private enum EditMenuOptions
    {
        NewControlScheme = 0, NewInputAction, Duplicate, Delete, Copy, Paste
    }

    private const float TOOLBAR_HEIGHT = 20.0f;
    private const float CURSOR_RECT_WIDTH = 10.0f;
    private const float MENU_WIDTH = 50.0f;
    private const float HIERARCHY_ITEM_HEIGHT = 18.0f;
    private const float FOLDOUT_ITEM_HEIGHT = 18.0f;
    private const float ACTION_PREFIX_WIDTH = 30.0f;


}
