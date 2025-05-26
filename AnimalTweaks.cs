using System;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace RestfulTweaks
{
    public partial class Plugin : BaseUnityPlugin
    {
        [HarmonyPatch(typeof(AnimalNPC), "Hit", new Type[] { typeof(Vector3), typeof(bool) })]
        [HarmonyPostfix]
        private static void AnimalNPCHitPostFix(AnimalNPC __instance)
        {
            Plugin.DebugLog($"AnimalNPC.Hit.PostFix: {__instance.GetType()} with {__instance.lives} health remaining");
            if (__instance.lives > 0)
                return;
            int extraDropAmount = 0;
            if (Plugin._cowLootExtra.Value > 0 && __instance.GetType() == typeof(CowNPC)) extraDropAmount = Plugin._cowLootExtra.Value;
            if (Plugin._pigLootExtra.Value > 0 && __instance.GetType() == typeof(PigNPC)) extraDropAmount = Plugin._pigLootExtra.Value;
            if (Plugin._sheepLootExtra.Value > 0 && __instance.GetType() == typeof(SheepNPC)) extraDropAmount = Plugin._sheepLootExtra.Value;
            if (Plugin._chickenLootExtra.Value > 0 && __instance.GetType() == typeof(ChickenNPC)) extraDropAmount = Plugin._chickenLootExtra.Value;
            if (extraDropAmount == 0) return;
            Plugin.DebugLog($"AnimalNPC.Hit.PostFix: Spawning {extraDropAmount} extra items per loot type.");
            Animal animal = __instance.placeable.itemSetup.item as Animal;
            if (animal == null)
            {
                Plugin.DebugLog("AnimalNPC.Hit.PostFix: Animal data missing, skipping extra drops.");
                return;
            }
            ItemProduction[] sacrificeItems = Traverse.Create(animal).Field("sacrificeItems").GetValue<ItemProduction[]>();
            if (sacrificeItems == null || sacrificeItems.Length == 0)
            {
                Plugin.DebugLog("AnimalNPC.Hit.PostFix: No sacrifice items found.");
                return;
            }
            foreach (var itemProduction in sacrificeItems)
            {
                var item = Traverse.Create(itemProduction).Field("item").GetValue<Item>();
                if (item != null)
                {
                    Food food = item as Food;
                    if (food != null)
                    {
                        Plugin.DebugLog($"AnimalNPC.Hit.PostFix: Spawning {extraDropAmount} extra {food.name}");
                        DroppedItem.SpawnDroppedItem(__instance.transform.position, food, extraDropAmount, false, false, 0);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(AnimalNPC), "Update")]
        [HarmonyPrefix]
        private static void AnimalNPCUpdatePrefix(AnimalNPC __instance)
        {
            if (_AnimalsNoNeeds.Value)
            {
                __instance.hasWalked = true;
                __instance.hasWater = true;
                __instance.hasFood = true;
            }
        }

        [HarmonyPatch(typeof(AnimalNPC), "IncrementProduction")]
        [HarmonyPrefix]
        private static void AnimalNPCIncrementProductionPrefix(AnimalNPC __instance)
        {
            bool value = Plugin._fasterMilk.Value;
            if (value)
            {
                Animal animal = __instance.placeable.itemSetup.item as Animal;
                bool productionLimitedToOnce = animal.productionLimitedToOnce;
                if (productionLimitedToOnce)
                {
                    __instance.productionProgress = Mathf.Max(1f, __instance.productionProgress);
                }
            }
        }

        [HarmonyPatch(typeof(CowNPC), "MouseHold")]  
        [HarmonyPostfix]
        private static void CowNPCMouseHoldPostfix(CowNPC __instance)
        {
            DebugLog($"CowNPCMouseHoldPostfix()");
            if (_infiniteMilk.Value)
            {
                _ = __instance.placeable.itemSetup.item as Animal;
                __instance.productionProgress = 1f;
            }
        }

        [HarmonyPatch(typeof(SheepNPC), "MouseHold")]
        [HarmonyPostfix]
        private static void SheepNPCMouseHoldPostfix(SheepNPC __instance)
        {
            DebugLog($"SheepNPCMouseHoldPostfix()");
            if (_infiniteMilk.Value)
            {
                _ = __instance.placeable.itemSetup.item as Animal;
                __instance.productionProgress = 1f;
            }
        }

        [HarmonyPatch(typeof(ChickenNPC), "IncrementProduction")]
        [HarmonyPrefix]
        private static void ChickenNPCIncrementProductionPrefix(ChickenNPC __instance)
        {
            if (_moreEggs.Value > 0)
            {
                HenHouse henHouse = __instance.currentBuilding as HenHouse;
                henHouse.IncrementEggsAmount(_moreEggs.Value);
            }
        }

        [HarmonyPatch(typeof(AnimalNPC), "Sick")]
        [HarmonyPrefix]
        private static bool AnimalNPCSickPrefix(AnimalNPC __instance)
        {
            return !_AnimalsNoSick.Value;
        }

        [HarmonyPatch(typeof(AnimalNPC), "IncrementLevel")]
        [HarmonyPrefix]
        private static void AnimalNPCIncrementLevelPrefix(AnimalNPC __instance)
        {
            if (_AnimalsFastGrow.Value)
            {
                __instance.level = __instance.maxLevel;
            }
        }

        [HarmonyPatch(typeof(AnimalNPC), "IncrementLevel")]
        [HarmonyPostfix]
        private static void AnimalNPCIncrementLevelPostfix(AnimalNPC __instance)
        {
            if (_AnimalsNoNeeds.Value)
            {
                __instance.hasWalked = true;
                __instance.hasWater = true;
                __instance.hasFood = true;
            }
        }
    }
}
