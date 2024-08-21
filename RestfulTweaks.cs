#undef CONSTRUCTIONFEATURES 

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
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private static Harmony _harmony;
        internal static ManualLogSource Log; // static copy of the BaseUnityPligin.Logger object so it can be accessed in static methods, initilized in constructor

        private static ConfigEntry<bool> _debugLogging;
        private static ConfigEntry<bool> _dumpItemListOnStart;
        private static ConfigEntry<int>  _dispensorStackSize;
        private static ConfigEntry<int> _itemStackSize;
        private static ConfigEntry<int> _agingBarrelStackSize;
        private static ConfigEntry<float> _moveSpeed;
        private static ConfigEntry<float> _moveRunMult;
        private static ConfigEntry<bool> _soilStaysWatered;
        private static ConfigEntry<bool> _soilWet3DaysRain;
        private static ConfigEntry<bool> _recipesNoFuel;
        private static ConfigEntry<int> _recipesQuickCook;
        private static ConfigEntry<bool> _recipesNoFragments;
        private static ConfigEntry<bool> _fireplaceNoFuelUse;
        private static ConfigEntry<bool> _dumpRecipeListOnStart;
        private static ConfigEntry<bool> _dumpCropListOnStart;
        private static ConfigEntry<bool> _dumpStaffGenDataOnStart;
        private static ConfigEntry<bool> _dumpReputationListOnStart;
        private static ConfigEntry<bool> _dumpIngredientGroupListOnStart;
        private static ConfigEntry<bool> _CropFastGrow;
        private static ConfigEntry<bool> _CropFastRegrow;
        private static ConfigEntry<bool> _staffNoNeg;
        private static ConfigEntry<bool> _staffRefreshOnOpen;
        private static ConfigEntry<bool> _staffAlways3Perks;
        private static ConfigEntry<int> _staffLevel;
        private static ConfigEntry<int> _moreTiles;
        private static ConfigEntry<int> _moreZones;
        private static ConfigEntry<int> _moreRooms;
        private static ConfigEntry<int> _moreCustomers;
        private static ConfigEntry<int> _moreDisponible;
        private static ConfigEntry<bool> _wilsonOneCoin;
        private static ConfigEntry<float> _moreValuableFish;
        private static ConfigEntry<float> _moreValuableMeat;
        private static ConfigEntry<float> _moreValuableVege;
        private static ConfigEntry<float> _moreValuableGrain;
        //private static ConfigEntry<float> _moreValuableAlcohol;
        private static ConfigEntry<float> _moreValuableFruit;
        //private static ConfigEntry<float> _moreValuableCheese;
        private static ConfigEntry<bool> _allSeasonCrops;
        private static ConfigEntry<bool> _easyBirdTraining;
        private static ConfigEntry<bool> _badBirdIsFunny;
        private static ConfigEntry<bool> _walkThroughCrops;
        //private static ConfigEntry<float> _xpMult;
        private static ConfigEntry<KeyCode> _hotkeyGrowCrops;
        private static ConfigEntry<KeyCode> _hotKeyBirdTalk;
        private static ConfigEntry<KeyCode> _hotkeyGrowTrees;
        private static ConfigEntry<KeyCode> _whatIsThatTree;

#if CONSTRUCTIONFEATURES
        private static ConfigEntry<bool> _buildNoMatsUsed;
        private static ConfigEntry<bool> _buildNoMatsUsedFarm;
