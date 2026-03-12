using System.IO;
using UnityEditor;
using UnityEngine;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    public partial class MISMainSetup : MISSetup
    {
        protected MISToolBar[] toolBars;
        protected int toolBarIndex = 0;


        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        [MenuItem("Tools/MIS/MIS Setup", false, (int)MISEditor.MISMenuItem.MISSetup)]
        public static void ShowWindow()
        {
            GetWindow(typeof(MISMainSetup), false, "MIS v" + MIS.MIS_VERSION);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected override void OnEnable()
        {
            base.OnEnable();

            maxSize = new Vector2(minWidth, minHeight);
            minSize = maxSize;

            //SetTitleVersion("MIS Setup", MIS.MIS_VERSION);
            misBanner = (Texture2D)Resources.Load("MIS_SetupBanner", typeof(Texture2D));

            // ----------------------------------------------------------------------------------------------------
            // 
            toolBars = new MISToolBar[]
            {
                new MISToolBar("MIS", MISContent),
                new MISToolBar("Management", ManagementContent),
                //new MISToolBar("Character Converter", CharacterConverterContent),
                new MISToolBar("Addons", AddonsContent),
                new MISToolBar("AI Addons", AIContent),
                new MISToolBar("ETC", ETCContent),
                new MISToolBar("Features", FeaturesContent)
            };
            toolBarIndex = 0;


            // ----------------------------------------------------------------------------------------------------
            // 
            InitializeRefactoringClass();


            // ----------------------------------------------------------------------------------------------------
            // 
            LoadMISSetupOptions();
            LoadSetupOptions();
            LoadAISetupOptions();
            LoadFeatureOptions();


            // ----------------------------------------------------------------------------------------------------
            // 
            OnEnableETC();


            // ----------------------------------------------------------------------------------------------------
            // 
            //RefreshInvectorCharacter();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected override void OnGUI()
        {
            base.OnGUI();

            DrawBanner();
            DrawToolbar();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void DrawBanner()
        {
            GUILayout.Label(misBanner, /*GUILayout.ExpandWidth(true), */GUILayout.Height(80));
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void DrawToolbar()
        {
            GUILayout.Space(-5);
            
            toolBarIndex = GUILayout.Toolbar(toolBarIndex, ToolbarNames());

            if (EditorApplication.isCompiling)
            {
                GUILayout.BeginVertical(skin.GetStyle("WindowBG"), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                {
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.HelpBox("Unity Editor is busy. Please wait...", MessageType.Info);
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndVertical();
            }
            else
            {
                toolBars[toolBarIndex].Draw();
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        string[] ToolbarNames()
        {
            string[] names = new string[toolBars.Length];

            for (int i = 0; i < toolBars.Length; i++)
                names[i] = toolBars[i];

            return names;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected override void LoadMISSetupOptions()
        {
            misSetupOption = AssetDatabase.LoadAssetAtPath<mvAddonSetupOption>(
                Path.Combine(MISEditor.MIS_EDITOR_PATH, "MISSetup/MISAddon/AddonSetupOptionData.asset"));

            if (misSetupOption != null && misSetupSO == null)
                misSetupSO = new SerializedObject(misSetupOption);
        }
    }
}