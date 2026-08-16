using CCL;
using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using IVSDKDotNet.Native;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static IVSDKDotNet.Native.Natives;
using static System.Net.Mime.MediaTypeNames;

namespace MissionStuff.ivsdk
{
    internal class ExplosiveNegotiation
    {
        private static uint timeLimit;

        // BooleShit
        private static bool missionStarted;
        private static bool failCutStart;
        private static bool cutsceneStart;
        private static bool timerStart;
        private static int cam;
        private static int cam2;
        private static int interpCam;

        // OtherShit
        private static uint aTimer;
        private static uint fTimer;
        private static uint beepTime;
        private static int romanPed;
        private static int driverPed;
        private static int bobcatVeh;
        private static int bombProp;
        private static int cutsceneSeq;
        private static Vector3 plyrCoords;
        private static Color bombColor;

        public static void Init(SettingsFile settings)
        {
            cutsceneSeq = -1;
            timeLimit = settings.GetUInteger("EXPLOSIVE NEGOTIATION", "TimeLimit", 270000);
        }
        public static void UnInit()
        {
            ACTIVATE_SCRIPTED_CAMS(false, false);
            DESTROY_ALL_CAMS();

            if (DOES_OBJECT_EXIST(bombProp))
                DELETE_OBJECT(ref bombProp);
            if (DOES_CHAR_EXIST(romanPed))
                MARK_CHAR_AS_NO_LONGER_NEEDED(romanPed);
            if (DOES_CHAR_EXIST(driverPed))
                DELETE_CHAR(ref driverPed);

            aTimer = 0;
            fTimer = 0;
            cutsceneSeq = -1;
            failCutStart = false;
            cutsceneStart = false;
            timerStart = false;
            missionStarted = false;
            romanPed = 0;
        }
        public static void Tick()
        {
            if (NativeGame.IsScriptRunning("roman12"))
            {
                if (!DOES_OBJECT_EXIST(bombProp) && HAS_MODEL_LOADED(GET_HASH_KEY("ec_bomb")))
                {
                    CREATE_OBJECT(GET_HASH_KEY("ec_bomb"), 1203.175f, 1438.862f, 30.020f, out bombProp, true);
                    SET_OBJECT_ROTATION(bombProp, 90f, 0f, 90f);
                    ADD_OBJECT_TO_INTERIOR_ROOM_BY_NAME(bombProp, "Factory_Room04");
                    SET_OBJECT_DYNAMIC(bombProp, false);
                    SET_OBJECT_PROOFS(bombProp, true, true, true, true, true);
                }

                if (IS_THIS_PRINT_BEING_DISPLAYED("FS_09", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0) && !missionStarted)
                {
                    CLEAR_BRIEF();
                    IVText.TheIVText.ReplaceTextOfTextLabel("TM_2_29", "~s~The warehouse is rigged to blow. Rescue ~b~Roman~s~ quickly.");
                    PRINT_NOW("TM_2_29", 5000, true);
                    missionStarted = true;
                }

                if (missionStarted)
                {
                    foreach (var ped in PedHelper.PedHandles)
                    {
                        int pedHandle = ped.Value;
                        if (!DOES_CHAR_EXIST(pedHandle))
                            continue;
                        if (!IS_PED_A_MISSION_PED(pedHandle))
                            continue;
                        if (pedHandle == Main.PlayerHandle)
                            continue;

                        GET_CHAR_MODEL(pedHandle, out uint pModel);

                        if (pModel == (uint)GET_HASH_KEY("ig_roman"))
                        {
                            UNLOCK_RAGDOLL(pedHandle, false);
                            SET_CHAR_INVINCIBLE(pedHandle, false);
                            SET_CHAR_ONLY_DAMAGED_BY_PLAYER(pedHandle, false);
                            romanPed = pedHandle;
                        }

                        if (!timerStart)
                        {
                            if (IS_PED_IN_COMBAT(pedHandle))
                            {
                                GET_GAME_TIMER(out fTimer);
                                timerStart = true;
                                break;
                            }
                        }
                    }
                }

                if (!HAS_MODEL_LOADED(GET_HASH_KEY("ec_bomb")))
                    REQUEST_MODEL(GET_HASH_KEY("ec_bomb"));

                ProcessTimer();
                ProcessCutscene();
                ProcessFailCutscene();
            }
            else if (missionStarted && !IS_CHAR_DEAD(Main.PlayerHandle))
            {
                UnInit();
            }
        }
        private static void ProcessTimer()
        {
            if (!timerStart)
                return;

            if (cutsceneSeq >= 0)
                return;

            if (DOES_CHAR_EXIST(romanPed))
            {
                if (IS_CHAR_PLAYING_ANIM(romanPed, "missroman12", "hostage_let_go"))
                    cutsceneSeq = 0;
            }

            if (Main.gTimer >= fTimer + timeLimit && IS_PLAYER_CONTROL_ON(Main.PlayerIndex))
            {
                CLEAR_BRIEF();
                SET_PLAYER_CONTROL(Main.PlayerIndex, false);
                cutsceneSeq = 0;
                failCutStart = true;
            }
            else if (!cutsceneStart && IS_PLAYER_CONTROL_ON(Main.PlayerIndex))
            {
                SET_TEXT_FONT(0);
                SET_TEXT_SCALE(0.3f, 0.3f);
                SET_TEXT_COLOUR(255, 255, 255, 255);
                SET_TEXT_DROPSHADOW(false, 0, 0, 0, 0);
                SET_TEXT_CENTRE(true);

                IVText.TheIVText.ReplaceTextOfTextLabel("TM_2_30", "Detonation:");
                DISPLAY_TEXT(0.9f, 0.35f, "TM_2_30");

                uint currentTime = (fTimer + timeLimit) - Main.gTimer;

                uint min = currentTime / 1000 / 60 % 100;
                uint sec = currentTime / 1000 % 60;

                USE_PREVIOUS_FONT_SETTINGS();

                DISPLAY_TEXT_WITH_NUMBER(0.8875f, 0.4f, "NUMBER", (int)min);

                USE_PREVIOUS_FONT_SETTINGS();
                DISPLAY_TEXT(0.8975f, 0.4f, "COLON");

                USE_PREVIOUS_FONT_SETTINGS();
                if (sec < 10)
                {
                    DISPLAY_TEXT_WITH_NUMBER(0.9065f, 0.4f, "NUMBER", 0);
                    USE_PREVIOUS_FONT_SETTINGS();
                    DISPLAY_TEXT_WITH_NUMBER(0.917f, 0.4f, "NUMBER", (int)sec);
                }
                else
                    DISPLAY_TEXT_WITH_NUMBER(0.911f, 0.4f, "NUMBER", (int)sec);
            }
        }
        private static void ProcessCutscene()
        {
            if (IS_THIS_PRINT_BEING_DISPLAYED("FOLROM", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)&& !cutsceneStart)
            {
                CLEAR_BRIEF();
                SET_PLAYER_CONTROL(Main.PlayerIndex, false);
                CLEAR_PRINTS();
                cutsceneSeq = 0;
                cutsceneStart = true;
            }

            if (!cutsceneStart)
                return;

            // Initialize cutscene
            if (cutsceneSeq <= 0)
            {
                DISPLAY_HUD(false);
                DISPLAY_RADAR(false);

                CREATE_CAM(14, out cam);
                SET_CAM_FOV(cam, 45);

                GET_OFFSET_FROM_OBJECT_IN_WORLD_COORDS(bombProp, new Vector3(0, 0, 1.25f), out Vector3 camOff);
                SET_CAM_POS(cam, camOff);
                POINT_CAM_AT_OBJECT(cam, bombProp);
                //POINT_CAM_AT_COORD(cam, -172.590f, 1374.672f, 34.198f);

                SET_CAM_ACTIVE(cam, true);
                SET_CAM_PROPAGATE(cam, true);
                ACTIVATE_SCRIPTED_CAMS(true, true);

                GET_GAME_TIMER(out fTimer);

                cutsceneSeq++;
            }
            // Show bomb
            else if (cutsceneSeq == 1)
            {
                CLEAR_CHAR_TASKS_IMMEDIATELY(romanPed);
                CLEAR_CHAR_TASKS_IMMEDIATELY(Main.PlayerHandle);

                _SET_CHAR_COORDINATES_NO_OFFSET(romanPed, 1174.883f, 1437.502f, 21.187f);
                _SET_CHAR_COORDINATES_NO_OFFSET(Main.PlayerHandle, 1177.694f, 1438.261f, 21.762f);

                SET_CHAR_HEADING(romanPed, 0);
                SET_CHAR_HEADING(Main.PlayerHandle, 180);

                GET_GAME_VIEWPORT_ID(out int viewPort);
                SET_ROOM_FOR_VIEWPORT_BY_NAME(viewPort, "Factory_Room04");
                beepTime = 400;
                cutsceneSeq++;
            }
            else if (cutsceneSeq == 2)
            {
                bombColor = Color.Green;
                if (Main.gTimer >= aTimer + beepTime)
                {
                    int soundID = GET_SOUND_ID();
                    PLAY_SOUND_FRONTEND(soundID, "F5_TRUCK_ARSON_BOMB_BLEEP");
                    RELEASE_SOUND_ID(soundID);
                    GET_GAME_TIMER(out aTimer);
                    bombColor = Color.Red;
                }
                if (Main.gTimer >= fTimer + 1800)
                {
                    _TASK_FOLLOW_NAV_MESH_TO_COORD(romanPed, 1173.709f, 1433.115f, 16.772f, 3, -1, 0.1f);
                    _TASK_FOLLOW_NAV_MESH_TO_COORD(Main.PlayerHandle, 1173.709f, 1433.115f, 16.772f, 3, -1, 0.1f);

                    GET_GAME_TIMER(out fTimer);

                    cutsceneSeq++;
                }
                DRAW_LIGHT_WITH_RANGE(1203.375f, 1438.862f, 30.120f, bombColor.R, bombColor.G, bombColor.B, 10f, 25.0f);
            }
            // Cut to Roman and Niko leaving building
            else if (cutsceneSeq == 3)
            {
                if (Main.gTimer >= aTimer + beepTime)
                {
                    int soundID = GET_SOUND_ID();
                    PLAY_SOUND_FRONTEND(soundID, "F5_TRUCK_ARSON_BOMB_BLEEP");
                    RELEASE_SOUND_ID(soundID);
                    GET_GAME_TIMER(out aTimer);

                    if (Main.gTimer >= fTimer + 2500)
                    {
                        beepTime -= 40;
                        if (beepTime < 40)
                            beepTime = 40;
                    }
                }
                if (Main.gTimer >= fTimer + 3500)
                {
                    SET_CAM_FOV(cam, 45);

                    GET_OFFSET_FROM_OBJECT_IN_WORLD_COORDS(bombProp, new Vector3(0, 0, 1.25f), out Vector3 camOff);
                    SET_CAM_POS(cam, camOff);
                    POINT_CAM_AT_OBJECT(cam, bombProp);

                    GET_GAME_TIMER(out fTimer);

                    cutsceneSeq++;
                }
                else
                {
                    SET_CAM_FOV(cam, 45);
                    SET_CAM_POS(cam, 1170.130f, 1446.873f, 22.122f);
                    POINT_CAM_AT_COORD(cam, 1173.265f, 1443.663f, 18.999f);
                    GET_GAME_VIEWPORT_ID(out int viewPort);
                    CLEAR_ROOM_FOR_VIEWPORT(viewPort);
                }
            }
            // Cut back to bomb before it explodes
            else if (cutsceneSeq == 4)
            {
                GET_GAME_VIEWPORT_ID(out int viewPort);
                SET_ROOM_FOR_VIEWPORT_BY_NAME(viewPort, "Factory_Room04");

                bombColor = Color.Green;
                if (Main.gTimer >= aTimer + beepTime)
                {
                    int soundID = GET_SOUND_ID();
                    PLAY_SOUND_FRONTEND(soundID, "F5_TRUCK_ARSON_BOMB_BLEEP");
                    RELEASE_SOUND_ID(soundID);
                    GET_GAME_TIMER(out aTimer);
                    bombColor = Color.Red;

                    beepTime -= 40;
                    if (beepTime < 40)
                        beepTime = 40;
                }
                if (Main.gTimer >= fTimer + 2000)
                {
                    ADD_EXPLOSION(new Vector3(1203.205f, 1438.862f, 30.020f), 2, 10, true, false, 0);

                    GET_GAME_TIMER(out fTimer);

                    cutsceneSeq++;
                }
                DRAW_LIGHT_WITH_RANGE(1203.375f, 1438.862f, 30.120f, bombColor.R, bombColor.G, bombColor.B, 10f, 25.0f);
            }
            // Explode bomb
            else if (cutsceneSeq == 5)
            {
                CLEAR_CHAR_TASKS(romanPed);
                CLEAR_CHAR_TASKS(Main.PlayerHandle);

                bobcatVeh = GET_CLOSEST_CAR(1173.762f, 1427.409f, 16.772f, 8f, 0, 70);

                _TASK_ENTER_CAR_AS_DRIVER(Main.PlayerHandle, bobcatVeh, 45000);
                _TASK_ENTER_CAR_AS_PASSENGER(romanPed, bobcatVeh, 45000, 0);

                SET_CAM_FOV(cam, 60);
                SET_CAM_POS(cam, 1169.558f, 1418.649f, 17.308f);
                POINT_CAM_AT_COORD(cam, 1198.204f, 1440.052f, 30.472f);
                GET_GAME_VIEWPORT_ID(out int viewPort);
                CLEAR_ROOM_FOR_VIEWPORT(viewPort);

                GET_GAME_TIMER(out fTimer);

                cutsceneSeq++;
            }
            // Cut back to Niko and Roman entering Bobcat while explosions happen
            else if (cutsceneSeq == 6 && Main.gTimer >= fTimer + 100)
            {
                SET_CAM_FOV(cam, 60);
                SET_CAM_POS(cam, 1169.558f, 1418.649f, 17.308f);
                POINT_CAM_AT_COORD(cam, 1198.204f, 1440.052f, 30.472f);
                GET_GAME_VIEWPORT_ID(out int viewPort);
                CLEAR_ROOM_FOR_VIEWPORT(viewPort);

                ADD_EXPLOSION(new Vector3(1191.55f, 1453.33f, 30.020f), 14, 10, true, false, 0);

                GET_GAME_TIMER(out fTimer);

                cutsceneSeq++;
            }
            // More explosions
            else if (cutsceneSeq == 7 && Main.gTimer >= fTimer + 350)
            {
                ADD_EXPLOSION(new Vector3(1192.0f, 1453.09f, 23.189f), 14, 10, true, false, 0);
                ADD_EXPLOSION(new Vector3(1217.57f, 1441.59f, 16.724f), 14, 10, true, false, 0);

                GET_GAME_TIMER(out fTimer);

                cutsceneSeq++;
            }
            else if (cutsceneSeq == 8 && Main.gTimer >= fTimer + 500)
            {
                ADD_EXPLOSION(new Vector3(1193.63f, 1458.24f, 16.724f), 14, 10, true, false, 0);
                GET_GAME_TIMER(out fTimer);
                cutsceneSeq++;
            }
            // Cut to other angle to hide vehicle teleporting
            else if (cutsceneSeq == 9 && Main.gTimer >= fTimer + 1450)
            {
                CLEAR_CHAR_TASKS_IMMEDIATELY(Main.PlayerHandle);
                SET_CAM_POS(cam, 1232.085f, 1399.366f, 17.40f);
                POINT_CAM_AT_COORD(cam, 1219.187f, 1425.0f, 29.668f);

                ADD_EXPLOSION(new Vector3(1211.0f, 1437.05f, 25.986f), 14, 10, true, false, 0);
                SET_CAM_COMPONENT_SHAKE(cam, (int)CameraShakeType.TRACK_UP_DOWN_2, (int)CameraShakeBehaviour.CONSTANT_PLUS_FADE_IN_OUT, 1000, 0.05f, 5.65f, 0f);
                SET_CAM_COMPONENT_SHAKE(cam, (int)CameraShakeType.TRACK_LEFT_RIGHT_2, (int)CameraShakeBehaviour.CONSTANT_PLUS_FADE_IN_OUT, 1000, 0.05f, 5.05f, 0f);
                SET_CAM_COMPONENT_SHAKE(cam, (int)CameraShakeType.PITCH_UP_DOWN, (int)CameraShakeBehaviour.CONSTANT_PLUS_FADE_IN_OUT, 1000, 0.01f, 5.65f, 0f);
                SET_CAM_COMPONENT_SHAKE(cam, (int)CameraShakeType.YAW_LEFT_RIGHT, (int)CameraShakeBehaviour.CONSTANT_PLUS_FADE_IN_OUT, 1000, 0.01f, 5.05f, 0f);
                GET_GAME_TIMER(out fTimer);
                cutsceneSeq++;
            }
            else if (cutsceneSeq == 10 && Main.gTimer >= fTimer + 50)
            {
                CREATE_CHAR_INSIDE_CAR(bobcatVeh, (int)ePedType.PED_TYPE_CIV_MALE, (uint)GET_HASH_KEY("m_y_gru2_lo_01"), out driverPed);
                SET_CHAR_VISIBLE(driverPed, false);
                SET_LOAD_COLLISION_FOR_CHAR_FLAG(driverPed, false);

                SET_CAR_ENGINE_ON(bobcatVeh, true, true);

                CLEAR_CHAR_TASKS_IMMEDIATELY(Main.PlayerHandle);
                ATTACH_PED_TO_CAR(Main.PlayerHandle, bobcatVeh, 0, -0.25f, 0.0f, 0.1f, 0, 0, 0, false);
                _TASK_PLAY_ANIM_NON_INTERRUPTABLE(Main.PlayerHandle, "sit_drive", "veh@std", 4.0f, 1, 0, 0, 0, -1);
                //WARP_CHAR_INTO_CAR(Main.PlayerHandle, bobcatVeh);

                SET_CAR_COORDINATES(bobcatVeh, 1183.123f, 1460.323f, 16.714f);
                SET_CAR_HEADING(bobcatVeh, 25);

                CLEAR_AREA(1174.183f, 1490.899f, 16.767f, 25.0f, true);

                _TASK_CAR_DRIVE_TO_COORD(driverPed, bobcatVeh, 1174.183f, 1490.899f, 16.767f, 12.0f, 0, 0, 3, 10.0f, -1);

                GET_GAME_TIMER(out fTimer);
                cutsceneSeq++;
            }
            // More explosions
            else if (cutsceneSeq == 11 && Main.gTimer >= fTimer + 600)
            {
                ADD_EXPLOSION(new Vector3(1215.95f, 1468.83f, 30.285f), 14, 10, true, false, 0);
                SET_CAM_COMPONENT_SHAKE(cam, (int)CameraShakeType.TRACK_UP_DOWN_2, (int)CameraShakeBehaviour.CONSTANT_PLUS_FADE_IN_OUT, 1000, 0.05f, 5.65f, 0f);
                SET_CAM_COMPONENT_SHAKE(cam, (int)CameraShakeType.TRACK_LEFT_RIGHT_2, (int)CameraShakeBehaviour.CONSTANT_PLUS_FADE_IN_OUT, 1000, 0.05f, 5.05f, 0f);
                SET_CAM_COMPONENT_SHAKE(cam, (int)CameraShakeType.PITCH_UP_DOWN, (int)CameraShakeBehaviour.CONSTANT_PLUS_FADE_IN_OUT, 1000, 0.01f, 5.65f, 0f);
                SET_CAM_COMPONENT_SHAKE(cam, (int)CameraShakeType.YAW_LEFT_RIGHT, (int)CameraShakeBehaviour.CONSTANT_PLUS_FADE_IN_OUT, 1000, 0.01f, 5.05f, 0f);
                GET_GAME_TIMER(out fTimer);
                cutsceneSeq++;
            }
            else if (cutsceneSeq == 12 && Main.gTimer >= fTimer + 900)
            {
                ADD_EXPLOSION(new Vector3(1219.718f, 1436.617f, 33.532f), 14, 10, true, false, 0);
                ADD_EXPLOSION(new Vector3(1215.95f, 1468.83f, 30.285f), 14, 10, true, false, 0);
                SET_CAM_COMPONENT_SHAKE(cam, (int)CameraShakeType.TRACK_UP_DOWN_2, (int)CameraShakeBehaviour.CONSTANT_PLUS_FADE_IN_OUT, 1500, 0.05f, 5.65f, 0f);
                SET_CAM_COMPONENT_SHAKE(cam, (int)CameraShakeType.TRACK_LEFT_RIGHT_2, (int)CameraShakeBehaviour.CONSTANT_PLUS_FADE_IN_OUT, 1500, 0.05f, 5.05f, 0f);
                SET_CAM_COMPONENT_SHAKE(cam, (int)CameraShakeType.PITCH_UP_DOWN, (int)CameraShakeBehaviour.CONSTANT_PLUS_FADE_IN_OUT, 1500, 0.01f, 5.65f, 0f);
                SET_CAM_COMPONENT_SHAKE(cam, (int)CameraShakeType.YAW_LEFT_RIGHT, (int)CameraShakeBehaviour.CONSTANT_PLUS_FADE_IN_OUT, 1500, 0.01f, 5.05f, 0f);
                GET_GAME_TIMER(out fTimer);
                cutsceneSeq++;
            }
            // Cut back to different angle as Niko and Roman drive off
            else if (cutsceneSeq == 13 && Main.gTimer >= fTimer + 1200)
            {
                //CLEAR_CHAR_TASKS(Main.PlayerHandle);

                _TASK_CAR_DRIVE_TO_COORD(driverPed, bobcatVeh, 1112.442f, 1553.404f, 16.714f, 15.0f, 0, 0, 2, 10.0f, -1);

                SET_CAM_POS(cam, 1153.644f, 1507.655f, 17.905f);
                POINT_CAM_AT_COORD(cam, 1182.595f, 1479.915f, 21.138f);

                CLEAR_AREA(1174.183f, 1490.899f, 16.767f, 50.0f, true);

                ADD_EXPLOSION(new Vector3(1193.21f, 1448.98f, 16.724f), 14, 10, true, false, 0.0f);
                SET_CAM_COMPONENT_SHAKE(cam, (int)CameraShakeType.TRACK_UP_DOWN_2, (int)CameraShakeBehaviour.CONSTANT_PLUS_FADE_IN_OUT, 1000, 0.05f, 5.65f, 0f);
                SET_CAM_COMPONENT_SHAKE(cam, (int)CameraShakeType.TRACK_LEFT_RIGHT_2, (int)CameraShakeBehaviour.CONSTANT_PLUS_FADE_IN_OUT, 1000, 0.05f, 5.05f, 0f);
                SET_CAM_COMPONENT_SHAKE(cam, (int)CameraShakeType.PITCH_UP_DOWN, (int)CameraShakeBehaviour.CONSTANT_PLUS_FADE_IN_OUT, 1000, 0.01f, 5.65f, 0f);
                SET_CAM_COMPONENT_SHAKE(cam, (int)CameraShakeType.YAW_LEFT_RIGHT, (int)CameraShakeBehaviour.CONSTANT_PLUS_FADE_IN_OUT, 1000, 0.01f, 5.05f, 0f);

                GET_GAME_TIMER(out fTimer);
                cutsceneSeq++;
            }
            // More explosions
            else if (cutsceneSeq == 14 && Main.gTimer >= fTimer + 350)
            {
                CLEAR_AREA(1174.183f, 1490.899f, 16.767f, 50.0f, true);
                ADD_EXPLOSION(new Vector3(1230.22f, 1453.5f, 25.986f), 14, 10, true, false, 0.0f);
                SET_CAM_COMPONENT_SHAKE(cam, (int)CameraShakeType.TRACK_UP_DOWN_2, (int)CameraShakeBehaviour.CONSTANT_PLUS_FADE_IN_OUT, 1000, 0.05f, 5.65f, 0f);
                SET_CAM_COMPONENT_SHAKE(cam, (int)CameraShakeType.TRACK_LEFT_RIGHT_2, (int)CameraShakeBehaviour.CONSTANT_PLUS_FADE_IN_OUT, 1000, 0.05f, 5.05f, 0f);
                SET_CAM_COMPONENT_SHAKE(cam, (int)CameraShakeType.PITCH_UP_DOWN, (int)CameraShakeBehaviour.CONSTANT_PLUS_FADE_IN_OUT, 1000, 0.01f, 5.65f, 0f);
                SET_CAM_COMPONENT_SHAKE(cam, (int)CameraShakeType.YAW_LEFT_RIGHT, (int)CameraShakeBehaviour.CONSTANT_PLUS_FADE_IN_OUT, 1000, 0.01f, 5.05f, 0f);

                GET_GAME_TIMER(out fTimer);
                cutsceneSeq++;
            }
            else if (cutsceneSeq == 15 && Main.gTimer >= fTimer + 300)
            {
                ADD_EXPLOSION(new Vector3(1195.76F, 1469.95f, 30.285f), 14, 10, true, false, 0.0f);
                SET_CAM_COMPONENT_SHAKE(cam, (int)CameraShakeType.TRACK_UP_DOWN_2, (int)CameraShakeBehaviour.CONSTANT_PLUS_FADE_IN_OUT, 3000, 0.04f, 5.65f, 0f);
                SET_CAM_COMPONENT_SHAKE(cam, (int)CameraShakeType.TRACK_LEFT_RIGHT_2, (int)CameraShakeBehaviour.CONSTANT_PLUS_FADE_IN_OUT, 3000, 0.04f, 5.05f, 0f);
                SET_CAM_COMPONENT_SHAKE(cam, (int)CameraShakeType.PITCH_UP_DOWN, (int)CameraShakeBehaviour.CONSTANT_PLUS_FADE_IN_OUT, 3000, 0.0075f, 5.65f, 0f);
                SET_CAM_COMPONENT_SHAKE(cam, (int)CameraShakeType.YAW_LEFT_RIGHT, (int)CameraShakeBehaviour.CONSTANT_PLUS_FADE_IN_OUT, 3000, 0.0075f, 5.05f, 0f);

                GET_GAME_TIMER(out fTimer);
                cutsceneSeq++;
            }
            // Fade to black
            else if (cutsceneSeq == 16 && Main.gTimer >= fTimer + 4000)
            {
                DO_SCREEN_FADE_OUT(1000);
                GET_GAME_TIMER(out fTimer);

                cutsceneSeq++;
            }
            // End cutscene
            else if (cutsceneSeq == 17 && IS_SCREEN_FADED_OUT())
            {
                DELETE_CHAR(ref driverPed);
                CLEAR_CHAR_TASKS_IMMEDIATELY(Main.PlayerHandle);
                DETACH_PED(Main.PlayerHandle, true);
                WARP_CHAR_INTO_CAR(Main.PlayerHandle, bobcatVeh);
                DISPLAY_HUD(true);
                DISPLAY_RADAR(true);
                SET_PLAYER_CONTROL(Main.PlayerIndex, true);
                SET_CAR_FORWARD_SPEED(bobcatVeh, 8);

                ACTIVATE_SCRIPTED_CAMS(false, false);
                DESTROY_ALL_CAMS();
                cutsceneSeq++;

                DO_SCREEN_FADE_IN(1000);
            }

            if (cutsceneSeq < 17)
            {
                if (IS_THIS_PRINT_BEING_DISPLAYED("FS_21", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0))
                    CLEAR_PRINTS();
                if (cutsceneSeq >= 10)
                    WARP_CHAR_INTO_CAR_AS_PASSENGER(romanPed, bobcatVeh, 0);
            }
        }
        private static void ProcessFailCutscene()
        {
            if (!failCutStart)
                return;

            // Initialize cutscene
            if (cutsceneSeq <= 0)
            {
                CLEAR_PRINTS();
                DISPLAY_HUD(false);
                DISPLAY_RADAR(false);

                CREATE_CAM(14, out cam);
                SET_CAM_FOV(cam, 45);

                GET_OFFSET_FROM_OBJECT_IN_WORLD_COORDS(bombProp, new Vector3(0, 0, 1.25f), out Vector3 camOff);
                SET_CAM_POS(cam, camOff);
                POINT_CAM_AT_OBJECT(cam, bombProp);
                //POINT_CAM_AT_COORD(cam, -172.590f, 1374.672f, 34.198f);

                SET_CAM_ACTIVE(cam, true);
                SET_CAM_PROPAGATE(cam, true);
                ACTIVATE_SCRIPTED_CAMS(true, true);

                plyrCoords = Main.PlayerPos;

                GET_GAME_TIMER(out fTimer);
                if (!IS_CHAR_IN_AREA_3D(Main.PlayerHandle, 1187.697f, 1433.195f, 15.761f, 1233.866f, 1473.360f, 52.156f, false))
                {
                    CLEAR_CHAR_TASKS_IMMEDIATELY(Main.PlayerHandle);
                    _SET_CHAR_COORDINATES_NO_OFFSET(Main.PlayerHandle, 1190.602f, 1447.547f, 30.285f);
                    SET_CHAR_HEADING(Main.PlayerHandle, 215);
                }

                cutsceneSeq++;
            }
            // Show bomb
            else if (cutsceneSeq == 1)
            {
                GET_GAME_VIEWPORT_ID(out int viewPort);
                SET_ROOM_FOR_VIEWPORT_BY_NAME(viewPort, "Factory_Room04");

                _TASK_GO_STRAIGHT_TO_COORD(Main.PlayerHandle, 1195.946f, 1440.589f, 30.305f, 4, 45000);

                GET_GAME_TIMER(out fTimer);
                GET_GAME_TIMER(out aTimer);
                beepTime = 400;

                cutsceneSeq++;
            }
            else if (cutsceneSeq == 2)
            {
                bombColor = Color.Green;
                if (Main.gTimer >= aTimer + beepTime)
                {
                    int soundID = GET_SOUND_ID();
                    PLAY_SOUND_FROM_POSITION(soundID, "F5_TRUCK_ARSON_BOMB_BLEEP", 1203.175f, 1438.862f, 30.020f);
                    RELEASE_SOUND_ID(soundID);
                    GET_GAME_TIMER(out aTimer);
                    bombColor = Color.Red;

                    beepTime -= 40;
                    if (beepTime < 40)
                        beepTime = 40;
                }
                if (Main.gTimer >= fTimer + 3000)
                {
                    ADD_EXPLOSION(new Vector3(1203.205f, 1438.862f, 30.020f), 2, 10, true, false, 0);

                    GET_GAME_TIMER(out fTimer);

                    cutsceneSeq++;
                }
                DRAW_LIGHT_WITH_RANGE(1203.375f, 1438.862f, 30.120f, bombColor.R, bombColor.G, bombColor.B, 10f, 25.0f);
            }
            // Explode bomb
            else if (cutsceneSeq == 3 && Main.gTimer >= fTimer + 100)
            {
                _SET_CHAR_COORDINATES_NO_OFFSET(Main.PlayerHandle, plyrCoords.X, plyrCoords.Y, plyrCoords.Z);
                SET_CAM_FOV(cam, 60);
                SET_CAM_POS(cam, 1169.558f, 1418.649f, 17.308f);
                POINT_CAM_AT_COORD(cam, 1198.204f, 1440.052f, 30.472f);
                GET_GAME_VIEWPORT_ID(out int viewPort);
                CLEAR_ROOM_FOR_VIEWPORT(viewPort);

                ADD_EXPLOSION(new Vector3(1191.55f, 1453.33f, 30.020f), 14, 10, true, false, 0);

                GET_GAME_TIMER(out fTimer);

                cutsceneSeq++;
            }
            else if (cutsceneSeq == 4 && Main.gTimer >= fTimer + 250)
            {
                if (DOES_CHAR_EXIST(romanPed) && IS_CHAR_IN_AREA_3D(romanPed, 1187.697f, 1433.195f, 15.761f, 1233.866f, 1473.360f, 52.156f, false))
                {
                    GET_CHAR_COORDINATES(romanPed, out Vector3 romanPos);
                    DAMAGE_CHAR(romanPed, 500, false);
                    ADD_EXPLOSION(romanPos, 14, 10, true, false, 1.0f);
                }

                ADD_EXPLOSION(new Vector3(1217.57f, 1441.59f, 16.724f), 14, 10, true, false, 0);
                ADD_EXPLOSION(new Vector3(1193.63f, 1458.24f, 16.724f), 14, 10, true, false, 0);

                GET_GAME_TIMER(out fTimer);

                cutsceneSeq++;
            }
            // Kill player if in building
            else if (cutsceneSeq == 5 && Main.gTimer >= fTimer + 500)
            {
                if (IS_CHAR_IN_AREA_3D(Main.PlayerHandle, 1187.697f, 1433.195f, 15.761f, 1233.866f, 1473.360f, 52.156f, false))
                {
                    GET_CHAR_COORDINATES(Main.PlayerHandle, out Vector3 plyrPos);
                    DAMAGE_CHAR(Main.PlayerHandle, 500, false);
                    ADD_EXPLOSION(plyrPos, 14, 10, true, false, 1.0f);
                }

                GET_GAME_TIMER(out fTimer);

                cutsceneSeq++;
            }
            else if (cutsceneSeq == 6 && Main.gTimer >= fTimer + 300)
            {
                ADD_EXPLOSION(new Vector3(1193.63f, 1458.24f, 16.724f), 14, 10, true, false, 0);

                GET_GAME_TIMER(out fTimer);

                cutsceneSeq++;
            }
            else if (cutsceneSeq == 7 && Main.gTimer >= fTimer + 4000 && !IS_CHAR_DEAD(Main.PlayerHandle))
            {
                DO_SCREEN_FADE_OUT(1000);
                GET_GAME_TIMER(out fTimer);

                cutsceneSeq++;
            }
            else if ((cutsceneSeq == 8 && !IS_SCREEN_FADED_OUT()))
            {
                DISPLAY_HUD(true);
                DISPLAY_RADAR(true);
                SET_PLAYER_CONTROL(Main.PlayerIndex, true);

                ACTIVATE_SCRIPTED_CAMS(false, false);
                DESTROY_ALL_CAMS();
                cutsceneSeq++;

                DO_SCREEN_FADE_IN(1000);
            }
            if (cutsceneSeq < 8 && !IS_CHAR_DEAD(Main.PlayerHandle))
            {
                if (cutsceneSeq >= 6)
                    SET_PLAYER_CONTROL(Main.PlayerIndex, false);
                CLEAR_PRINTS();
                ACTIVATE_SCRIPTED_CAMS(true, true);
            }
        }
    }
}
