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
    internal class ProgressLock
    {
        // IniShit
        private static int numberOfLocks;

        // MissionInis
        private static Vector3 missionMarkerPos;
        private static Vector3 tpCoords;
        private static float tpHdng;
        private static float tpDist;

        private static int mStatID;
        private static float mStatMinProg;
        private static float mStatMaxProg;

        private static int reqStatID;
        private static float reqStatProg;
        private static string tpMessage;

        // OtherShit
        private static bool tpThePlayer;
        public static void Init(SettingsFile settings)
        {
            numberOfLocks = settings.GetInteger("MAIN", "NumberOfLockedMissions", 0);
        }
        private static void GetMissionLock(SettingsFile settings, int lockedMissionIndex)
        {
            if (settings.DoesSectionExists("MissionLock" + lockedMissionIndex))
            {
                missionMarkerPos = settings.GetVector3("MissionLock" + lockedMissionIndex, "MissionStartPos", Vector3.Zero);
                tpCoords = settings.GetVector3("MissionLock" + lockedMissionIndex, "TeleportAwayCoords", Vector3.Zero);
                tpHdng = settings.GetFloat("MissionLock" + lockedMissionIndex, "TeleportAwayHeading", 0);
                tpDist = settings.GetFloat("MissionLock" + lockedMissionIndex, "MissionDistance", 0);

                mStatID = settings.GetInteger("MissionLock" + lockedMissionIndex, "MissionStatID", 0);
                mStatMinProg = settings.GetFloat("MissionLock" + lockedMissionIndex, "MissionMinProgress", 0);
                mStatMaxProg = settings.GetFloat("MissionLock" + lockedMissionIndex, "MissionMaxProgress", 0);

                reqStatID = settings.GetInteger("MissionLock" + lockedMissionIndex, "MissionUnlockStat", 0);
                reqStatProg = settings.GetFloat("MissionLock" + lockedMissionIndex, "MissionUnlockRequirement", 0);
                tpMessage = settings.GetValue("MissionLock" + lockedMissionIndex, "MessageToDisplay", "");
            }
        }
        public static void Tick()
        {
            for (int i = 0; i < numberOfLocks; i++)
            {
                GetMissionLock(Main.mainSettings, i);
                //IVGame.ShowSubtitleMessage(GET_MISSION_FLAG().ToString());
                if (LOCATE_CHAR_ANY_MEANS_3D(Main.PlayerHandle, missionMarkerPos.X, missionMarkerPos.Y, missionMarkerPos.Z, tpDist, tpDist, tpDist, false))
                {
                    if (!GET_MISSION_FLAG() && GET_FLOAT_STAT(mStatID) > mStatMinProg && GET_FLOAT_STAT(mStatID) < mStatMaxProg && !HAS_DEATHARREST_EXECUTED())
                    {
                        if (GET_FLOAT_STAT(reqStatID) < reqStatProg && !tpThePlayer)
                        {
                            SET_PLAYER_CONTROL(Main.PlayerIndex, false);
                            if (IS_CHAR_IN_ANY_CAR(Main.PlayerHandle))
                            {
                                GET_CAR_CHAR_IS_USING(Main.PlayerHandle, out int pVeh);
                                FREEZE_CAR_POSITION(pVeh, true);
                            }
                            else
                                FREEZE_CHAR_POSITION(Main.PlayerHandle, true);

                            DO_SCREEN_FADE_OUT(1000);
                            tpThePlayer = true;
                        }
                    }
                    if (tpThePlayer && IS_SCREEN_FADED_OUT())
                    {
                        if (IS_CHAR_IN_ANY_CAR(Main.PlayerHandle))
                        {
                            GET_CAR_CHAR_IS_USING(Main.PlayerHandle, out int pVeh);
                            FREEZE_CAR_POSITION(pVeh, false);
                            SET_CAR_COORDINATES(pVeh, tpCoords);
                            SET_CAR_HEADING(pVeh, tpHdng);
                            SET_CAR_ON_GROUND_PROPERLY(pVeh);
                        }
                        else
                        {
                            FREEZE_CHAR_POSITION(Main.PlayerHandle, false);
                            SET_CHAR_COORDINATES(Main.PlayerHandle, tpCoords);
                            SET_CHAR_HEADING(Main.PlayerHandle, tpHdng);
                        }
                        DO_SCREEN_FADE_IN(1000);
                        SET_PLAYER_CONTROL(Main.PlayerIndex, true);
                        IVText.TheIVText.ReplaceTextOfTextLabel("PLACEHOLDERSL", tpMessage);
                        PRINT_HELP("PLACEHOLDERSL");
                        //IVGame.ShowSubtitleMessage(tpMessage, 5000);
                        tpThePlayer = false;
                    }
                }
            }
        }
    }
}
