using CCL;
using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using IVSDKDotNet.Native;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static IVSDKDotNet.Native.Natives;

namespace MissionStuff.ivsdk
{
    internal class DeathAndTaxes
    {
        // IniShit
        public static bool reduceHealth;
        private static bool romanRescue;

        private static int billAmountNormal;
        private static int billAmountRecession;
        private static int healthReduction;
        private static int reductionTime;
        private static int minPlayerHeath;

        private static uint cachedGameTimer;

        // OtherShit
        private static int billAmount;
        private static bool loadSaveData;
        private static int timesDied;
        private static bool hasDied;
        private static uint pMoney;
        private static int pMaxHealth;
        private static int pHealth;
        private static uint currHealth;
        private static uint fTimer;
        public static void Init(SettingsFile settings)
        {
            reduceHealth = settings.GetBoolean("REALISTIC U.S. HEALTHCARE", "NoFreeHealthcare", false);
            romanRescue = settings.GetBoolean("REALISTIC U.S. HEALTHCARE", "RomanFreebie", false);

            billAmountNormal = settings.GetInteger("REALISTIC U.S. HEALTHCARE", "HospitalBillAmount", 5000);
            if (Main.recessionEnable)
                billAmountRecession = settings.GetInteger("2008 RECESSION SIMULATOR", "HospitalBillAmount", 7500);

            healthReduction = settings.GetInteger("REALISTIC U.S. HEALTHCARE", "MaxHealthReductionPerDeath", 10);
            reductionTime = settings.GetInteger("REALISTIC U.S. HEALTHCARE", "HealthReductionTime", 720000);
            minPlayerHeath = settings.GetInteger("REALISTIC U.S. HEALTHCARE", "MinPlayerHealth", 120);
        }
        private static void GetCachedSaveData(SettingsFile settings)
        {
            if (!settings.DoesSectionExists(IVGenericGameStorage.ValidSaveName))
                settings.AddSection(IVGenericGameStorage.ValidSaveName);
            if (!settings.DoesKeyExists(IVGenericGameStorage.ValidSaveName, "GameTimer"))
                settings.AddKeyToSection(IVGenericGameStorage.ValidSaveName, "GameTimer");
            if (!settings.DoesKeyExists(IVGenericGameStorage.ValidSaveName, "HealthReductionMult"))
                settings.AddKeyToSection(IVGenericGameStorage.ValidSaveName, "HealthReductionMult");

            cachedGameTimer = settings.GetUInteger(IVGenericGameStorage.ValidSaveName, "GameTimer", 0);
            timesDied = settings.GetInteger(IVGenericGameStorage.ValidSaveName, "HealthReductionMult", 0);
        }
        public static void SetSaveData(SettingsFile settings)
        {
            if (!settings.DoesSectionExists(IVGenericGameStorage.ValidSaveName))
                settings.AddSection(IVGenericGameStorage.ValidSaveName);
            if (!settings.DoesKeyExists(IVGenericGameStorage.ValidSaveName, "GameTimer"))
                settings.AddKeyToSection(IVGenericGameStorage.ValidSaveName, "GameTimer");
            if (!settings.DoesKeyExists(IVGenericGameStorage.ValidSaveName, "HealthReductionMult"))
                settings.AddKeyToSection(IVGenericGameStorage.ValidSaveName, "HealthReductionMult");

            settings.SetUInteger(IVGenericGameStorage.ValidSaveName, "GameTimer", fTimer);
            settings.SetInteger(IVGenericGameStorage.ValidSaveName, "HealthReductionMult", timesDied);
        }
        public static void IngameStart()
        {
            loadSaveData = false;
        }
        public static void Tick()
        {
            if (!loadSaveData)
            {
                GetCachedSaveData(Main.savefileSettings);
                fTimer = cachedGameTimer;
                pHealth = 200 - healthReduction * timesDied;
                loadSaveData = true;
            }

            if (IS_CHAR_DEAD(Main.PlayerHandle) && IS_SCREEN_FADING_OUT())
            {
                if (GET_FLOAT_STAT(22) < 80 || !Main.recessionEnable)
                    billAmount = billAmountNormal;
                else
                    billAmount = billAmountRecession;

                STORE_SCORE(Main.PlayerIndex, out pMoney);
                hasDied = true;
            }
            else if (hasDied && IS_SCREEN_FADING_IN() && !IS_CHAR_DEAD(Main.PlayerHandle))
            {
                STORE_SCORE(Main.PlayerIndex, out uint currMoney);
                ADD_SCORE(Main.PlayerIndex, (int)(-currMoney));
                ADD_SCORE(Main.PlayerIndex, (int)pMoney);

                if (currMoney >= billAmount || (romanRescue && GET_INT_STAT(261) <= 1))
                {
                    if (!romanRescue || GET_INT_STAT(261) > 1)
                        ADD_SCORE(Main.PlayerIndex, -billAmount);
                }

                else if (reduceHealth)
                {
                    GET_GAME_TIMER(out fTimer);
                    timesDied++;
                    GET_PLAYER_MAX_HEALTH(Main.PlayerIndex, out pMaxHealth);
                    pHealth = (pMaxHealth - healthReduction * timesDied);
                }
                hasDied = false;
            }
            if (!IS_CHAR_DEAD(Main.PlayerHandle) && reduceHealth && pHealth < pMaxHealth)
            {
                GET_CHAR_HEALTH(Main.PlayerHandle, out currHealth);
                if (currHealth > pHealth)
                {
                    if (pHealth > minPlayerHeath)
                        INCREASE_PLAYER_MAX_HEALTH(Main.PlayerIndex, (-healthReduction * timesDied));
                    else
                        INCREASE_PLAYER_MAX_HEALTH(Main.PlayerIndex, (minPlayerHeath - 200));
                }

                if (Main.gTimer > fTimer + reductionTime)
                {
                    GET_GAME_TIMER(out fTimer);
                    timesDied--;
                }
            }
        }
    }
}
