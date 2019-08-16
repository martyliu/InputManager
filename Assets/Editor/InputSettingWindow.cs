using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public partial class InputSettingWindow  : EditorWindow
{
    /*
    InputManager manager;
    List<ControlScheme> Schemes { get { return manager.Schemes; } }

    [SerializeField]
    LeftPanelSelection m_selection;

    GUIStyle selectedLabelStyle;

    float curLeftPanelWidth;
    Rect cursorChangeRect;
    bool resizing = false;
    Vector2 leftScrollViewPosition = Vector2.zero;
    Vector2 rightScrollView = Vector2.zero;

    int currentSelectSchemeIndex = -1;

    void InitStyle()
    {
        if(selectedLabelStyle == null)
        {
            selectedLabelStyle = (GUIStyle)"BottomShadowInwards";
            //selectedButtonStyle = EditorStyles.miniButton;
            //selectedButtonStyle.
        }
    }

    private void OnEnable()
    {
        IsOpen = true;

        if (m_selection == null)
            m_selection = new LeftPanelSelection();
        if (manager == null)
            manager = FindObjectOfType<InputManager>();

        curLeftPanelWidth = this.position.width / 2.0f;
        cursorChangeRect = new Rect(this.position.width / 2.0f, 0, 5.0f, this.position.height);

    }

    #region GUI
    private void OnGUI()
    {
        if (manager == null)
        {
            manager = FindObjectOfType<InputManager>();
            if(manager == null)
            {
                GUILayout.Label("请在clientEntrance场景下配置");
                return;
            }
        }

        InitStyle();

        ValidateSelection();

        Undo.RecordObject(manager, "InputSetting");

        DrawMainToolBar();

        DrawResizeablePanel();

        DrawLeftPanel();

        if(!m_selection.IsEmpty)
            DrawMainPanel();

        if (GUI.changed)
            EditorUtility.SetDirty(manager);
    }

    void DrawMainToolBar()
    {
        Rect screenRect = new Rect(0.0f, 0.0f, position.width, TOOLBAR_HEIGHT);
        Rect editMenuRect = new Rect(0.0f, 0.0f, MENU_WIDTH, screenRect.height);
        Rect paddingLabelRect = new Rect(editMenuRect.xMax, 0.0f, screenRect.width - MENU_WIDTH , screenRect.height);

        GUI.BeginGroup(screenRect);

        DrawEditMenu(editMenuRect);
        EditorGUI.LabelField(paddingLabelRect, "", EditorStyles.toolbarButton);
        GUI.EndGroup();
    }

    void DrawEditMenu(Rect screenRect)
    {
        EditorGUI.LabelField(screenRect, "Edit", EditorStyles.toolbarDropDown);
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 &&
            screenRect.Contains(Event.current.mousePosition))
        {
            CreateEditMenu(new Rect(screenRect.x, screenRect.yMax, 0.0f, 0.0f));
        }
    }
    #endregion GUI

    #region Menu
    void CreateEditMenu(Rect rectPos)
    {
        GenericMenu editMenu = new GenericMenu();
        editMenu.AddItem(new GUIContent("New Control Scheme"), false, HandleEditMenuOption, EditMenuOptions.NewControlScheme);
        if (m_selection.IsSelectingControlScheme)
            editMenu.AddItem(new GUIContent("New Action"), false, HandleEditMenuOption, EditMenuOptions.NewInputAction);
        else
            editMenu.AddDisabledItem(new GUIContent("New Action"));

        editMenu.AddSeparator("");

        if(!m_selection.IsEmpty)
            editMenu.AddItem(new GUIContent("Duplicate"), false, HandleEditMenuOption, EditMenuOptions.Duplicate);
        else
            editMenu.AddDisabledItem(new GUIContent("Duplicate"));

        if (!m_selection.IsEmpty)
            editMenu.AddItem(new GUIContent("Delete"), false, HandleEditMenuOption, EditMenuOptions.Delete);
        else
            editMenu.AddDisabledItem(new GUIContent("Delete"));

        if(!m_selection.IsEmpty)
            editMenu.AddItem(new GUIContent("Copy"), false, HandleEditMenuOption, EditMenuOptions.Copy);
        else
            editMenu.AddDisabledItem(new GUIContent("Copy"));

        if(!m_selection.IsEmpty)
            editMenu.AddItem(new GUIContent("Paste"), false, HandleEditMenuOption, EditMenuOptions.Paste);
        else
            editMenu.AddDisabledItem(new GUIContent("Paste"));

        editMenu.AddSeparator("");

        editMenu.DropDown(rectPos);
    }

    void HandleEditMenuOption(object userData)
    {

    }
    #endregion Menu

    #region LeftPanel

    void DrawResizeablePanel()
    {
        var resizeRect = new Rect(curLeftPanelWidth, TOOLBAR_HEIGHT, CURSOR_RECT_WIDTH, position.height);

        /// 
        EditorGUIUtility.AddCursorRect(resizeRect, MouseCursor.ResizeHorizontal);
        switch (Event.current.type)
        {
            case EventType.MouseDown:
                if (cursorChangeRect.Contains(Event.current.mousePosition))
                {
                    resizing = true;
                    Event.current.Use();
                }
                break;
            case EventType.MouseUp:
                if (resizing)
                {
                    resizing = false;
                    Event.current.Use();
                }
                break;
            case EventType.MouseDrag:
                if (resizing)
                {
                    curLeftPanelWidth += Event.current.delta.x;
                    curLeftPanelWidth = Mathf.Clamp(curLeftPanelWidth, 30.0f, this.position.width - 30.0f);
                    cursorChangeRect.Set(curLeftPanelWidth, cursorChangeRect.y, cursorChangeRect.width, this.position.height);
                    Event.current.Use();
                    this.Repaint();
                }
                break;
        }

    }

    void DrawLeftPanel()
    {
        var screenRect = new Rect(0.0f, TOOLBAR_HEIGHT, curLeftPanelWidth, position.height);
        Rect scrollViewRect = new Rect(screenRect.x, screenRect.y, screenRect.width, position.height - screenRect.y);
        var viewRect = new Rect(0.0f, 0.0f, scrollViewRect.width, CalculateLeftPanelHeight());

        float itemPosY = 0.0f;

        GUI.Box(screenRect, "");
        leftScrollViewPosition = GUI.BeginScrollView(screenRect,  leftScrollViewPosition, viewRect);

        //leftScrollViewPosition = EditorGUILayout.BeginScrollView(leftScrollViewPosition, GUILayout.Width(curLeftPanelWidth));
        for(int i = 0, count = Schemes.Count; i < count; i++)
        {
            Rect schemeRect = new Rect(1.0f, itemPosY, viewRect.width - 2.0f, HIERARCHY_ITEM_HEIGHT);
            DrawOneScheme(schemeRect, i);
            itemPosY += HIERARCHY_ITEM_HEIGHT;

            if(Schemes[i].IsExpaned)
            {
                for(int j = 0, aCount = Schemes[i].Actions.Count; j < aCount; j++)
                {
                    Rect actionRect = new Rect(ACTION_PREFIX_WIDTH, itemPosY, viewRect.width - 2.0f, HIERARCHY_ITEM_HEIGHT);
                    DrawOneAction(actionRect, i, j);
                    itemPosY += HIERARCHY_ITEM_HEIGHT;
                }

            }
        }

        GUI.EndScrollView();
        //EditorGUILayout.EndScrollView();
    }

    void DrawOneScheme(Rect rect, int index)
    {
        var scheme = Schemes[index];

        Rect foldRect = new Rect(rect.x, rect.y, FOLDOUT_ITEM_HEIGHT, rect.height);
        Rect nameRect = new Rect(foldRect.xMax, rect.y, rect.width - FOLDOUT_ITEM_HEIGHT, rect.height);
        scheme.IsExpaned = EditorGUI.Foldout(foldRect, scheme.IsExpaned, "");

        /// selected 
        if(m_selection.IsSelectingControlScheme && m_selection.ControlScheme == index )
        {
            GUI.Label(nameRect, scheme.Name, (GUIStyle)"BottomShadowInwards");

        }
        else
        {
            GUI.Label(nameRect, scheme.Name);
        }

        if(Event.current.type == EventType.MouseDown && nameRect.Contains(Event.current.mousePosition))
        {
            m_selection.SelectOneScheme(index);

            // 右键
            if(Event.current.button == 1)
            {
                ShowSchemeContextMenu();
            }

            Event.current.Use();
            this.Repaint();
        }
    }

    void DrawOneAction(Rect nameRect, int schemeIdx, int actionIdx)
    {
        var scheme = Schemes[schemeIdx];
        var action = scheme.Actions[actionIdx];

        if ( m_selection.IsSelectingAction && m_selection.Action == actionIdx)
        {
            GUI.Label(nameRect, action.Name, (GUIStyle)"BottomShadowInwards");
        }
        else
        {
            GUI.Label(nameRect, action.Name);
        }

        if (Event.current.type == EventType.MouseDown && nameRect.Contains(Event.current.mousePosition))
        {
            m_selection.SelectOneAction(schemeIdx, actionIdx);

            // 右键
            if (Event.current.button == 1)
            {
                ShowActionContextMenu();
            }

            Event.current.Use();
            this.Repaint();
        }
    }

    /// <summary>
    ///  右键菜单
    /// </summary>
    void ShowSchemeContextMenu()
    {
    }

    void ShowActionContextMenu()
    {
    }

    #endregion LeftPanel

    #region RightPanel
    void DrawMainPanel()
    {
        Rect rect = new Rect(curLeftPanelWidth, TOOLBAR_HEIGHT, position.width - curLeftPanelWidth, position.height - TOOLBAR_HEIGHT);

        if(m_selection.IsSelectingAction)
        {
            DrawInputAcitonFields(rect);
        }
        else if(m_selection.IsSelectingControlScheme)
        {
            DrawControlSchemeFields(rect, m_selection.ControlScheme);

        }else
        {
            return;
        }

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            if (position.Contains(Event.current.mousePosition))
            {
                Event.current.Use();
                GUI.FocusControl(null);
                Repaint();
            }
        }
    }

    void DrawControlSchemeFields(Rect rect, int schemeIdx)
    {
        var controlScheme = manager.Schemes[schemeIdx];
        rect.x += 5;
        rect.y += 5;
        rect.width -= 10;

        GUILayout.BeginArea(rect);
        controlScheme.Name = EditorGUILayout.TextField("Name", controlScheme.Name);
        controlScheme.Description = EditorGUILayout.TextField("Description", controlScheme.Description);
        GUILayout.EndArea();
    }


    void DrawInputAcitonFields(Rect rect)
    {

    }
    #endregion RightPanel

    private void OnDestroy()
    {
        IsOpen = false;
    }

    #region Utility
    void ValidateSelection()
    {
        if (manager == null || Schemes == null || Schemes.Count == 0)
            m_selection.Reset();
        else if(m_selection.IsSelectingControlScheme && m_selection.ControlScheme >= Schemes.Count)
            m_selection.Reset();
        else if(m_selection.IsSelectingAction && m_selection.Action >= Schemes[m_selection.ControlScheme].Actions.Count)
            m_selection.Reset();
    }

    float CalculateLeftPanelHeight()
    {
        float height = 0;
        foreach(var s in Schemes)
        {
            height += HIERARCHY_ITEM_HEIGHT;
            if(s.IsExpaned)
            {
                foreach(var a in s.Actions)
                {
                    height += HIERARCHY_ITEM_HEIGHT;
                }
            }
        }
        return height;
    }

    #endregion Utility


    #region Static
    public static bool IsOpen { get; private set; }

    [MenuItem("MechaWar/Input Manager", true)]
    public static bool IsOpenWindow()
    {
        var inputManager = FindObjectOfType<InputManager>();
        return inputManager != null;
    }


    [MenuItem("MechaWar/Input Manager")]
    public static void OpenWindow()
    {
        if(!IsOpen)
        {
            var inputManager = FindObjectOfType<InputManager>();
            if(inputManager != null)
            {
                var window = EditorWindow.GetWindow<InputSettingWindow>("Input Editor");
                window.manager = inputManager;
            }
        }
    }

    public static void CloseWindow()
    {
        if(IsOpen)
        {
            var window = EditorWindow.GetWindow<InputSettingWindow>("Input Editor");
            window.Close();
        }
    }

    #endregion Static
    */
}
