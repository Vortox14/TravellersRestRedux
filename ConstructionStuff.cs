#undef CONSTRUCTIONFEATURES
using BepInEx;

namespace RestfulTweaks
{
    public partial class Plugin : BaseUnityPlugin
    {
#if CONSTRUCTIONFEATURES
        [HarmonyPatch(typeof(ConstructionPlayerInfo), "RemoveMaterialsUsed")]
        [HarmonyPrefix]
        private static bool ConstructionPlayerInfoRemoveMaterialsUsedPrefix()
        {
            DebugLog("ConstructionPlayerInfo.RemoveMaterialsUsed.Prefix");
            return (_buildNoMatsUsed.Value) ? false : true;
        }

        [HarmonyPatch(typeof(ConstructionPlayerInfo), "RemoveMaterialsFromPlayer")]
        [HarmonyPrefix]
        private static bool ConstructionPlayerInfoRemoveMaterialsFromPlayerPrefix()
        {
            DebugLog("ConstructionPlayerInfo.RemoveMaterialsFromPlayer.Prefix");
            return (_buildNoMatsUsed.Value) ? false : true;
        }

        /*
        [HarmonyPatch(typeof(ConstructionPlayerInfo), "GetMaterialsFromContainer")]
        [HarmonyPrefix]
        private static bool ConstructionPlayerInfoGetMaterialsFromContainerPrefix()
        {
            DebugLog("ConstructionPlayerInfo.GetMaterialsFromContainer.Prefix");
            return (_buildNoMatsUsed.Value) ? false : true;
        }
        */

        [HarmonyPatch(typeof(ConstructionPlayerInfo), "CanPay")]  //Only used by FarmConstructionManager
        [HarmonyPrefix]
        private static bool ConstructionPlayerInfoCanPayPrefix(ref bool __result)
        {
            DebugLog("ConstructionPlayerInfo.CanPay.Prefix");
            if (_buildNoMatsUsedFarm.Value)
            {
                __result = true;
                return false;
            }
            else
            {
                return true;
            }
        }
#endif
    }

}
