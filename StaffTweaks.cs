using BepInEx;
using HarmonyLib;

namespace RestfulTweaks
{
    public partial class Plugin : BaseUnityPlugin
    {
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

        [HarmonyPatch(typeof(HireStaffUI), "OpenUI")]
        [HarmonyPrefix]
        private static void HireStaffUIOpenUIPrefix()
        {
            if (_staffRefreshOnOpen.Value)
            {
                StaffManager.CreateRandomOptionsWorkers();
            }

        }

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
