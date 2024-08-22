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

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // Bird Stuff

        [HarmonyPatch(typeof(BirdNPC), "MouseUp")]
        [HarmonyPrefix]
        private static void BirdNPCMouseUpPrefix(BirdNPC __instance)
        {
            DebugLog("BirdNPC.MouseUP.Prefix");
            if (_easyBirdTraining.Value)
            {
                __instance.canGiveCookieTime = 1f;
                //__instance.cookieDecrement = 0.02f;
                //__instance.cookieIncrement = 0.5f;
                __instance.birdSpeech.lastCommentWasPositive = true;

                ItemSetup isetupBird = __instance.placeable.itemSetup; //We need to find the Randomly named private field of type ItemInstance in this variable, and cast it as a BirdInstance
                BirdInstance theBird = null;
                DebugLog(String.Format("BirdNPC.MouseUP.Prefix name: {0}", isetupBird.name));


                FieldInfo[] isBirdFieldInfo = isetupBird.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance); //all private fields.
                //Could simplifiy this with .where, if I understood the syntax better. e.g.:
                //var fields = this.GetType().GetFields().Where(field => Attribute.IsDefined(field, typeof(MyAttribute))).ToList();

                foreach (FieldInfo fi in isBirdFieldInfo)
                {
                    if (fi.FieldType == typeof(ItemInstance))
                    {
                        theBird = (BirdInstance)fi.GetValue(isetupBird);
                        break;

                    }
                }

                if (theBird != null)
                {
                    float reflectedCommentsQuality = Traverse.Create(theBird).Field("_commentsQuality").GetValue<float>(); //private field
                    DebugLog(String.Format("BirdNPC.MouseUP.Prefix: Name: {0} Qual: {1} CookiesPerDay: {2} LastCookieGiven:{3}", theBird.birdName, reflectedCommentsQuality, theBird.cookiesGivenPerDay, theBird.lastCookieGivenTime));
                    theBird.cookiesGivenPerDay = 0;
                    //theBird.lastCookieGivenTime = -100f;
                }
                else
                {
                    DebugLog("BirdNPC.MouseUP.Prefix: Sorry, cound not find BirdNPC.placeable.itemSetup.<BirdInstance field>");
                }
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
