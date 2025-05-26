using BepInEx;
using HarmonyLib;
using System;

namespace RestfulTweaks
{
    public partial class Plugin : BaseUnityPlugin
    {
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
                    int newHr = (craftTime - newMin) / (60) % 24;
                    int newDay = (craftTime - newMin - 60 * newHr) / (60 * 24) % 7;
                    int newWk = (craftTime - newMin - 60 * newHr - 60 * 24 * newDay) / (60 * 24 * 7) % 16;
                    int newYr = (craftTime - newMin - 60 * newHr - 60 * 24 * newDay - 60 * 24 * 7 * newWk) / (60 * 24 * 7 * 16);
                    allRecipes[i].time = new GameDate.Time(newYr, newWk, newDay, newHr, newMin);
                }
            }
            setupDoneRecipes = true;
        }
    }
}
