using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime;
using System.Windows.Forms;
using static IVSDKDotNet.Native.Natives;

namespace MissionStuff.ivsdk
{
    public class Main : Script
    {
        // PlayerStuff
        public static IVPed PlayerPed { get; set; }
        public static int PlayerIndex { get; set; }
        public static int PlayerHandle { get; set; }
        public static Vector3 PlayerPos { get; set; }

        // IniBooleShit
        public static bool tripSkipEnable;
        public static bool buffedPedsEnable;
        public static bool noProofsEnable;
        public static bool moreWantedEnable;
        public static bool SCOLoadEnable;
        public static bool missionLockEnable;
        public static bool betterRaceEnable;
        public static bool gangsEnable;
        public static bool nikoSorrowEnable;
        public static bool costlyDeathEnable;
        public static bool removeWeapEnable;
        public static bool unrestEnable;
        public static bool lowerStatEnable;
        public static bool executeFixEnable;
        public static bool reduceMoneyEnable;
        public static bool policeBribeEnable;
        public static bool copShotgunFixEnable;
        public static bool recessionEnable;
        public static bool fastAllyEnable;

        // HangoutRewards
        public static bool romanRevenueEnable;
        public static bool pillsEnable;
        public static bool brucieEnable;
        public static bool carmenEnable;
        public static bool kikiEnable;
        public static bool alexEnable;

        // MissionShit
        public static bool timedToBlowEnable;
        public static bool heartTimeEnable;
        public static bool buoysAhoyEnable;
        public static bool escuelaOfTheSleepEnable;
        public static bool removeEvidenceEnable;
        public static bool hollandNightsEnable;
        public static bool masterBaitEnable;
        public static bool chaseExtendEnable;
        public static bool explosiveTrapEnable;

        // SettingsFileShit
        public static SettingsFile mainSettings;
        public static SettingsFile scoSettings;
        public static SettingsFile savefileSettings;
        public static SettingsFile bribeSettings;

        // OtherINIShit
        public static int menuKey;

        // OtherShit
        public static uint gTimer;
        public static uint mTimer;
        public static float frameTime;
        internal static SimpleMenu actionMenu;
        public Main()
        {
            Uninitialize += Main_Uninitialize;
            Initialized += Main_Initialized;
            GameLoad += Main_GameLoad;
            IngameStartup += Main_IngameStartup;
            Tick += Main_Tick;
        }

