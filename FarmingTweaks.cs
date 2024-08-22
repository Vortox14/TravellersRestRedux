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
        // Make all crop trees/reharvestable crops insantly ready to harvest again.
        // Can call manually from Unity Explorer Console with RestfulTweaks.Plugin.RegrowRegrowables();
        public static void RegrowRegrowables()
        {
            DebugLog("RegrowRegrowables(): This is where my code would go, IF I HAD ANY!");
        }

        ////////////////////////////////////////////////////////////////////////////////////////
        // Grow all crops
        // Can call manually from Unity Explorer Console with RestfulTweaks.Plugin.GrowAllCrops();

        public static void GrowAllCrops()
        {
            foreach (FertileSoil f in UnityEngine.Object.FindObjectsOfType<FertileSoil>())
            {
                if (f.plantedCropSetter != null && f.plantedCropSetter.growable != null)
                {
                    f.plantedCropSetter.growable.GrowPlant();
                }
            }
        }



        ////////////////////////////////////////////////////////////////////////////////////////
        // Grow all trees
        // Can call manually from Unity Explorer Console with RestfulTweaks.Plugin.GrowAllTrees();

        public static void GrowAllTrees()
        {
            foreach (Tree t in UnityEngine.Object.FindObjectsOfType<Tree>())
            {
                bool isCrop = Traverse.Create(t).Field("isCropTree").GetValue<bool>();
                if (!isCrop) continue;
                CropSetter cSet = Traverse.Create(t).Field("cropSetter").GetValue<CropSetter>();
                Crop crop = Traverse.Create(cSet).Field("_crop").GetValue<Crop>();

                // Lets check if the Cropsetter and crop are different, using the number at the start of the cropsetter object name and the crop object name
                string cropSetNumStr = cSet.name.Substring(0, cSet.name.IndexOf(" "));
                string cropNumStr = crop.name.Substring(0, crop.name.IndexOf(" "));
                if (cropSetNumStr != cropNumStr)
                {
                    DebugLog($"GrowAllTrees(): ERROR: Cropsetter/Crop mismatch! (\"{cSet.name}\",\"{crop.name}\") (\"{cropSetNumStr}\", \"{cropNumStr}\")");
                    if (_GrowTreesTypeFix.Value)
                    {
                        int cropInt = crop.id;
                        int cropSetInt;
                        bool parsed = Int32.TryParse(cropSetNumStr, out cropSetInt);
                        if (!parsed)
                        {
                            DebugLog($"GrowAllTrees(): ERROR count not get int from string \"{cropSetNumStr}\" that was taken from \"{cSet.name}\": skipping Crop Type Fix");
                        }
                        else
                        {
                            DebugLog($"GrowAllTrees(): changing Tree.cropsetter._crop.growablePrefabs from {cropInt} to {cropSetInt} before growing");

                            //copy growablePrefabs[] from the correct crop over Tree.cropSetter._crop.growablePrefabs[]
                            Crop correctCrop = CropDatabaseAccessor.GetCrop(cropSetInt);
                            crop.growablePrefabs = correctCrop.growablePrefabs;
                        }
                    }
                }

                string preName = t.name;
                int preAge = t.currentAge;
                t.SetCurrentAge(t.currentAge + 1);
                cSet.UpdateCropVisual(t.currentAge);
                int postAge = t.currentAge;


                DebugLog($"GrowAllTrees(): Age: {preAge} -> {postAge} Name: \"{preName}\"");
            }
        }

        // ---------------------- Tree growth troubleshooting --------------------------------------
        /*
        public static string Prefabs2String(Tree t, CropSetter c)
        {
            string s = "";
            s += "Tree.cropsetter prefabs: ";
            if (c is null || c.PNFPGNOALCI.growablePrefabs is null)
            {
                s += "NULL";
            }
            else if (c.PNFPGNOALCI.growablePrefabs.Length == 0)
            {
                s += "NONE";
            }
            else
            {
                for (int i = 0; i < c.PNFPGNOALCI.growablePrefabs.Length; i++)
                {
                    if (i > 0) s += "|";
                    s += $"\"{c.PNFPGNOALCI.growablePrefabs[i].name}\"";

                }
            }

            s += " Tree.placable prefabs: ";
            if (t is null || t.placeable.itemSetup.item is null )
            {
                s += "NULL";
            }
            else if (t.placeable.itemSetup.item.growablePrefabs.Length == 0)
            {
                s += "NONE";
            }
            else
            {
                for (int i = 0; i < t.placeable.itemSetup.item.growablePrefabs.Length; i++)
                {
                    if (i > 0) s += "|";
                    s += $"\"{t.placeable.itemSetup.item.growablePrefabs[i].name}\"";
                }
            }
            return s;
        }
        [HarmonyPatch(typeof(Tree), "OnDestroy")]
        [HarmonyPrefix]
        public static void TreeOnDestroyPrefix(Tree __instance, CropSetter ___cropSetter, bool ___isCropTree)
        {
            if (___isCropTree) DebugLog($"Tree.OnDestroy.Prefix(): Age: {__instance.currentAge}, Name: \"{__instance.name}\" Crop: {___cropSetter.PNFPGNOALCI.id}:\"{___cropSetter.PNFPGNOALCI.name}");
            //DebugLog(Prefabs2String(__instance, ___cropSetter));
        }
        [HarmonyPatch(typeof(Tree), "Awake")]
        [HarmonyPostfix]
        public static void TreeAwakePostfix(Tree __instance, CropSetter ___cropSetter, bool ___isCropTree)
        {
            if (___isCropTree)   DebugLog($"Tree.Awake.Postfix(): Age: {__instance.currentAge}, Name: \"{__instance.name}\" Crop: {___cropSetter.PNFPGNOALCI.id}:\"{___cropSetter.PNFPGNOALCI.name}");
            //DebugLog(Prefabs2String(__instance, ___cropSetter));
        }

     
        [HarmonyPatch(typeof(Tree), "SetCurrentAge")]
        [HarmonyPrefix]
        public static void TreeSetCurrentAgePrefix(Tree __instance, CropSetter ___cropSetter)
        {
            DebugLog($"Tree.SetCurrentAge.Prefix(): Age: {__instance.currentAge}, Name: \"{__instance.name}\" Crop: {___cropSetter.PNFPGNOALCI.id}:\"{___cropSetter.PNFPGNOALCI.name}");
            DebugLog(Prefabs2String(__instance, ___cropSetter));

            //return this.cropSetter.PNFPGNOALCI.growablePrefabs;, return this.placeable.itemSetup.item.growablePrefabs;
        }


        [HarmonyPatch(typeof(Tree), "SetCurrentAge")]
        [HarmonyPostfix]
        public static void TreeSetCurrentAgePostfix(Tree __instance, CropSetter ___cropSetter)
        {
            DebugLog($"Tree.SetCurrentAge.Postfix(): Age: {__instance.currentAge}, Name: \"{__instance.name}\" Crop: {___cropSetter.PNFPGNOALCI.id}:\"{___cropSetter.PNFPGNOALCI.name}");
            DebugLog(Prefabs2String(__instance, ___cropSetter));
        }
        */
        // ---------------------- End  growth troubleshooting --------------------------------------


        ////////////////////////////////////////////////////////////////////////////////////////
        // ListTreeTypes
        // Can call manually from Unity Explorer Console with RestfulTweaks.Plugin.GrowAllTrees();

        public static void WhatIsThatTree()
        {
            DebugLog("~~~~~ Trees ~~~~~");

            foreach (Tree t in UnityEngine.Object.FindObjectsOfType<Tree>())
            {

                bool isCrop = Traverse.Create(t).Field("isCropTree").GetValue<bool>();
                if (isCrop)
                {
                    CropSetter cs = Traverse.Create(t).Field("cropSetter").GetValue<CropSetter>();
                    Crop c = Traverse.Create(cs).Field("_crop").GetValue<Crop>();
                    DebugLog($"name:{t.name} cropsetter:\"{cs.name}\" crop: \"{c.name}\" age:\"{t.currentAge}\"");
                }
            }
            DebugLog("~~~~~~~~~~~~~~~~~");
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // CropSetter Stuff

        [HarmonyPatch(typeof(CropSetter), "Awake")]
        [HarmonyPostfix]
        private static void CropSetterAwakePostfix(CropSetter __instance)
        {
            //DebugLog("CropSetter.awake.Postfix");
            if (__instance.cropCollider != null && !__instance.IsTreeCrop() && _walkThroughCrops.Value) __instance.cropCollider.enabled = false;
            //DebugLog(String.Format("collisionDetection: {0} cutDetection: {1} cropCollider: {2}", (__instance.collisionDetection == null) ? "null" : "active", (__instance.cutDetection == null) ? "null" : "active", __instance.cropCollider.enabled));
        }
        [HarmonyPatch(typeof(CropSetter), "UpdateCropVisual")]
        [HarmonyPostfix]
        private static void CropSetterUpdateCropVisualPostfix(CropSetter __instance)
        {
            //DebugLog("UpdateCropVisual.awake.Postfix");
            if (__instance.cropCollider != null && !__instance.IsTreeCrop() && _walkThroughCrops.Value) __instance.cropCollider.enabled = false;
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // Crops Stuff
        // The Recipe database is not accessible during Plugin.Awake(), so we attach to the Accessor Awake() function

        [HarmonyPatch(typeof(CropDatabaseAccessor), "Awake")]
        [HarmonyPostfix]
        private static void CropDatabaseAccessorAwakePostFix(CropDatabaseAccessor __instance)
        {
            if (setupDoneCrops) return;
            DebugLog("CropDatabaseAccessor.Awake.PostFix");
            //Crop[] allCrops = CropDatabaseAccessor.GetInstance().allCrops;
            //CropsDatabase reflectedCropDatabaseSO = Traverse.Create(__instance).Field("CropDatabaseSO").GetValue<CropsDatabase>().Crops;
            Crop[] allCrops = Traverse.Create(__instance).Field("CropDatabaseSO").GetValue<CropsDatabase>().Crops;

            DebugLog(String.Format("Found {0} crops", allCrops.Length));
            if (_dumpCropListOnStart.Value) Log.LogInfo(string.Format("id, nameId, name, daysToGrow, daysUntilNewHarvest, reusable"));

            for (int i = 0; i < allCrops.Length; i++)
            {
                if (_CropFastGrow.Value && allCrops[i].daysToGrow > 0) allCrops[i].daysToGrow = 1;
                if (_CropFastRegrow.Value && allCrops[i].reusable) allCrops[i].daysUntilNewHarvest = 1;
                if (_allSeasonCrops.Value) allCrops[i].avaliableSeasons = CropSeason.All;
                if (_dumpCropListOnStart.Value)
                {
                    Log.LogInfo(String.Format("Recipe: {0}, {1}, {2}, {3}, {4}, {5}", allCrops[i].id, allCrops[i].nameId, allCrops[i].name, allCrops[i].daysToGrow, allCrops[i].daysUntilNewHarvest, allCrops[i].reusable));
                }

            }
            setupDoneCrops = true;
        }




        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // Soil Stays Watered

        [HarmonyPatch(typeof(FertileSoil), "CheckWater")]
        [HarmonyPrefix]
        static bool FertileSoilCheckWaterPrefix(FertileSoil __instance)
        {
            if (_soilStaysWatered.Value)
            {
                __instance.daysUntilDry = 3;
            }
            if (_soilWet3DaysRain.Value)
            {
                if (Weather.IsWeatherActive(Weather.WeatherType.Rain) && __instance.daysUntilDry <= 3) __instance.daysUntilDry = 3;
            }
            return true; // flow thorugh to normal Update
        }














    }
}
