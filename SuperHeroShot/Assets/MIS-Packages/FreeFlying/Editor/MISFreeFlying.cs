using UnityEditor;
using UnityEngine;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    [InitializeOnLoad]
    public class MISFreeFlying
    {
        // ----------------------------------------------------------------------------------------------------
        // Package
        public static string PACKAGE_VERSION = "1.5.7";
        public static int PACKAGE_VERSION_CODE = 24;


        // ----------------------------------------------------------------------------------------------------
        // MIS
        public static string MIS_MIN_VERSION = "2.7.14";
        public static int MIN_MIS_VERSION_CODE = 64;


        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        static MISFreeFlying()
        {
            if (!SessionState.GetBool(MISFeature.MIS_PACKAGE_FREEFLYING, false))
            {
                if (!HasValidVersion())
                    Debug.LogError("Currently installed MIS version is not compatible with " + MISFeature.MIS_PACKAGE_FREEFLYING + ". Please upgrade MIS to make it work properly.");

                SessionState.SetBool(MISFeature.MIS_PACKAGE_FREEFLYING, true);
            }

            if (MISMainSetup.HasMISRefactoringDone && !ScriptingDefineSymbolManager.IsSymbolAlreadyDefined(MISFeature.MIS_FEATURE_FREEFLYING))
            {
                ScriptingDefineSymbolManager.AddDefineSymbol(MISFeature.MIS_FEATURE_FREEFLYING);
                MISMainSetup.SetAddonVersion(MISFeature.MIS_FREEFLYING_OPTION_PATH, PACKAGE_VERSION);
            }
            else if (!MISMainSetup.HasMISRefactoringDone && ScriptingDefineSymbolManager.IsSymbolAlreadyDefined(MISFeature.MIS_FEATURE_FREEFLYING))
            {
                ScriptingDefineSymbolManager.RemoveDefineSymbol(MISFeature.MIS_FEATURE_FREEFLYING);
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public static bool HasValidVersion()
        {
            return MIN_MIS_VERSION_CODE <= MIS.MIS_VERSION_CODE;
        }
    }
}
