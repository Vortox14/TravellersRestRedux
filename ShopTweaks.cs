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
        // Refresh Every Day 

        [HarmonyPatch(typeof(ShopDatabaseAccessor), "Awake")]
        [HarmonyPostfix]
        private static void ShopDatabaseAccessorAwakePostfix()
        {
            if (!(_shopUpdateDaily.Value || _shopAllItems.Value || _shopMoreItems.Value)) return; //To avoid the pause from getting the full shop list

            List<Shop> allShops = ShopDatabaseAccessor.GetAllShops();
            DebugLog($"ShopDatabaseAccessorAwakePostfix(): {allShops.Count()} shops found");

            //Iterate through each shop and make changes (will only take effect next inventory update)
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
            FieldInfo[] piFieldInfo = dbA.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance); //all private fields.
            foreach (FieldInfo fi in piFieldInfo) // now look for the one of type Slot[]
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
