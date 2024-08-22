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
        // Staff Generation Stuff

        // Default values:
        // reputation, prob1Perk, prob2Perk, lvlRangePerk1, lvlRangePerk2, lvlRangePerk3
        // 7, 80, 20, (1, 2), (1, 2), (0, 0)
        // 10, 60, 40, (1, 3), (2, 4), (0, 0)
        // 13, 25, 60, (2, 5), (3, 5), (3, 5)
        // 16, 0, 80, (0, 0), (4, 7), (5, 7)
        // 19, 0, 70, (0, 0), (5, 8), (7, 10)
        // 21, 0, 50, (0, 0), (6, 9), (9, 11)
        // 24, 0, 40, (0, 0), (7, 10), (10, 15)


        [HarmonyPatch(typeof(StaffManager), "Awake")]
        [HarmonyPostfix]
        private static void StaffManagerAwakePostfix()
        {
            if (setupDoneStaffManager) return;
            DebugLog("StaffManager.Awake.Postfix");
            if (_dumpStaffGenDataOnStart.Value) Log.LogInfo("id, reputation, prob1Perk, prob2Perk, lvlRangePerk1, lvlRangePerk2, lvlRangePerk3");
            StaffManager s = StaffManager.GetInstance();
            StaffManager.StaffGenerationValues[] q = s.staffGenerationTable;
            for (int i = 0; i < q.Length; i++)
            {
                if (_staffAlways3Perks.Value)
                {
                    q[i].prob1Perk = 0; q[i].prob2Perk = 0;
                }
                if (_staffLevel.Value >= 0)
                {
                    q[i].lvlRangePerk1.x = _staffLevel.Value; q[i].lvlRangePerk1.y = _staffLevel.Value;
                    q[i].lvlRangePerk2.x = _staffLevel.Value; q[i].lvlRangePerk2.y = _staffLevel.Value;
                    q[i].lvlRangePerk3.x = _staffLevel.Value; q[i].lvlRangePerk3.y = _staffLevel.Value;
                }
                if (_dumpStaffGenDataOnStart.Value) Log.LogInfo(string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}", i, q[i].reputation, q[i].prob1Perk, q[i].prob2Perk, q[i].lvlRangePerk1, q[i].lvlRangePerk2, q[i].lvlRangePerk3));
            }
            setupDoneStaffManager = true;
        }


        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // Refresh Staff on every HireUI open

        [HarmonyPatch(typeof(HireStaffUI), "OpenUI")]
        [HarmonyPrefix]
        private static void HireStaffUIOpenUIPrefix()
        {
            if (_staffRefreshOnOpen.Value)
            {
                StaffManager.CreateRandomOptionsWorkers();
            }

        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // No Negative Perks on new Staff

        [HarmonyPatch(typeof(StaffManager), "CreateRandomOptionsWorkers")]
        [HarmonyPostfix]
        private static void StaffManagerCreateRandomOptionsWorkersPostFix()
        {
            DebugLog("StaffManager.CreateRandomOptionsWorkers.PostFix");
            StaffManager s = StaffManager.GetInstance();
            if (_staffNoNeg.Value)
            {
                foreach (EmployeeInfo w in s.barworkerOptions) w.perksInfo.RemoveAt(w.perksInfo.Count - 1);
                foreach (EmployeeInfo x in s.bouncerOptions) x.perksInfo.RemoveAt(x.perksInfo.Count - 1);
                foreach (EmployeeInfo y in s.waiterOptions) y.perksInfo.RemoveAt(y.perksInfo.Count - 1);
                foreach (EmployeeInfo z in s.houseKeeperOptions) z.perksInfo.RemoveAt(z.perksInfo.Count - 1);

            }

        }







    }





}
