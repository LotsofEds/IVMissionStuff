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
        public static bool replaceWeaponEnable;
        public static bool SCOLoadEnable;
        public static bool missionLockEnable;

        // MissionShit
        public static bool timedToBlowEnable;
        public static bool heartTimeEnable;
        public static bool buoysAhoyEnable;

        // SettingsFileShit
        public static SettingsFile mainSettings;

        // OtherShit
        public static bool dontCrash = false;
        public Main()
        {
            Uninitialize += Main_Uninitialize;
            Initialized += Main_Initialized;
            GameLoad += Main_GameLoad;
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
        }

        private void Main_Initialized(object sender, EventArgs e)
        {
            mainSettings = new SettingsFile(string.Format("{0}\\IVSDKDotNet\\scripts\\MissionStuff.ini", IVGame.GameStartupPath));
            mainSettings.Load();
            Init(Settings);
            if (tripSkipEnable)
                TripSkip.Init(Settings);
            if (buffAlliesEnable || replaceWeaponEnable)
                ArmoredAllies.Init(Settings);
            if (replaceWeaponEnable)
                WeaponReplace.Init(Settings);
            if (noProofsEnable)
                RemoveProofs.Init(Settings);
            if (moreWantedEnable)
                WantedStars.Init(Settings);
            if (timedToBlowEnable)
                TimedToBlow.Init(Settings);
            if (heartTimeEnable)
                HaveAHeartTimed.Init(Settings);
            if (buoysAhoyEnable)
                BuoysAhoy.Init(Settings);
            if (SCOLoadEnable)
                SCOLoader.Init(Settings);
            if (missionLockEnable)
                ProgressLock.Init(Settings);
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
            noProofsEnable = settings.GetBoolean("MAIN", "RemoveEnemyProofs", false);
            moreWantedEnable = settings.GetBoolean("MAIN", "MoreWantedStars", false);
            replaceWeaponEnable = settings.GetBoolean("MAIN", "ReplaceMissionPedWeapons", false);
            SCOLoadEnable = settings.GetBoolean("MAIN", "SCOLoader", false);
            missionLockEnable = settings.GetBoolean("MAIN", "MissionLocks", false);

            timedToBlowEnable = settings.GetBoolean("MAIN", "TimedToBlow", false);
            heartTimeEnable = settings.GetBoolean("MAIN", "Pacemaker", false);
            buoysAhoyEnable = settings.GetBoolean("MAIN", "BuoysAhoyRevamp", false);
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

            PedHelper.GrabAllPeds();

            if (tripSkipEnable)
                TripSkip.Tick();
            if (noProofsEnable)
                RemoveProofs.Tick();
            if (moreWantedEnable)
                WantedStars.Tick();
            if (replaceWeaponEnable)
                WeaponReplace.Tick();
            if (missionLockEnable)
                ProgressLock.Tick();
            if (SCOLoadEnable)
                SCOLoader.Tick();

            //SET_CHAR_PROOFS(Main.PlayerHandle, false, false, false, false, false);
            ArmoredAllies.Tick();

            if (timedToBlowEnable)
                TimedToBlow.Tick();
            if (heartTimeEnable)
                HaveAHeartTimed.Tick();
            if (buoysAhoyEnable)
                BuoysAhoy.Tick();
        }
    }
}
