using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UIElements.UIRAtlasAllocator;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Xml.Linq;
using System.Text;
using UnityEngine.Playables;
using static CropsDatabase;

namespace RestfulTweaks
{
    public partial class Plugin : BaseUnityPlugin
    {

        ////////////////////////////////////////////////////////////////////////////////////////
        // Make All Birds Speak
        // Can call manually from Unity Explorer Console with RestfulTweaks.Plugin.AllBirdsTalk();

        public static void AllBirdsTalk()
        {
            foreach (BirdNPC b in UnityEngine.Object.FindObjectsOfType<BirdNPC>())
            {
                //Traverse.Create(b.birdSpeech).Method("APACANKHCJF").GetValue();                
                // BirdInstance birdInstance = this.placeable.itemSetup.NBNDGFJBMMO as BirdInstance; <-- that's where the cookie count is, and some other training related stuff
                b.birdSpeech.ChatBark("BirdPositiveComments");
                b.birdSpeech.lastCommentWasPositive = true;
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
