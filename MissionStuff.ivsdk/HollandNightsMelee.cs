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
    internal class HollandNightsMelee
    {
        private static readonly List<string> SCOList = new List<string>();
        public static readonly List<string> ModelList = new List<string>();
        public static readonly List<int> PedList = new List<int>();
        private static readonly List<int> OldWeaponList = new List<int>();
        private static readonly List<int> NewWeaponList = new List<int>();

        private static string missionName;
        private static uint armorChance;
        private static uint healthIncrease;

        public static void Init(SettingsFile settings)
        {
            string SCOString = settings.GetValue("MAIN", "BuffEnemiesSCOList", "");

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

            string OldWeapString = settings.GetValue(scoName, "EnemyWeaponToReplace", "-1");

            OldWeaponList.Clear();
            foreach (var WeaponValue in OldWeapString.Split(','))
            {
                int WeaponID = Int32.Parse(WeaponValue.Trim());
                OldWeaponList.Add(WeaponID);
            }

            string NewWeapString = settings.GetValue(scoName, "EnemyWeaponReplacement", "-1");

            NewWeaponList.Clear();
            foreach (var WeaponValue in NewWeapString.Split(','))
            {
                int WeaponID = Int32.Parse(WeaponValue.Trim());
                NewWeaponList.Add(WeaponID);
            }

            armorChance = settings.GetUInteger(scoName, "EnemiesArmorChance", 0);
            healthIncrease = settings.GetUInteger(scoName, "EnemyHealthIncrease", 0);
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
                        if (IS_CHAR_INJURED(pedHandle))
                            continue;
                        if (PedList.Contains(pedHandle))
                            continue;

                        GET_CHAR_MODEL(pedHandle, out uint pModel);
                        foreach (string pedModel in ModelList)
                        {
                            if (pModel == GET_HASH_KEY(pedModel))
                                continue;

                            if (!PedList.Contains(pedHandle))
                            {
                                PedList.Add(pedHandle);

                                GET_CHAR_HEALTH(pedHandle, out uint pHealth);
                                SET_CHAR_MAX_HEALTH(pedHandle, (uint)(pHealth + healthIncrease));
                                SET_CHAR_HEALTH(pedHandle, (uint)(pHealth + healthIncrease));

                                if (GENERATE_RANDOM_INT_IN_RANGE(0, 100) < armorChance)
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
                                        //REMOVE_ALL_CHAR_WEAPONS(ped);
                                        //GIVE_DELAYED_WEAPON_TO_CHAR(ped, NewWeaponList[OldWeaponList.IndexOf(oldWeap)], 100, false);
                                        GIVE_WEAPON_TO_CHAR(ped, NewWeaponList[OldWeaponList.IndexOf(oldWeap)], 999, false);
                                        if (NewWeaponList[OldWeaponList.IndexOf(oldWeap)] < 4)
                                            REMOVE_WEAPON_FROM_CHAR(ped, pedWeap);
                                    }
                                }
                            }
                        }

                        /*if (HAS_CHAR_BEEN_DAMAGED_BY_WEAPON(ped, 57))
                            continue;

                        GET_CHAR_HEALTH(ped, out uint pHealth);
                        SET_CHAR_MAX_HEALTH(ped, (uint)(pHealth + healthIncrease));
                        SET_CHAR_HEALTH(ped, (uint)(pHealth + healthIncrease));*/
                    }
                }
                else if (missionName == MissionSCO)
                {
                    PedList.Clear();
                    armorChance = 0;
                    missionName = "";
                }
            }
        }
    }
}
