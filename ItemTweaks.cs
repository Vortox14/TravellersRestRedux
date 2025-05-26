using BepInEx;
using HarmonyLib;
using System;

namespace RestfulTweaks
{
    public partial class Plugin : BaseUnityPlugin
    {
        [HarmonyPatch(typeof(DrinkDispenser), "Awake")]
        [HarmonyPostfix]
        static void DrinkDispenserAwakePostfix(DrinkDispenser __instance)
        {
            int x = __instance.maxStack;
            if (_dispensorStackSize.Value >= 0) { __instance.maxStack = (_dispensorStackSize.Value == 0) ? _itemStackSize.Value : _dispensorStackSize.Value; }
            DebugLog(String.Format("DrinkDispenser.Awake.Postfix maxstack: {0} -> {1}", x, __instance.maxStack));
        }

        [HarmonyPatch(typeof(AgingBarrel), "Awake")]
        [HarmonyPostfix]
        static void AgingBarrelAwakePostfix(AgingBarrel __instance)
        {
            if (_agingBarrelStackSize.Value >= 0)
            {
                for (int i = 0; i < __instance.inputSlot.Length; i++)
                {
                    _ = __instance.inputSlot[i].maxStack;
                    __instance.inputSlot[i].maxStack = _agingBarrelStackSize.Value;
                }
            }
        }

        [HarmonyPatch(typeof(CommonReferences), "Awake")]
        [HarmonyPostfix]
        private static void CommonReferencesAwakePostfix(CommonReferences __instance)
        {
            if (_endlessWater.Value) __instance.bucketItem = __instance.bucketOfWaterItem;
        }

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
                if (_itemStackSize.Value > 0 && x.amountStack == 99) { x.amountStack = _itemStackSize.Value; }
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
                    if ((_moreValuableFruit.Value != 1.0f) && (subType == IngredientType.Fruit || subType == IngredientType.Berries)) x.sellPrice = PriceXFloat(x.sellPrice, _moreValuableFruit.Value);
                    if ((_moreValuableMeat.Value != 1.0f) && (subType == IngredientType.Meat)) x.sellPrice = PriceXFloat(x.sellPrice, _moreValuableMeat.Value);
                    if ((_moreValuableVege.Value != 1.0f) && (subType == IngredientType.Veg || subType == IngredientType.Legumes)) x.sellPrice = PriceXFloat(x.sellPrice, _moreValuableVege.Value);
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
