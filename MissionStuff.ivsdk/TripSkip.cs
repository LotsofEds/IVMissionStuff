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

        // OtherShit
        private static string missionName;
        private static bool hasSkipped;
        private static bool activateTripSkip;
        private static bool endSkipTrip;
        private static bool printHelp;
        private static uint fTimer;
        private static int pVehicle;
        private static float plyrSpeed;
        private static float carSpeed;

        private static readonly List<string> MissionList = new List<string>();
        private static readonly List<int> PedList = new List<int>();

        private static MissionData[] missionData;
        public static void IngameStart()
        {
            missionName = "";
        }

        public static void UnInit()
        {
            MissionList.Clear();
            PedList.Clear();
        }
        public static void Init(SettingsFile settings)
        {
            debug = settings.GetBoolean("TRIP SKIP", "Debug", false);
            chargeMoney = settings.GetBoolean("TRIP SKIP", "ChargeMoney", false);
            costMult = settings.GetFloat("TRIP SKIP", "CostMultiplier", 0.1f);

            string scoString = settings.GetValue("TRIP SKIP", "SCOList", "");
            MissionList.Clear();

            foreach (var scoName in scoString.Split(','))
            {
                MissionList.Add(scoName);
                if (Main.scoSettings.DoesSectionExists(scoName))
                    continue;
                else
                    IVGame.Console.Print("~r~ERROR: Script name in TripSkip SCOList does not have a section in SCOSettings.ini!");
            }
            missionData = new MissionData[MissionList.Count];
            
            foreach (var scoName in scoString.Split(','))
            {
                int i = MissionList.IndexOf(scoName);

                missionData[i] = new MissionData();

                missionData[i].RequireMissionVeh = Main.scoSettings.GetBoolean(scoName, "TSRequireMissionVeh", false);
                missionData[i].RequireAnyVeh = Main.scoSettings.GetBoolean(scoName, "TSRequireVehicle", false);
                missionData[i].TpCoords = Main.scoSettings.GetVector3(scoName, "TSTeleportCoords", Vector3.Zero);
                missionData[i].TpHdng = Main.scoSettings.GetFloat(scoName, "TSTeleportHeading", 0);
                missionData[i].CheckpointGXT = Main.scoSettings.GetValue(scoName, "CheckpointGXT", "");

                string pedString = Main.scoSettings.GetValue(scoName, "TSFriendlyModels", "none");
                missionData[i].ModelList = new List<string>();

                foreach (var pedModel in pedString.Split(','))
                    missionData[i].ModelList.Add(pedModel);
            }
        }
        private static void LoadCheckpointData(string scoName)
        {
            int i = MissionList.IndexOf(scoName);

            missionVeh = missionData[i].RequireMissionVeh;
            reqVeh = missionData[i].RequireAnyVeh;
            teleportCoords = missionData[i].TpCoords;
            teleportHdng = missionData[i].TpHdng;
            textToCheck = missionData[i].CheckpointGXT;
        }
        public static void Tick()
        {
            SkipTheTrip();
            EndTrip();
            
            foreach (string sco in MissionList)
            {
                if (NativeGame.IsScriptRunning(sco))
                {
                    if (!IS_SCREEN_FADED_OUT() && IS_PLAYER_CONTROL_ON(Main.PlayerIndex))
                    {
                        LoadCheckpointData(sco);

                        if (IS_CHAR_IN_ANY_CAR(Main.PlayerHandle))
                            GET_CAR_CHAR_IS_USING(Main.PlayerHandle, out pVehicle);
                        else
                            pVehicle = -1;
                        //bool saveCheckpoint = true;
                        bool canTripSkip = true;

                        foreach (var ped in PedHelper.PedHandles)
                        {
                            int pedHandle = ped.Value;
                            if (!DOES_CHAR_EXIST(pedHandle))
                                continue;
                            if (!IS_PED_A_MISSION_PED(pedHandle))
                                continue;
                            if (pedHandle == Main.PlayerHandle)
                                continue;
                            if (PedList.Contains(pedHandle))
                                continue;

                            GET_CHAR_MODEL(pedHandle, out int pModel);

                            foreach (string pedModel in missionData[MissionList.IndexOf(sco)].ModelList)
                            {
                                if (pModel == GET_HASH_KEY(pedModel))
                                    PedList.Add(pedHandle);
                            }
                        }
                        if (PedList.Count < missionData[MissionList.IndexOf(sco)].ModelList.Count && missionData[MissionList.IndexOf(sco)].ModelList[0] != "none")
                            canTripSkip = false;

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
                                hasSkipped = false;
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
                                    IVText.TheIVText.ReplaceTextOfTextLabel("TM_2_23", "Hold ~INPUT_PICKUP~ when stopped to skip the trip.");
                                else
                                    IVText.TheIVText.ReplaceTextOfTextLabel("TM_2_23", "Hold ~INPUT_PICKUP~ when stopped to skip the trip. It will cost $" + ((int)(pDist * costMult)).ToString());

                                PRINT_HELP("TM_2_23");
                                printHelp = true;
                            }
                            STORE_SCORE(Main.PlayerIndex, out uint pMoney);
                            GET_CHAR_SPEED(Main.PlayerHandle, out plyrSpeed);
                            if (IS_CHAR_SITTING_IN_ANY_CAR(Main.PlayerHandle))
                                GET_CAR_SPEED(pVehicle, out carSpeed);
                            if (NativeControls.IsGameKeyPressed(0, GameKey.Action) && plyrSpeed < 0.1f && carSpeed < 0.1f)
                            {
                                if (Main.gTimer >= fTimer + 2000)
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
                                        IVText.TheIVText.ReplaceTextOfTextLabel("TM_2_24", "~r~You don't have enough money!");
                                        PRINT_HELP("TM_2_24");
                                    }
                                }
                            }
                            else
                                GET_GAME_TIMER(out fTimer);
                        }
                    }
                }
                else if (missionName == sco)
                {
                    printHelp = false;
                    PedList.Clear();
                    hasSkipped = false;
                    activateTripSkip = false;
                    endSkipTrip = false;
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

            GET_GAME_TIMER(out fTimer);
            endSkipTrip = true;
        }
        private static void EndTrip()
        {
            if (Main.gTimer < fTimer + 2000)
                return;

            if (!endSkipTrip)
                return;

            if (reqVeh || missionVeh)
                SET_CAR_ON_GROUND_PROPERLY(pVehicle);

            if (!IS_SCREEN_FADED_IN() && !IS_SCREEN_FADING_IN())
                DO_SCREEN_FADE_IN(1000);
            endSkipTrip = false;
            activateTripSkip = false;
        }
    }
    public class MissionData
    {
        public bool RequireMissionVeh {  get; set; }
        public bool RequireAnyVeh { get; set; }
        public Vector3 TpCoords { get; set; }
        public float TpHdng { get; set; }
        public string CheckpointGXT { get; set; }
        public List<string> ModelList { get; set; }
        public MissionData()
        {

        }
    }
}