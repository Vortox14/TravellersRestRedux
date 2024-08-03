using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

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
        private static ConfigEntry<bool> _recipesNoFuel;
        private static ConfigEntry<bool> _recipesNoFragments;
        private static ConfigEntry<bool> _fireplaceNoFuelUse;

        private static List<Item> itemDB = new List<Item>();


        public static ItemDatabaseAccessor itemDatabaseAccessor;
        //public static RecipeDatabaseAccessor recipeDatabaseAccessor; //Not needed since RecipeDatabaseAccessor is full of useful static functions
        public Plugin()
        {
            // bind to config settings
            _debugLogging = Config.Bind("Debug", "Debug Logging", false, "Logs additional information to console");
            _dispensorStackSize = Config.Bind("Stacks", "Tap/Keg Stack Size", 0, "Change the amount of drinks you can store in taps/kegs; set to -1 to disable, set to 0 to use item stack size");
            _agingBarrelStackSize = Config.Bind("Stacks", "Aging Barrel Stack Size", 0, "NOT WORKING Change the amount of drinks you can store in aging barrels; set to -1 to disable, set to 0 to use item stack size");
            _itemStackSize = Config.Bind("Stacks", "Item Stack Size", 999, "Change the stack size of any item that normally stacks to 99; set to -1 to disable");
            _dumpItemListOnStart= Config.Bind("Database", "List Items on start", false, "set to true to print a list of all items to console on startup");
            _moveSpeed = Config.Bind("Movement", "Walking Speed", 2.5f, "walking speed; set to 2.5f for default speed ");
            _moveRunMult = Config.Bind("Movement", "Run Speed Multiplier", 1.6f, "run speed multiplier; set to 1.6f for default speed ");
            _soilStaysWatered = Config.Bind("Farming", "Soil Stays Wet", false, "Soil stays watered");
            _recipesNoFuel = Config.Bind("Recipes", "No Fuel", true, "Recipes no longer require fuel");
            _recipesNoFragments= Config.Bind("Recipes", "No Fragment Cost", true, "Recipes No longer cost recipe Fragmenst to purchase");
            _fireplaceNoFuelUse = Config.Bind("Misc", "Fireplace does not consume fuel", false, "fireplace no longer consume fuel");
        }

        private void Awake()
        {
            // Plugin startup logic
            Log = Logger;
            initDBs();
            recipeChanges();
            setPlayerSpeed();
            _harmony = Harmony.CreateAndPatchAll(typeof(Plugin));
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        private void OnDestroy()
        {
            _harmony.UnpatchSelf();
        }
        

        private static void setPlayerSpeed()
        {
            if (_moveRunMult.Value != 1.6f && _moveSpeed.Value != 2.5f)
            {
                PlayerController x = UnityEngine.Object.FindObjectOfType<PlayerController>();
                x.speed = _moveSpeed.Value;
                x.sprintMultiplier = _moveRunMult.Value;

            }
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

        private static void recipeChanges()
        {
            RecipeDatabase reflectedRecipes = Traverse.Create(RecipeDatabaseAccessor.GetInstance()).Field("recipeDatabaseSO").GetValue<RecipeDatabase>();
            if (reflectedRecipes != null)
            {
                for (int i =0;i< reflectedRecipes.recipes.Length;i++)
                {
                    DebugLog(reflectedRecipes.recipes[i].name);
                }

            } 
            else
            {
                DebugLog("recipeChanges: could not find recipeDatabaseSO");
            }
        }

        private static void DumpItems()
        {
            
            int reflectedItemId;
            string reflectedItemIDesc;

            
            Log.LogInfo(string.Format("~~~~~~~~~~~~~~~~"));
            Log.LogInfo(string.Format("id, name, price, sellPrice, amountStack, shop, category, tags, description"));
            foreach (Item x in Plugin.itemDB)
            {
                reflectedItemId = Traverse.Create(x).Field("id").GetValue<int>();                 //Protected
                reflectedItemIDesc = Traverse.Create(x).Field("description").GetValue<string>();  //Protected

                //translationByID

                Log.LogInfo(String.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}",
                    reflectedItemId, x.nameId, Price2Copper(x.price), Price2Copper(x.sellPrice), x.amountStack, x.shop, x.category, Tags2String(x.tags), LocalisationSystem.Get(reflectedItemIDesc)));
            }
            Log.LogInfo(string.Format("~~~~~~~~~~~~~~~~"));
        }
        public static int Price2Copper(Price x)
        {
            return x.gold * 100000 + x.silver * 100 + x.copper;
        }
        public static string Tags2String(Tag[] x)
        {
            return string.Join(":", x);
        }


        public static void DebugLog(string message)
        {
            // Log a message to console only if debug is enabled in console
            if (_debugLogging.Value)
            {
                Log.LogInfo(string.Format("DEBUG: {0}", message));
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
        // Adjust stack size for items
        //
        // While we're at it, populate Plugin.itemDB with all the items that are in ItemDatabaseAccessor.itemDatabaseSO
        // Because I can't figure out how to access objects in unity scene via C# or I would just use access
        // Scene:DontDestoryOnLoad, GameObject: Databases, Component: ItemDatabaseAccessor, Field: itemDatabaseSO

        [HarmonyPatch(typeof(ItemDatabaseAccessor), "SetUpDatabase")]
        [HarmonyPostfix]
        static void SetUpDatabasePostfix(ItemDatabaseAccessor __instance)
        {
            ItemDatabase reflectedItemDatabaseSO = Traverse.Create(__instance).Field("itemDatabaseSO").GetValue<ItemDatabase>();

            Item x;
            
            for (int i = 0; i < reflectedItemDatabaseSO.items.Length; i++)
            {
                x = reflectedItemDatabaseSO.items[i];
                Plugin.itemDB.Add(x); //put a copy in there for later access
                if (_itemStackSize.Value > 0 && x.amountStack == 99 ) { x.amountStack = _itemStackSize.Value; } //Only change items with default stack size of 99

            }
            DebugLog(String.Format("SetUpDatabase.Postfix {0} Items in Plugin.itemDB", Plugin.itemDB.Count));

            if (_dumpItemListOnStart.Value) Plugin.DumpItems();
        }

    }
}
