using System;
using System.IO;
using UnityEngine;

namespace UnityEditor.UI
{
    internal delegate void RenameResultCallback(bool userAccepted, string name, object userData);
    internal delegate string RenameCheckCallback(string name, object userData);

    internal struct RenameWindowArgs
    {
        public string windowTitle;
        public string infoTextAboveTextField;
        public string infoTextBelowTextField;
        public string okButttonText;
        public string cancelButttonText;
        public string startName;
        public object userData;
        public RenameResultCallback resultCallback;
        public RenameCheckCallback checkNameCallback;
    }

    internal class RenameWindow : EditorWindow
    {
        RenameWindowArgs m_Args;
        string m_Name;
        bool m_UserAccepted;
        GUIContent m_ErrorMessage = new GUIContent();
        Vector2 m_ErrorMessageSize;

        bool hasErrors { get { return !string.IsNullOrEmpty(m_ErrorMessage.text); } }

        static class Layout
        {
            public static readonly float margin = 15f;
            public static readonly float windowWidth = 450;
            public static readonly float windowBaseHeight = 90f;
            public static readonly float windowContentWidth = windowWidth - 2 * margin;
            public static readonly float widthBetweenButtons = 10f;
        }

        static class Styles
        {
            public static string textFieldID = "TextField";
            public static GUIStyle buttonStyle = new GUIStyle("Button");

            static Styles()
            {
                buttonStyle.padding.left = buttonStyle.padding.right = 30;
                buttonStyle.padding.top = buttonStyle.padding.bottom = 5;
            }
        }

        public static void Show(RenameWindowArgs args)
        {
            if (args.resultCallback == null)
                throw new ArgumentNullException("resultCallback");

            if (string.IsNullOrEmpty(args.windowTitle))
                throw new ArgumentException("Missing window title");

            CloseExisting();
            RenameWindow window = CreateInstance<RenameWindow>();
            window.Init(args);
        }

        public static void CloseExisting()
        {
            var renameWindows = Resources.FindObjectsOfTypeAll<RenameWindow>();
            foreach (var renameWindow in renameWindows)
                renameWindow.Close();
        }

        void Init(RenameWindowArgs args)
        {
            m_Args = args;
            m_Name = args.startName != null ? args.startName : string.Empty;
            titleContent = new GUIContent(m_Args.windowTitle);

            RefreshWindowHeight(true);
            CheckValidNameCallback();
            ShowModalUtility();
        }

        void RefreshWindowHeight (bool setInitialPosition)
        {
            float height = Layout.windowBaseHeight;
            if (!string.IsNullOrEmpty(m_Args.infoTextAboveTextField))
                height += 25;
            if (!string.IsNullOrEmpty(m_Args.infoTextBelowTextField))
                height += 20;
            if (hasErrors)
                height += m_ErrorMessageSize.y + 25f;

            var size = new Vector2(Layout.windowWidth, height);
            if (setInitialPosition)
            {
                var mainViewPosition = WindowUtils.GetMainViewPosition();
                var center = mainViewPosition.center;
                position = new Rect(center.x - size.x / 2, center.y - size.y / 2, size.x, size.y);
            }
            minSize = maxSize = size;
        }

        void CheckValidNameCallback()
        {
            if (m_Args.checkNameCallback != null)
            {
                var prevErrorMsg = m_ErrorMessage.text;
                m_ErrorMessage.text = m_Args.checkNameCallback(m_Name, m_Args.userData);
                if (!string.IsNullOrEmpty(m_ErrorMessage.text))
                {
                    m_ErrorMessageSize.y = EditorStyles.helpBox.CalcHeight(m_ErrorMessage, Layout.windowContentWidth);
                }

                if (prevErrorMsg != m_ErrorMessage.text)
                {
                    RefreshWindowHeight(false);
                }
            }
        }

        void OnGUI()
        {
            HandleKeyboard();

            using (new GUILayout.AreaScope(new Rect(Layout.margin, Layout.margin, Layout.windowContentWidth, position.height - 2 * Layout.margin)))
            {
                if (!string.IsNullOrEmpty(m_Args.infoTextAboveTextField))
                {
                    GUILayout.Label(m_Args.infoTextAboveTextField);
                    GUILayout.Space(5);
                }

                GUI.SetNextControlName(Styles.textFieldID);
                EditorGUI.FocusTextInControl(Styles.textFieldID);
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    m_Name = GUILayout.TextField(m_Name, GUILayout.ExpandWidth(true));
                    if (check.changed)
                        CheckValidNameCallback();
                }

                if (hasErrors)
                {
                    GUILayout.Space(5);
                    EditorGUILayout.HelpBox(m_ErrorMessage.text, MessageType.Error);
                }

                if (!string.IsNullOrEmpty(m_Args.infoTextBelowTextField))
                {
                    GUILayout.Space(15f);
                    using (new EditorGUI.DisabledScope(true))
                    {
                        GUILayout.Label(m_Args.infoTextBelowTextField, GUILayout.ExpandWidth(true));
                    }
                }

                GUILayout.FlexibleSpace();

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();

                    if (Application.platform == RuntimePlatform.OSXEditor)
                    {
                        CancelButton();
                        GUILayout.Space(Layout.widthBetweenButtons);
                        OkButton();
                    }
                    else // Windows and Linux
                    {
                        OkButton();
                        GUILayout.Space(Layout.widthBetweenButtons);
                        CancelButton();

                    }

                }
            }
        }

        void OkButton()
        {
            using (new EditorGUI.DisabledScope(hasErrors))
            {
                if (GUILayout.Button(m_Args.okButttonText, Styles.buttonStyle))
                    UserAcceptedName();
            }
        }

        void CancelButton()
        {
            if (GUILayout.Button(m_Args.cancelButttonText, Styles.buttonStyle))
                Cancel();
        }

        void HandleKeyboard()
        {
            var evt = Event.current;
            if (evt.type == EventType.KeyDown)
            {
                if (evt.keyCode == KeyCode.Escape)
                {
                    evt.Use();
                    Cancel();
                }

                if (!hasErrors)
                {
                    if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                    {
                        evt.Use();
                        UserAcceptedName();
                    }
                }
            }
        }

        void Cancel()
        {
            m_UserAccepted = false;
            Close();
        }

        void UserAcceptedName()
        {
            m_UserAccepted = true;
            Close();
        }

        private void OnDestroy()
        {
            var resultCallback = m_Args.resultCallback;
            m_Args.resultCallback = null;
            resultCallback?.Invoke(m_UserAccepted, m_UserAccepted ? m_Name : m_Args.startName, m_Args.userData);
        }
    }

    static class WindowUtils
    {
        // Todo: Move to core
        public static Rect GetMainViewPosition()
        {
            var mainViewType = Type.GetType("UnityEditor.MainView, UnityEditor");
            if (mainViewType == null)
                throw new MissingMemberException("Can't find internal type MainView.");

            var mainViews = Resources.FindObjectsOfTypeAll(mainViewType);
            if (mainViews == null || mainViews.Length == 0)
                throw new MissingMemberException("Can't find instance of MainView.");

            var mainView = mainViews[0];
            var screenPositionProperty = mainViewType.GetProperty("screenPosition", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (screenPositionProperty == null)
                throw new MissingFieldException("Can't find internal field 'screenPosition'.");

            var pos = (Rect)screenPositionProperty.GetValue(mainView, null);
            return pos;
         }
    }
 }