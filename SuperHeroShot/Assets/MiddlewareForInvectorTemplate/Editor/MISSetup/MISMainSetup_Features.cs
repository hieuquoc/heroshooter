using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    public partial class MISMainSetup
    {
        // ----------------------------------------------------------------------------------------------------
        // 
        protected List<FeatureSetup> unityFeatureList;
        protected List<FeatureSetup> thirdpartyFeatureList;

        float unityGroupHeight = 320f;
        float thirdpartyGroupHeight = 320f;

        Vector2 unityFeatureScroll;
        Vector2 thirdpartyFeaturesScroll;


        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected override void FeaturesContent()
        {
            GUILayout.BeginVertical(skin.GetStyle("WindowBG"), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            {
                EditorGUILayout.HelpBox("The following packages are not available in MIS unless they are enabled in MIS Setup. In order to avoid compiler errors, be sure to disable them before removing.", MessageType.Info);

                GUILayout.Space(5);

                // ----------------------------------------------------------------------------------------------------
                // Unity
                GUILayout.BeginVertical("box", GUILayout.MinHeight(unityGroupHeight));
                {
                    UnityFeatures();
                }
                GUILayout.EndVertical();

                GUILayout.Space(10);

                // ----------------------------------------------------------------------------------------------------
                // Third-Party
                GUILayout.BeginVertical("box", GUILayout.MinHeight(thirdpartyGroupHeight));
                {
                    ThirdPartyFeatures();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndVertical();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void UnityFeatures()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("<b>Unity Features</b>", GUILayout.MaxHeight(25));
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();

            unityFeatureScroll = EditorGUILayout.BeginScrollView(unityFeatureScroll, false, false, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            {
                for (int i = 0, n = unityFeatureList.Count; i < n; i++)
                {
                    unityFeatureList[i].featureItem(unityFeatureList[i].featureName);
                }
            }
            EditorGUILayout.EndScrollView();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void ThirdPartyFeatures()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("<b>Third-Party Features</b>", GUILayout.MaxHeight(25));
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();

            thirdpartyFeaturesScroll = EditorGUILayout.BeginScrollView(thirdpartyFeaturesScroll, false, false, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            {
                for (int i = 0, n = thirdpartyFeatureList.Count; i < n; i++)
                {
                    thirdpartyFeatureList[i].featureItem(thirdpartyFeatureList[i].featureName);
                }
            }
            EditorGUILayout.EndScrollView();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected override void LoadFeatureOptions()
        {
            // ----------------------------------------------------------------------------------------------------
            // Unity
            unityFeatureList = new()
            {
                //LoadFeatureSetup(null,
                //    "Animation Rigging", MISFeature.UNITY_FEATURE_ANIMATION_RIGGING, UnityFeatureItem),

                //LoadFeatureSetup(null,
                //    "Cinemachine", MISFeature.UNITY_FEATURE_CINEMACHINE, UnityFeatureItem),

                //LoadFeatureSetup(null,
                //    "Input System", MISFeature.UNITY_FEATURE_INPUT_SYSTEM, FeatureItem),

                //LoadFeatureSetup(null,
                //    "Timeline", MISFeature.UNITY_FEATURE_TIMELINE, FeatureItem),
            };


            // ----------------------------------------------------------------------------------------------------
            // Third-Party
            thirdpartyFeatureList = new()
            {
                //LoadFeatureSetup(null,
                //    "Crest4", MISFeature.ETC_FEATURE_CREST4, ThirdPartyFeatureItem),

                LoadFeatureSetup(null,
                    "Crest5", MISFeature.ETC_FEATURE_CREST5, ThirdPartyFeatureItem),

                //LoadFeatureSetup(null,
                //    "Vegetation Engine", MISFeature.ETC_FEATURE_VEGETATION_ENGINE, ThirdPartyFeatureItem),

                //LoadFeatureSetup(null,
                //    "Visual Engine", MISFeature.ETC_FEATURE_VISUAL_ENGINE, ThirdPartyFeatureItem),
            };
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public void UnityFeatureItem(string featureName)
        {
            FeatureSetup featureSetup = unityFeatureList.Find(x => x.featureName == featureName);

            if (featureSetup == null)
                return;

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(featureName, skin.box, GUILayout.Height(30));

                if (TogglePackageFeatureButton(featureSetup))
                {
                    AssetDatabase.Refresh();
                }
            }
            GUILayout.EndHorizontal();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public void ThirdPartyFeatureItem(string featureName)
        {
            FeatureSetup featureSetup = thirdpartyFeatureList.Find(x => x.featureName == featureName);

            if (featureSetup == null)
                return;

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(featureName, skin.box, GUILayout.Height(30));

                if (TogglePackageFeatureButton(featureSetup))
                {
                    AssetDatabase.Refresh();
                }
            }
            GUILayout.EndHorizontal();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual bool TogglePackageFeatureButton(FeatureSetup featureSetup)
        {
            if (featureSetup == null)
                return false;

            if (featureSetup.isFeatureEnabled)
            {
                if (GUILayout.Button(new GUIContent(iconDotGreen, "Disable feature"), GUILayout.Height(30), GUILayout.Width(30)))
                {
                    if (!MISPackageFeature.HasImported(featureSetup.feature))
                    {
                        if (EditorUtility.DisplayDialog("Warning", $"Cannot find {featureSetup.featureName} in this project.", "Ok"))
                            return false;
                    }

                    if (EditorUtility.DisplayDialog("Confirm", $"Would you like to disable {featureSetup.featureName}?", "Ok", "Cancel"))
                    {
                        ToggleFeature(featureSetup, false);
                        return true;
                    }
                }
            }
            else
            {
                if (GUILayout.Button(new GUIContent(iconDotRed, "Enable feature"), GUILayout.Height(30), GUILayout.Width(30)))
                {
                    if (!MISPackageFeature.HasImported(featureSetup.feature))
                    {
                        if (EditorUtility.DisplayDialog("Warning", $"Cannot find {featureSetup.featureName} in this project.", "Ok"))
                            return false;
                    }

                    if (EditorUtility.DisplayDialog("Confirm", $"Would you like to enable {featureSetup.featureName}?", "Ok", "Cancel"))
                    {
                        ToggleFeature(featureSetup, true);
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
