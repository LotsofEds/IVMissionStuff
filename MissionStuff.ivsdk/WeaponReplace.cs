using CCL;
using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using IVSDKDotNet.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static IVSDKDotNet.Native.Natives;

namespace MissionStuff.ivsdk
{
    internal class WeaponReplace
    {
        private static readonly List<string> SCOList = new List<string>();
        public static readonly List<string> ModelList = new List<string>();
        public static readonly List<int> PedList = new List<int>();
        private static readonly List<int> OldWeaponList = new List<int>();
        private static readonly List<int> NewWeaponList = new List<int>();

        private static string missionName;
        private static bool giveArmor;

        public static void Init(SettingsFile settings)
        {
            string SCOString = settings.GetValue("MAIN", "WeaponReplaceSCOList", "");

            SCOList.Clear();
            foreach (string SCOName in SCOString.Split(','))
            {
                SCOList.Add(SCOName);
            }
        }
        private static void LoadWeaponData(SettingsFile settings, string scoName)
        {
            string pedString = settings.GetValue(scoName, "FriendlyModels", "");

            ModelList.Clear();
            foreach (var pedModel in pedString.Split(','))
                ModelList.Add(pedModel);

            string OldWeapString = settings.GetValue(scoName, "EnemyWeaponToReplace", "");

            OldWeaponList.Clear();
            foreach (var WeaponValue in OldWeapString.Split(','))
            {
                int WeaponID = Int32.Parse(WeaponValue.Trim());
                OldWeaponList.Add(WeaponID);
            }

            string NewWeapString = settings.GetValue(scoName, "EnemyWeaponReplacement", "");

            NewWeaponList.Clear();
            foreach (var WeaponValue in NewWeapString.Split(','))
            {
                int WeaponID = Int32.Parse(WeaponValue.Trim());
                NewWeaponList.Add(WeaponID);
            }

            giveArmor = settings.GetBoolean(scoName, "EnemiesHaveArmor", false);
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
                        LoadWeaponData(Main.mainSettings, MissionSCO);
                    }

                    foreach (var ped in PedHelper.PedHandles)
                    {
                        int pedHandle = ped.Value;
                        if (!IS_PED_A_MISSION_PED(pedHandle))
                            continue;
                        if (pedHandle == Main.PlayerHandle)
                            continue;
                        if (PedList.Contains(pedHandle))
                            continue;

                        GET_CHAR_MODEL(pedHandle, out uint pModel);
                        foreach (string pedModel in ModelList)
                        {
                            if (pModel == GET_HASH_KEY(pedModel))
                                continue;

                            PedList.Add(pedHandle);

                        }
                    }

                    foreach (var ped in PedList)
                    {
                        if (!DOES_CHAR_EXIST(ped))
                            continue;

                        GET_CURRENT_CHAR_WEAPON(ped, out int pedWeap);

                        if (pedWeap > 0 && (pedWeap < 46 || pedWeap > 57))
                        {
                            foreach (var oldWeap in OldWeaponList)
                            {
                                if (OldWeaponList.Contains(pedWeap))
                                {
                                    if (pedWeap == OldWeaponList[OldWeaponList.IndexOf(oldWeap)])
                                    {
                                        //REMOVE_WEAPON_FROM_CHAR(ped, pedWeap);
                                        //REMOVE_ALL_CHAR_WEAPONS(ped);
                                        //GIVE_DELAYED_WEAPON_TO_CHAR(ped, NewWeaponList[OldWeaponList.IndexOf(oldWeap)], 100, false);
                                        GIVE_WEAPON_TO_CHAR(ped, NewWeaponList[OldWeaponList.IndexOf(oldWeap)], 999, false);
                                    }
                                }
                            }
                        }

                        if (HAS_CHAR_BEEN_DAMAGED_BY_WEAPON(ped, 57))
                            continue;

                        GET_CHAR_ARMOUR(ped, out uint pedArmor);

                        if (giveArmor && pedArmor < 100)
                            ADD_ARMOUR_TO_CHAR(ped, 100);
                    }
                }
                else if (missionName == MissionSCO)
                {
                    PedList.Clear();
                    giveArmor = false;
                    missionName = "";
                }
            }
        }
    }
}