        private void Main_GameLoad(object sender, EventArgs e)
        {
            if (brucieEnable)
                BrucieCarService.GameLoad();
        }
        private void Main_IngameStartup(object sender, EventArgs e)
        {
            if (tripSkipEnable)
                TripSkip.IngameStart();
            if (romanRevenueEnable)
                GiveYouSharesNB.IngameStart();
            if (costlyDeathEnable)
                DeathAndTaxes.IngameStart();
            if (pillsEnable)
                Pills.IngameStart();
            if (removeWeapEnable)
                VCSBuyBackWeapons.IngameStart();
            if (unrestEnable)
                UnrestfulSleep.IngameStart();
            if (reduceMoneyEnable)
                ReducedMoney.IngameStart();
        }
        private void Main_Uninitialize(object sender, EventArgs e)
        {
            TripSkip.UnInit();
            TimedToBlow.UnInit();
            BuoysAhoy.UnInit();
            EscuelaOfTheSleep.UnInit();
            Pills.UnInit();
            VCSBuyBackWeapons.UnInit();
            NiksteinFiles.UnInit();
            HollandNightsMelee.UnInit();
            MasterBaiter.UnInit();
            RealPoliceCorruption.UnInit();
            ChaseExtender.UnInit();
            ExplosiveNegotiation.UnInit();
            ReducedMoney.UnInit();
        }
        private void Main_Initialized(object sender, EventArgs e)
        {
            mainSettings = Settings;
            mainSettings.Load();
            savefileSettings = new SettingsFile(string.Format("{0}\\IVSDKDotNet\\scripts\\MissionStuff\\SaveData.ini", IVGame.GameStartupPath));
            savefileSettings.Load();
            bribeSettings = new SettingsFile(string.Format("{0}\\IVSDKDotNet\\scripts\\MissionStuff\\BribeSettings.ini", IVGame.GameStartupPath));
            bribeSettings.Load();
            scoSettings = new SettingsFile(string.Format("{0}\\IVSDKDotNet\\scripts\\MissionStuff\\SCOSettings.ini", IVGame.GameStartupPath));
            scoSettings.Load();

            actionMenu = new SimpleMenu("Action Menu");

            Init(Settings);

            if (tripSkipEnable)
                TripSkip.Init(Settings);
            if (buffedPedsEnable)
                BuffedPeds.Init(Settings);
            if (noProofsEnable)
                RemoveProofs.Init(Settings);
            if (moreWantedEnable)
                WantedStars.Init(Settings);
            if (SCOLoadEnable)
                SCOLoader.Init(Settings);
            if (missionLockEnable)
                ProgressLock.Init(Settings);
            if (betterRaceEnable)
                BetterRaceAI.Init(Settings);
            if (gangsEnable)
                GangRelationships.Init(Settings);
            if (nikoSorrowEnable)
                BrokeAndOnTheRun.Init(Settings);
            if (costlyDeathEnable)
                DeathAndTaxes.Init(Settings);
            if (removeWeapEnable)
                VCSBuyBackWeapons.Init(Settings);
            if (lowerStatEnable)
                RelationshipAdjust.Init(Settings);
            if (executeFixEnable)
                ExecutionFix.Init(Settings);
            if (reduceMoneyEnable)
                ReducedMoney.Init(Settings);
            if (policeBribeEnable)
                RealPoliceCorruption.Init(Settings);
            if (copShotgunFixEnable)
                CopShotgunFix.Init(Settings);
            if (fastAllyEnable)
                FasterAllies.Init(Settings);

            if (romanRevenueEnable)
                GiveYouSharesNB.Init(Settings);
            if (pillsEnable)
                Pills.Init(Settings);
            if (brucieEnable)
                BrucieCarService.Init(Settings);
            if (carmenEnable)
                CarmenAbility.Init(Settings);
            if (kikiEnable)
                KikiAbility.Init(Settings);
            if (alexEnable)
                AlexAbility.Init(Settings);

            if (timedToBlowEnable)
                TimedToBlow.Init(Settings);
            if (heartTimeEnable)
                HaveAHeartTimed.Init(Settings);
            if (buoysAhoyEnable)
                BuoysAhoy.Init(Settings);
            if (escuelaOfTheSleepEnable)
                EscuelaOfTheSleep.Init();
            if (removeEvidenceEnable)
                NiksteinFiles.Init();
            if (hollandNightsEnable)
                HollandNightsMelee.Init(Settings);
            if (masterBaitEnable)
                MasterBaiter.Init(Settings);
            if (chaseExtendEnable)
                ChaseExtender.Init(Settings);
            if (explosiveTrapEnable)
                ExplosiveNegotiation.Init(Settings);

            //GetGlobals.Init();
        }
        private static void Init(SettingsFile settings)
        {
            tripSkipEnable = settings.GetBoolean("TRIP SKIP", "Enable", false);
            buffedPedsEnable = settings.GetBoolean("TWEAKED MISSION PEDS", "Enable", false);
            noProofsEnable = settings.GetBoolean("REMOVE PED PROOFS", "Enable", false);
            moreWantedEnable = settings.GetBoolean("MORE WANTED STARS", "Enable", false);
            SCOLoadEnable = settings.GetBoolean("SCO LOADER", "Enable", false);
            missionLockEnable = settings.GetBoolean("MISSION LOCKS", "Enable", false);
            betterRaceEnable = settings.GetBoolean("BETTER RACE AI", "Enable", false);
            gangsEnable = settings.GetBoolean("GANGS HATE NIKO", "Enable", false);
            nikoSorrowEnable = settings.GetBoolean("NIKO'S SORROW", "Enable", false);
            costlyDeathEnable = settings.GetBoolean("REALISTIC U.S. HEALTHCARE", "Enable", false);
            removeWeapEnable = settings.GetBoolean("REMOVE WEAPONS ON DEATH", "Enable", false);
            unrestEnable = settings.GetBoolean("RESTLESS SLEEP", "Enable", false);
            lowerStatEnable = settings.GetBoolean("LOWER RELATIONSHIP", "Enable", false);
            executeFixEnable = settings.GetBoolean("EXECUTION COMPATIBILITY", "Enable", false);
            reduceMoneyEnable = settings.GetBoolean("REDUCED REWARDS", "Enable", false);
            policeBribeEnable = settings.GetBoolean("ACTUAL BRIBES", "Enable", false);
            copShotgunFixEnable = settings.GetBoolean("COP CAR SHOTGUN FIX", "Enable", false);
            recessionEnable = settings.GetBoolean("2008 RECESSION SIMULATOR", "Enable", false);
            fastAllyEnable = settings.GetBoolean("KEEP UP, MOTHERFUCKER", "Enable", false);

            romanRevenueEnable = settings.GetBoolean("I'LL GIVE YOU SHARES, NB", "Enable", false);
            pillsEnable = settings.GetBoolean("PACKIE'S PILLS", "Enable", false);
            brucieEnable = settings.GetBoolean("NOW *THIS* IS HOW WE ROLL", "Enable", false);
            carmenEnable = settings.GetBoolean("CARING CARMEN", "Enable", false);
            kikiEnable = settings.GetBoolean("BETTER CALL KIKI", "Enable", false);
            alexEnable = settings.GetBoolean("LIBERATED MAN", "Enable", false);

            timedToBlowEnable = settings.GetBoolean("TIMED TO BLOW", "Enable", false);
            heartTimeEnable = settings.GetBoolean("PACEMAKER", "Enable", false);
            buoysAhoyEnable = settings.GetBoolean("ALIVE IF NOT EXACTLY WELL", "Enable", false);
            escuelaOfTheSleepEnable = settings.GetBoolean("ESCUELA OF THE SLEEP", "Enable", false);
            removeEvidenceEnable = settings.GetBoolean("YOU'RE UNDERCOVER, AS IN DEEP", "Enable", false);
            hollandNightsEnable = settings.GetBoolean("HOLLAND HALLWAY HEAD KNOCKING", "Enable", false);
            masterBaitEnable = settings.GetBoolean("MASTER BAITER", "Enable", false);
            chaseExtendEnable = settings.GetBoolean("B-BUT SCRIPTED CHASES BAAAD", "Enable", false);
            explosiveTrapEnable = settings.GetBoolean("EXPLOSIVE NEGOTIATION", "Enable", false);

            menuKey = settings.GetInteger("MAIN", "MenuKey", 0);
        }
        public static bool InitialChecks()
        {
            if (IS_PAUSE_MENU_ACTIVE()) return false;
            return true;
        }
        private void Main_Tick(object sender, EventArgs e)
        {
            PlayerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());
            PlayerHandle = PlayerPed.GetHandle();
            PlayerIndex = (int)GET_PLAYER_ID();
            PlayerPos = PlayerPed.Matrix.Pos;

