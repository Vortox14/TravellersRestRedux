using BepInEx;
using HarmonyLib;

namespace RestfulTweaks
{
    public partial class Plugin : BaseUnityPlugin
    {
        public static void AllBirdsTalk()
        {
            foreach (BirdNPC birdNPC in UnityEngine.Object.FindObjectsOfType<BirdNPC>())
            {
                birdNPC.birdSpeech.ChatBark("BirdPositiveComments");
                birdNPC.birdSpeech.lastCommentWasPositive = true;
            }
        }

        [HarmonyPatch(typeof(BirdSpeech), "ChangeReputation")]
        [HarmonyPrefix]
        private static void BirdSpeechChangeReputation(BirdSpeech __instance)
        {
            DebugLog("BirdNPC.ChangeReputation.Prefix");
            if (_badBirdIsFunny.Value)
            {
                __instance.lastCommentWasPositive = true;
            }
        }
    }
}
