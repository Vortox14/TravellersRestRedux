using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace RestfulTweaks
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private static Harmony _harmony;
        internal static ManualLogSource Log;

        private static ConfigEntry<bool> _debugLogging;
        private static ConfigEntry<int>  _dispensorStackSize;
        private readonly ConfigEntry<KeyCode> _itemDumphotKey;
        static ItemDatabaseAccessor reflectedItemDatabaseAccessor;

        public Plugin()
        {
            // bind to config settings
            _debugLogging = Config.Bind("Debug", "Debug Logging", false, "Logs additional information to console");
            _dispensorStackSize = Config.Bind("General", "Tap/Keg Stack Size", 99, "Change the amount of drinks you can store in taps/kegs");
            _itemDumphotKey = Config.Bind("General", "Item Dump HotKey", KeyCode.F10, "Press to dump list of items to console");
        }

        private void Awake()
        {
            // Plugin startup logic
            Log = Logger;
            _harmony = Harmony.CreateAndPatchAll(typeof(Plugin));
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        private void OnDestroy()
        {
            _harmony.UnpatchSelf();
        }
        
        private void Update()
        {
            //find item of type type: ItemDatabaseAccessor
            //Ah, back to that problem of "how to find a unity object with what we want as a component"
            //Scene:DontDestoryOnLoad GameObject: Databases Components: lots of databases and database accessors inclusing ItemDatabaseAccessor.

            //This will look for all <ItemDatabase> in every <ItemDatabaseAccessor>... I think.
            //https://stackoverflow.com/questions/57637680/getting-a-reference-to-all-objects-of-a-type-in-class-in-c-sharp


            if (Input.GetKeyDown(_itemDumphotKey.Value))
            {
                Type t = typeof(ItemDatabaseAccessor);
                ItemDatabase[] dbSOs = t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    .Where(fieldInfo => fieldInfo.FieldType.Equals(typeof(ItemDatabase)))
                    .Select(fieldInfo => fieldInfo.GetValue(this))
                    .Cast<ItemDatabase>()
                    .ToArray();
                Log.LogInfo(String.Format("Found {0} ItemDatabaseAccessor.ItemDatabase objects", dbSOs.Length));
                int reflectedItemId;
                Item x;
                foreach (ItemDatabase DB in dbSOs)
                {
                    Logger.LogInfo(string.Format("~~~~~~~~~~~~~~~~"));
                    for (int i = 0; i < DB.items.Length; i++)
                    {
                        

                        x = DB.items[i];
                        reflectedItemId = Traverse.Create(x).Field("id").GetValue<int>();
                        Log.LogInfo(String.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}", reflectedItemId, x.nameId, Price2Copper(x.price), Price2Copper(x.sellPrice), x.amountStack, x.shop, x.category));
                    }
                    Logger.LogInfo(string.Format("~~~~~~~~~~~~~~~~"));

                }




            }
        }
        public static int Price2Copper(Price x)
        {
            return x.gold * 100000 + x.silver * 100 + x.copper;
        }
        

        public static void DebugLog(string message)
        {
            // Log a message to console only if debug is enabled in console
            if (_debugLogging.Value)
            {
                Log.LogInfo(string.Format("NepRestfulTweaks: Debug: {0}", message));
            }
        }


        [HarmonyPatch(typeof(DrinkDispenser), "Awake")]
        [HarmonyPostfix]
        static void DrinkDispenserAwakePostfix(DrinkDispenser __instance)
        {
            // Need to look for *container.maxstack*, not slot.maxstack.

            int x = __instance.maxStack;
            if (__instance.maxStack < _dispensorStackSize.Value) { __instance.maxStack = _dispensorStackSize.Value; }
            DebugLog(String.Format("DrinkDispenser.Awake.Postfix maxstack: {0} -> {1}", x, __instance.maxStack));
        }

        // Other Container types that modify maxStack: AnimalFeederChicken.Start()
        // Other Classes that inherit from Container: ActionBarInventory, BarMenuInventory, BuildingInventory, DrinkDispenser, Fireplace, Inventory, ItemContainer, ModifierUI, 
        // Note these may not modify maxstack, but if it is 0 (teh default) will do their own thing after looking at Maxstack.
        //
        // AgingRack, AgingBarrel - these inherit from MonoBehavior

        [HarmonyPatch(typeof(ItemDatabaseAccessor), "SetUpDatabase")]
        [HarmonyPostfix]
        static void SetUpDatabasePostfix(ItemDatabaseAccessor __instance)
        {
            DebugLog("SetUpDatabase.Postfix");
            ItemDatabase reflectedItemDatabaseSO = Traverse.Create(__instance).Field("itemDatabaseSO").GetValue<ItemDatabase>();
            Item x;
            int reflectedItemId;
            DebugLog(String.Format("SetUpDatabase.Postfix {0} Items", reflectedItemDatabaseSO.items.Length));
            Log.LogInfo("id, name, price, sellPrice, amountStack, shop, category");
            for (int i = 0; i < reflectedItemDatabaseSO.items.Length; i++)
            {
                x = reflectedItemDatabaseSO.items[i];
                reflectedItemId = Traverse.Create(x).Field("id").GetValue<int>();
                Log.LogInfo(String.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}", reflectedItemId, x.nameId, Price2Copper(x.price), Price2Copper(x.sellPrice), x.amountStack,x.shop,x.category));


            }

            reflectedItemDatabaseAccessor = __instance;
        }

    }
}
