using BepInEx;
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace RestfulTweaks
{
    public partial class Plugin : BaseUnityPlugin
    {
        [HarmonyPatch(typeof(ShopDatabaseAccessor), "Awake")]
        [HarmonyPostfix]
        private static void ShopDatabaseAccessorAwakePostfix()
        {
            if (!(_shopUpdateDaily.Value || _shopAllItems.Value || _shopMoreItems.Value)) return;
            List<Shop> allShops = ShopDatabaseAccessor.GetAllShops();
            DebugLog($"ShopDatabaseAccessorAwakePostfix(): {allShops.Count()} shops found");
            foreach (Shop s in allShops)
            {
                DebugLog($"ShopDatabaseAccessorAwakePostfix(): Shop Name:{s.name} Item Count:{s.shopItems.Count}");
                if (_shopUpdateDaily.Value) s.updateDays = new List<Day> { Day.Mon, Day.Tue, Day.Wed, Day.Thurs, Day.Fri, Day.Sat, Day.Sun };
                if (s.shopItems is null) continue;
                for (int i = 0; i < s.shopItems.Count; i++)
                {
                    if (_shopAllItems.Value) s.shopItems[i].alwaysAppear = true;
                    if (_shopMoreItems.Value) s.shopItems[i].unlimited = true;
                }
            }
        }

        public static void ShopRefresh()
        {
            List<Shop> allShops = ShopDatabaseAccessor.GetAllShops();
            DebugLog($"{allShops.Count()} shops found");
            ShopDatabaseAccessor dbA = ShopDatabaseAccessor.GetInstance();
            Dictionary<int, Shop> reflectedShopDict = null;
            FieldInfo[] piFieldInfo = dbA.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (FieldInfo fi in piFieldInfo)
            {
                if (fi.FieldType == typeof(Dictionary<int, Shop>))
                {
                    reflectedShopDict = (Dictionary<int, Shop>)fi.GetValue(dbA);
                    break;
                }
            }
            if (reflectedShopDict == null)
            {
                DebugLog($"ShopRefresh(): Unable to find reflected Dictionary<int, Shop>");
                return;
            }
            foreach (KeyValuePair<int, Shop> keyValuePair in reflectedShopDict)
            {
                if (keyValuePair.Value.limitedItems)
                {
                    try
                    {
                        ShopDatabaseAccessor.CreateNewShopList(keyValuePair.Value);
                    }
                    catch (Exception ex)
                    {
                        DebugLog("ShopRefresh(): Exception: " + ex.ToString());
                    }
                }
            }
        }
    }

}
