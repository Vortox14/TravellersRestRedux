using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;

namespace RestfulTweaks
{
    public partial class Plugin : BaseUnityPlugin
    {
        public static void DumpRecipeList()
        {
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
                int reflectedItemId = Traverse.Create(x).Field("id").GetValue<int>();
                string reflectedItemIDesc = Traverse.Create(x).Field("description").GetValue<string>();
                itemName = (x.translationByID) ? LocalisationSystem.Get("Items/item_name_" + reflectedItemId.ToString()) : x.nameId;
                itemName = "\"" + itemName + "\"";
                itemDesc = (x.translationByID) ? LocalisationSystem.Get("Items/item_description_" + reflectedItemId.ToString()) : reflectedItemIDesc;
                itemDesc = "\"" + itemDesc + "\"";
                itemShop = "\"" + x.shop + "\"";
                itemCategory = "\"" + x.category + "\"";
                if (x.GetType() == typeof(Food)) itemSubType = (x as Food).ingredientType.ToString();
                else if (x.GetType() == typeof(Fish)) itemSubType = (x as Fish).fishType.ToString();
                else itemSubType = "";
                Log.LogInfo(String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}", reflectedItemId, itemName, itemDesc, Price2Copper(x.price), Price2Copper(x.sellPrice), x.amountStack, itemShop, itemCategory, Tags2String(x.tags), x.wilsonCoins, x.wilsonCoinsPrice, x.GetType(), itemSubType));
            }
        }

        public static void DumpIngredientGroupList()
        {
            Item y;
            IngredientGroup x;
            string itemName;
            string itemDesc;
            Log.LogInfo(string.Format("id, name, desc, ingredientsTypes, PossibleItems, itemModAux, cheapestIngredient"));
            for (int i = 0; i < itemDatabaseSO.items.Length; i++)
            {
                y = itemDatabaseSO.items[i];
                if (y.GetType() != typeof(IngredientGroup)) continue;
                x = itemDatabaseSO.items[i] as IngredientGroup;

                // 'id' is a protected field in IngredientGroup, so we can't access it directly (e.g. x.id).
                // Using HarmonyLib's Traverse to reflectively access it instead.
                int reflectedItemId = Traverse.Create(x).Field("id").GetValue<int>();

                // Same for 'description' — protected in the original class.
                // Reflection lets us bypass C#'s access level restrictions at runtime.
                string reflectedItemIDesc = Traverse.Create(x).Field("description").GetValue<string>();
                itemName = (x.translationByID) ? LocalisationSystem.Get("Items/item_name_" + reflectedItemId.ToString()) : x.nameId;
                itemName = "\"" + itemName + "\"";
                itemDesc = (x.translationByID) ? LocalisationSystem.Get("Items/item_description_" + reflectedItemId.ToString()) : reflectedItemIDesc;
                itemDesc = "\"" + itemDesc + "\"";
                List<ItemMod> xPossibleItems = Traverse.Create(x).Field("possibleItems").GetValue<List<ItemMod>>();
                ItemMod xCheapestIngredient = Traverse.Create(x).Field("cheapestIngredient").GetValue<ItemMod>();
                ItemMod xItemModAux = Traverse.Create(x).Field("itemModAux").GetValue<ItemMod>();
                Log.LogInfo(String.Format("{0},{1},{2},{3},{4},{5},{6}", reflectedItemId, itemName, itemDesc, string.Join(":", x.ingredientsTypes), ItemModList2String(xPossibleItems), ItemMod2String(xItemModAux), ItemMod2String(xCheapestIngredient)));
            }
        }
    }
}
