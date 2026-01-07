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
        public static readonly List<int> WeaponList = new List<int>();
        private static readonly List<int> HealthList = new List<int>();

        // OtherShit
        private static string missionName;
        private static bool giveArmor;

        public static void Init(SettingsFile settings)
        {
            string SCOString = settings.GetValue("MAIN", "BuffAlliesSCOList", "");

            SCOList.Clear();
            foreach (string SCOName in SCOString.Split(','))
            {
                SCOList.Add(SCOName);
            }
        }
        private static void LoadHealthData(SettingsFile settings, string scoName)
        {
            string pedString = settings.GetValue(scoName, "FriendlyModels", "");

            ModelList.Clear();
            foreach (var pedModel in pedString.Split(','))
                ModelList.Add(pedModel);

            string HealthString = settings.GetValue(scoName, "AllyHealthIncrease", "");

            HealthList.Clear();
            foreach (var HealthValue in HealthString.Split(','))
            {
                int HealthAmount = Int32.Parse(HealthValue.Trim());
                HealthList.Add(HealthAmount);
            }

            string WeapString = settings.GetValue(scoName, "AllyWeaponReplacement", "");

            WeaponList.Clear();
            foreach (var WeaponValue in WeapString.Split(','))
            {
                int WeaponID = Int32.Parse(WeaponValue.Trim());
                WeaponList.Add(WeaponID);
            }

            giveArmor = settings.GetBoolean(scoName, "AlliesHaveArmor", false);
        }
        public static void Tick()
        {
            foreach (string MissionSCO in SCOList)
            {
                if (NativeGame.IsScriptRunning(MissionSCO))
                {
                    if (missionName != MissionSCO)
                    {
                        missionName = MissionSCO;
                        LoadHealthData(Main.mainSettings, MissionSCO);
                    }
                    foreach (var ped in PedHelper.PedHandles)
                    {
                        int pedHandle = ped.Value;
                        if (!IS_PED_A_MISSION_PED(pedHandle))
                            continue;
                        if (pedHandle == Main.PlayerHandle)
                            continue;
                        if (IS_CHAR_INJURED(pedHandle))
                            continue;
                        if (PedList.Contains(pedHandle))
                            continue;

                        foreach (string pedModel in ModelList)
                        {
                            GET_CHAR_MODEL(pedHandle, out int pModel);

                            if (pModel == GET_HASH_KEY(pedModel))
                            {
                                PedList.Add(pedHandle);

                                if (HealthList[ModelList.IndexOf(pedModel)] > 0)
                                {
                                    GET_CHAR_HEALTH(pedHandle, out uint pHealth);
                                    SET_CHAR_MAX_HEALTH(pedHandle, (uint)(pHealth + HealthList[ModelList.IndexOf(pedModel)]));
                                    SET_CHAR_HEALTH(pedHandle, (uint)(pHealth + HealthList[ModelList.IndexOf(pedModel)]));
                                }

                                if (giveArmor)
                                {
                                    GET_CHAR_ARMOUR(pedHandle, out uint pedArmor);
                                    if (pedArmor < 100)
                                        ADD_ARMOUR_TO_CHAR(pedHandle, 100);
                                }
                            }
                        }
                    }

                    foreach (var ped in PedList)
                    {
                        /*if (!DOES_CHAR_EXIST(ped))
                            PedList.Remove(ped);
                        if (ModelList.Count() != PedList.Count)*/
                        if (!DOES_CHAR_EXIST(ped))
                            continue;

                        //IVGame.ShowSubtitleMessage(PedList.Count() + "  " + ModelList.Count() + "  " + HealthList.Count() + "  " + WeaponList.Count());
                        GET_CURRENT_CHAR_WEAPON(ped, out int pedWeap);
                        GET_CHAR_MODEL(ped, out int pModel);

                        foreach (string pedModel in ModelList)
                        {
                            if (pModel == GET_HASH_KEY(pedModel))
                            {
                                if (pedWeap != WeaponList[ModelList.IndexOf(pedModel)] && WeaponList[ModelList.IndexOf(pedModel)] > -1)
                                    GIVE_WEAPON_TO_CHAR(ped, WeaponList[ModelList.IndexOf(pedModel)], 9999, false);

                                /*if (HAS_CHAR_BEEN_DAMAGED_BY_WEAPON(ped, 57))
                                    continue;

                                if (HealthList[ModelList.IndexOf(pedModel)] > 0)
                                {
                                    GET_CHAR_HEALTH(ped, out uint pHealth);
                                    SET_CHAR_MAX_HEALTH(ped, (uint)(pHealth + HealthList[ModelList.IndexOf(pedModel)]));
                                    SET_CHAR_HEALTH(ped, (uint)(pHealth + HealthList[ModelList.IndexOf(pedModel)]));
                                }*/
                            }
                        }
                    }
                }
                else if (missionName == MissionSCO)
                {
                    PedList.Clear();
                    missionName = "";
                }
            }
        }
    }
}
