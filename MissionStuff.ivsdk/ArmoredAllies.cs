using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CCL;
using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using IVSDKDotNet.Native;
using static IVSDKDotNet.Native.Natives;

namespace MissionStuff.ivsdk
{
    internal class ArmoredAllies
    {
        // ListShit
        private static readonly List<string> SCOList = new List<string>();
        public static readonly List<string> ModelList = new List<string>();
        public static readonly List<int> PedList = new List<int>();
        private static readonly List<int> HealthList = new List<int>();

        // OtherShit
        private static string missionName;
        private static bool gotList;

        public static void Init(SettingsFile settings)
        {
            string SCOString = settings.GetValue("MAIN", "BuffedSCOList", "");

            SCOList.Clear();
            foreach (string SCOName in SCOString.Split(','))
            {
                SCOList.Add(SCOName);
            }
        }
        private static void LoadHealthData(SettingsFile settings, string scoName)
        {
            if (!gotList)
            {
                string pedString = settings.GetValue(scoName, "FriendlyModels", "");

                ModelList.Clear();
                foreach (var pedModel in pedString.Split(','))
                    ModelList.Add(pedModel);

                string HealthString = settings.GetValue(scoName, "HealthList", "");

                HealthList.Clear();
                foreach (var HealthValue in HealthString.Split(','))
                {
                    int HealthAmount = Int32.Parse(HealthValue.Trim());
                    HealthList.Add(HealthAmount);
                }
                gotList = true;
            }
        }
        public static void Tick()
        {
            foreach (string MissionSCO in SCOList)
            {
                if (NativeGame.IsScriptRunning(MissionSCO))
                {
                    missionName = MissionSCO;
                    LoadHealthData(Main.mainSettings, MissionSCO);
                    foreach (var ped in PedHelper.PedHandles)
                    {
                        int pedHandle = ped.Value;
                        if (!IS_PED_A_MISSION_PED(pedHandle))
                            continue;
                        if (pedHandle == Main.PlayerHandle)
                            continue;
                        if (HAS_CHAR_BEEN_DAMAGED_BY_WEAPON(pedHandle, 57))
                            continue;
                        if (PedList.Contains(pedHandle))
                            continue;

                        GET_CHAR_MODEL(pedHandle, out int pModel);

                        foreach (string pedModel in ModelList)
                        {
                            if (pModel == GET_HASH_KEY(pedModel))
                                PedList.Add(pedHandle);
                        }
                    }

                    foreach (var ped in PedList)
                    {
                        if (HAS_CHAR_BEEN_DAMAGED_BY_WEAPON(ped, 57))
                            continue;

                        GET_CHAR_HEALTH(ped, out uint pHealth);
                        SET_CHAR_HEALTH(ped, (uint)(pHealth + HealthList[SCOList.IndexOf(MissionSCO)]));
                        ADD_ARMOUR_TO_CHAR(ped, 100);
                    }
                }
                else if (missionName == MissionSCO)
                {
                    gotList = false;
                    PedList.Clear();
                }
            }
        }
    }
}
