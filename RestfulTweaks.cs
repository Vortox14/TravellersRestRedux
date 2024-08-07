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

namespace RestfulTweaks
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private static Harmony _harmony;
        internal static ManualLogSource Log; // static copy of the BaseUnityPligin.Logger object so it can be accessed in static methods, initilized in constructor

        private static ConfigEntry<bool> _debugLogging;
        private static ConfigEntry<bool> _dumpItemListOnStart;
        //private readonly ConfigEntry<KeyCode> _dumpItemHotkey;
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
        private static ConfigEntry<bool> _CropFastGrow;
        private static ConfigEntry<bool> _CropFastRegrow;
        private static ConfigEntry<bool> _staffNoNeg;
        private static ConfigEntry<bool> _staffRefreshOnOpen;
        private static ConfigEntry<bool> _staffAlways3Perks;
        private static ConfigEntry<int> _staffLevel;
        private static ConfigEntry<bool> _dumpStaffGenData;
        private static ConfigEntry<bool> _dumpReputationListOnStart;
        private static ConfigEntry<int> _moreTiles;
        private static ConfigEntry<int> _moreZones;
        private static ConfigEntry<int> _moreRooms;
        private static ConfigEntry<int> _moreCustomers;
        private static ConfigEntry<int> _moreDisponible;
        //private static ConfigEntry<bool> _catNeverGetsAngry; //Cat hates me and will not stop getting angry!
        private static ConfigEntry<bool> _wilsonOneCoin;
        private static ConfigEntry<float> _moreValuableFish;
        private static ConfigEntry<bool> _easyBirdTraining;

        public static int itemIdJuice = 1325;

        public static ItemDatabaseAccessor itemDatabaseAccessor;
        //public static RecipeDatabaseAccessor recipeDatabaseAccessor; //Not needed since RecipeDatabaseAccessor is full of useful static functions
        public Plugin()
        {
            // bind to config settings
            _debugLogging = Config.Bind("Debug", "Debug Logging", false, "Logs additional information to console");
            _dispensorStackSize = Config.Bind("Stacks", "Tap/Keg Stack Size", -1, "Change the amount of drinks you can store in taps/kegs; set to -1 to disable, set to 0 to use item stack size");
            _agingBarrelStackSize = Config.Bind("Stacks", "Aging Barrel Stack Size", -1, "NOT WORKING Change the amount of drinks you can store in aging barrels; set to -1 to disable, set to 0 to use item stack size");
            _itemStackSize = Config.Bind("Stacks", "Item Stack Size", -1, "Change the stack size of any item that normally stacks to 99; set to -1 to disable");
            _dumpItemListOnStart= Config.Bind("Database", "List Items on start", false, "set to true to print a list of all items to console on startup");
            _dumpRecipeListOnStart = Config.Bind("Database", "List Recipes on start", false, "set to true to print a list of all recipes to console on startup");
            _dumpReputationListOnStart = Config.Bind("Database", "List Reputation milestones on start", false, "set to true to print a list of all reputation milestones to console on startup"); 
            _dumpStaffGenData = Config.Bind("Database", "List staff generation data on start", false, "set to true to print a list of staff generation data on startup");
            _moveSpeed = Config.Bind("Movement", "Walking Speed", 2.5f, "walking speed; set to 2.5 for default speed ");
            _moveRunMult = Config.Bind("Movement", "Run Speed Multiplier", 1.6f, "run speed multiplier; set to 1.6 for default speed ");
            _soilStaysWatered = Config.Bind("Farming", "Soil Stays Wet", false, "Soil stays watered");
            _soilWet3DaysRain = Config.Bind("Farming", "Rain Fully waters woil", false, "rain will make soil wet for the next 3 days, like watering");
            _recipesNoFuel = Config.Bind("Recipes", "No Fuel", false, "Recipes no longer require fuel");
            _recipesNoFragments = Config.Bind("Recipes", "No Fragment Cost", false, "Cave Recipies only cost one fragment");
            _fireplaceNoFuelUse = Config.Bind("Misc", "Fireplace does not consume fuel", false, "fireplace no longer consumes fuel");
            _recipesQuickCook = Config.Bind("Recipes", "Quick Crafting", -1, "Sets the maximum time recipes take to craft in minutes; set to -1 to disable");
            _dumpCropListOnStart = Config.Bind("Database", "List Crops on start", false, "set to true to print a list of all crops to console on startup");
            _CropFastGrow = Config.Bind("Farming", "Fast Growing Crops", false, "All crops advance one growth stage per day");
            _CropFastRegrow = Config.Bind("Farming", "Fast Regrowing Crops", false, "Crops that allow multiple harvests can be harvested every day");
            _staffNoNeg = Config.Bind("Staff", "No Negative Perks", false, "New Staff will not have any negative perks");
            _staffRefreshOnOpen = Config.Bind("Staff", "Refresh Applicants on Open", false, "Refresh the list of new staff available to hire every time the hiring interface is opened");
            _staffAlways3Perks = Config.Bind("Staff", "Always Three Perks", false, "NOT WORKING New hires will always have three positive perks");
            _staffLevel = Config.Bind("Staff", "Starting Level", -1, "Starting level for new hires; set to -1 to disable, set to 31 for all three skills at level 5");
            //_catNeverGetsAngry = Config.Bind("Misc", "Cat Never Gets Upset", false, "NOT WORKING prevents your cat from lowering its opinion of you");
            _moreTiles = Config.Bind("Milestones", "More Zone Tiles", -1, "increase number of tiles for crafting/dining zone; set to -1 to disable");
            _moreZones = Config.Bind("Milestones", "More Crafting Zones", -1, "NOT WELL TESTED! increase number of zones for crafting; set to -1 to disable");
            _moreRooms = Config.Bind("Milestones", "More Rentable Rooms", -1, "increase number of rooms for rent; set to -1 to disable");
            _moreCustomers = Config.Bind("Milestones", "More Customer", -1, "increase customer capacity; set to -1 to disable");
            _moreDisponible = Config.Bind("Milestones", "More Floor Tiles", -1, "increase total number of floor tiles allowed; set to -1 to disable");
            _wilsonOneCoin = Config.Bind("Misc", "Wilson Price Reduction", false, "Wilson only charges 1 coin per item");
            _moreValuableFish = Config.Bind("Misc", "Fish price increase", 1.0f, "increase the value of fish; set to 1.0 to disable");
            _easyBirdTraining = Config.Bind("Misc", "Easy Bird Training", false, "More benefit from crackers, giving cracker at wrong time results in less benefit instead of loss");

        }

        private void Awake()
        {
            // Plugin startup logic
            Log = Logger;
            initDBs();
            _harmony = Harmony.CreateAndPatchAll(typeof(Plugin));
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
        //private void Update()
        //{
        //    if (Input.GetKeyDown(_dumpItemHotkey.Value))
        //    {
        //        DumpItemList();
        //    }
        //}
        private void OnDestroy()
        {
            _harmony.UnpatchSelf();
        }
        



        //private void Update()
        //{
        //    if (Input.GetKeyDown(_itemDumphotKey.Value))
        //    {
        //        Plugin.DumpItems();
        //    }
        //}
        private static void initDBs()
        {
            if (itemDatabaseAccessor == null)
            {
                itemDatabaseAccessor = UnityEngine.Object.FindObjectOfType<ItemDatabaseAccessor>();
            }
            //Not needed since RecipeDatabaseAccessor is full of useful static functions
            //if (recipeDatabaseAccessor == null)
            //{
            //    recipeDatabaseAccessor = RecipeDatabaseAccessor.GetInstance();
            //}

        }
        public static void DebugLog(string message)
        {
            // Log a message to console only if debug is enabled in console
            if (_debugLogging.Value)
            {
                Log.LogInfo(string.Format("DEBUG: {0}", message));
            }
        }

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
            ItemDatabaseAccessor db = ItemDatabaseAccessor.GetInstance();
            ItemDatabase reflectedItemDatabaseSO = Traverse.Create(db).Field("itemDatabaseSO").GetValue<ItemDatabase>();

            Item x;
            string itemName;
            string itemDesc;
            string itemShop;
            string itemCategory;

            Log.LogInfo(string.Format("id, name, desc, price, sellPrice, amountStack, shop, category, tags, wilsonCoins, wilsonCoinsPrice, getType()"));
            for (int i = 0; i < reflectedItemDatabaseSO.items.Length; i++)
            {
                x = reflectedItemDatabaseSO.items[i];
                int reflectedItemId = Traverse.Create(x).Field("id").GetValue<int>();                 //Protected
                string reflectedItemIDesc = Traverse.Create(x).Field("description").GetValue<string>();  //Protected

                itemName = (x.translationByID) ? LocalisationSystem.Get("Items/item_name_" + reflectedItemId.ToString()) : x.nameId;
                itemName = "\"" + itemName + "\"";
                itemDesc = (x.translationByID) ? LocalisationSystem.Get("Items/item_description_" + reflectedItemId.ToString()) : reflectedItemIDesc;
                itemDesc = "\"" + itemDesc + "\"";
                itemShop = "\"" + x.shop + "\"";
                itemCategory =  "\"" + x.category + "\"";
                Log.LogInfo(String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
                    reflectedItemId, itemName, itemDesc, Price2Copper(x.price), Price2Copper(x.sellPrice), x.amountStack, itemShop, itemCategory, Tags2String(x.tags),
                     x.wilsonCoins, x.wilsonCoinsPrice, x.GetType()));


            }
        }


        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // Bird Stuff


        [HarmonyPatch(typeof(BirdNPC), "MouseUP")]
        [HarmonyPrefix]
        private static void BirdNPCMouseUPPrefix(BirdNPC __instance)
        {
            DebugLog("BirdNPC.MouseUP.Prefix");
            if (_easyBirdTraining.Value)
            {
                __instance.canGiveCookieTime = 0f;
                __instance.cookieDecrement = -0.1f;
                __instance.cookieIncrement = 0.25f;
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



        /*   The cat hates me and nothing I do will make it not hate me, so I'm ignoring it for now
         
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // Cat Opinion
        [HarmonyPatch(typeof(CatNPC), "MinusRelationship")]
        [HarmonyPrefix]
        private static bool CatNPCMinusRelationshipPrefix()
        {
            DebugLog("CatNPC.MinusRelationship.Prefix");
            return !_catNeverGetsAngry.Value; //skip the original function when this option is enabled
        }
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // Cat Opinion
        [HarmonyPatch(typeof(CatNPC))]
        [HarmonyPatch("Relationshop", MethodType.Setter)]
        [HarmonyPrefix]
        private static void CatNPCset_RelationshopPrefix(ref float __value)
        {
            DebugLog(String.Format("CatNPC.set_Relationshop.Prefix: {0}",__value));
        }
        */



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
            DebugLog("StaffManager.Awake.Postfix");
            if (_dumpStaffGenData.Value) Log.LogInfo("id, reputation, prob1Perk, prob2Perk, lvlRangePerk1, lvlRangePerk2, lvlRangePerk3");
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
                if (_dumpStaffGenData.Value) Log.LogInfo(string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}",i, q[i].reputation, q[i].prob1Perk, q[i].prob2Perk, q[i].lvlRangePerk1, q[i].lvlRangePerk2, q[i].lvlRangePerk3));
            }



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
        // No Negative Perks on Staff

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
                if (_dumpCropListOnStart.Value)
                {
                    Log.LogInfo(String.Format("Recipe: {0}, {1}, {2}, {3}, {4}, {5}", allCrops[i].id, allCrops[i].nameId, allCrops[i].name, allCrops[i].daysToGrow, allCrops[i].daysUntilNewHarvest, allCrops[i].reusable));
                }

            }
        }


        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // Recipe Stuff
        // The Recipe database is not accessible during Plugin.Awake(), so we attach to the Accessor Awake() function

        [HarmonyPatch(typeof(RecipeDatabaseAccessor), "Awake")]
        [HarmonyPostfix]
        private static void RecipeDatabaseAccessorAwakePostFix(RecipeDatabaseAccessor __instance)
        {
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

        }


        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // Soil Stays Watered

        [HarmonyPatch(typeof(FertileSoil), "CheckWater")]
        [HarmonyPrefix]
        static bool FertileSoilCheckWaterPrefix(FertileSoil __instance)
        {
            if (_soilStaysWatered.Value)
            {
                return false; //just disable the update so water level is not changed
            }
            else if (_soilWet3DaysRain.Value)
            {
                if (Weather.IsWeatherActive(Weather.WeatherType.Rain) && __instance.daysUntilDry <= 3) __instance.daysUntilDry = 3;
                return true; // so the "do stuff when soil changes wetness state" routine will still run
            }
            else
            {                
                return true; // flow thorugh to normal Update
            }
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
            if (_dispensorStackSize.Value >= 0) { __instance.maxStack = _dispensorStackSize.Value; }

            DebugLog(String.Format("DrinkDispenser.Awake.Postfix maxstack: {0} -> {1}", x, __instance.maxStack));
        }

        // Other Container types that modify maxStack: AnimalFeederChicken.Start()
        // Other Classes that inherit from Container: ActionBarInventory, BarMenuInventory, BuildingInventory, DrinkDispenser, Fireplace, Inventory, ItemContainer, ModifierUI, 
        // Note these may not modify maxstack, but if it is 0 (teh default) will do their own thing after looking at Maxstack.
        //
        // AgingRack, AgingBarrel - these inherit from MonoBehavior

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        /*
        // Aging Barrel Stack Size
        [HarmonyPatch(typeof(AgingBarrelUI), "Awake")]
        [HarmonyPostfix]
        static void AgingBarrelUIAwakePostfix(AgingBarrelUI __instance)
        {
            SlotUI[] reflectedInputSlot1 = Traverse.Create(__instance).Field("inputSlot").GetValue<SlotUI[]>();
            SlotUI[] reflectedInputSlot3 = Traverse.Create(__instance).Field("inputSlot3").GetValue<SlotUI[]>();
            SlotUI[] reflectedInputSlot5 = Traverse.Create(__instance).Field("inputSlot6").GetValue<SlotUI[]>();
            Slot x;
            int y;

            for (int i = 0; i < reflectedInputSlot1.Length; i++)
            {
                x = Traverse.Create(reflectedInputSlot1[i]).Field("slot").GetValue<Slot>();
                y = Traverse.Create(x).Field("stack").GetValue<int>();
                DebugLog(String.Format("Slot:1 index:{0} maxStack:{1}",i, y));
            }
            for (int j = 0; j < reflectedInputSlot3.Length; j++)
            {
                x = Traverse.Create(reflectedInputSlot3[j]).Field("slot").GetValue<Slot>();
                y = Traverse.Create(x).Field("stack").GetValue<int>();
                DebugLog(String.Format("Slot:1 index:{0} maxStack:{1}", i, y));
            }
            for (int k = 0; k < reflectedInputSlot5.Length; k++)
            {
                x = Traverse.Create(reflectedInputSlot1[k]).Field("slot").GetValue<Slot>();
                y = Traverse.Create(x).Field("stack").GetValue<int>();
                DebugLog(String.Format("Slot:5 index:{0} maxStack:{1}", i, y));
            }

        }
        */



        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // Item database stuff 
        //
        // Scene:DontDestoryOnLoad, GameObject: Databases, Component: ItemDatabaseAccessor, Field: itemDatabaseSO

        [HarmonyPatch(typeof(ItemDatabaseAccessor), "SetUpDatabase")]
        [HarmonyPostfix]
        static void SetUpDatabasePostfix(ItemDatabaseAccessor __instance)
        {
            ItemDatabase reflectedItemDatabaseSO = Traverse.Create(__instance).Field("itemDatabaseSO").GetValue<ItemDatabase>();

            Item x;
 
            if (_dumpItemListOnStart.Value) DumpItemList();
            for (int i = 0; i < reflectedItemDatabaseSO.items.Length; i++)
            {
                x = reflectedItemDatabaseSO.items[i];

                if (_itemStackSize.Value > 0 && x.amountStack == 99) { x.amountStack = _itemStackSize.Value; } //Only change items with default stack size of 99
                if (_wilsonOneCoin.Value && x.wilsonCoins && x.wilsonCoinsPrice > 0) x.wilsonCoinsPrice = 1;
                if (_moreValuableFish.Value != 1.0f && x.GetType() == typeof(Fish))
                {
                    x.price = PriceXFloat(x.price, _moreValuableFish.Value);
                    x.sellPrice = PriceXFloat(x.sellPrice, _moreValuableFish.Value);
                }
            }
            if (_dumpItemListOnStart.Value) DumpItemList();
        }
    }
}
