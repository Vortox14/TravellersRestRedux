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


namespace RestfulTweaks
{
    public partial class Plugin : BaseUnityPlugin
    {
        // /////////////////////////////////
        // Dont mess up rooms after sleeping in them!

        [HarmonyPatch(typeof(RentedRoom), "MessUpRoom")]
        [HarmonyPrefix]
        public static bool RentedRoomMessUpRoomPrefix()
        {
            return (!(Plugin._custCleanRooms.Value));
        }

        // /////////////////////////////////
        // Tables ignore attempts to make them messy

        [HarmonyPatch(typeof(Table), "AddDirtiness")]
        [HarmonyPrefix]
        public static bool TableAddDirtinessPrefix()
        {
            return (!(Plugin._custCleanTable.Value));
        }



        public static void LogCustomerInfo(Customer x)
        {
            Plugin.DebugLog($"CustomerInfo: floorDirtProbability -> {x.customerInfo.floorDirtProbability}");
            Plugin.DebugLog($"CustomerInfo: rowdyCustomersProbability -> {x.customerInfo.rowdyCustomersProbability}");
            Plugin.DebugLog($"CustomerInfo: calmRowdyCustomersProbability -> {x.customerInfo.calmRowdyCustomersProbability}");
            Plugin.DebugLog($"CustomerInfo: requestOrderPatience -> {x.customerInfo.requestOrderPatience}");
            Plugin.DebugLog($"CustomerInfo: requestRoomPatience -> {x.customerInfo.requestRoomPatience}");
            Plugin.DebugLog($"CustomerInfo: requestAgainProbability -> {x.customerInfo.requestAgainProbability}");
            Plugin.DebugLog($"CustomerInfo: timeEatingMin -> {x.customerInfo.timeEatingMin}");
            Plugin.DebugLog($"CustomerInfo: timeEatingMax -> {x.customerInfo.timeEatingMax}");
            Plugin.DebugLog($"CustomerInfo: timeEatingMin -> {x.customerInfo.timeEatingLastOrdersMin}");
            Plugin.DebugLog($"CustomerInfo: timeEatingMax -> {x.customerInfo.timeEatingLastOrdersMax}");
        }


        // /////////////////////////////////
        // Assorted customer tweaks

        [HarmonyPatch(typeof(Customer), "Awake")]
        [HarmonyPostfix]
        public static void CustomerAwakePostfix(Customer __instance)
        {
            //Changing CustomerInfo changes it for *every* customer, so needs a check or static base values
            if (!setupDoneCustomerInfo)
            {
                Plugin.DebugLog("CustomerAwakePostfix(): ------ Pre change data -----");
                LogCustomerInfo(__instance);
                if (Plugin._custCleanFloor.Value)
                {
                    __instance.customerInfo.floorDirtProbability = 0;
                }
                if (Plugin._custNeverAngry.Value)
                {
                    __instance.customerInfo.rowdyCustomersProbability = 0;
                }
                if (Plugin._custCanCalm.Value)
                {
                    __instance.customerInfo.calmRowdyCustomersProbability = 100;
                }
                if (Plugin._custMorePatient.Value)
                {
                    __instance.customerInfo.requestOrderPatience = 100;
                    __instance.customerInfo.requestRoomPatience = 100;
                }
                if (Plugin._custNeverLeave.Value)
                {
                    __instance.customerInfo.requestAgainProbability = 100;
                }

                if (Plugin._custFastEating.Value != 1.0f)
                {
                    //Gets weird if set to zero.  
                    __instance.customerInfo.timeEatingMin = Math.Max(1, Mathf.FloorToInt(__instance.customerInfo.timeEatingMin / Plugin._custFastEating.Value));
                    __instance.customerInfo.timeEatingMax = Math.Max(1, Mathf.FloorToInt(__instance.customerInfo.timeEatingMax / Plugin._custFastEating.Value));
                    __instance.customerInfo.timeEatingLastOrdersMin = Math.Max(1, Mathf.FloorToInt(__instance.customerInfo.timeEatingMin / Plugin._custFastEating.Value));
                    __instance.customerInfo.timeEatingLastOrdersMax = Math.Max(1, Mathf.FloorToInt(__instance.customerInfo.timeEatingMax / Plugin._custFastEating.Value));
                }
                Plugin.DebugLog("CustomerAwakePostfix(): ------ Post change data -----");
                LogCustomerInfo(__instance);
                Plugin.DebugLog("CustomerAwakePostfix(): -----------------------------");
                setupDoneCustomerInfo = true;
            }
        }
    }
}