            if (PlayerPed == null)
                return;

            if (!InitialChecks())
                return;

            GET_GAME_TIMER(out gTimer);
            GET_FRAME_TIME(out frameTime);

            PedHelper.GrabAllPeds();
            VehHelper.GrabAllVehicles();

            ProcessMenu();

            if (hollandNightsEnable)
                HollandNightsMelee.Tick();

            if (tripSkipEnable)
                TripSkip.Tick();
            if (noProofsEnable)
                RemoveProofs.Tick();
            if (moreWantedEnable)
                WantedStars.Tick();
            if (buffedPedsEnable)
                BuffedPeds.Tick();
            if (missionLockEnable)
                ProgressLock.Tick();
            if (SCOLoadEnable)
                SCOLoader.Tick();
            if (betterRaceEnable)
                BetterRaceAI.Tick();
            if (gangsEnable)
                GangRelationships.Tick();
            if (nikoSorrowEnable)
                BrokeAndOnTheRun.Tick();
            if (costlyDeathEnable)
                DeathAndTaxes.Tick();
            if (removeWeapEnable)
                VCSBuyBackWeapons.Tick();
            if (unrestEnable)
                UnrestfulSleep.Tick();
            if (lowerStatEnable)
                RelationshipAdjust.Tick();
            if (executeFixEnable)
                ExecutionFix.Tick();
            if (reduceMoneyEnable)
                ReducedMoney.Tick();
            if (policeBribeEnable)
                RealPoliceCorruption.Tick();
            if (copShotgunFixEnable)
                CopShotgunFix.Tick();
            if (fastAllyEnable)
                FasterAllies.Tick();

