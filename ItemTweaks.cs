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
        // Endless Water

        [HarmonyPatch(typeof(CommonReferences), "Awake")]
        [HarmonyPostfix]
        private static void CommonReferencesAwakePostfix(CommonReferences __instance)
        {
            if (_endlessWater.Value) __instance.bucketItem = __instance.bucketOfWaterItem; //So when the "empty" bucket is put in inventory after use it is actually a full bucket. Except that makes it impossible to fill a bucket.
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
                    if ((_moreValuableFruit.Value != 1.0f) && (subType == IngredientType.Fruit || subType == IngredientType.Berries)) x.sellPrice = PriceXFloat(x.sellPrice, _moreValuableFruit.Value);
                    if ((_moreValuableMeat.Value != 1.0f) && (subType == IngredientType.Meat)) x.sellPrice = PriceXFloat(x.sellPrice, _moreValuableMeat.Value);
                    if ((_moreValuableVege.Value != 1.0f) && (subType == IngredientType.Veg || subType == IngredientType.Legumes)) x.sellPrice = PriceXFloat(x.sellPrice, _moreValuableVege.Value);
                    //if ((_moreValuableAlcohol.Value != 1.0f) && (subType == IngredientType.Beer || subType == IngredientType.Cocktail || subType == IngredientType.Distillate || subType == IngredientType.Liqueur || subType == IngredientType.Wine)) x.sellPrice = PriceXFloat(x.sellPrice, _moreValuableAlcohol.Value);
                    //if ((_moreValuableCheese.Value != 1.0f)  && (subType == IngredientType.Cheese)) x.sellPrice = PriceXFloat(x.sellPrice, _moreValuableCheese.Value);
                    if ((_moreValuableFish.Value != 1.0f) && (subType == IngredientType.Shellfish)) x.sellPrice = PriceXFloat(x.sellPrice, _moreValuableFish.Value);
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