#endif
        private static ConfigEntry<bool> _endlessWater;
        private static bool setupDoneItems = false;
        private static bool setupDoneRecipes = false;
        private static bool setupDoneCrops = false;
        private static bool setupDoneStaffManager = false;

        private static ConfigEntry<int> _cowLootExtra;
        private static ConfigEntry<int> _chickenLootExtra;
        private static ConfigEntry<int> _pigLootExtra;
        private static ConfigEntry<int> _sheepLootExtra;
        private static ConfigEntry<bool> _GrowTreesTypeFix;

        // ----------------------------------------------------
        // Some Accessor objects
        private static CommonReferences myCommonReferences;
        public static CommonReferences commonReferences
        {
            get
            {
                if (myCommonReferences == null) myCommonReferences = UnityEngine.Object.FindObjectOfType<CommonReferences>();
                return myCommonReferences;
            }
        }
        // -------------
        private static ItemDatabaseAccessor myItemDatabaseAccessor;
        public static ItemDatabaseAccessor itemDatabaseAccessor
        {
            get
            {
                if (myItemDatabaseAccessor == null) myItemDatabaseAccessor = UnityEngine.Object.FindObjectOfType<ItemDatabaseAccessor>();
                return myItemDatabaseAccessor;
            }
        }
        private static ItemDatabase myitemDatabaseSO;
        public static ItemDatabase itemDatabaseSO
        {
            get
            {
                if (myitemDatabaseSO == null) myitemDatabaseSO = Traverse.Create(itemDatabaseAccessor).Field("itemDatabaseSO").GetValue<ItemDatabase>();
                return myitemDatabaseSO;
            }
        }

        private static RecipeDatabaseAccessor myRecipeDatabaseAccessor;
        public static RecipeDatabaseAccessor recipeDatabaseAccessor
        {
            get
            {
                if (myRecipeDatabaseAccessor == null) myRecipeDatabaseAccessor = RecipeDatabaseAccessor.GetInstance();
                return myRecipeDatabaseAccessor;
            }
        }
        private static RecipeDatabase myRecipeDatabaseSO;
        public static RecipeDatabase recipeDatabaseSO
        {
            get
            {
                if (myRecipeDatabaseSO == null) myRecipeDatabaseSO = Traverse.Create(recipeDatabaseAccessor).Field("recipeDatabaseSO").GetValue<RecipeDatabase>();
                return myRecipeDatabaseSO;
            }
        }



        // ----------------------------------------------------


        //public static RecipeDatabaseAccessor recipeDatabaseAccessor; //Not needed since RecipeDatabaseAccessor is full of useful static functions
        public Plugin()
        {
            // bind to config settings
            _debugLogging = Config.Bind("Debug", "Debug Logging", false, "Logs additional information to console");

            _dispensorStackSize   = Config.Bind("Stacks", "Tap/Keg Stack Size", -1, "Change the amount of drinks you can store in taps/kegs; set to -1 to disable, set to 0 to use item stack size");
            _agingBarrelStackSize = Config.Bind("Stacks", "Aging Barrel Stack Size", -1, "Change the amount of drinks you can store in aging barrels; set to -1 to disable, set to 0 to use item stack size");
            _itemStackSize        = Config.Bind("Stacks", "Item Stack Size", -1, "Change the stack size of any item that normally stacks to 99; set to -1 to disable");

            _dumpItemListOnStart            = Config.Bind("Database", "List Items on start", false, "set to true to print a list of all items to console on startup");
            _dumpRecipeListOnStart          = Config.Bind("Database", "List Recipes on start", false, "set to true to print a list of all recipes to console on startup");
            _dumpReputationListOnStart      = Config.Bind("Database", "List Reputation milestones on start", false, "set to true to print a list of all reputation milestones to console on startup"); 
            _dumpStaffGenDataOnStart        = Config.Bind("Database", "List staff generation data on start", false, "set to true to print a list of staff generation data on startup");
            _dumpIngredientGroupListOnStart = Config.Bind("Database", "List Ingredient Group data on start", false, "set to true to print a list of ingredient Groups on startup");
            _dumpCropListOnStart            = Config.Bind("Database", "List Crops on start", false, "set to true to print a list of all crops to console on startup");

            _moveSpeed   = Config.Bind("Movement", "Walking Speed", 2.5f, "walking speed; set to 2.5 for default speed ");
            _moveRunMult = Config.Bind("Movement", "Run Speed Multiplier", 1.6f, "run speed multiplier; set to 1.6 for default speed ");

            _soilStaysWatered = Config.Bind("Farming", "Soil Stays Wet", false, "Soil stays watered");
            _soilWet3DaysRain = Config.Bind("Farming", "Rain Fully waters soil", false, "rain will make soil wet for the next 3 days, like watering");
            _CropFastGrow     = Config.Bind("Farming", "Fast Growing Crops", false, "All crops advance one growth stage per day");
            _CropFastRegrow   = Config.Bind("Farming", "Fast Regrowing Crops", false, "Crops that allow multiple harvests can be harvested every day");
            _walkThroughCrops = Config.Bind("Farming", "Walk Through Crops", false, "Lets you walk through your crops.");
            _hotkeyGrowCrops  = Config.Bind("Farming", "grow all crops hotkey", KeyCode.None, "Press to instantly grow planted crops");
            _GrowTreesTypeFix = Config.Bind("Farming", "grow trees type fix", true, "workarpund for tree growth key changeing type");
            _hotkeyGrowTrees = Config.Bind("Farming", "grow all trees hotkey", KeyCode.None, "Press to instantly grow all trees");
            _allSeasonCrops   = Config.Bind("Farming", "All-season crops", false, "All crops can be grown in any season.");
            _whatIsThatTree   = Config.Bind("Farming", "What is that tree", KeyCode.None, "For Troubleshooting - lists details of crop trees");


            _cowLootExtra     = Config.Bind("Animals", "Cow Bonus Loot", 0, "Increase Cow loot by this amount; set to 0 to disable");
            _pigLootExtra     = Config.Bind("Animals", "Pig Bonus Loot", 0, "Increase Pig loot by this amount; set to 0 to disable");
            _chickenLootExtra = Config.Bind("Animals", "Chicken Bonus Loot", 0, "Increase Chicken loot by this amount; set to 0 to disable");
            _sheepLootExtra   = Config.Bind("Animals", "Sheep Bonus Loot", 0, "Increase Sheep loot by this amount; set to 0 to disable");

            _recipesNoFuel      = Config.Bind("Recipes", "No Fuel", false, "Recipes no longer require fuel");
            _recipesNoFragments = Config.Bind("Recipes", "No Fragment Cost", false, "Cave Recipies only cost one fragment");
            _recipesQuickCook   = Config.Bind("Recipes", "Quick Crafting", -1, "Sets the maximum time recipes take to craft in minutes; set to -1 to disable");

            _staffNoNeg         = Config.Bind("Staff", "No Negative Perks", false, "New Staff will not have any negative perks");
            _staffRefreshOnOpen = Config.Bind("Staff", "Refresh Applicants on Open", false, "Refresh the list of new staff available to hire every time the hiring interface is opened");
            _staffAlways3Perks  = Config.Bind("Staff", "Always Three Perks", false, "NOT WORKING New hires will always have three positive perks");
            _staffLevel         = Config.Bind("Staff", "Starting Level", -1, "Starting level for new hires; set to -1 to disable, set to 31 for all three skills at level 5");

            _moreTiles      = Config.Bind("Milestones", "More Zone Tiles", -1, "increase number of tiles for crafting/dining zone; set to -1 to disable");
            _moreZones      = Config.Bind("Milestones", "More Crafting Zones", -1, "NOT WELL TESTED increase number of zones for crafting; set to -1 to disable");
            _moreRooms      = Config.Bind("Milestones", "More Rentable Rooms", -1, "increase number of rooms for rent; set to -1 to disable");
            _moreCustomers  = Config.Bind("Milestones", "More Customers", -1, "increase customer capacity; set to -1 to disable");
            _moreDisponible = Config.Bind("Milestones", "More Floor Tiles", -1, "increase total number of floor tiles allowed; set to -1 to disable");



            _easyBirdTraining   = Config.Bind("Misc", "Easy Bird Training", false, "NOT WORKING More benefit from crackers, giving cracker at wrong time results in less benefit instead of loss");
            _badBirdIsFunny     = Config.Bind("Misc", "Naughty Bird is Funny", false, "Patrons like a naughty bird, so everything your bird says causes reputation gain instead of loss");
            _fireplaceNoFuelUse = Config.Bind("Misc", "Fireplace does not consume fuel", false, "fireplace no longer consumes fuel");
            //_xpMult             = Config.Bind("Misc", "XP Multiplier", 1.0f, "NOT WORKING increase the amopunt of reputation earned; set to 1.0 to disable");
            _endlessWater       = Config.Bind("Misc", "Endless Water", false, "DO NOT USE DURING TUTORIAL. PREVENTS FILLING BUCKETS AT WELL. Buckets of water do not empty when used");
            _hotKeyBirdTalk     = Config.Bind("Misc", "All Birds Talk", KeyCode.None, "Make your birds say something nice");

            _wilsonOneCoin       = Config.Bind("Prices", "Wilson Price Reduction", false, "Wilson only charges 1 coin per item");
            _moreValuableFish    = Config.Bind("Prices", "Fish price increase", 1.0f, "increase the value of fish/shellfish; set to 1.0 to disable");
            _moreValuableMeat    = Config.Bind("Prices", "Meat price increase", 1.0f, "increase the value of meat; set to 1.0 to disable");
            _moreValuableVege    = Config.Bind("Prices", "Vege price increase", 1.0f, "increase the value of Vegetables/Legumes; set to 1.0 to disable");
            _moreValuableFruit   = Config.Bind("Prices", "Fruit price increase", 1.0f, "increase the value of Fruit/Berries; set to 1.0 to disable");
            _moreValuableGrain   = Config.Bind("Prices", "Grain price increase", 1.0f, "increase the value of Grains; set to 1.0 to disable");
            //Setting sellPrice doesn't work for objects where the price gets determined (at least partly) by the stuff it is made of.
            //_moreValuableAlcohol = Config.Bind("Prices", "Alcohol price increase", 1.0f, "increase the value of Beer/Cocktails/Spirits/Liquer/Wine; set to 1.0 to disable");
            //_moreValuableCheese  = Config.Bind("Prices", "Cheese price increase", 1.0f, "increase the value of Cheese; set to 1.0 to disable");
#if CONSTRUCTIONFEATURES
            _buildNoMatsUsed = Config.Bind("Building", "No Materials used", false, "Building materials not consumed by construction (you still need enough to do the construction)");
            _buildNoMatsUsedFarm = Config.Bind("Building", "Farm Construction No Materials used", false, "TEST WITH BARN/COOP CONSTRUCTION");
#endif
        }

        private void Awake()
        {
            // Plugin startup logic
            Log = Logger;
            _harmony = Harmony.CreateAndPatchAll(typeof(Plugin));
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} {PluginInfo.PLUGIN_VERSION} is loaded!");
        }
        private void Update()
        {
            if (Input.GetKeyDown(_hotKeyBirdTalk.Value))
            {
                AllBirdsTalk();
            }
            else if (Input.GetKeyDown(_hotkeyGrowCrops.Value))
            {
                GrowAllCrops();
            }
            else if (Input.GetKeyDown(_hotkeyGrowTrees.Value))
            {
                GrowAllTrees();
            }
            else if (Input.GetKeyDown(_whatIsThatTree.Value))
            {
                WhatIsThatTree();
            }
        }
        private void OnDestroy()
        {
            _harmony.UnpatchSelf();
        }
 
        public static void DebugLog(string message)
        {
            // Log a message to console only if debug is enabled in console
            if (_debugLogging.Value)
            {
                Log.LogInfo(string.Format("DEBUG: {0}", message));
            }
        }

        // //////////////////////////////////////////////////////////////////////
        // A bunch of functions for converting things to text to spit out 
        public static int Price2Copper(Price x)
        {
            return x.gold * 100000 + x.silver * 100 + x.copper;
        }

        public static Price Copper2Price(int x)
        {
            Price p = new Price();
            p.gold = Mathf.FloorToInt((float)(x / 10000));
            x -= p.gold * 10000;
            p.silver = Mathf.FloorToInt((float)(x / 100));
            x -= p.silver * 100;
            p.copper = x;
            return p;
        }

        public static Price PriceXFloat(Price p, float x)
        {
            return Copper2Price(Mathf.FloorToInt(x * Price2Copper(p)));
        }

        public static string Tags2String(Tag[] x)
        {
            return string.Join(":", x);
        }

        public static string Item2String(Item x)
        {
            if (x == null) return "nullItem";
            int xId = Traverse.Create(x).Field("id").GetValue<int>();
            string xName = (x.translationByID) ? LocalisationSystem.Get("Items/item_name_" + xId.ToString()) : x.nameId;
            return String.Format("{0}:{1}",xId, xName);
        }
        public static string ItemMod2String(ItemMod x)
        {
            if (x.item == null) return "-";
            string a = Item2String(x.item); //The base item
            String b = (x.mod == null) ? "" : Item2String(x.mod); //the modifier item, or empty string if no modifier
            return String.Format("[{0}({1})]", a,b ); 
        }
        public static string ItemModList2String(List<ItemMod> x)
        {
            string s = "";
            foreach (ItemMod itemMod in x)
            {
                s += ItemMod2String(itemMod);
            }
            return s;
            
        }

        public static string RecipeIngredients2String(RecipeIngredient[] x)
        {
            string result = string.Empty;
            for (int i = 0; i < x.Length; i++)
            {
                int id= Traverse.Create(x[i].item).Field("id").GetValue<int>();
                int modId = Traverse.Create(x[i].mod).Field("id").GetValue<int>();
                result += String.Format("[{0}:{1}:{2}]", id, x[i].amount, modId);
            }
            return result;
        }
        public static string IngredientTypes2String(IngredientType[] x)
        {
            string result = string.Empty;
            for (int i = 0; i < x.Length; i++)
            {
                result += String.Format("[{0}]", x[i]);
            }
            return result;
        }

        ////////////////////////////////////////////////////////////////////////////////////////
        // Make All Birds Speak
        // Cam call manually from Unity Explorer Console with RestfulTweaks.Plugin.AllBirdsTalk();

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

                string cropSetNumStr = cSet.name.Substring(0, cSet.name.IndexOf(" "));
                string cropNumStr =    crop.name.Substring(0, crop.name.IndexOf(" "));



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

                            // not suprising, this does not work.
                            //crop = CropDatabaseAccessor.GetCrop(cropSetInt);
                            //Try direct on the growable prefabs?
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



        ////////////////////////////////////////////////////////////////////////////////////////
        // Dump comma seperated list of recipes to console
        // Cam call manually from Unity Explorer Console with RestfulTweaks.Plugin.DumpRecipeList();

        public static void DumpRecipeList()
        {
            //RecipeDatabaseAccessor db = RecipeDatabaseAccessor.GetInstance();
            Recipe[] r = RecipeDatabaseAccessor.GetAllRecipes();
            Log.LogInfo("id, name, outputItemId, outputAmount, crafTime, fuel, recipeFragments, cannotRepeat, ingredientsNeeded, modiferNeeded, modiferTypes, recipeGroup");
            for (int i = 0; i < r.Length; i++)
            {
                int craftTime = r[i].time.weeks * 7 * 24 * 60 + r[i].time.days * 24 * 60 + r[i].time.hours * 60 + r[i].time.mins;
                int outputId = Traverse.Create(r[i].output.item).Field("id").GetValue<int>();
                Log.LogInfo(String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
                    r[i].id, r[i].name, outputId, r[i].output.amount, craftTime, r[i].fuel, r[i].recipeFragments, r[i].cannotRepeatIngredients,
                    RecipeIngredients2String(r[i].ingredientsNeeded), IngredientTypes2String(r[i].modiferNeeded), IngredientTypes2String(r[i].modiferTypes), r[i].recipeGroup));  
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////
        // Dump comma seperated list of items to console
        // Can call manually from Unity Explorer Console with RestfulTweaks.Plugin.DumpItemList();

        public static void DumpItemList()
        {


            Item x;
            string itemName;
            string itemDesc;
            string itemShop;
            string itemCategory;
            string itemSubType;

            Log.LogInfo(string.Format("id, name, desc, price, sellPrice, amountStack, shop, category, tags, wilsonCoins, wilsonCoinsPrice, getType(), subType"));
            for (int i = 0; i < itemDatabaseSO.items.Length; i++)
            {
                x = itemDatabaseSO.items[i];
                int reflectedItemId = Traverse.Create(x).Field("id").GetValue<int>();                 //Protected
                string reflectedItemIDesc = Traverse.Create(x).Field("description").GetValue<string>();  //Protected

                itemName = (x.translationByID) ? LocalisationSystem.Get("Items/item_name_" + reflectedItemId.ToString()) : x.nameId;
                itemName = "\"" + itemName + "\"";
                itemDesc = (x.translationByID) ? LocalisationSystem.Get("Items/item_description_" + reflectedItemId.ToString()) : reflectedItemIDesc;
                itemDesc = "\"" + itemDesc + "\"";
                itemShop = "\"" + x.shop + "\"";
                itemCategory = "\"" + x.category + "\"";

                // For food and fish, look at the subtype.
                if (x.GetType() == typeof(Food))      itemSubType = (x as Food).ingredientType.ToString();
                else if (x.GetType() == typeof(Fish)) itemSubType = (x as Fish).fishType.ToString();
                else                                  itemSubType = "";


                Log.LogInfo(String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}",
                    reflectedItemId, itemName, itemDesc, Price2Copper(x.price), Price2Copper(x.sellPrice), x.amountStack, itemShop, itemCategory, Tags2String(x.tags),
                     x.wilsonCoins, x.wilsonCoinsPrice, x.GetType(), itemSubType));


            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////
        // Dump comma seperated list of IngredientGroups to console
        // Can call manually from Unity Explorer Console with RestfulTweaks.Plugin.DumpIngredientGroupList();

        public static void DumpIngredientGroupList()
        {


            Item y;
            IngredientGroup x;
            string itemName;
            string itemDesc;


            Log.LogInfo(string.Format("id, name, desc, ingredientsTypes, PossibleItems, itemModAux, cheapestIngredient"));
            for (int i = 0; i < itemDatabaseSO.items.Length; i++)
            {
                
                y = itemDatabaseSO.items[i];                           // there has to be a better way to do this but this works ¯\_(ツ)_/¯
                if (y.GetType() != typeof(IngredientGroup)) continue;  // actually this doesn't work :-(
                x = itemDatabaseSO.items[i] as IngredientGroup;
                int reflectedItemId = Traverse.Create(x).Field("id").GetValue<int>();                 //Protected
                string reflectedItemIDesc = Traverse.Create(x).Field("description").GetValue<string>();  //Protected

                itemName = (x.translationByID) ? LocalisationSystem.Get("Items/item_name_" + reflectedItemId.ToString()) : x.nameId;
                itemName = "\"" + itemName + "\"";
                itemDesc = (x.translationByID) ? LocalisationSystem.Get("Items/item_description_" + reflectedItemId.ToString()) : reflectedItemIDesc;
                itemDesc = "\"" + itemDesc + "\"";
                //get private fields
                List<ItemMod> xPossibleItems = Traverse.Create(x).Field("possibleItems").GetValue<List<ItemMod>>();
                ItemMod xCheapestIngredient = Traverse.Create(x).Field("cheapestIngredient").GetValue<ItemMod>();
                ItemMod xItemModAux = Traverse.Create(x).Field("itemModAux").GetValue<ItemMod>();

                Log.LogInfo(String.Format("{0},{1},{2},{3},{4},{5},{6}",
                    reflectedItemId, itemName, itemDesc, 
                    string.Join(":", x.ingredientsTypes),
                    ItemModList2String(xPossibleItems), ItemMod2String(xItemModAux), ItemMod2String(xCheapestIngredient)

                    ));


            }
        }
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // Animal  Loot increase
        // Drop specific number extra of whatever the first item in the Animal's loot table is

        [HarmonyPatch(typeof(AnimalNPC), "Hit")]
        [HarmonyPostfix]
        private static void AnimalNPCHitPostFix(AnimalNPC __instance)
        {
            DebugLog($"AnimalNPC.Hit.PostFix: {__instance.GetType().ToString()} with {__instance.lives} health remaining");
            if (__instance.lives > 0) return; // Not dead
            int extraItems = 0;
            if (_cowLootExtra.Value     > 0 && __instance.GetType() == typeof(CowNPC))     extraItems = _cowLootExtra.Value;
            if (_pigLootExtra.Value     > 0 && __instance.GetType() == typeof(PigNPC))     extraItems = _pigLootExtra.Value;
            if (_sheepLootExtra.Value   > 0 && __instance.GetType() == typeof(SheepNPC))   extraItems = _sheepLootExtra.Value;
            if (_chickenLootExtra.Value > 0 && __instance.GetType() == typeof(ChickenNPC)) extraItems = _chickenLootExtra.Value;            
            if (extraItems == 0) return;
            DebugLog($"AnimalNPC.Hit.PostFix: Spawning {extraItems} extra items");
            Animal animalItem = __instance.placeable.itemSetup.item as Animal;
            ItemProduction[] reflectedSacrificeItems = Traverse.Create(animalItem).Field("sacrificeItems").GetValue<ItemProduction[]>();
            Food food = reflectedSacrificeItems[0].item as Food;
            DroppedItem.SpawnDroppedItem(__instance.gameObject.transform.position, food, extraItems, false, false, 0);
        }




        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // Endless Water

        [HarmonyPatch(typeof(CommonReferences), "Awake")]
        [HarmonyPostfix]
        private static void CommonReferencesAwakePostfix(CommonReferences __instance)
        {
            if (_endlessWater.Value) __instance.bucketItem = __instance.bucketOfWaterItem; //So when the "empty" bucket is put in inventory after use it is actually a full bucket. Except that makes it impossible to fill a bucket.
        }



#if CONSTRUCTIONFEATURES
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // Free Building

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

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // XP Mult
        /*
        [HarmonyPatch(typeof(TavernReputation), "ChangeReputation")]
        [HarmonyPrefix]
        private static bool TavernReputationChangeReputationPrefix(object[] __args, MethodBase __originalMethod)
        {
            var parameters = __originalMethod.GetParameters();
            DebugLog(String.Format("{0}", parameters));
            if (_xpMult.Value != 1.0f)
            {

                int pre = (int)__args[0];
                int post = Mathf.FloorToInt(_xpMult.Value * pre);
                __args[0] = post;
                DebugLog(String.Format("TavernReputation.ChangeReputation.Prefix: {0} -> {1}", pre, __args[0]));
            }
            return true;
        }
        */

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

                foreach ( FieldInfo fi in isBirdFieldInfo)
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



        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // Reputation Milestone Stuff

        [HarmonyPatch(typeof(ReputationDBAccessor), "Awake")]
        [HarmonyPrefix] //Has to be a prefix so our changes are done before this.SetUpDatabase(); 

        private static void ReputationDBAccessorAwakePrefix(ReputationDBAccessor __instance)
        {
            DebugLog("ReputationDBAccessor.Awake.Prefix");
            ReputationInfo[] repDB = ReputationDBAccessor.GetAllReputations();
            if(_dumpReputationListOnStart.Value) Log.LogInfo("repNumber, craftingTiles, craftingZonesNumber, customersCapacity, diningTiles, diningZonesNumber, floorDisponible, rentedRoomsNumber, repMax");

                         
            for (int i = 0; i < repDB.Length; i++)
            {
                if (_moreTiles.Value > 0) {repDB[i].craftingTiles += _moreTiles.Value; repDB[i].diningTiles += _moreTiles.Value;}
                if (_moreZones.Value > 0) repDB[i].craftingZonesNumber += _moreZones.Value;
                if (_moreRooms.Value > 0) repDB[i].rentedRoomsNumber += _moreRooms.Value;
                if (_moreCustomers.Value > 0) repDB[i].customersCapacity += _moreCustomers.Value;
                if (_moreDisponible.Value > 0) repDB[i].floorDisponible += _moreDisponible.Value;

                if (_dumpReputationListOnStart.Value) Log.LogInfo(String.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}",
                    repDB[i].repNumber, repDB[i].craftingTiles, repDB[i].craftingZonesNumber, repDB[i].customersCapacity, repDB[i].diningTiles, 
                    repDB[i].diningZonesNumber, repDB[i].floorDisponible, repDB[i].rentedRoomsNumber, repDB[i].repMax));
            }
        }





        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // Staff Generation Stuff

        // Default values:
        // reputation, prob1Perk, prob2Perk, lvlRangePerk1, lvlRangePerk2, lvlRangePerk3
        // 7, 80, 20, (1, 2), (1, 2), (0, 0)
        // 10, 60, 40, (1, 3), (2, 4), (0, 0)
        // 13, 25, 60, (2, 5), (3, 5), (3, 5)
        // 16, 0, 80, (0, 0), (4, 7), (5, 7)
        // 19, 0, 70, (0, 0), (5, 8), (7, 10)
        // 21, 0, 50, (0, 0), (6, 9), (9, 11)
        // 24, 0, 40, (0, 0), (7, 10), (10, 15)


        [HarmonyPatch(typeof(StaffManager), "Awake")]
        [HarmonyPostfix]
        private static void StaffManagerAwakePostfix()
        {
            if (setupDoneStaffManager) return;
            DebugLog("StaffManager.Awake.Postfix");
            if (_dumpStaffGenDataOnStart.Value) Log.LogInfo("id, reputation, prob1Perk, prob2Perk, lvlRangePerk1, lvlRangePerk2, lvlRangePerk3");
            StaffManager s = StaffManager.GetInstance();
            StaffManager.StaffGenerationValues[] q = s.staffGenerationTable;
            for (int i = 0; i < q.Length; i++)
            {
                if (_staffAlways3Perks.Value)
                {
                    q[i].prob1Perk = 0; q[i].prob2Perk = 0;
                }
                if (_staffLevel.Value >= 0)
                {
                    q[i].lvlRangePerk1.x = _staffLevel.Value; q[i].lvlRangePerk1.y = _staffLevel.Value;
                    q[i].lvlRangePerk2.x = _staffLevel.Value; q[i].lvlRangePerk2.y = _staffLevel.Value;
                    q[i].lvlRangePerk3.x = _staffLevel.Value; q[i].lvlRangePerk3.y = _staffLevel.Value;
                }
                if (_dumpStaffGenDataOnStart.Value) Log.LogInfo(string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}",i, q[i].reputation, q[i].prob1Perk, q[i].prob2Perk, q[i].lvlRangePerk1, q[i].lvlRangePerk2, q[i].lvlRangePerk3));
            }
            setupDoneStaffManager=true;
        }


        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // Refresh Staff on every HireUI open

        [HarmonyPatch(typeof(HireStaffUI), "OpenUI")]
        [HarmonyPrefix]
        private static void HireStaffUIOpenUIPrefix()
        {
            if (_staffRefreshOnOpen.Value)
            {
                StaffManager.CreateRandomOptionsWorkers();
            }
            
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // No Negative Perks on new Staff

        [HarmonyPatch(typeof(StaffManager), "CreateRandomOptionsWorkers")]
        [HarmonyPostfix]
        private static void StaffManagerCreateRandomOptionsWorkersPostFix()
        {
            DebugLog("StaffManager.CreateRandomOptionsWorkers.PostFix");
            StaffManager s = StaffManager.GetInstance();
            if (_staffNoNeg.Value)
            {
                foreach (EmployeeInfo w in s.barworkerOptions) w.perksInfo.RemoveAt(w.perksInfo.Count - 1);
                foreach (EmployeeInfo x in s.bouncerOptions) x.perksInfo.RemoveAt(x.perksInfo.Count - 1);
                foreach (EmployeeInfo y in s.waiterOptions) y.perksInfo.RemoveAt(y.perksInfo.Count - 1);
                foreach (EmployeeInfo z in s.houseKeeperOptions) z.perksInfo.RemoveAt(z.perksInfo.Count - 1);
                
            }

        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // Player Speed

        [HarmonyPatch(typeof(PlayerController), "Awake")]
        [HarmonyPostfix]
        private static void setPlayerSpeed(PlayerController __instance)
        {
            __instance.speed = _moveSpeed.Value;
            __instance.sprintMultiplier = _moveRunMult.Value;
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
        // Recipe Stuff
        // The Recipe database is not accessible during Plugin.Awake(), so we attach to the Accessor Awake() function

        [HarmonyPatch(typeof(RecipeDatabaseAccessor), "Awake")]
        [HarmonyPostfix]
        private static void RecipeDatabaseAccessorAwakePostFix(RecipeDatabaseAccessor __instance)
        {
            if (setupDoneRecipes) return;
            DebugLog("RecipeDatabaseAccessor.Awake.PostFix");
            Recipe[] allRecipes = RecipeDatabaseAccessor.GetAllRecipes();
            DebugLog(String.Format("Found {0} recipes", allRecipes.Length));
            if (_dumpRecipeListOnStart.Value) DumpRecipeList();
            for (int i = 0; i < allRecipes.Length; i++)
            {
                int craftTime = allRecipes[i].time.weeks * 7 * 24 * 60 + allRecipes[i].time.days * 24 * 60 + allRecipes[i].time.hours * 60 + allRecipes[i].time.mins;
                if (_recipesNoFuel.Value) allRecipes[i].fuel = 0;
                if (_recipesNoFragments.Value && allRecipes[i].recipeFragments > 0) allRecipes[i].recipeFragments = 1;
                if (_recipesQuickCook.Value > -1 && craftTime > _recipesQuickCook.Value)
                {
                    craftTime = _recipesQuickCook.Value;
                    int newMin = (craftTime) % 60;
                    int newHr  = (craftTime - newMin)/(60) % 24;
                    int newDay = (craftTime - newMin - 60 * newHr) / (60*24) % 7;
                    int newWk  = (craftTime - newMin - 60 * newHr - 60 * 24 * newDay) / (60 * 24 * 7) % 16; //16 weeks in a year
                    int newYr  = (craftTime - newMin - 60 * newHr - 60 * 24 * newDay - 60 * 24 * 7 * newWk) / (60 * 24 * 7  *16); 
                    allRecipes[i].time = new GameDate.Time(newYr, newWk, newDay, newHr, newMin);
                    
                }

            }
            setupDoneRecipes = true;

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

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // Fireplace does not consume fuel

        [HarmonyPatch(typeof(Fireplace), "Update")]
        [HarmonyPrefix]
        static bool FireplaceUpdatePrefix(Fireplace __instance)
        {
            if (_fireplaceNoFuelUse.Value)
            {
                return false; //just disable the update so fuel is never checvked
            }
            else
            {
                return true; // flow thorugh to normal Update
            }
        }


        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        // Adjust the maxstack for Taps/Kegs
        // Doesn't work above 99, might be bacuse of the items 
        // Need to look for *container.maxstack*, not slot.maxstack.
        [HarmonyPatch(typeof(DrinkDispenser), "Awake")]
        [HarmonyPostfix]
        static void DrinkDispenserAwakePostfix(DrinkDispenser __instance)
        {
            

            int x = __instance.maxStack;
            if (_dispensorStackSize.Value >= 0) { __instance.maxStack = (_dispensorStackSize.Value == 0) ? _itemStackSize.Value : _dispensorStackSize.Value; }  //if this is set to actual 0 it causes issues with buckets of water, so set to match the stack size


            DebugLog(String.Format("DrinkDispenser.Awake.Postfix maxstack: {0} -> {1}", x, __instance.maxStack));
        }

        // Other Container types that modify maxStack: AnimalFeederChicken.Start()
        // Other Classes that inherit from Container: ActionBarInventory, BarMenuInventory, BuildingInventory, DrinkDispenser, Fireplace, Inventory, ItemContainer, ModifierUI, 
        // Note these may not modify maxstack, but if it is 0 (teh default) will do their own thing after looking at Maxstack.
        //
        // AgingRack, AgingBarrel - these inherit from MonoBehavior

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // Aging Barrel Stack Size
        // Seems to set this a few times and everything else inherits from that?  Whatever, setting maxStacks to same value it already is does not cause issues.

        [HarmonyPatch(typeof(AgingBarrel), "Awake")]
        [HarmonyPostfix]
        static void AgingBarrelAwakePostfix(AgingBarrel __instance)
        {
            //DebugLog(String.Format("AgingBarrel.Awake.Postfix"));
            if (_agingBarrelStackSize.Value >= 0)
            {
                for (int i = 0; i < __instance.inputSlot.Length; i++)
                {
                    int pre = __instance.inputSlot[i].maxStack;
                    __instance.inputSlot[i].maxStack = _agingBarrelStackSize.Value;
                    //DebugLog(String.Format("AgingBarrel.Awake.Postfix maxstack[{0}]: {1} -> {2}", i, pre, __instance.inputSlot[i].maxStack));
                }
            }
        }




        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // Item database stuff 
        //
        // Scene:DontDestoryOnLoad, GameObject: Databases, Component: ItemDatabaseAccessor, Field: itemDatabaseSO

        [HarmonyPatch(typeof(ItemDatabaseAccessor), "SetUpDatabase")]
        [HarmonyPostfix]
        static void SetUpDatabasePostfix(ItemDatabaseAccessor __instance)
        {
            if (setupDoneItems) return;
            Item x;
            if (_dumpItemListOnStart.Value) DumpItemList();
            for (int i = 0; i < itemDatabaseSO.items.Length; i++)
            {
                x = itemDatabaseSO.items[i];

                int preSellPrice = Price2Copper(x.sellPrice);
                string xType = "";
                if (_itemStackSize.Value > 0 && x.amountStack == 99) { x.amountStack = _itemStackSize.Value; } //Only change items with default stack size of 99
                if (_wilsonOneCoin.Value && x.wilsonCoins && x.wilsonCoinsPrice > 0) x.wilsonCoinsPrice = 1;
                if (_moreValuableFish.Value != 1.0f && x.GetType() == typeof(Fish))
                {
                    xType = "Fish:";
                    x.price = PriceXFloat(x.price, _moreValuableFish.Value);
                    x.sellPrice = PriceXFloat(x.sellPrice, _moreValuableFish.Value);
                }
                if (x.GetType() == typeof(Food))
                {                   
                    IngredientType subType = (x as Food).ingredientType;
                    xType = String.Format("Food:{0}:", subType.ToString());
                    // sellPrice only works for "base" items, not things made of other things.
                    if ((_moreValuableFruit.Value != 1.0f)   && (subType == IngredientType.Fruit || subType == IngredientType.Berries)) x.sellPrice = PriceXFloat(x.sellPrice, _moreValuableFruit.Value);
                    if ((_moreValuableMeat.Value != 1.0f)    && (subType == IngredientType.Meat)) x.sellPrice = PriceXFloat(x.sellPrice, _moreValuableMeat.Value);
                    if ((_moreValuableVege.Value != 1.0f)    && (subType == IngredientType.Veg || subType == IngredientType.Legumes)) x.sellPrice = PriceXFloat(x.sellPrice, _moreValuableVege.Value);
                    //if ((_moreValuableAlcohol.Value != 1.0f) && (subType == IngredientType.Beer || subType == IngredientType.Cocktail || subType == IngredientType.Distillate || subType == IngredientType.Liqueur || subType == IngredientType.Wine)) x.sellPrice = PriceXFloat(x.sellPrice, _moreValuableAlcohol.Value);
                    //if ((_moreValuableCheese.Value != 1.0f)  && (subType == IngredientType.Cheese)) x.sellPrice = PriceXFloat(x.sellPrice, _moreValuableCheese.Value);
                    if ((_moreValuableFish.Value != 1.0f)    && (subType == IngredientType.Shellfish)) x.sellPrice = PriceXFloat(x.sellPrice, _moreValuableFish.Value);
                    if ((_moreValuableGrain.Value != 1.0f) && (subType == IngredientType.Grain)) x.sellPrice = PriceXFloat(x.sellPrice, _moreValuableGrain.Value);
                    
                }
                if (preSellPrice != Price2Copper(x.sellPrice))
                {
                    int reflectedItemId = Traverse.Create(x).Field("id").GetValue<int>();
                    DebugLog(String.Format("Price change: id {0}{1} {2} -> {3}", xType, reflectedItemId, preSellPrice, Price2Copper(x.sellPrice)));
                } 





            }
            if (_dumpItemListOnStart.Value) DumpItemList();
            if (_dumpIngredientGroupListOnStart.Value) DumpIngredientGroupList();
            setupDoneItems = true;
        }
    }
}
