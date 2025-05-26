using BepInEx;
using HarmonyLib;
using System;
using UnityEngine;

namespace RestfulTweaks
{
    public partial class Plugin : BaseUnityPlugin
    {
        [HarmonyPatch(typeof(RentedRoom), "CleanRoom")]
        [HarmonyPatch(new Type[] { typeof(bool) })]
        [HarmonyPrefix]
        public static bool RentedRoomCleanRoomPrefix()
        {
            return !Plugin._custCleanRooms.Value;
        }

        [HarmonyPatch(typeof(Table), "AddDirtiness")]
        [HarmonyPatch(new Type[] { typeof(float), typeof(bool), typeof(bool) })]
        [HarmonyPrefix]
        public static bool TableAddDirtinessPrefix()
        {
            return !Plugin._custCleanTable.Value;
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
            Plugin.DebugLog($"CustomerInfo: timeEatingLastOrdersMin -> {x.customerInfo.timeEatingLastOrdersMin}");
            Plugin.DebugLog($"CustomerInfo: timeEatingLastOrdersMax -> {x.customerInfo.timeEatingLastOrdersMax}");
            Plugin.DebugLog($"CustomerInfo: tableDirtyPenalty -> {x.customerInfo.tableDirtyPenalty}");
            Plugin.DebugLog($"CustomerInfo: tableVeryDirtyPenalty -> {x.customerInfo.tableVeryDirtyPenalty}");
            Plugin.DebugLog($"CustomerInfo: floorDirtPenalty -> {x.customerInfo.floorDirtPenalty}");
            Plugin.DebugLog($"CustomerInfo: tavernDirty -> {x.customerInfo.tavernDirty}");
            Plugin.DebugLog($"CustomerInfo: tavernFilthy -> {x.customerInfo.tavernFilthy}");
            Plugin.DebugLog($"CustomerInfo: tavernDisgusting -> {x.customerInfo.tavernDisgusting}");
            Plugin.DebugLog($"CustomerInfo: temperaturePenalty -> {x.customerInfo.temperaturePenalty}");
            Plugin.DebugLog($"CustomerInfo: notEnoughLightEvery10secs -> {x.customerInfo.notEnoughLightEvery10secs}");
        }

        [HarmonyPatch(typeof(Customer), "Awake")]
        [HarmonyPostfix]
        public static void CustomerAwakePostfix(Customer __instance)
        {
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
                if (Plugin._custAlwaysLeave.Value)
                {
                    __instance.customerInfo.requestAgainProbability = 0;
                }
                if (Plugin._custIgnoreDirt.Value)
                {
                    __instance.customerInfo.tableDirtyPenalty = 0.0f;
                    __instance.customerInfo.tableVeryDirtyPenalty = 0.0f;
                    __instance.customerInfo.floorDirtPenalty = 0.0f;
                    __instance.customerInfo.tavernDirty = 0.0f;
                    __instance.customerInfo.tavernFilthy = 0.0f;
                    __instance.customerInfo.tavernDisgusting = 0.0f;
                    __instance.customerInfo.temperaturePenalty = 0.0f;
                    __instance.customerInfo.notEnoughLightEvery10secs = 0.0f;
                }
                if (Plugin._custFastEating.Value != 1.0f)
                {
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

