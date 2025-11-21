using CCL;
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
    internal class TripSkip
    {
        // IniShit
        private static bool debug;
        private static bool chargeMoney;
        private static bool missionVeh;
        private static bool reqVeh;
        private static string textToCheck;
        private static float costMult;
        private static float pDist;
        private static Vector3 teleportCoords;
        private static float teleportHdng;
        private static GameKey tripSkipKey;

        // OtherShit
        private static string missionName;
        private static bool gotList;
        private static bool hasSkipped;
        private static bool activateTripSkip;
        private static bool endSkipTrip;
        private static bool printHelp;
        private static uint gTimer;
        private static uint fTimer;
        private static int pVehicle;

        public static readonly List<string> MissionList = new List<string>();
        public static readonly List<string> ModelList = new List<string>();
        public static readonly List<int> PedList = new List<int>();

        public static void GameLoad()
        {
            missionName = "";
        }

        public static void UnInit()
        {
            MissionList.Clear();
            ModelList.Clear();
            PedList.Clear();
        }
        public static void Init(SettingsFile settings)
        {
            debug = settings.GetBoolean("MAIN", "TripSkipDebug", false);
            chargeMoney = settings.GetBoolean("MAIN", "ChargeMoney", false);
            costMult = settings.GetFloat("MAIN", "CostMultiplier", 0.1f);
            tripSkipKey = (GameKey)settings.GetInteger("MAIN", "TripSkipKey", 23);

            string scoString = settings.GetValue("MAIN", "TripSkipSCOList", "");
            MissionList.Clear();

            foreach (var scoName in scoString.Split(','))
            {
                MissionList.Add(scoName);
                if (settings.DoesSectionExists(scoName))
                    continue;
                else
                    IVGame.Console.Print("ERROR: Script name in SCOList does not have a section!");
            }
        }
        private static void LoadCheckpointData(SettingsFile settings, string scoName)
        {
            missionVeh = settings.GetBoolean(scoName, "TSRequireMissionVeh", false);
            reqVeh = settings.GetBoolean(scoName, "TSRequireVehicle", false);
            teleportCoords = settings.GetVector3(scoName, "TSTeleportCoords", Vector3.Zero);
            teleportHdng = settings.GetFloat(scoName, "TSTeleportHeading", 0);
            textToCheck = settings.GetValue(scoName, "CheckpointGXT", "");

            string pedString = settings.GetValue(scoName, "FriendlyModels", "");

            if (!gotList)
            {
                ModelList.Clear();
                foreach (var pedModel in pedString.Split(','))
                    ModelList.Add(pedModel);
                gotList = true;
            }
        }
        public static void Tick()
        {
            /*if (IS_THIS_PRINT_BEING_DISPLAYED("J1_CS2_END", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0))
                IVGame.ShowSubtitleMessage("ass");*/
            SkipTheTrip();
            EndTrip();

            if (!IS_SCREEN_FADED_OUT())
            {
                foreach (string sco in MissionList)
                {
                    if (NativeGame.IsScriptRunning(sco))
                    {
                        PedHelper.GrabAllPeds();
                        LoadCheckpointData(Main.mainSettings, sco);

                        if (IS_CHAR_IN_ANY_CAR(Main.PlayerHandle))
                            GET_CAR_CHAR_IS_USING(Main.PlayerHandle, out pVehicle);
                        else
                            pVehicle = -1;
                        //bool saveCheckpoint = true;
                        bool canTripSkip = true;

                        foreach (var ped in PedHelper.PedHandles)
                        {
                            int pedHandle = ped.Value;
                            if (!IS_PED_A_MISSION_PED(pedHandle))
                                continue;
                            if (pedHandle == Main.PlayerHandle)
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
                            if (!DOES_CHAR_EXIST(ped))
                                continue;

                            if (missionName == sco)
                            {
                                if (missionVeh && (!IS_CHAR_SITTING_IN_CAR(ped, pVehicle) || !IS_CAR_A_MISSION_CAR(pVehicle)))
                                {
                                    canTripSkip = false;
                                    break;
                                }
                                if (reqVeh && !IS_CHAR_SITTING_IN_CAR(ped, pVehicle))
                                {
                                    canTripSkip = false;
                                    break;
                                }
                            }
                            else
                            {
                                canTripSkip = false;
                                break;
                            }
                        }

                        if (missionVeh && (!IS_CHAR_SITTING_IN_ANY_CAR(Main.PlayerHandle) || !IS_CAR_A_MISSION_CAR(pVehicle)))
                            canTripSkip = false;

                        if (reqVeh && !IS_CHAR_SITTING_IN_ANY_CAR(Main.PlayerHandle))
                            canTripSkip = false;

                        if (missionName != sco && IS_THIS_PRINT_BEING_DISPLAYED(textToCheck, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0) && !hasSkipped)
                        {
                            hasSkipped = true;
                            if (debug)
                                IVGame.ShowSubtitleMessage("Checkpoint saved");
                            missionName = sco;
                        }

                        if (missionName == sco && canTripSkip && !hasSkipped)
                        {
                            if (!printHelp)
                            {
                                GET_DISTANCE_BETWEEN_COORDS_3D(Main.PlayerPos.X, Main.PlayerPos.Y, Main.PlayerPos.Z, teleportCoords.X, teleportCoords.Y, teleportCoords.Z, out pDist);
                                if (!chargeMoney)
                                    IVText.TheIVText.ReplaceTextOfTextLabel("PLACEHOLDER_1", "Hold the ~g~" + tripSkipKey.ToString() + "~s~ key to trip skip.");
                                else
                                    IVText.TheIVText.ReplaceTextOfTextLabel("PLACEHOLDER_1", "Hold the ~g~" + tripSkipKey.ToString() + "~s~ key to trip skip. It will cost $" + ((int)(pDist * costMult)).ToString());
                                PRINT_HELP("PLACEHOLDER_1");
                                printHelp = true;
                            }
                            STORE_SCORE(Main.PlayerIndex, out uint pMoney);
                            if (NativeControls.IsGameKeyPressed(0, tripSkipKey))
                            {
                                GET_GAME_TIMER(out gTimer);
                                if (gTimer >= fTimer + 2000)
                                {
                                    if (!chargeMoney || (chargeMoney && pMoney >= ((int)(pDist * costMult))))
                                    {
                                        if (chargeMoney)
                                            ADD_SCORE(Main.PlayerIndex, -((int)(pDist * costMult)));
                                        GET_GAME_TIMER(out fTimer);
                                        hasSkipped = true;
                                        activateTripSkip = true;
                                        DO_SCREEN_FADE_OUT(1000);
                                    }
                                    else if (chargeMoney && !IS_HELP_MESSAGE_BEING_DISPLAYED())
                                    {
                                        IVText.TheIVText.ReplaceTextOfTextLabel("PLACEHOLDERSL", "~r~You don't have enough money!");
                                        PRINT_HELP("PLACEHOLDERSL");
                                    }
                                }
                            }
                            else
                                GET_GAME_TIMER(out fTimer);
                        }
                    }
                    else if (missionName == sco)
                    {
                        printHelp = false;
                        gotList = false;
                        PedList.Clear();
                        if (hasSkipped)
                            hasSkipped = false;
                    }
                }
            }
        }
        private static void SkipTheTrip()
        {
            if (!activateTripSkip)
                return;

            if (endSkipTrip)
                return;

            if (!IS_SCREEN_FADED_OUT())
                return;

            if (reqVeh || missionVeh)
            {
                SET_CAR_COORDINATES(pVehicle, teleportCoords);
                SET_CAR_HEADING(pVehicle, teleportHdng);
            }
            else
            {
                SET_CHAR_COORDINATES(Main.PlayerHandle, teleportCoords);
                SET_CHAR_HEADING(pVehicle, teleportHdng);
            }
            GET_GAME_TIMER(out gTimer);

            if (gTimer >= fTimer + 2500)
                endSkipTrip = true;
        }
        private static void EndTrip()
        {
            if (!endSkipTrip)
                return;

            if (reqVeh || missionVeh)
                SET_CAR_ON_GROUND_PROPERLY(pVehicle);

            DO_SCREEN_FADE_IN(1000);
            endSkipTrip = false;
            activateTripSkip = false;
        }
    }
}