            if (romanRevenueEnable)
                GiveYouSharesNB.Tick();
            if (pillsEnable)
                Pills.Tick();
            if (brucieEnable)
                BrucieCarService.Tick();
            if (carmenEnable)
                CarmenAbility.Tick();
            if (kikiEnable)
                KikiAbility.Tick();
            if (alexEnable)
                AlexAbility.Tick();

            //GoodEnding.Tick();

            //GetGlobals.Tick();

            //SET_CHAR_PROOFS(Main.PlayerHandle, false, false, false, false, false);

            if (timedToBlowEnable)
                TimedToBlow.Tick();
            if (heartTimeEnable)
                HaveAHeartTimed.Tick();
            if (buoysAhoyEnable)
                BuoysAhoy.Tick();
            if (escuelaOfTheSleepEnable)
                EscuelaOfTheSleep.Tick();
            if (removeEvidenceEnable)
                NiksteinFiles.Tick();
            if (masterBaitEnable)
                MasterBaiter.Tick();
            if (chaseExtendEnable)
                ChaseExtender.Tick();
            if (explosiveTrapEnable)
                ExplosiveNegotiation.Tick();

            if (DID_SAVE_COMPLETE_SUCCESSFULLY() && GET_IS_DISPLAYINGSAVEMESSAGE())
            {
                Pills.SavePillCount(savefileSettings);
                UnrestfulSleep.SaveData();
                GiveYouSharesNB.SaveMoney(savefileSettings);
                DeathAndTaxes.SetSaveData(savefileSettings);
            }
        }
        public void ProcessMenu()
        {
            if (NativeControls.IsGameKeyPressed(0, (GameKey)menuKey))
            {
                if (Main.gTimer >= mTimer + 500)
                    actionMenu.Show();
            }
            else
                GET_GAME_TIMER(out mTimer);

            //if (actionMenu.IsActive && IS_CONTROL_JUST_PRESSED(0, (int)GameKey.NavBack))
                //actionMenu.Hide();

            actionMenu.Tick();
        }
        public static List<int> GetWeaponInventory(bool IncludeMelee)
        {
            List<int> inventory = new List<int>();

            for (int i = 0; i <= 8; i++)
            {
                GET_CHAR_WEAPON_IN_SLOT(Main.PlayerPed.GetHandle(), i, out int weaponInSlot, out _, out _);
                if (weaponInSlot == 0) continue;

                var info = IVWeaponInfo.GetWeaponInfo((uint)weaponInSlot);
                if (info == null) continue;

                if (info.FireType != 0 || IncludeMelee)
                {
                    inventory.Add(weaponInSlot);
                }
            }

            return inventory;
        }
        public static Dictionary<int, int> GetWeaponAmmoCounts()
        {
            Dictionary<int, int> ammoCounts = new Dictionary<int, int>();

            foreach (int weapon in GetWeaponInventory(false))
            {
                GET_AMMO_IN_CHAR_WEAPON(Main.PlayerPed.GetHandle(), (int)weapon, out int ammo);
                ammoCounts[weapon] = ammo;
            }

            return ammoCounts;
        }
        public static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
