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
    internal class BuffedPeds
    {
        // ListShit
        private static readonly List<string> SCOList = new List<string>();
        public static readonly List<string> ModelList = new List<string>();
        public static readonly List<int> PedList = new List<int>();
        public static readonly List<int> SpecialPedList = new List<int>();
        private static readonly List<bool> RagdollList = new List<bool>();
        public static readonly List<int> WeaponList = new List<int>();
        private static readonly List<int> HealthList = new List<int>();
        private static readonly List<bool> ArmorList = new List<bool>();
        private static readonly List<int> OldWeaponList = new List<int>();
        private static readonly List<int> NewWeaponList = new List<int>();
        private static readonly List<int> WeapChanceList = new List<int>();
        private static readonly List<int> PedWeapList = new List<int>();
        public static readonly List<string> GXTList = new List<string>();

        private static bool buffIfHurt;

        // OtherShit
        private static string missionName;
        //private static string buffGXT;
        private static bool buffMissionPeds;
        private static bool giveBuffs;
        private static uint armorChance;
        private static uint healthIncrease;

        public static void Init(SettingsFile settings)
        {
            string SCOString = settings.GetValue("TWEAKED MISSION PEDS", "SCOList", "");

            SCOList.Clear();
            foreach (string SCOName in SCOString.Split(','))
            {
                if (!Main.scoSettings.DoesSectionExists(SCOName))
                    IVGame.Console.Print("~r~ERROR: Script name in MoreWantedStars SCOList does not have a section in SCOSettings.ini!");
                else
                    SCOList.Add(SCOName);
            }
        }
        private static void ClearLists()
        {
            ModelList.Clear();
            PedList.Clear();
            SpecialPedList.Clear();
            RagdollList.Clear();
            WeaponList.Clear();
            HealthList.Clear();
            ArmorList.Clear();
            OldWeaponList.Clear();
            NewWeaponList.Clear();
            WeapChanceList.Clear();
            PedWeapList.Clear();
            GXTList.Clear();
        }
        private static void LoadMissionData(SettingsFile settings, string scoName)
        {
            ClearLists();

            string pedString = settings.GetValue(scoName, "SpecialPedModels", "");
            foreach (var pedModel in pedString.Split(','))
                ModelList.Add(pedModel);

            string HealthString = settings.GetValue(scoName, "SpecialPedHealth", "0");
            foreach (var HealthValue in HealthString.Split(','))
            {
                int HealthAmount = Int32.Parse(HealthValue.Trim());
                HealthList.Add(HealthAmount);
            }

            string RagdollString = settings.GetValue(scoName, "SpecialPedNoRagdoll", "false");
            foreach (var RagdollValue in RagdollString.Split(','))
            {
                bool RagdollPed = Boolean.Parse(RagdollValue.Trim());
                RagdollList.Add(RagdollPed);
            }

            string WeapString = settings.GetValue(scoName, "SpecialPedWeaponReplacement", "-1");
            foreach (var WeaponValue in WeapString.Split(','))
            {
                int WeaponID = Int32.Parse(WeaponValue.Trim());
                WeaponList.Add(WeaponID);
            }

            string ArmorString = settings.GetValue(scoName, "SpecialPedArmor", "false");
            foreach (var GiveArmor in ArmorString.Split(','))
            {
                bool ArmoredPed = Boolean.Parse(GiveArmor.Trim());
                ArmorList.Add(ArmoredPed);
            }

            string BuffGXTs = settings.GetValue(scoName, "BuffSpecialPedGXT", "none");
            foreach (var gxts in BuffGXTs.Split(','))
                GXTList.Add(gxts);
            //buffGXT = settings.GetValue(scoName, "BuffSpecialPedGXT", "none");

            buffMissionPeds = settings.GetBoolean(scoName, "BuffAllOtherMissionPeds", false);

            string OldWeapString = settings.GetValue(scoName, "MissionPedWeaponToReplace", "-1");
            foreach (var WeaponValue in OldWeapString.Split(','))
            {
                int WeaponID = Int32.Parse(WeaponValue.Trim());
                OldWeaponList.Add(WeaponID);
            }

            string NewWeapString = settings.GetValue(scoName, "MissionPedWeaponReplacement", "-1");
            foreach (var WeaponValue in NewWeapString.Split(','))
            {
                int WeaponID = Int32.Parse(WeaponValue.Trim());
                NewWeaponList.Add(WeaponID);
            }

            string WeapChanceString = settings.GetValue(scoName, "WeaponReplaceChance", "-1");
            foreach (var WeaponValue in WeapChanceString.Split(','))
            {
                int WeaponChance = Int32.Parse(WeaponValue.Trim());
                WeapChanceList.Add(WeaponChance);
            }

            buffIfHurt = settings.GetBoolean(scoName, "SpecialPedApplyBuffUntilHurtByPlayer", false);
            healthIncrease = settings.GetUInteger(scoName, "MissionPedHealthIncrease", 0);
            armorChance = settings.GetUInteger(scoName, "MissionPedArmorChance", 0);
        }
        public static void Tick()
        {
            foreach (string MissionSCO in SCOList)
            {
                if (NativeGame.IsScriptRunning(MissionSCO))
                {
                    if (missionName != MissionSCO)
                    {
                        giveBuffs = false;
                        missionName = MissionSCO;
                        LoadMissionData(Main.scoSettings, MissionSCO);
                    }

                    foreach (string gxt in GXTList)
                    {
                        if (IS_THIS_PRINT_BEING_DISPLAYED(gxt, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0) || gxt == "none")
                            giveBuffs = true;
                    }

                    foreach (var ped in PedHelper.PedHandles)
                    {
                        int pedHandle = ped.Value;
                        if (!DOES_CHAR_EXIST(pedHandle))
                            continue;
                        if (!IS_PED_A_MISSION_PED(pedHandle))
                            continue;
                        if (pedHandle == Main.PlayerHandle)
                            continue;
                        if (IS_CHAR_INJURED(pedHandle))
                            continue;

                        GET_CHAR_MODEL(pedHandle, out int pModel);

                        foreach (string pedModel in ModelList)
                        {
                            if (pModel == GET_HASH_KEY(pedModel))
                            {
                                if (giveBuffs && !SpecialPedList.Contains(pedHandle))
                                {
                                    if (HealthList.Count == ModelList.Count && HealthList[ModelList.IndexOf(pedModel)] > 0)
                                    {
                                        SET_CHAR_MAX_HEALTH(pedHandle, (uint)(HealthList[ModelList.IndexOf(pedModel)]));
                                        SET_CHAR_HEALTH(pedHandle, (uint)(HealthList[ModelList.IndexOf(pedModel)]));
                                    }

                                    if (ArmorList.Count == ModelList.Count && ArmorList[ModelList.IndexOf(pedModel)])
                                    {
                                        GET_CHAR_ARMOUR(pedHandle, out uint pedArmor);
                                        if (pedArmor < 100)
                                            ADD_ARMOUR_TO_CHAR(pedHandle, 100);
                                    }

                                    if (RagdollList.Count == ModelList.Count && RagdollList[ModelList.IndexOf(pedModel)])
                                        UNLOCK_RAGDOLL(pedHandle, false);

                                    SpecialPedList.Add(pedHandle);
                                }
                            }
                            else if (buffMissionPeds && !PedList.Contains(pedHandle))
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

                    foreach (var ped in SpecialPedList)
                    {
                        if (!DOES_CHAR_EXIST(ped))
                            continue;

                        //IVGame.ShowSubtitleMessage(PedList.Count() + "  " + ModelList.Count() + "  " + HealthList.Count() + "  " + WeaponList.Count());
                        GET_CURRENT_CHAR_WEAPON(ped, out int pedWeap);
                        GET_CHAR_MODEL(ped, out int pModel);

                        foreach (string pedModel in ModelList)
                        {
                            if (pModel == GET_HASH_KEY(pedModel))
                            {
                                if (!HAS_CHAR_BEEN_DAMAGED_BY_CHAR(ped, Main.PlayerHandle, false) && buffIfHurt)
                                {
                                    if (HealthList.Count == ModelList.Count && HealthList[ModelList.IndexOf(pedModel)] > 0)
                                    {
                                        SET_CHAR_MAX_HEALTH(ped, (uint)(HealthList[ModelList.IndexOf(pedModel)]));
                                        SET_CHAR_HEALTH(ped, (uint)(HealthList[ModelList.IndexOf(pedModel)]));
                                    }

                                    if (ArmorList.Count == ModelList.Count && ArmorList[ModelList.IndexOf(pedModel)])
                                    {
                                        GET_CHAR_ARMOUR(ped, out uint pedArmor);
                                        if (pedArmor < 100)
                                            ADD_ARMOUR_TO_CHAR(ped, 100);
                                    }

                                    if (RagdollList.Count == ModelList.Count && RagdollList[ModelList.IndexOf(pedModel)])
                                        UNLOCK_RAGDOLL(ped, false);
                                }
                                if (pedWeap != WeaponList[ModelList.IndexOf(pedModel)] && WeaponList[ModelList.IndexOf(pedModel)] > -1)
                                    GIVE_WEAPON_TO_CHAR(ped, WeaponList[ModelList.IndexOf(pedModel)], 9999, false);
                            }
                        }
                    }
                    foreach (var ped in PedList)
                    {
                        if (!DOES_CHAR_EXIST(ped))
                            continue;
                        if (PedWeapList.Contains(ped))
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
                                        int randInt = GENERATE_RANDOM_INT_IN_RANGE(0, 100);

                                        if (WeapChanceList[OldWeaponList.IndexOf(oldWeap)] > randInt)
                                        {
                                            GIVE_WEAPON_TO_CHAR(ped, NewWeaponList[OldWeaponList.IndexOf(oldWeap)], 999, false);
                                            //if (IVWeaponInfo.GetWeaponInfo((uint)NewWeaponList[OldWeaponList.IndexOf(oldWeap)]).WeaponSlot <= 1)
                                            REMOVE_WEAPON_FROM_CHAR(ped, pedWeap);
                                        }
                                        PedWeapList.Add(ped);
                                    }
                                }
                            }
                        }
                    }
                }
                else if (missionName == MissionSCO)
                {
                    ClearLists();

                    giveBuffs = false;
                    missionName = "";
                }
            }
        }
    }
}
