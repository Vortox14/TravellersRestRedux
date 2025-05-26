using BepInEx;
using HarmonyLib;
using System;
using System.Reflection;

namespace RestfulTweaks
{
    public partial class Plugin : BaseUnityPlugin
    {
        public static void RegrowRegrowables()
        {
            DebugLog("RegrowRegrowables(): This is where my code would go, IF I HAD ANY!");
        }

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

        public static void GrowAllCropTrees()
        {
            var trees = UnityEngine.Object.FindObjectsOfType<Tree>();

            Plugin.DebugLog($"GrowAllCropTrees(): Found {trees.Length} trees.");

            foreach (Tree tree in trees)
            {
                try
                {
                    var cropSetterField = tree.GetType().GetField("cropSetter", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    if (cropSetterField == null)
                    {
                        Plugin.DebugLog($"GrowAllCropTrees(): Tree {tree.name} has no cropSetter field, skipping.");
                        continue;
                    }

                    CropSetter cropSetter = cropSetterField.GetValue(tree) as CropSetter;
                    if (cropSetter == null)
                    {
                        Plugin.DebugLog($"GrowAllCropTrees(): Tree {tree.name} cropSetter is null, skipping.");
                        continue;
                    }

                    bool isTreeCrop = cropSetter.IsTreeCrop();
                    if (!isTreeCrop)
                    {
                        Plugin.DebugLog($"GrowAllCropTrees(): Tree {tree.name} is not a tree crop, skipping.");
                        continue;
                    }

                    Crop crop = null;

                    foreach (var field in cropSetter.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                    {
                        if (field.FieldType == typeof(Crop))
                        {
                            crop = (Crop)field.GetValue(cropSetter);
                            Plugin.DebugLog($"GrowAllCropTrees(): Found Crop field {field.Name} dynamically.");
                            break;
                        }
                    }

                    if (crop == null)
                    {
                        foreach (var prop in cropSetter.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                        {
                            if (prop.PropertyType == typeof(Crop) && prop.CanRead)
                            {
                                crop = (Crop)prop.GetValue(cropSetter);
                                Plugin.DebugLog($"GrowAllCropTrees(): Found Crop property {prop.Name} dynamically.");
                                break;
                            }
                        }
                    }

                    if (crop == null)
                    {
                        Plugin.DebugLog($"GrowAllCropTrees(): Could not find Crop on {tree.name}, skipping.");
                        continue;
                    }

                    int currentAge = tree.currentAge;
                    int maxAge = crop.growablePrefabs.Length - 1;
                    bool grewTree = false;

                    if (currentAge < maxAge)
                    {
                        Plugin.DebugLog($"GrowAllCropTrees(): Growing tree {tree.name} from Age {currentAge} to MaxAge {maxAge}.");
                        tree.SetCurrentAge(maxAge);
                        cropSetter.UpdateCropVisual(tree.currentAge);
                        grewTree = true;
                    }
                    else
                    {
                        Plugin.DebugLog($"GrowAllCropTrees(): Tree {tree.name} already at max age.");
                    }

                    Harvestable harvestable = cropSetter.harvestable;
                    if (harvestable != null)
                    {
                        harvestable.isHarvestable = true;

                        if (grewTree)
                            Plugin.DebugLog($"GrowAllCropTrees(): Set {tree.name} as ready to harvest (after growing).");
                        else
                            Plugin.DebugLog($"GrowAllCropTrees(): Set {tree.name} as ready to harvest (already max age).");
                    }
                    else
                    {
                        Plugin.DebugLog($"GrowAllCropTrees(): {tree.name} has no harvestable component.");
                    }
                }
                catch (Exception ex)
                {
                    Plugin.DebugLog($"GrowAllCropTrees(): Exception while processing tree {tree.name}: {ex.Message}");
                }
            }
        }

        public static void GrowAllWoodFarmTrees()
        {
            var trees = UnityEngine.Object.FindObjectsOfType<Tree>();

            Plugin.DebugLog($"GrowAllWoodFarmTrees(): Found {trees.Length} trees.");

            foreach (Tree tree in trees)
            {
                try
                {
                    var cropSetterField = tree.GetType().GetField("cropSetter", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    if (cropSetterField == null)
                    {
                        Plugin.DebugLog($"GrowAllWoodFarmTrees(): Tree {tree.name} has no cropSetter field, skipping.");
                        continue;
                    }

                    CropSetter cropSetter = cropSetterField.GetValue(tree) as CropSetter;
                    if (cropSetter == null)
                    {
                        Plugin.DebugLog($"GrowAllWoodFarmTrees(): Tree {tree.name} cropSetter is null, skipping.");
                        continue;
                    }

                    bool isTreeCrop = cropSetter.IsTreeCrop();
                    if (isTreeCrop)
                    {
                        Plugin.DebugLog($"GrowAllWoodFarmTrees(): Tree {tree.name} is a crop tree, skipping.");
                        continue;
                    }

                    Crop crop = null;
                    foreach (var field in cropSetter.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                    {
                        if (field.FieldType == typeof(Crop))
                        {
                            crop = (Crop)field.GetValue(cropSetter);
                            Plugin.DebugLog($"GrowAllWoodFarmTrees(): Found Crop field {field.Name} dynamically.");
                            break;
                        }
                    }

                    if (crop == null)
                    {
                        foreach (var prop in cropSetter.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                        {
                            if (prop.PropertyType == typeof(Crop) && prop.CanRead)
                            {
                                crop = (Crop)prop.GetValue(cropSetter);
                                Plugin.DebugLog($"GrowAllWoodFarmTrees(): Found Crop property {prop.Name} dynamically.");
                                break;
                            }
                        }
                    }

                    if (crop == null)
                    {
                        Plugin.DebugLog($"GrowAllWoodFarmTrees(): Could not find Crop on {tree.name}, skipping.");
                        continue;
                    }

                    int currentAge = tree.currentAge;
                    int maxAge = crop.growablePrefabs.Length - 1;

                    if (currentAge < maxAge)
                    {
                        Plugin.DebugLog($"GrowAllWoodFarmTrees(): Growing wood tree {tree.name} from Age {currentAge} to MaxAge {maxAge}.");
                        tree.SetCurrentAge(maxAge);
                        cropSetter.UpdateCropVisual(tree.currentAge);
                    }
                    else
                    {
                        Plugin.DebugLog($"GrowAllWoodFarmTrees(): Wood tree {tree.name} already at max age.");
                    }
                }
                catch (Exception ex)
                {
                    Plugin.DebugLog($"GrowAllWoodFarmTrees(): Exception while processing tree {tree.name}: {ex.Message}");
                }
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
        // ---------------------- End growth troubleshooting --------------------------------------

        public static void WhatIsThatTree()
        {
            DebugLog("~~~~~ Trees ~~~~~");
            foreach (Tree tree in UnityEngine.Object.FindObjectsOfType<Tree>())
            {
                bool isCrop = Traverse.Create(tree).Field("isCropTree").GetValue<bool>();
                if (isCrop)
                {
                    CropSetter cs = Traverse.Create(tree).Field("cropSetter").GetValue<CropSetter>();
                    Crop crop = Traverse.Create(cs).Field("_crop").GetValue<Crop>();
                    DebugLog($"name:{tree.name} cropsetter:\"{cs.name}\" crop: \"{crop.name}\" age:\"{tree.currentAge}\"");
                }
            }
            DebugLog("~~~~~~~~~~~~~~~~~");
        }

        [HarmonyPatch(typeof(CropSetter), "Awake")]
        [HarmonyPostfix]
        private static void CropSetterAwakePostfix(CropSetter __instance)
        {
            if (__instance.cropCollider != null && !__instance.IsTreeCrop() && _walkThroughCrops.Value) __instance.cropCollider.enabled = false;            
        }

        [HarmonyPatch(typeof(CropSetter), "UpdateCropVisual")]
        [HarmonyPostfix]
        private static void CropSetterUpdateCropVisualPostfix(CropSetter __instance)
        {
            if (__instance.cropCollider != null && !__instance.IsTreeCrop() && _walkThroughCrops.Value) __instance.cropCollider.enabled = false;
        }

        [HarmonyPatch(typeof(CropDatabaseAccessor), "Awake")]
        [HarmonyPostfix]
        private static void CropDatabaseAccessorAwakePostFix(CropDatabaseAccessor __instance)
        {
            if (setupDoneCrops) return;
            DebugLog("CropDatabaseAccessor.Awake.PostFix");
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
            return true;
        }
    }
}
