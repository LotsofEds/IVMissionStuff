using CCL;
using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using IVSDKDotNet.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static IVSDKDotNet.Native.Natives;

namespace MissionStuff.ivsdk
{
    internal class BuoysAhoy
    {
        // IniShit
        private static int plyrWeapon;
        private static int enemyWeaponA;
        private static int enemyWeaponB;
        private static int enemyAccuracy;
        private static int enemyFireRate;
        private static uint enemyHealth;
        private static string newMessage;
        private static string warnMessage;

        // BooleShit
        private static bool switchSeats;
        private static bool startChase;
        private static bool triggerCutscene;
        private static bool warnPlayer;
        private static bool endCutscene;

        // ArrayShit
        private static int[] enemyVehicles = new int[16];
        private static int[] driverPeds = new int[16];
        private static int[] shooterPeds = new int[16];
        private static int[] landEnemyPeds = new int[9];

        // OtherShit
        private static int pVeh;
        private static int bernieCheckPoint;
        private static int barrelOne;
        private static int barrelTwo;

        private static int pedCheckpoint;
        private static int berniePed;
        private static int taskStatus;

        private static uint pWeapSlot = 0;
        private static int pWeap = 0;
        private static int pAmmo1;
        private static int pAmmo2;

        // CutsceneShit
        private static int cam;
        private static int cam2;
        private static int interpCam;
        private static uint fTimer;
        public static void UnInit()
        {
            for (int i = 0; i < 16; i++)
            {
                DELETE_CAR(ref enemyVehicles[i]);
                DELETE_CHAR(ref driverPeds[i]);
                DELETE_CHAR(ref shooterPeds[i]);
            }
            for (int i = 0; i < 9; i++)
            {
                DELETE_CHAR(ref landEnemyPeds[i]);
            }
            MARK_MODEL_AS_NO_LONGER_NEEDED(GET_HASH_KEY("squalo"));
            MARK_MODEL_AS_NO_LONGER_NEEDED(GET_HASH_KEY("jetmax"));
            MARK_MODEL_AS_NO_LONGER_NEEDED(GET_HASH_KEY("bm_drum_fla2"));
            MARK_MODEL_AS_NO_LONGER_NEEDED(GET_HASH_KEY("m_y_gru2_lo_01"));
            MARK_MODEL_AS_NO_LONGER_NEEDED(GET_HASH_KEY("m_m_gru2_lo_02"));

            MARK_OBJECT_AS_NO_LONGER_NEEDED(barrelOne);
            MARK_OBJECT_AS_NO_LONGER_NEEDED(barrelTwo);

            if (pWeapSlot != 2 && pWeapSlot != 4 && pWeapSlot != 5 && pWeapSlot > 0)
                IVWeaponInfo.GetWeaponInfo((uint)plyrWeapon).WeaponSlot = pWeapSlot;

            SET_CHAR_CAN_BE_SHOT_IN_VEHICLE(Main.PlayerHandle, true);

            if (!HAS_CHAR_GOT_WEAPON(Main.PlayerHandle, pWeap) && pWeap > 0)
                GIVE_WEAPON_TO_CHAR(Main.PlayerHandle, pWeap, pAmmo1, false);
            taskStatus = 0;
            bernieCheckPoint = -1;
            switchSeats = false;
            startChase = false;
            triggerCutscene = false;
            warnPlayer = false;
            endCutscene = false;
        }
        public static void Init(SettingsFile settings)
        {
            plyrWeapon = settings.GetInteger("MAIN", "BAWeaponToGive", 15);
            enemyWeaponA = settings.GetInteger("MAIN", "BAEnemyWeapon1", 13);
            enemyWeaponB = settings.GetInteger("MAIN", "BAEnemyWeapon2", 14);
            enemyAccuracy = settings.GetInteger("MAIN", "BAEnemyAccuracy", 70);
            enemyFireRate = settings.GetInteger("MAIN", "BAEnemyFireRate", 60);
            enemyHealth = settings.GetUInteger("MAIN", "BAEnemyHealth", 145);
            newMessage = settings.GetValue("MAIN", "BAMessage", "");
            warnMessage = settings.GetValue("MAIN", "BAWarning", "");
        }
        public static void ChaseEndCutscene()
        {
            if (!DOES_CAM_EXIST(cam))
            {
                DISPLAY_HUD(false);
                DISPLAY_RADAR(false);
                SET_PLAYER_CONTROL(Main.PlayerIndex, false);
                for (int i = 13; i < 15; i++)
                {
                    if (!IS_CHAR_INJURED(shooterPeds[i]))
                        SET_CHAR_HEALTH(shooterPeds[i], 105);
                    if (!IS_CHAR_INJURED(driverPeds[i]))
                        SET_CHAR_HEALTH(driverPeds[i], 105);
                }
                SET_CAR_PROOFS(pVeh, true, true, true, true, true);
                CREATE_CAM(14, out cam);
                ATTACH_CAM_TO_VEHICLE(cam, pVeh);
                SET_CAM_ATTACH_OFFSET_IS_RELATIVE(cam, true);
                SET_CAM_ATTACH_OFFSET(cam, 0, -4.5f, 1.25f);
                SET_CAM_FOV(cam, 60);
                POINT_CAM_AT_VEHICLE(cam, pVeh);
                SET_CAM_ACTIVE(cam, true);
                SET_CAM_PROPAGATE(cam, true);
                ACTIVATE_SCRIPTED_CAMS(true, true);

                CREATE_CAM(14, out cam2);
                //SET_CAM_PROPAGATE(cam2, true);
                CREATE_CAM(3, out interpCam);

                /*SET_CHAR_WILL_DO_DRIVEBYS(Main.PlayerHandle, true);
                SET_AMMO_IN_CLIP(Main.PlayerHandle, plyrWeapon, 20);
                if (!IS_CHAR_INJURED(shooterPeds[13]))
                    _TASK_SHOOT_AT_CHAR(Main.PlayerHandle, shooterPeds[13], 4000, (int)eShootMode.SHOOT_MODE_CONTINUOUS);
                //_TASK_DRIVE_BY(Main.PlayerHandle, shooterPeds[13], 1, 0, 0, 0, 540, 8, true, 45000);
                //_TASK_COMBAT(Main.PlayerHandle, shooterPeds[13]);
                else if (!IS_CHAR_INJURED(shooterPeds[14]))
                    _TASK_SHOOT_AT_CHAR(Main.PlayerHandle, shooterPeds[14], 4000, (int)eShootMode.SHOOT_MODE_CONTINUOUS);
                //_TASK_DRIVE_BY(Main.PlayerHandle, shooterPeds[14], 1, 0, 0, 0, 540, 8, true, 45000);
                //_TASK_COMBAT(Main.PlayerHandle, shooterPeds[14]);
                else if (!IS_CHAR_INJURED(driverPeds[13]) && !IS_PED_IN_COMBAT(Main.PlayerHandle))
                    _TASK_SHOOT_AT_CHAR(Main.PlayerHandle, driverPeds[13], 4000, (int)eShootMode.SHOOT_MODE_CONTINUOUS);
                //_TASK_DRIVE_BY(Main.PlayerHandle, driverPeds[13], 1, 0, 0, 0, 540, 8, true, 45000);
                //_TASK_COMBAT(Main.PlayerHandle, driverPeds[13]);
                else if (!IS_CHAR_INJURED(driverPeds[14]) && !IS_PED_IN_COMBAT(Main.PlayerHandle))
                    _TASK_SHOOT_AT_CHAR(Main.PlayerHandle, driverPeds[14], 4000, (int)eShootMode.SHOOT_MODE_CONTINUOUS);
                //_TASK_DRIVE_BY(Main.PlayerHandle, driverPeds[14], 1, 0, 0, 0, 540, 8, true, 45000);
                //_TASK_COMBAT(Main.PlayerHandle, driverPeds[14]);*/

                GET_GAME_TIMER(out fTimer);
            }
            else
            {
                if (Main.gTimer >= fTimer + 4000 && !endCutscene)
                {
                    for (int i = 0; i < 16; i++)
                    {
                        DELETE_CAR(ref enemyVehicles[i]);
                        DELETE_CHAR(ref driverPeds[i]);
                        DELETE_CHAR(ref shooterPeds[i]);
                    }
                    for (int i = 0; i < 9; i++)
                    {
                        DELETE_CHAR(ref landEnemyPeds[i]);
                    }
                    ATTACH_CAM_TO_VEHICLE(cam2, pVeh);
                    SET_CAM_ATTACH_OFFSET_IS_RELATIVE(cam2, true);
                    SET_CAM_ATTACH_OFFSET(cam2, 0, -4.0f, 1.25f);
                    SET_CAM_FOV(cam2, 30);
                    POINT_CAM_AT_COORD(cam2, 854.2511f, 931.4077f, 6.9515f);

                    SET_CAM_ACTIVE(interpCam, true);
                    SET_CAM_PROPAGATE(interpCam, true);
                    ACTIVATE_SCRIPTED_CAMS(true, true);
                    SET_CAM_INTERP_STYLE_CORE(interpCam, cam, cam2, 3000, false);
                    endCutscene = true;
                }
                else if (endCutscene && Main.gTimer >= fTimer + 7000)
                {
                    SET_CAM_ACTIVE(cam, false);
                    SET_CAM_ACTIVE(interpCam, false);
                    SET_CAM_ACTIVE(cam2, true);
                    SET_CAM_PROPAGATE(cam2, true);
                    ACTIVATE_SCRIPTED_CAMS(true, true);
                    if (Main.gTimer >= fTimer + 8000)
                    {
                        SET_INTERP_FROM_SCRIPT_TO_GAME(true, 2000);
                        //DESTROY_ALL_CAMS();
                        ACTIVATE_SCRIPTED_CAMS(false, false);
                        SET_GAME_CAM_PITCH(0.0f);
                        SET_GAME_CAM_HEADING(0.0f);
                        SET_CAM_BEHIND_PED(Main.PlayerHandle);
                        //if (IVMenuManager.HudOn)
                        DISPLAY_HUD(true);
                        DISPLAY_RADAR(true);
                        SET_PLAYER_CONTROL(Main.PlayerIndex, true);

                        triggerCutscene = false;
                    }
                }
            }
        }
        public static void ProcessChase()
        {
            ALTER_WANTED_LEVEL(Main.PlayerIndex, 0);
            APPLY_WANTED_LEVEL_CHANGE_NOW(Main.PlayerIndex);

            GET_SCRIPT_TASK_STATUS(berniePed, 15, out taskStatus);
            //IVGame.ShowSubtitleMessage(bernieCheckPoint.ToString());

            IVVehicle ass = IVVehicle.FromUIntPtr(Main.PlayerPed.GetVehicle());
            if (!warnPlayer && (GET_ENGINE_HEALTH(pVeh) < 200 || ass.PetrolTankHealth < 200))
            {
                //IVGame.ShowSubtitleMessage(GET_ENGINE_HEALTH(pVeh).ToString() + "  " + GET_PETROL_TANK_HEALTH(pVeh).ToString());
                warnPlayer = true;
                IVText.TheIVText.ReplaceTextOfTextLabel("PLACEHOLDERSL", warnMessage);
                PRINT_HELP("PLACEHOLDERSL");
            }
            if (IS_THIS_PRINT_BEING_DISPLAYED("BER3_GOD2", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0))
                IVGame.ShowSubtitleMessage(newMessage, 5000);

            for (int i = 0; i < 16; i++)
            {
                if (!DOES_CHAR_EXIST(shooterPeds[i]))
                    continue;

                if (!DOES_CHAR_EXIST(driverPeds[i]))
                    continue;

                SET_BLOCKING_OF_NON_TEMPORARY_EVENTS(shooterPeds[i], true);
                SET_CHAR_WILL_DO_DRIVEBYS(shooterPeds[i], true);
                SET_CHAR_RELATIONSHIP_GROUP(shooterPeds[i], (int)eRelationshipGroup.RELATIONSHIP_GROUP_MISSION_2);

                SET_CHAR_NOT_DAMAGED_BY_RELATIONSHIP_GROUP(shooterPeds[i], (int)eRelationshipGroup.RELATIONSHIP_GROUP_MISSION_2, true);
                SET_CHAR_SUFFERS_CRITICAL_HITS(shooterPeds[i], false);

                if (!IS_PED_IN_COMBAT(shooterPeds[i]) && !IS_CHAR_INJURED(shooterPeds[i]))
                {
                    SET_CHAR_HEALTH(driverPeds[i], enemyHealth);
                    SET_CHAR_HEALTH(shooterPeds[i], enemyHealth);
                    _TASK_COMBAT(shooterPeds[i], Main.PlayerHandle);
                    SET_CHAR_ACCURACY(shooterPeds[i], (uint)enemyAccuracy);
                    SET_CHAR_SHOOT_RATE(shooterPeds[i], enemyFireRate);
                }

                SET_BLOCKING_OF_NON_TEMPORARY_EVENTS(driverPeds[i], true);
                SET_CHAR_RELATIONSHIP_GROUP(driverPeds[i], (int)eRelationshipGroup.RELATIONSHIP_GROUP_MISSION_2);
                SET_CHAR_NOT_DAMAGED_BY_RELATIONSHIP_GROUP(driverPeds[i], (int)eRelationshipGroup.RELATIONSHIP_GROUP_MISSION_2, true);

                SET_CHAR_SUFFERS_CRITICAL_HITS(driverPeds[i], false);

                GET_SCRIPT_TASK_STATUS(driverPeds[i], 15, out int enemyTaskStatus);
                if (enemyTaskStatus == 7)
                {
                    CLEAR_CHAR_TASKS(driverPeds[i]);
                    _TASK_CAR_DRIVE_TO_COORD(driverPeds[i], enemyVehicles[i], Main.PlayerPos.X, Main.PlayerPos.Y, Main.PlayerPos.Z, 25.0f, 1, 0, 0, 20.0f, 45000);

                    if (i == 5)
                    {
                        if (pedCheckpoint == 0)
                        {
                            _TASK_CAR_DRIVE_TO_COORD(driverPeds[i], enemyVehicles[i], 418.805f, -195.915f, 0, 20.0f, 1, 0, 0, 5.0f, 45000);
                            pedCheckpoint++;
                        }
                    }
                    if (i == 6)
                    {
                        if (pedCheckpoint == 1)
                            _TASK_CAR_DRIVE_TO_COORD(driverPeds[i], enemyVehicles[i], 410.305f, -194.415f, 0, 18.0f, 1, 0, 0, 5.0f, 45000);

                        else if (pedCheckpoint == 2)
                            EXPLODE_CAR(enemyVehicles[i], true, false);
                        pedCheckpoint++;
                    }
                }
            }

            for (int i = 0; i < 9; i++)
            {
                if (!DOES_CHAR_EXIST(landEnemyPeds[i]))
                    continue;

                SET_BLOCKING_OF_NON_TEMPORARY_EVENTS(landEnemyPeds[i], true);
                SET_CHAR_RELATIONSHIP_GROUP(landEnemyPeds[i], (int)eRelationshipGroup.RELATIONSHIP_GROUP_MISSION_2);
                BLEND_OUT_CHAR_MOVE_ANIMS(landEnemyPeds[i]);

                SET_CHAR_NOT_DAMAGED_BY_RELATIONSHIP_GROUP(landEnemyPeds[i], (int)eRelationshipGroup.RELATIONSHIP_GROUP_MISSION_2, true);
                SET_CHAR_SUFFERS_CRITICAL_HITS(landEnemyPeds[i], false);

                if (!IS_PED_IN_COMBAT(landEnemyPeds[i]))
                {
                    //FREEZE_CHAR_POSITION(landEnemyPeds[i], true);
                    //SET_CHAR_RELATIONSHIP(landEnemyPeds[i], (int)eRelationship.RELATIONSHIP_HATE, (int)eRelationshipGroup.RELATIONSHIP_GROUP_PLAYER);
                    _TASK_COMBAT(landEnemyPeds[i], Main.PlayerHandle);
                    SET_CHAR_ACCURACY(landEnemyPeds[i], (uint)enemyAccuracy);
                    SET_CHAR_SHOOT_RATE(landEnemyPeds[i], enemyFireRate);
                }
            }

            if (bernieCheckPoint == -1 && DOES_VEHICLE_EXIST(enemyVehicles[0]))
            {
                SET_CAR_HEADING(enemyVehicles[0], 90);
                //SET_CAR_HEADING(enemyVehicles[1], 90);

                CREATE_CHAR_INSIDE_CAR(enemyVehicles[0], (int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, (uint)GET_HASH_KEY("m_y_gru2_lo_01"), out driverPeds[0]);
                //CREATE_CHAR_INSIDE_CAR(enemyVehicles[1], (int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, (uint)GET_HASH_KEY("m_y_gru2_lo_01"), out driverPeds[1]);

                CREATE_CHAR_AS_PASSENGER(enemyVehicles[0], (int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, (uint)GET_HASH_KEY("m_y_gru2_lo_01"), 0, out shooterPeds[0]);
                //CREATE_CHAR_AS_PASSENGER(enemyVehicles[1], (int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, (uint)GET_HASH_KEY("m_y_gru2_lo_01"), 0, out shooterPeds[1]);

                _TASK_CAR_DRIVE_TO_COORD(driverPeds[0], enemyVehicles[0], 900.053f, -892.568f, 0.0f, 25.0f, 1, 0, 0, 20.0f, 45000);
                //_TASK_CAR_DRIVE_TO_COORD(driverPeds[1], enemyVehicles[1], 900.053f, -892.568f, 0.0f, 25.0f, 1, 0, 0, 20.0f, 45000);

                GIVE_WEAPON_TO_CHAR(shooterPeds[0], enemyWeaponA, 9999, true);
                //GIVE_WEAPON_TO_CHAR(shooterPeds[1], 7, 9999, true);

                bernieCheckPoint = 0;
            }
            if (taskStatus == 7)
            {
                if (bernieCheckPoint < 19)
                {
                    CLEAR_CHAR_TASKS(berniePed);
                    if (bernieCheckPoint == 0)
                    {
                        _TASK_CAR_DRIVE_TO_COORD(berniePed, pVeh, 900.053f, -892.568f, 0.0f, 25.0f, 1, 0, 0, 20.0f, 45000);

                        CREATE_CAR(GET_HASH_KEY("jetmax"), 827.708f, -949.809f, 0, out enemyVehicles[1], true);
                        SET_CAR_HEADING(enemyVehicles[1], 45);
                    }
                    else if (bernieCheckPoint == 1)
                    {
                        _TASK_CAR_DRIVE_TO_COORD(berniePed, pVeh, 782.036f, -862.989f, 0.0f, 25.0f, 1, 0, 0, 20.0f, 45000);

                        CREATE_CHAR_INSIDE_CAR(enemyVehicles[1], (int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, (uint)GET_HASH_KEY("m_y_gru2_lo_01"), out driverPeds[1]);
                        CREATE_CHAR_AS_PASSENGER(enemyVehicles[1], (int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, (uint)GET_HASH_KEY("m_m_gru2_lo_02"), 0, out shooterPeds[1]);

                        _TASK_CAR_DRIVE_TO_COORD(driverPeds[1], enemyVehicles[1], 782.036f, -862.989f, 0.0f, 25.0f, 1, 0, 0, 20.0f, 45000);
                        GIVE_WEAPON_TO_CHAR(shooterPeds[1], enemyWeaponB, 9999, true);

                        CREATE_CAR(GET_HASH_KEY("jetmax"), 718.853f, -897.707f, 0, out enemyVehicles[2], true);
                        SET_CAR_HEADING(enemyVehicles[2], 45);
                    }
                    else if (bernieCheckPoint == 2)
                    {
                        _TASK_CAR_DRIVE_TO_COORD(berniePed, pVeh, 680.764f, -831.553f, 0.0f, 25.0f, 1, 0, 0, 20.0f, 45000);

                        CREATE_CHAR_INSIDE_CAR(enemyVehicles[2], (int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, (uint)GET_HASH_KEY("m_m_gru2_lo_02"), out driverPeds[2]);
                        CREATE_CHAR_AS_PASSENGER(enemyVehicles[2], (int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, (uint)GET_HASH_KEY("m_m_gru2_lo_02"), 0, out shooterPeds[2]);

                        _TASK_CAR_DRIVE_TO_COORD(driverPeds[2], enemyVehicles[2], 680.764f, -831.553f, 0.0f, 25.0f, 1, 0, 0, 20.0f, 45000);
                        GIVE_WEAPON_TO_CHAR(shooterPeds[2], enemyWeaponB, 9999, true);

                        // 795.732, -663.598, 90 -- 651.395, -597.315
                        CREATE_CAR(GET_HASH_KEY("jetmax"), 795.732f, -663.598f, 0, out enemyVehicles[3], true);
                        SET_CAR_HEADING(enemyVehicles[3], 320);

                        // 734.611, -565.419, 125
                        CREATE_CAR(GET_HASH_KEY("squalo"), 734.611f, -565.419f, 0, out enemyVehicles[4], true);
                        SET_CAR_HEADING(enemyVehicles[4], 125);
                    }
                    else if (bernieCheckPoint == 3)
                    {
                        _TASK_CAR_DRIVE_TO_COORD(berniePed, pVeh, 580.851f, -508.069f, 0.0f, 20.0f, 1, 0, 0, 20.0f, 45000);

                        CREATE_CHAR_INSIDE_CAR(enemyVehicles[3], (int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, (uint)GET_HASH_KEY("m_m_gru2_lo_02"), out driverPeds[3]);
                        CREATE_CHAR_AS_PASSENGER(enemyVehicles[3], (int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, (uint)GET_HASH_KEY("m_y_gru2_lo_01"), 0, out shooterPeds[3]);

                        _TASK_CAR_DRIVE_TO_COORD(driverPeds[3], enemyVehicles[3], 580.851f, -508.069f, 0.0f, 18.0f, 1, 0, 0, 20.0f, 45000);
                        GIVE_WEAPON_TO_CHAR(shooterPeds[3], enemyWeaponA, 9999, true);

                        CREATE_CHAR_INSIDE_CAR(enemyVehicles[4], (int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, (uint)GET_HASH_KEY("m_y_gru2_lo_01"), out driverPeds[4]);
                        CREATE_CHAR_AS_PASSENGER(enemyVehicles[4], (int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, (uint)GET_HASH_KEY("m_m_gru2_lo_02"), 0, out shooterPeds[4]);

                        _TASK_CAR_DRIVE_TO_COORD(driverPeds[4], enemyVehicles[4], 580.851f, -508.069f, 0.0f, 14.0f, 1, 0, 0, 17.0f, 45000);
                        GIVE_WEAPON_TO_CHAR(shooterPeds[4], enemyWeaponB, 9999, true);

                        // 411.376, -434.724, 270
                        CREATE_CAR(GET_HASH_KEY("squalo"), 411.376f, -434.724f, 0, out enemyVehicles[5], true);
                        SET_CAR_HEADING(enemyVehicles[5], 270);

                        MARK_CHAR_AS_NO_LONGER_NEEDED(driverPeds[0]);
                        //MARK_CHAR_AS_NO_LONGER_NEEDED(driverPeds[1]);
                        MARK_CHAR_AS_NO_LONGER_NEEDED(shooterPeds[0]);
                        //MARK_CHAR_AS_NO_LONGER_NEEDED(shooterPeds[1]);

                        MARK_CAR_AS_NO_LONGER_NEEDED(enemyVehicles[0]);
                        //MARK_CAR_AS_NO_LONGER_NEEDED(enemyVehicles[1]);
                    }
                    else if (bernieCheckPoint == 4)
                    {
                        _TASK_CAR_DRIVE_TO_COORD(berniePed, pVeh, 482.485f, -436.772f, 0.0f, 20.0f, 1, 0, 0, 20.0f, 45000);

                        // 410.090, -380.657, 270
                        CREATE_CAR(GET_HASH_KEY("squalo"), 410.090f, -380.657f, 0, out enemyVehicles[6], true);
                        SET_CAR_HEADING(enemyVehicles[6], 270);

                        MARK_CHAR_AS_NO_LONGER_NEEDED(driverPeds[1]);
                        MARK_CHAR_AS_NO_LONGER_NEEDED(shooterPeds[1]);

                        MARK_CAR_AS_NO_LONGER_NEEDED(enemyVehicles[1]);
                    }
                    else if (bernieCheckPoint == 5)
                    {
                        _TASK_CAR_DRIVE_TO_COORD(berniePed, pVeh, 460.292f, -310.086f, 0.0f, 20.0f, 1, 0, 0, 20.0f, 45000);

                        CREATE_CHAR_INSIDE_CAR(enemyVehicles[5], (int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, (uint)GET_HASH_KEY("m_y_gru2_lo_01"), out driverPeds[5]);
                        CREATE_CHAR_AS_PASSENGER(enemyVehicles[5], (int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, (uint)GET_HASH_KEY("m_m_gru2_lo_02"), 0, out shooterPeds[5]);

                        _TASK_CAR_DRIVE_TO_COORD(driverPeds[5], enemyVehicles[5], 435.292f, -431.086f, 0.0f, 15.0f, 1, 0, 0, 10.0f, 45000);
                        GIVE_WEAPON_TO_CHAR(shooterPeds[5], enemyWeaponB, 9999, true);

                        CREATE_CHAR_INSIDE_CAR(enemyVehicles[6], (int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, (uint)GET_HASH_KEY("m_y_gru2_lo_01"), out driverPeds[6]);
                        CREATE_CHAR_AS_PASSENGER(enemyVehicles[6], (int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, (uint)GET_HASH_KEY("m_m_gru2_lo_02"), 0, out shooterPeds[6]);

                        _TASK_CAR_DRIVE_TO_COORD(driverPeds[6], enemyVehicles[6], 410.292f, -310.086f, 0.0f, 15.0f, 1, 0, 0, 20.0f, 45000);
                        GIVE_WEAPON_TO_CHAR(shooterPeds[6], enemyWeaponA, 9999, true);
                        pedCheckpoint = 0;

                        // 472.001, -211.134, 30
                        CREATE_CAR(GET_HASH_KEY("squalo"), 472.001f, -211.134f, 0, out enemyVehicles[7], true);
                        SET_CAR_HEADING(enemyVehicles[7], 30);

                        MARK_CHAR_AS_NO_LONGER_NEEDED(driverPeds[2]);
                        MARK_CHAR_AS_NO_LONGER_NEEDED(shooterPeds[2]);

                        MARK_CAR_AS_NO_LONGER_NEEDED(enemyVehicles[2]);
                    }
                    else if (bernieCheckPoint == 6)
                    {
                        _TASK_CAR_DRIVE_TO_COORD(berniePed, pVeh, 414.805f, -196.915f, 0.0f, 25.0f, 1, 0, 0, 10.0f, 45000);

                        CREATE_CHAR_INSIDE_CAR(enemyVehicles[7], (int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, (uint)GET_HASH_KEY("m_y_gru2_lo_01"), out driverPeds[7]);
                        CREATE_CHAR_AS_PASSENGER(enemyVehicles[7], (int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, (uint)GET_HASH_KEY("m_y_gru2_lo_01"), 0, out shooterPeds[7]);

                        _TASK_CAR_DRIVE_TO_COORD(driverPeds[7], enemyVehicles[7], 376.749f, -81.512f, 0.0f, 11.0f, 1, 0, 0, 20.0f, 45000);
                        GIVE_WEAPON_TO_CHAR(shooterPeds[7], enemyWeaponB, 9999, true);

                        MARK_CHAR_AS_NO_LONGER_NEEDED(driverPeds[3]);
                        MARK_CHAR_AS_NO_LONGER_NEEDED(shooterPeds[3]);

                        MARK_CAR_AS_NO_LONGER_NEEDED(enemyVehicles[3]);
                    }
                    else if (bernieCheckPoint == 7)
                    {
                        _TASK_CAR_DRIVE_TO_COORD(berniePed, pVeh, 376.749f, -81.512f, 0.0f, 25.0f, 1, 0, 0, 20.0f, 45000);

                        // 284.845, 187.376, 0
                        CREATE_CAR(GET_HASH_KEY("jetmax"), 284.845f, 187.376f, 0, out enemyVehicles[8], true);
                        SET_CAR_HEADING(enemyVehicles[8], 0);

                        MARK_CHAR_AS_NO_LONGER_NEEDED(driverPeds[4]);
                        MARK_CHAR_AS_NO_LONGER_NEEDED(shooterPeds[4]);

                        MARK_CAR_AS_NO_LONGER_NEEDED(enemyVehicles[4]);
                    }
                    else if (bernieCheckPoint == 8)
                    {
                        _TASK_CAR_DRIVE_TO_COORD(berniePed, pVeh, 304.614f, 179.926f, 0.0f, 25.0f, 0, 0, 3, 20.0f, 45000);

                        MARK_CHAR_AS_NO_LONGER_NEEDED(driverPeds[5]);
                        MARK_CHAR_AS_NO_LONGER_NEEDED(shooterPeds[5]);

                        MARK_CAR_AS_NO_LONGER_NEEDED(enemyVehicles[5]);

                        // 245.368, 246.036, 345
                        CREATE_CAR(GET_HASH_KEY("jetmax"), 245.368f, 246.036f, 0, out enemyVehicles[9], true);
                        SET_CAR_HEADING(enemyVehicles[9], 345);
                    }
                    else if (bernieCheckPoint == 9)
                    {
                        _TASK_CAR_DRIVE_TO_COORD(berniePed, pVeh, 265.203f, 381.674f, 0.0f, 20.0f, 1, 0, 0, 20.0f, 45000);

                        CREATE_CHAR_INSIDE_CAR(enemyVehicles[8], (int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, (uint)GET_HASH_KEY("m_m_gru2_lo_02"), out driverPeds[8]);
                        CREATE_CHAR_AS_PASSENGER(enemyVehicles[8], (int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, (uint)GET_HASH_KEY("m_y_gru2_lo_01"), 0, out shooterPeds[8]);

                        _TASK_CAR_DRIVE_TO_COORD(driverPeds[8], enemyVehicles[8], 265.203f, 381.674f, 0.0f, 20.0f, 1, 0, 0, 20.0f, 45000);
                        GIVE_WEAPON_TO_CHAR(shooterPeds[8], enemyWeaponB, 9999, true);

                        CREATE_CHAR_INSIDE_CAR(enemyVehicles[9], (int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, (uint)GET_HASH_KEY("m_m_gru2_lo_02"), out driverPeds[9]);
                        CREATE_CHAR_AS_PASSENGER(enemyVehicles[9], (int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, (uint)GET_HASH_KEY("m_m_gru2_lo_02"), 0, out shooterPeds[9]);

                        _TASK_CAR_DRIVE_TO_COORD(driverPeds[9], enemyVehicles[9], 259.477f, 490.362f, 0.0f, 15.0f, 1, 0, 0, 20.0f, 45000);
                        GIVE_WEAPON_TO_CHAR(shooterPeds[9], enemyWeaponA, 9999, true);

                        // 296.643, 544.845, 30
                        CREATE_CAR(GET_HASH_KEY("jetmax"), 296.643f, 544.845f, 0, out enemyVehicles[10], true);
                        SET_CAR_HEADING(enemyVehicles[10], 30);
                    }
                    else if (bernieCheckPoint == 10)
                    {
                        _TASK_CAR_DRIVE_TO_COORD(berniePed, pVeh, 264.477f, 490.362f, 0.0f, 20.0f, 1, 0, 0, 20.0f, 45000);

                        MARK_CHAR_AS_NO_LONGER_NEEDED(driverPeds[6]);
                        MARK_CHAR_AS_NO_LONGER_NEEDED(shooterPeds[6]);

                        MARK_CAR_AS_NO_LONGER_NEEDED(enemyVehicles[6]);
                    }
                    else if (bernieCheckPoint == 11)
                    {
                        _TASK_CAR_DRIVE_TO_COORD(berniePed, pVeh, 250.312f, 708.392f, 0.0f, 25.0f, 1, 0, 0, 20.0f, 45000);

                        MARK_CHAR_AS_NO_LONGER_NEEDED(driverPeds[7]);
                        MARK_CHAR_AS_NO_LONGER_NEEDED(shooterPeds[7]);

                        MARK_CAR_AS_NO_LONGER_NEEDED(enemyVehicles[7]);

                        CREATE_CHAR_INSIDE_CAR(enemyVehicles[10], (int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, (uint)GET_HASH_KEY("m_y_gru2_lo_01"), out driverPeds[10]);
                        CREATE_CHAR_AS_PASSENGER(enemyVehicles[10], (int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, (uint)GET_HASH_KEY("m_m_gru2_lo_02"), 0, out shooterPeds[10]);

                        _TASK_CAR_DRIVE_TO_COORD(driverPeds[10], enemyVehicles[10], 250.312f, 708.392f, 0.0f, 16.0f, 1, 0, 0, 20.0f, 45000);
                        GIVE_WEAPON_TO_CHAR(shooterPeds[10], enemyWeaponA, 9999, true);

                        // 296.735, 739.097, 0
                        CREATE_CAR(GET_HASH_KEY("jetmax"), 296.735f, 739.097f, 0, out enemyVehicles[11], true);
                        SET_CAR_HEADING(enemyVehicles[11], 0);

                        // 210.260, 727.156, 345
                        CREATE_CAR(GET_HASH_KEY("jetmax"), 220.648f, 738.216f, 0, out enemyVehicles[12], true);
                        SET_CAR_HEADING(enemyVehicles[12], 345);
                    }
                    else if (bernieCheckPoint == 12)
                    {
                        _TASK_CAR_DRIVE_TO_COORD(berniePed, pVeh, 256.771f, 831.921f, 0.0f, 25.0f, 1, 0, 0, 20.0f, 45000);

                        // 397.037, 1059.955, 2.72, 90
                        CREATE_CHAR((int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, GET_HASH_KEY("m_y_gru2_lo_01"), 397.037f, 1059.955f, 1.92f, out landEnemyPeds[0], true);
                        // 397.037, 1072.724, 2.72, 90
                        CREATE_CHAR((int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, GET_HASH_KEY("m_y_gru2_lo_01"), 397.037f, 1072.724f, 1.92f, out landEnemyPeds[1], true);
                        // 397.037, 1081.338, 2.72, 90
                        CREATE_CHAR((int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, GET_HASH_KEY("m_m_gru2_lo_02"), 397.037f, 1081.338f, 1.92f, out landEnemyPeds[2], true);
                        // 397.037, 1107.842, 2.72, 90
                        CREATE_CHAR((int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, GET_HASH_KEY("m_y_gru2_lo_01"), 397.037f, 1107.842f, 1.92f, out landEnemyPeds[3], true);
                        // 397.037, 1119.625, 2.72, 90
                        CREATE_CHAR((int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, GET_HASH_KEY("m_m_gru2_lo_02"), 397.037f, 1119.625f, 1.92f, out landEnemyPeds[4], true);

                        GIVE_WEAPON_TO_CHAR(landEnemyPeds[0], enemyWeaponA, 9999, true);
                        GIVE_WEAPON_TO_CHAR(landEnemyPeds[1], enemyWeaponB, 9999, true);
                        GIVE_WEAPON_TO_CHAR(landEnemyPeds[2], enemyWeaponA, 9999, true);
                        GIVE_WEAPON_TO_CHAR(landEnemyPeds[3], enemyWeaponA, 9999, true);
                        GIVE_WEAPON_TO_CHAR(landEnemyPeds[4], enemyWeaponB, 9999, true);

                        for (int i = 0; i < 5; i++)
                        {
                            SET_CHAR_HEALTH(landEnemyPeds[i], enemyHealth);
                            SET_CHAR_HEADING(landEnemyPeds[i], 90);
                        }

                        CREATE_CHAR_INSIDE_CAR(enemyVehicles[11], (int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, (uint)GET_HASH_KEY("m_y_gru2_lo_01"), out driverPeds[11]);
                        CREATE_CHAR_AS_PASSENGER(enemyVehicles[11], (int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, (uint)GET_HASH_KEY("m_m_gru2_lo_02"), 0, out shooterPeds[11]);

                        _TASK_CAR_DRIVE_TO_COORD(driverPeds[11], enemyVehicles[11], 379.606f, 965.348f, 0.0f, 18.0f, 1, 0, 0, 18.0f, 45000);
                        GIVE_WEAPON_TO_CHAR(shooterPeds[11], enemyWeaponB, 9999, true);

                        CREATE_CHAR_INSIDE_CAR(enemyVehicles[12], (int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, (uint)GET_HASH_KEY("m_y_gru2_lo_01"), out driverPeds[12]);
                        CREATE_CHAR_AS_PASSENGER(enemyVehicles[12], (int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, (uint)GET_HASH_KEY("m_m_gru2_lo_02"), 0, out shooterPeds[12]);

                        _TASK_CAR_DRIVE_TO_COORD(driverPeds[12], enemyVehicles[12], 256.771f, 831.921f, 0.0f, 20.0f, 1, 0, 0, 20.0f, 45000);
                        GIVE_WEAPON_TO_CHAR(shooterPeds[12], enemyWeaponA, 9999, true);

                        // 290.904, 1201.204, 260
                        CREATE_CAR(GET_HASH_KEY("jetmax"), 290.904f, 1201.204f, 0, out enemyVehicles[13], true);
                        SET_CAR_HEADING(enemyVehicles[13], 260);
                    }
                    else if (bernieCheckPoint == 13)
                    {
                        _TASK_CAR_DRIVE_TO_COORD(berniePed, pVeh, 369.606f, 965.348f, 0.0f, 25.0f, 1, 0, 0, 20.0f, 45000);

                        CREATE_CHAR_INSIDE_CAR(enemyVehicles[13], (int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, (uint)GET_HASH_KEY("m_y_gru2_lo_01"), out driverPeds[13]);
                        CREATE_CHAR_AS_PASSENGER(enemyVehicles[13], (int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, (uint)GET_HASH_KEY("m_y_gru2_lo_01"), 0, out shooterPeds[13]);

                        _TASK_CAR_DRIVE_TO_COORD(driverPeds[13], enemyVehicles[13], 674.991f, 1111.501f, 0.0f, 17.0f, 1, 0, 0, 20.0f, 45000);
                        GIVE_WEAPON_TO_CHAR(shooterPeds[13], enemyWeaponA, 9999, true);

                        // 397.037, 1059.955, 2.72, 90
                        CREATE_CHAR((int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, GET_HASH_KEY("m_m_gru2_lo_02"), 406.624f, 1121.498f, 1.92f, out landEnemyPeds[5], true);
                        // 397.037, 1072.724, 2.72, 90
                        CREATE_CHAR((int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, GET_HASH_KEY("m_m_gru2_lo_02"), 419.318f, 1121.498f, 1.92f, out landEnemyPeds[6], true);
                        // 397.037, 1081.338, 2.72, 90
                        //CREATE_CHAR((int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, GET_HASH_KEY("m_y_gru2_lo_01"), 585.210f, 1124.123f, 0.384f, out landEnemyPeds[7], true);
                        // 397.037, 1107.842, 2.72, 90
                        //CREATE_CHAR((int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, GET_HASH_KEY("m_y_gru2_lo_01"), 593.662f, 1124.123f, 2.275f, out landEnemyPeds[8], true);

                        GIVE_WEAPON_TO_CHAR(landEnemyPeds[5], enemyWeaponB, 9999, true);
                        GIVE_WEAPON_TO_CHAR(landEnemyPeds[6], enemyWeaponB, 9999, true);
                        //GIVE_WEAPON_TO_CHAR(landEnemyPeds[7], enemyWeaponA, 9999, true);
                        //GIVE_WEAPON_TO_CHAR(landEnemyPeds[8], enemyWeaponB, 9999, true);

                        CREATE_OBJECT(GET_HASH_KEY("bm_drum_fla2"), 396.037f, 1072.724f, 2.02f, out barrelOne, true);

                        for (int i = 5; i < 7; i++)
                        {
                            SET_CHAR_HEALTH(landEnemyPeds[i], enemyHealth);
                            SET_CHAR_HEADING(landEnemyPeds[i], 0);
                        }
                        MARK_CHAR_AS_NO_LONGER_NEEDED(driverPeds[8]);
                        MARK_CHAR_AS_NO_LONGER_NEEDED(shooterPeds[8]);

                        MARK_CAR_AS_NO_LONGER_NEEDED(enemyVehicles[8]);

                        // 498.625, 1129.392, 0
                        CREATE_CAR(GET_HASH_KEY("jetmax"), 498.625f, 1129.392f, 0, out enemyVehicles[14], true);
                        SET_CAR_HEADING(enemyVehicles[14], 0);
                    }
                    else if (bernieCheckPoint == 14)
                    {
                        _TASK_CAR_DRIVE_TO_COORD(berniePed, pVeh, 375.939f, 1116.398f, 0.0f, 25.0f, 1, 0, 0, 20.0f, 45000);
                        APPLY_FORCE_TO_OBJECT(barrelOne, 3, 0, 0, 0.1f, 0, 0, 0, 0, 1, 1, 1);

                        CREATE_OBJECT(GET_HASH_KEY("bm_drum_fla2"), 412.971f, 1120.498f, 2.02f, out barrelTwo, true);

                        MARK_CHAR_AS_NO_LONGER_NEEDED(driverPeds[9]);
                        MARK_CHAR_AS_NO_LONGER_NEEDED(shooterPeds[9]);

                        MARK_CAR_AS_NO_LONGER_NEEDED(enemyVehicles[9]);
                    }
                    else if (bernieCheckPoint == 15)
                    {
                        _TASK_CAR_DRIVE_TO_COORD(berniePed, pVeh, 472.502f, 1136.906f, 0.0f, 25.0f, 1, 0, 0, 20.0f, 45000);

                        CREATE_CHAR_INSIDE_CAR(enemyVehicles[14], (int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, (uint)GET_HASH_KEY("m_m_gru2_lo_02"), out driverPeds[14]);
                        CREATE_CHAR_AS_PASSENGER(enemyVehicles[14], (int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, (uint)GET_HASH_KEY("m_y_gru2_lo_01"), 0, out shooterPeds[14]);

                        _TASK_CAR_DRIVE_TO_COORD(driverPeds[14], enemyVehicles[14], 679.895f, 1145.142f, 0.0f, 12.0f, 1, 0, 0, 20.0f, 45000);
                        GIVE_WEAPON_TO_CHAR(shooterPeds[14], enemyWeaponB, 9999, true);

                        APPLY_FORCE_TO_OBJECT(barrelOne, 3, 0, 0, 0.1f, 0, 0, 0, 0, 1, 1, 1);

                        MARK_CHAR_AS_NO_LONGER_NEEDED(driverPeds[10]);
                        MARK_CHAR_AS_NO_LONGER_NEEDED(shooterPeds[10]);

                        MARK_CAR_AS_NO_LONGER_NEEDED(enemyVehicles[10]);

                        MARK_OBJECT_AS_NO_LONGER_NEEDED(barrelOne);
                    }
                    else if (bernieCheckPoint == 16)
                    {
                        _TASK_CAR_DRIVE_TO_COORD(berniePed, pVeh, 609.895f, 1130.142f, 0.0f, 20.0f, 1, 0, 0, 20.0f, 45000);

                        MARK_OBJECT_AS_NO_LONGER_NEEDED(barrelTwo);

                        MARK_CHAR_AS_NO_LONGER_NEEDED(driverPeds[11]);
                        MARK_CHAR_AS_NO_LONGER_NEEDED(shooterPeds[11]);

                        MARK_CAR_AS_NO_LONGER_NEEDED(enemyVehicles[11]);

                        MARK_CHAR_AS_NO_LONGER_NEEDED(driverPeds[12]);
                        MARK_CHAR_AS_NO_LONGER_NEEDED(shooterPeds[12]);

                        MARK_CAR_AS_NO_LONGER_NEEDED(enemyVehicles[12]);
                    }
                    else if (bernieCheckPoint == 17)
                    {
                        _TASK_CAR_DRIVE_TO_COORD(berniePed, pVeh, 674.991f, 1109.501f, 0.0f, 25.0f, 1, 0, 0, 20.0f, 45000);
                        triggerCutscene = true;
                    }
                    else if (bernieCheckPoint == 18)
                    {
                        _TASK_CAR_DRIVE_TO_COORD(berniePed, pVeh, 799.487f, 994.604f, 0.0f, 25.0f, 1, 0, 0, 20.0f, 45000);
                        LOCK_CAR_DOORS(pVeh, 1);
                    }
                }
                else if (bernieCheckPoint == 19)
                {
                    startChase = false;
                    SET_PLAYER_CONTROL(Main.PlayerIndex, true);
                }
                bernieCheckPoint++;
            }

            GET_AMMO_IN_CHAR_WEAPON(Main.PlayerHandle, plyrWeapon, out int pAmmo);
            if (pAmmo < (IVWeaponInfo.GetWeaponInfo((uint)plyrWeapon).ClipSize * 2))
                ADD_AMMO_TO_CHAR(Main.PlayerHandle, plyrWeapon, 1);
        }
        public static void Tick()
        {
            if (NativeGame.IsScriptRunning("bernie3"))
            {
                if (startChase)
                    ProcessChase();
                if (triggerCutscene)
                    ChaseEndCutscene();
                else if (endCutscene && Main.gTimer >= fTimer + 12000)
                    DESTROY_ALL_CAMS();

                if (IS_THIS_PRINT_BEING_DISPLAYED("BER3_GOD3b", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0))
                    IVGame.ShowSubtitleMessage("~s~Follow ~r~Dimitri's men~s~ and get rid of them.", 5000);

                else if (LOCATE_CHAR_IN_CAR_3D(Main.PlayerHandle, 1129.244f, -883.3863f, 3.5f, 4.0f, 4.0f, 4.0f, false))
                {
                    if (!HAS_MODEL_LOADED(GET_HASH_KEY("squalo")))
                        REQUEST_MODEL(GET_HASH_KEY("squalo"));

                    if (!HAS_MODEL_LOADED(GET_HASH_KEY("jetmax")))
                        REQUEST_MODEL(GET_HASH_KEY("jetmax"));

                    if (!HAS_MODEL_LOADED(GET_HASH_KEY("bm_drum_fla2")))
                        REQUEST_MODEL(GET_HASH_KEY("bm_drum_fla2"));

                    if (!HAS_MODEL_LOADED(GET_HASH_KEY("m_y_gru2_lo_01")))
                        REQUEST_MODEL(GET_HASH_KEY("m_y_gru2_lo_01"));

                    if (!HAS_MODEL_LOADED(GET_HASH_KEY("m_m_gru2_lo_02")))
                        REQUEST_MODEL(GET_HASH_KEY("m_m_gru2_lo_02"));

                    if (IS_CHAR_SITTING_IN_ANY_CAR(Main.PlayerHandle))
                        GET_CAR_CHAR_IS_USING(Main.PlayerHandle, out pVeh);

                    if (IVWeaponInfo.GetWeaponInfo((uint)plyrWeapon).WeaponSlot != 2 && IVWeaponInfo.GetWeaponInfo((uint)plyrWeapon).WeaponSlot != 4 && IVWeaponInfo.GetWeaponInfo((uint)plyrWeapon).WeaponSlot != 5 && IVWeaponInfo.GetWeaponInfo((uint)plyrWeapon).WeaponSlot > 0)
                    {
                        pWeapSlot = IVWeaponInfo.GetWeaponInfo((uint)plyrWeapon).WeaponSlot;
                        GET_CHAR_WEAPON_IN_SLOT(Main.PlayerHandle, 5, out pWeap, out pAmmo1, out pAmmo2);
                        GET_AMMO_IN_CHAR_WEAPON(Main.PlayerHandle, pWeap, out pAmmo1);
                        REMOVE_WEAPON_FROM_CHAR(Main.PlayerHandle, pWeap);
                        IVWeaponInfo.GetWeaponInfo((uint)plyrWeapon).WeaponSlot = 5;
                    }

                    switchSeats = true;
                }
                else if (IS_PLAYER_CONTROL_ON(Main.PlayerIndex) && switchSeats)
                {
                    WARP_CHAR_INTO_CAR_AS_PASSENGER(Main.PlayerHandle, pVeh, (int)VehicleSeat.RightRear);

                    foreach (var ped in PedHelper.PedHandles)
                    {
                        int pedHandle = ped.Value;
                        if (!IS_PED_A_MISSION_PED(pedHandle))
                            continue;
                        if (pedHandle == Main.PlayerHandle)
                            continue;

                        GET_CHAR_MODEL(pedHandle, out uint pModel);
                        if (pModel == GET_HASH_KEY("ig_bernie_crane"))
                        {
                            berniePed = pedHandle;
                            WARP_CHAR_INTO_CAR(pedHandle, pVeh);
                            _TASK_CAR_DRIVE_TO_COORD(pedHandle, pVeh, 1043.469f, -886.864f, 3.5f, 20.0f, 1, 0, 0, 20.0f, 45000);
                            LOCK_CAR_DOORS(pVeh, 4);

                            GIVE_WEAPON_TO_CHAR(Main.PlayerHandle, plyrWeapon, (int)IVWeaponInfo.GetWeaponInfo((uint)plyrWeapon).MaxAmmo, true);
                            SET_CURRENT_CHAR_WEAPON(Main.PlayerHandle, plyrWeapon, true);

                            SET_CHAR_CAN_BE_SHOT_IN_VEHICLE(Main.PlayerHandle, false);
                            SET_CHAR_CAN_BE_SHOT_IN_VEHICLE(pedHandle, false);

                            //CREATE_CAR(GET_HASH_KEY("squalo"), 1188.035f, -1009.409f, 0, out enemyVehicles[0], true);
                            CREATE_CAR(GET_HASH_KEY("squalo"), 1188.035f, -938.572f, 0, out enemyVehicles[0], true);
                            //CREATE_CAR(GET_HASH_KEY("squalo"), 1208.035f, -938.572f, 0, out enemyVehicles[1], true);

                            bernieCheckPoint = -1;

                            startChase = true;
                        }
                        else
                        {
                            if (IS_CHAR_IN_ANY_BOAT(pedHandle) && !IS_CHAR_ARMED(pedHandle, 4))
                                SET_CHAR_CAN_BE_SHOT_IN_VEHICLE(pedHandle, false);
                        }
                    }
                    switchSeats = false;
                }
            }
            else if (startChase)
            {
                for (int i = 0; i < 16; i++)
                {
                    DELETE_CAR(ref enemyVehicles[i]);
                    DELETE_CHAR(ref driverPeds[i]);
                    DELETE_CHAR(ref shooterPeds[i]);
                }
                for (int i = 0; i < 9; i++)
                {
                    DELETE_CHAR(ref landEnemyPeds[i]);
                }

                MARK_MODEL_AS_NO_LONGER_NEEDED(GET_HASH_KEY("squalo"));
                MARK_MODEL_AS_NO_LONGER_NEEDED(GET_HASH_KEY("jetmax"));
                MARK_MODEL_AS_NO_LONGER_NEEDED(GET_HASH_KEY("bm_drum_fla2"));
                MARK_MODEL_AS_NO_LONGER_NEEDED(GET_HASH_KEY("m_y_gru2_lo_01"));
                MARK_MODEL_AS_NO_LONGER_NEEDED(GET_HASH_KEY("m_m_gru2_lo_02"));

                MARK_OBJECT_AS_NO_LONGER_NEEDED(barrelOne);
                MARK_OBJECT_AS_NO_LONGER_NEEDED(barrelTwo);

                if (pWeapSlot != 2 && pWeapSlot != 4 && pWeapSlot != 5 && pWeapSlot > 0)
                    IVWeaponInfo.GetWeaponInfo((uint)plyrWeapon).WeaponSlot = pWeapSlot;

                SET_CHAR_CAN_BE_SHOT_IN_VEHICLE(Main.PlayerHandle, true);

                if (!HAS_CHAR_GOT_WEAPON(Main.PlayerHandle, pWeap) && pWeap > 0)
                    GIVE_WEAPON_TO_CHAR(Main.PlayerHandle, pWeap, pAmmo1, false);
                taskStatus = 0;
                bernieCheckPoint = -1;
                switchSeats = false;
                startChase = false;
                triggerCutscene = false;
                warnPlayer = false;
                endCutscene = false;
            }
        }
    }
}
