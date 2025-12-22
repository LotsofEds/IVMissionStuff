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
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static IVSDKDotNet.Native.Natives;

namespace MissionStuff.ivsdk
{
    internal class DeathAndTaxes
    {
        // IniShit
        private static bool reduceHealth;

        private static int billAmount;
        private static int healthReduction;
        private static int reductionTime;
        private static int minPlayerHeath;

        // OtherShit
        private static bool hasDied;
        private static uint pMoney;
        private static int pMaxHealth;
        private static int pHealth;
        private static uint fTimer;
        public static void Init(SettingsFile settings)
        {
            reduceHealth = settings.GetBoolean("MAIN", "NoFreeHealthcare", false);

            billAmount = settings.GetInteger("MAIN", "HospitalBillAmount", 10000);
            healthReduction = settings.GetInteger("MAIN", "HealthReductionPerDeath", 10);
            reductionTime = settings.GetInteger("MAIN", "HealthReductionTime", 720000);
            minPlayerHeath = settings.GetInteger("MAIN", "MinPlayerHealth", 20);
        }
        public static void Tick()
        {
            if (IS_CHAR_DEAD(Main.PlayerHandle) && IS_SCREEN_FADING_OUT())
            {
                STORE_SCORE(Main.PlayerIndex, out pMoney);
                hasDied = true;
            }
            else if (hasDied && IS_SCREEN_FADING_IN() && !IS_CHAR_DEAD(Main.PlayerHandle))
            {
                STORE_SCORE(Main.PlayerIndex, out uint currMoney);
                if (currMoney < billAmount)
                {
                    GET_PLAYER_MAX_HEALTH(Main.PlayerIndex, out pMaxHealth);
                    if ((pMaxHealth - healthReduction) > minPlayerHeath)
                        INCREASE_PLAYER_MAX_HEALTH(Main.PlayerIndex, (- healthReduction));
                    pHealth = (pMaxHealth - healthReduction);
                }
                ADD_SCORE(Main.PlayerIndex, (int)(-currMoney));
                ADD_SCORE(Main.PlayerIndex, (int)pMoney);
                ADD_SCORE(Main.PlayerIndex, -10000);
                hasDied = false;
            }
            if (!IS_CHAR_DEAD(Main.PlayerHandle) && pHealth < pMaxHealth)
                INCREASE_PLAYER_MAX_HEALTH(Main.PlayerIndex, (-healthReduction));
        }
    }
}
