using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

        // IniShit
        public static bool tripSkipEnable;
        public static bool buffAlliesEnable;
        public static bool noProofsEnable;
        public static bool moreWantedEnable;
        public static bool buffEnemiesEnable;
        public static bool SCOLoadEnable;
        public static bool missionLockEnable;
        public static bool betterRaceEnable;
        public static bool pillsEnable;
        public static bool gangsEnable;
        public static bool nikoSorrowEnable;
        public static bool romanRevenueEnable;
        public static bool costlyDeathEnable;
        public static bool removeWeapEnable;

        // MissionShit
        public static bool timedToBlowEnable;
        public static bool heartTimeEnable;
        public static bool buoysAhoyEnable;
        public static bool escuelaOfTheSleepEnable;

        // SettingsFileShit
        public static SettingsFile mainSettings;
        public static SettingsFile bribeSettings;

        // OtherShit
        public static bool dontCrash = false;
        public static uint gTimer;
        public Main()
        {
            Uninitialize += Main_Uninitialize;
            Initialized += Main_Initialized;
            GameLoad += Main_GameLoad;
            KeyDown += Main_KeyDown;
            KeyUp += Main_KeyUp;
            Tick += Main_Tick;
        }
        private void Main_GameLoad(object sender, EventArgs e)
        {
            if (tripSkipEnable)
                TripSkip.GameLoad();
        }
        private void Main_Uninitialize(object sender, EventArgs e)
        {
            TripSkip.UnInit();
            TimedToBlow.UnInit();
            BuoysAhoy.UnInit();
            EscuelaOfTheSleep.UnInit();
            Pills.UnInit();
        }

        private void Main_KeyDown(object sender, KeyEventArgs e)
        {
            if (!InitialChecks())
                return;

            if (PlayerPed == null)
                return;

            if (pillsEnable)
                Pills.KeyDown();
        }
        private void Main_KeyUp(object sender, KeyEventArgs e)
        {
            if (!InitialChecks())
                return;

            if (PlayerPed == null)
                return;

            if (pillsEnable)
                Pills.KeyUp();
        }

        private void Main_Initialized(object sender, EventArgs e)
        {
            mainSettings = new SettingsFile(string.Format("{0}\\IVSDKDotNet\\scripts\\MissionStuff.ini", IVGame.GameStartupPath));
            mainSettings.Load();
            bribeSettings = new SettingsFile(string.Format("{0}\\IVSDKDotNet\\scripts\\MissionStuff\\BribeSettings.ini", IVGame.GameStartupPath));
            bribeSettings.Load();
            Init(Settings);
            if (tripSkipEnable)
                TripSkip.Init(Settings);
            if (buffAlliesEnable)
                ArmoredAllies.Init(Settings);
            if (buffEnemiesEnable)
                BuffedEnemies.Init(Settings);
            if (noProofsEnable)
                RemoveProofs.Init(Settings);
            if (moreWantedEnable)
                WantedStars.Init(Settings);
            if (SCOLoadEnable)
                SCOLoader.Init(Settings);
            if (missionLockEnable)
                ProgressLock.Init(Settings);
            if (missionLockEnable)
                ProgressLock.Init(Settings);
            if (betterRaceEnable)
                BetterRaceAI.Init(Settings);
            if (pillsEnable)
                Pills.Init(Settings);
            if (gangsEnable)
                GangRelationships.Init(Settings);
            if (nikoSorrowEnable)
                BrokeAndOnTheRun.Init(Settings);
            if (romanRevenueEnable)
                GiveYouSharesNB.Init(Settings);
            if (costlyDeathEnable)
                DeathAndTaxes.Init(Settings);
            if (removeWeapEnable)
                VCSBuyBackWeapons.Init(Settings);

            if (timedToBlowEnable)
                TimedToBlow.Init(Settings);
            if (heartTimeEnable)
                HaveAHeartTimed.Init(Settings);
            if (buoysAhoyEnable)
                BuoysAhoy.Init(Settings);
            if (escuelaOfTheSleepEnable)
                EscuelaOfTheSleep.Init();
        }
        public static bool InitialChecks()
        {
            if (IS_PAUSE_MENU_ACTIVE()) return false;
            return true;
        }
        private static void Init(SettingsFile settings)
        {
            tripSkipEnable = settings.GetBoolean("MAIN", "TripSkip", false);
            buffAlliesEnable = settings.GetBoolean("MAIN", "BuffAllies", false);
            buffEnemiesEnable = settings.GetBoolean("MAIN", "BuffEnemies", false);
            noProofsEnable = settings.GetBoolean("MAIN", "RemoveEnemyProofs", false);
            moreWantedEnable = settings.GetBoolean("MAIN", "MoreWantedStars", false);
            SCOLoadEnable = settings.GetBoolean("MAIN", "SCOLoader", false);
            missionLockEnable = settings.GetBoolean("MAIN", "MissionLocks", false);
            betterRaceEnable = settings.GetBoolean("MAIN", "BetterRaceAI", false);
            pillsEnable = settings.GetBoolean("MAIN", "PackiePills", false);
            gangsEnable = settings.GetBoolean("MAIN", "GangsHateNiko", false);
            nikoSorrowEnable = settings.GetBoolean("MAIN", "NikosSorrow", false);
            romanRevenueEnable = settings.GetBoolean("MAIN", "BellicEntShares", false);
            costlyDeathEnable = settings.GetBoolean("MAIN", "DeadAndBroke", false);
            removeWeapEnable = settings.GetBoolean("MAIN", "RemoveWeaponsOnDeath", false);

            timedToBlowEnable = settings.GetBoolean("MAIN", "TimedToBlow", false);
            heartTimeEnable = settings.GetBoolean("MAIN", "Pacemaker", false);
            buoysAhoyEnable = settings.GetBoolean("MAIN", "BuoysAhoyRevamp", false);
            escuelaOfTheSleepEnable = settings.GetBoolean("MAIN", "EscuelaOfTheSleep", false);
        }
        private void Main_Tick(object sender, EventArgs e)
        {
            PlayerPed = IVPed.FromUIntPtr(IVPlayerInfo.FindThePlayerPed());
            PlayerHandle = PlayerPed.GetHandle();
            PlayerIndex = (int)GET_PLAYER_ID();
            PlayerPos = PlayerPed.Matrix.Pos;

            if (!InitialChecks())
                return;

            if (PlayerPed == null)
                return;

            GET_GAME_TIMER(out gTimer);

            PedHelper.GrabAllPeds();
            VehHelper.GrabAllVehicles();

            if (tripSkipEnable)
                TripSkip.Tick();
            if (noProofsEnable)
                RemoveProofs.Tick();
            if (moreWantedEnable)
                WantedStars.Tick();
            if (buffAlliesEnable)
                ArmoredAllies.Tick();
            if (buffEnemiesEnable)
                BuffedEnemies.Tick();
            if (missionLockEnable)
                ProgressLock.Tick();
            if (SCOLoadEnable)
                SCOLoader.Tick();
            if (betterRaceEnable)
                BetterRaceAI.Tick();
            if (pillsEnable)
                Pills.Tick();
            if (gangsEnable)
                GangRelationships.Tick();
            if (nikoSorrowEnable)
                BrokeAndOnTheRun.Tick();
            if (romanRevenueEnable)
                GiveYouSharesNB.Tick();
            if (costlyDeathEnable)
                DeathAndTaxes.Tick();
            if (removeWeapEnable)
                VCSBuyBackWeapons.Tick();
            //SET_CHAR_PROOFS(Main.PlayerHandle, false, false, false, false, false);

            if (timedToBlowEnable)
                TimedToBlow.Tick();
            if (heartTimeEnable)
                HaveAHeartTimed.Tick();
            if (buoysAhoyEnable)
                BuoysAhoy.Tick();
            if (escuelaOfTheSleepEnable)
                EscuelaOfTheSleep.Tick();
        }
    }
}
