using CCL;
using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using IVSDKDotNet.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Drawing;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static IVSDKDotNet.Native.Natives;

namespace MissionStuff.ivsdk
{
    internal class HollandNightsMelee
    {
        // IniShit
        private static bool fistOnly;
        private static int weaponToGive;
        private static int ammoToGive;

        // WeaponList
        private static List<int> inventory = new List<int>();
        private static Dictionary<int, int> ammo = new Dictionary<int, int>();

        // PedLists
        private static readonly List<int> PedList = new List<int>();
        private static readonly List<int> PedTPListA = new List<int>();
        private static readonly List<int> PedTPListB = new List<int>();

        // Booleshit
        private static bool introCutscene;
        private static bool missionStarted;
        private static bool hasWeapons;
        private static bool hasCachedWeapons;
        private static bool removeOrNot;
        private static bool removeWeapons;
        private static bool inCombat;
        private static bool teleportNow;
        private static bool canCollectWeaps;

        // CutsceneShit
        private static int cutsceneSeq = -1;
        /*private static bool triggerGunCutscene;
        private static bool startGunCutscene;
        private static bool endGunCutscene;*/

        // SpawnsNShit
        private static int[] fenceObj = new int[10];
        private static int[] gateObj = new int[10];
        private static int[] postObj = new int[6];
        private static int[] invisFence = new int[6];
        private static int[] doors = new int[2];
        private static int[] healthPickups = new int[2];

        // MissionDataShit
        private static int injPedCount = 0;
        private static int reqdBodyCount = 100;
        private static int missionStage = 0;

        // OtherShit
        private static int clarencePed = 0;
        private static int fakeClarence = 0;
        private static int fakeGun = 0;
        private static uint fTimer;
        private static int wBlip;

        // CamShit
        private static int cam;
        private static int cam2;
        private static int interpCam;

        private static int CreateObject_DontRequestModel(int hash, float x, float y, float z, float heading)
        {
            int obj;
            CREATE_OBJECT_NO_OFFSET(hash, x, y, z, out obj, true);
            SET_OBJECT_HEADING(obj, heading);
            return obj;
        }
        private static void RestoreWeapons()
        {
            foreach (var weapon in inventory)
            {
                int ammoToGive = ammo.ContainsKey(weapon) ? ammo[weapon] : 0;
                GIVE_WEAPON_TO_CHAR(Main.PlayerHandle, weapon, ammoToGive, true);
            }

            if (IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("TM_2_27"))
                CLEAR_HELP();
        }
        public static void UnInit()
        {
            for (int i = 0; i < 10; i++)
            {
                if (DOES_OBJECT_EXIST(fenceObj[i]))
                    MARK_OBJECT_AS_NO_LONGER_NEEDED(fenceObj[i]);
                if (DOES_OBJECT_EXIST(gateObj[i]))
                    MARK_OBJECT_AS_NO_LONGER_NEEDED(gateObj[i]);
            }
            for (int i = 0; i < 6; i++)
            {
                if (DOES_OBJECT_EXIST(invisFence[i]))
                    DELETE_OBJECT(ref invisFence[i]);
                if (DOES_OBJECT_EXIST(postObj[i]))
                    MARK_OBJECT_AS_NO_LONGER_NEEDED(postObj[i]);
            }
            for (int i = 0; i < 2; i++)
            {
                if (DOES_OBJECT_EXIST(doors[i]))
                    MARK_OBJECT_AS_NO_LONGER_NEEDED(doors[i]);
                if (DOES_PICKUP_EXIST(healthPickups[i]))
                    REMOVE_PICKUP(healthPickups[i]);
            }
            ACTIVATE_SCRIPTED_CAMS(false, false);
            DESTROY_ALL_CAMS();

            if (IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("TM_2_26"))
                CLEAR_HELP();

            MARK_MODEL_AS_NO_LONGER_NEEDED(GET_HASH_KEY("bm_gate_13"));
            MARK_MODEL_AS_NO_LONGER_NEEDED(GET_HASH_KEY("bm_fence_13"));
            MARK_MODEL_AS_NO_LONGER_NEEDED(GET_HASH_KEY("cj_fence_23_3"));
            MARK_MODEL_AS_NO_LONGER_NEEDED(GET_HASH_KEY("cj_fence_23_pst"));
            MARK_MODEL_AS_NO_LONGER_NEEDED(GET_HASH_KEY("cj_gate_3_7r"));
            MARK_MODEL_AS_NO_LONGER_NEEDED(GET_HASH_KEY("cj_ext_door_9"));
            MARK_MODEL_AS_NO_LONGER_NEEDED(GET_HASH_KEY("cj_first_aid_pickup"));

            missionStarted = false;
            introCutscene = false;
            hasWeapons = false;
            removeOrNot = false;
            removeWeapons = false;
            inCombat = false;
            teleportNow = false;
            canCollectWeaps = false;

            if (DOES_BLIP_EXIST(wBlip))
                REMOVE_BLIP(wBlip);
            if (DOES_OBJECT_EXIST(fakeGun))
                DELETE_OBJECT(ref fakeGun);
            if (DOES_CHAR_EXIST(fakeClarence))
                DELETE_CHAR(ref fakeClarence);

            if (IS_CHAR_DEAD(Main.PlayerHandle) || HAS_CHAR_BEEN_ARRESTED(Main.PlayerHandle))
                RestoreWeapons();

            cutsceneSeq = -1;
            /*triggerGunCutscene = false;
            startGunCutscene = false;
            endGunCutscene = false;*/

            missionStage = 0;
            reqdBodyCount = 100;
            injPedCount = 0;
            PedList.Clear();
            PedTPListA.Clear();
            PedTPListB.Clear();
        }
        public static void Init(SettingsFile settings)
        {
            fistOnly = settings.GetBoolean("HOLLAND HALLWAY HEAD KNOCKING", "FistsOnly", false);
            weaponToGive = settings.GetInteger("HOLLAND HALLWAY HEAD KNOCKING", "WeaponToGive", 7);
            ammoToGive = settings.GetInteger("HOLLAND HALLWAY HEAD KNOCKING", "AmmoToGive", 51);
        }
        private static void SpawnObjects()
        {
            if (!HAS_MODEL_LOADED(GET_HASH_KEY("bm_fence_13")))
                REQUEST_MODEL(GET_HASH_KEY("bm_fence_13"));
            if (!HAS_MODEL_LOADED(GET_HASH_KEY("bm_gate_13")))
                REQUEST_MODEL(GET_HASH_KEY("bm_gate_13"));
            if (!HAS_MODEL_LOADED(GET_HASH_KEY("cj_fence_23_3")))
                REQUEST_MODEL(GET_HASH_KEY("cj_fence_23_3"));
            if (!HAS_MODEL_LOADED(GET_HASH_KEY("cj_fence_23_pst")))
                REQUEST_MODEL(GET_HASH_KEY("cj_fence_23_pst"));
            if (!HAS_MODEL_LOADED(GET_HASH_KEY("cj_gate_3_7r"))) 
                REQUEST_MODEL(GET_HASH_KEY("cj_gate_3_7r"));
            if (!HAS_MODEL_LOADED(GET_HASH_KEY("cj_ext_door_9")))
                REQUEST_MODEL(GET_HASH_KEY("cj_ext_door_9"));
            if (!HAS_MODEL_LOADED(GET_HASH_KEY("cj_first_aid_pickup")))
                REQUEST_MODEL(GET_HASH_KEY("cj_first_aid_pickup"));

            if (IS_PLAYER_CONTROL_ON(Main.PlayerIndex))
            {
                // Main Fences & Gates
                fenceObj[0] = CreateObject_DontRequestModel(GET_HASH_KEY("bm_fence_13"), -57.6508f, 1325.15f, 20.7074f, 90f);
                fenceObj[1] = CreateObject_DontRequestModel(GET_HASH_KEY("bm_fence_13"), -61.0304f, 1327.25f, 26.2806f, -90f);
                fenceObj[2] = CreateObject_DontRequestModel(GET_HASH_KEY("bm_fence_13"), -61.0304f, 1327.25f, 30.6326f, -90f);
                fenceObj[3] = CreateObject_DontRequestModel(GET_HASH_KEY("bm_fence_13"), -118.663f, 1325.15f, 20.7074f, 90f);
                fenceObj[4] = CreateObject_DontRequestModel(GET_HASH_KEY("bm_fence_13"), -115.287f, 1327.25f, 26.2806f, -90f);
                fenceObj[5] = CreateObject_DontRequestModel(GET_HASH_KEY("bm_fence_13"), -115.287f, 1327.25f, 30.6326f, -90f);

                gateObj[0] = CreateObject_DontRequestModel(GET_HASH_KEY("bm_gate_13"), -57.6508f, 1326.54f, 20.4627f, 90f);
                gateObj[1] = CreateObject_DontRequestModel(GET_HASH_KEY("bm_gate_13"), -61.0304f, 1325.86f, 26.0338f, -90f);
                gateObj[2] = CreateObject_DontRequestModel(GET_HASH_KEY("bm_gate_13"), -61.0304f, 1325.86f, 30.3858f, -90f);
                gateObj[3] = CreateObject_DontRequestModel(GET_HASH_KEY("bm_gate_13"), -118.663f, 1326.54f, 20.4627f, 90f);
                gateObj[4] = CreateObject_DontRequestModel(GET_HASH_KEY("bm_gate_13"), -115.287f, 1325.86f, 26.0338f, -90f);
                gateObj[5] = CreateObject_DontRequestModel(GET_HASH_KEY("bm_gate_13"), -115.287f, 1325.86f, 30.3858f, -90f);

                // Prevent the player from reaching Clarence until beating all goons
                fenceObj[6] = CreateObject_DontRequestModel(GET_HASH_KEY("cj_fence_23_3"), -57.1592f, 1306.21f, 39.0867f, 0f);
                fenceObj[7] = CreateObject_DontRequestModel(GET_HASH_KEY("cj_fence_23_3"), -39.8481f, 1306.21f, 39.0867f, 180f);
                fenceObj[8] = CreateObject_DontRequestModel(GET_HASH_KEY("cj_fence_23_3"), -136.4392f, 1306.21f, 39.0867f, 0f);
                fenceObj[9] = CreateObject_DontRequestModel(GET_HASH_KEY("cj_fence_23_3"), -119.1281f, 1306.21f, 39.0867f, 180f);

                postObj[0] = CreateObject_DontRequestModel(GET_HASH_KEY("cj_fence_23_pst"), -51.0392f, 1306.21f, 37.8497f, 0f);
                postObj[1] = CreateObject_DontRequestModel(GET_HASH_KEY("cj_fence_23_pst"), -45.969f, 1306.21f, 37.8497f, 0f);
                postObj[2] = CreateObject_DontRequestModel(GET_HASH_KEY("cj_fence_23_pst"), -130.3192f, 1306.21f, 37.8497f, 0f);
                postObj[3] = CreateObject_DontRequestModel(GET_HASH_KEY("cj_fence_23_pst"), -125.249f, 1306.21f, 37.8497f, 0f);

                gateObj[6] = CreateObject_DontRequestModel(GET_HASH_KEY("cj_gate_3_7r"), -51.0518f, 1306.21f, 39.3297f, 180f);
                gateObj[7] = CreateObject_DontRequestModel(GET_HASH_KEY("cj_fence_23_3"), -51.0518f, 1306.21f, 42.3297f, 0f);
                gateObj[8] = CreateObject_DontRequestModel(GET_HASH_KEY("cj_gate_3_7r"), -130.3318f, 1306.21f, 39.3297f, 180f);
                gateObj[9] = CreateObject_DontRequestModel(GET_HASH_KEY("cj_fence_23_3"), -130.3318f, 1306.21f, 42.3297f, 0f);

                // Doors leading outside
                doors[0] = CreateObject_DontRequestModel(GET_HASH_KEY("cj_ext_door_9"), -57.303f, 1326.84f, 34.9383f, -90f);
                doors[1] = CreateObject_DontRequestModel(GET_HASH_KEY("cj_ext_door_9"), -119.012f, 1326.84f, 34.9383f, -90f);

                // Invisible fences that get removed if the player progresses
                invisFence[0] = CreateObject_DontRequestModel(GET_HASH_KEY("bm_fence_13"), -89.1963f, 1301.54f, 25.4303f, 120f);
                invisFence[1] = CreateObject_DontRequestModel(GET_HASH_KEY("bm_fence_13"), -78.9455f, 1300.7f, 34.2933f, 90f);
                invisFence[2] = CreateObject_DontRequestModel(GET_HASH_KEY("bm_gate_13"), -88.2908f, 1329.7f, 34.6934f, 90f);
                invisFence[3] = CreateObject_DontRequestModel(GET_HASH_KEY("bm_fence_13"), -102.636f, 1325.021f, 25.4303f, 0f);
                invisFence[4] = CreateObject_DontRequestModel(GET_HASH_KEY("bm_fence_13"), -64.547f, 1327.449f, 25.4303f, 90f);
                invisFence[5] = CreateObject_DontRequestModel(GET_HASH_KEY("bm_fence_13"), -116.346f, 1328.352f, 25.4303f, 135f);

                // Invisible fences that never get removed
                //invisFence[3] = CreateObject_DontRequestModel(GET_HASH_KEY("bm_gate_13"), -118.812f, 1328.67f, 33.6875f, 90f);

                CREATE_PICKUP_ROTATE((uint)GET_HASH_KEY("cj_first_aid_pickup"), (uint)ePickupType.PICKUP_TYPE_WEAPON, 200, -71.6177f, 1300.25f, 26.2303f, 0, 0, 0, out healthPickups[0]);
                CREATE_PICKUP_ROTATE((uint)GET_HASH_KEY("cj_first_aid_pickup"), (uint)ePickupType.PICKUP_TYPE_WEAPON, 200, -104.7227f, 1300.25f, 26.2303f, 0, 0, 0, out healthPickups[1]);

                for (int i = 0; i < 8; i++)
                {
                    FREEZE_OBJECT_POSITION(fenceObj[i], true);
                    SET_OBJECT_DYNAMIC(fenceObj[i], false);
                }
                for (int i = 0; i < 4; i++)
                {
                    FREEZE_OBJECT_POSITION(postObj[i], true);
                    SET_OBJECT_DYNAMIC(postObj[i], false);
                }
                for (int i = 0; i < 6; i++)
                {
                    FREEZE_OBJECT_POSITION(invisFence[i], true);
                    SET_OBJECT_DYNAMIC(invisFence[i], false);
                    SET_OBJECT_VISIBLE(invisFence[i], false);
                }

                SET_OBJECT_VISIBLE(gateObj[7], false);
                SET_OBJECT_VISIBLE(gateObj[9], false);

                // Main Fences & Gates
                SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("bm_gate_13"), -57.6508f, 1326.54f, 20.4627f, true, 0.0f);
                SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("bm_gate_13"), -118.663f, 1326.54f, 20.4627f, true, 0.0f);
                SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("bm_gate_13"), -61.0304f, 1325.86f, 26.0338f, true, 0.0f);
                SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("bm_gate_13"), -115.287f, 1325.86f, 26.0338f, true, 0.0f);
                SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("bm_gate_13"), -61.0304f, 1325.86f, 30.3858f, true, 0.0f);
                SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("bm_gate_13"), -115.287f, 1325.86f, 30.3858f, true, 0.0f);

                // Prevent the player from reaching Clarence until beating all goons
                SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("cj_gate_3_7r"), -51.0518f, 1306.21f, 39.3297f, false, 0.0f);
                SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("cj_gate_3_7r"), -130.3318f, 1306.21f, 39.3297f, false, 0.0f);

                // Doors leading outside
                SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("cj_ext_door_9"), -57.303f, 1326.84f, 34.9383f, true, 90.0f);
                SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("cj_ext_door_9"), -119.012f, 1326.84f, 34.9383f, true, 90.0f);

                // Invis Fences
                SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("bm_gate_13"), -88.2908f, 1329.7f, 34.6934f, true, 0.0f);
                //SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("bm_gate_13"), -118.812f, 1328.67f, 33.6875f, true, 0.0f);

                missionStarted = true;
            }
        }
        private static void ProcessEnemyPeds()
        {
            //IVGame.ShowSubtitleMessage(inCombat.ToString());
            foreach (var ped in PedHelper.PedHandles)
            {
                int pedHandle = ped.Value;
                if (!DOES_CHAR_EXIST(pedHandle))
                    continue;
                if (!IS_PED_A_MISSION_PED(pedHandle))
                    continue;
                if (pedHandle == Main.PlayerHandle)
                    continue;
                if (IS_CHAR_INJURED(pedHandle))
                    continue;
                if (PedList.Contains(pedHandle))
                    continue;

                GET_CHAR_MODEL(pedHandle, out int pModel);

                if (pModel == GET_HASH_KEY("ig_clarence"))
                    clarencePed = pedHandle;

                PedList.Add(pedHandle);

                if (PedTPListA.Count < 2 && LOCATE_CHAR_ANY_MEANS_3D(pedHandle, -59.1508f, 1325.15f, 20.7074f, 2.5f, 2.5f, 2.5f, false))
                {
                    PedTPListA.Add(pedHandle);
                }
                if (PedTPListB.Count < 2 && LOCATE_CHAR_ANY_MEANS_3D(pedHandle, -117.163f, 1325.15f, 20.7074f, 2.5f, 2.5f, 2.5f, false))
                {
                    PedTPListB.Add(pedHandle);
                }
            }

            foreach (var ped in PedList)
            {
                if (!DOES_CHAR_EXIST(ped))
                {
                    PedList.Remove(ped);
                    return;
                }
                else if (IS_CHAR_INJURED(ped) && IS_PED_A_MISSION_PED(ped))
                {
                    PedList.Remove(ped);
                    injPedCount++;
                    return;
                }
                if (IS_PED_IN_COMBAT(ped) || IS_CHAR_IN_MELEE_COMBAT(ped))
                    inCombat = true;

                GET_CURRENT_CHAR_WEAPON(ped, out int pedWeap);

                if (missionStage >= 2 && missionStage < 7 && pedWeap > 0 && (pedWeap < 46 || pedWeap > 57))
                {
                    GIVE_WEAPON_TO_CHAR(ped, 0, 999, false);
                    REMOVE_WEAPON_FROM_CHAR(ped, pedWeap);
                }
            }
        }
        private static void TeleportToSafeLoc()
        {
            if (IS_SCREEN_FADED_OUT())
            {
                if (removeWeapons)
                {
                    inventory = Main.GetWeaponInventory(fistOnly ? true : false);
                    ammo = Main.GetWeaponAmmoCounts();

                    if (fistOnly)
                        REMOVE_ALL_CHAR_WEAPONS(Main.PlayerHandle);
                    else
                    {
                        for (int i = 2; i < 10; i++)
                        {
                            GET_CHAR_WEAPON_IN_SLOT(Main.PlayerHandle, i, out int pWeap, out int pAmmo0, out int pAmmo1);
                            REMOVE_WEAPON_FROM_CHAR(Main.PlayerHandle, pWeap);
                        }
                    }
                    hasCachedWeapons = true;
                }

                GET_DISTANCE_BETWEEN_COORDS_3D(Main.PlayerPos.X, Main.PlayerPos.Y, Main.PlayerPos.Z, -117.163f, 1325.15f, 20.7074f, out float pDist1);
                GET_DISTANCE_BETWEEN_COORDS_3D(Main.PlayerPos.X, Main.PlayerPos.Y, Main.PlayerPos.Z, -59.1508f, 1325.15f, 20.7074f, out float pDist2);

                if (pDist1 <= pDist2)
                {
                    _SET_CHAR_COORDINATES_NO_OFFSET(Main.PlayerHandle, -115.163f, 1325.15f, 19.7074f);
                    SET_CHAR_HEADING(Main.PlayerHandle, 90.0f);
                }
                else
                {
                    _SET_CHAR_COORDINATES_NO_OFFSET(Main.PlayerHandle, -61.1508f, 1325.15f, 19.7074f);
                    SET_CHAR_HEADING(Main.PlayerHandle, 270.0f);
                }
                SET_PLAYER_CONTROL(Main.PlayerIndex, true);

                hasWeapons = false;
                removeOrNot = false;
                removeWeapons = false;
                teleportNow = false;
                DO_SCREEN_FADE_IN(1000);
            }
        }
        private static void ProcessRemoveWeapons()
        {
            if (!removeOrNot && !inCombat && (LOCATE_CHAR_ANY_MEANS_3D(Main.PlayerHandle, -117.163f, 1325.15f, 20.7074f, 1.0f, 1.0f, 1.0f, true) || LOCATE_CHAR_ANY_MEANS_3D(Main.PlayerHandle, -59.1508f, 1325.15f, 20.7074f, 1.0f, 1.0f, 1.0f, true)))
            {
                for (int i = fistOnly ? 1 : 2; i < 10; i++)
                {
                    GET_CHAR_WEAPON_IN_SLOT(Main.PlayerHandle, i, out int pWeap, out int pAmmo0, out int pAmmo1);
                    if (pWeap > 0 && (pWeap < 46 || pWeap > 57))
                    {
                        hasWeapons = true;
                        break;
                    }
                }
                if (hasWeapons)
                {
                    SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("bm_gate_13"), -57.6508f, 1326.54f, 20.4627f, true, 0.0f);
                    SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("bm_gate_13"), -118.663f, 1326.54f, 20.4627f, true, 0.0f);
                    //CLEAR_CHAR_TASKS(Main.PlayerHandle);
                    SET_PLAYER_CONTROL(Main.PlayerIndex, false);
                    removeOrNot = true;
                }
                else
                {
                    SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("bm_gate_13"), -57.6508f, 1326.54f, 20.4627f, false, 0.0f);
                    SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("bm_gate_13"), -118.663f, 1326.54f, 20.4627f, false, 0.0f);
                }
            }
            else if (inCombat && missionStage < 2)
            {
                SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("bm_gate_13"), -57.6508f, 1326.54f, 20.4627f, false, 0.0f);
                SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("bm_gate_13"), -118.663f, 1326.54f, 20.4627f, false, 0.0f);
                SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("bm_gate_13"), -61.0304f, 1325.86f, 26.0338f, false, 0.0f);
                SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("bm_gate_13"), -115.287f, 1325.86f, 26.0338f, false, 0.0f);
                SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("bm_gate_13"), -61.0304f, 1325.86f, 30.3858f, false, 0.0f);
                SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("bm_gate_13"), -115.287f, 1325.86f, 30.3858f, false, 0.0f);
                SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("cj_ext_door_9"), -57.303f, 1326.84f, 34.9383f, false, 90.0f);
                SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("cj_ext_door_9"), -119.012f, 1326.84f, 34.9383f, false, 90.0f);
                SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("cj_gate_3_7r"), -51.0518f, 1306.21f, 39.3297f, false, 0.0f);
                SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("cj_gate_3_7r"), -130.3318f, 1306.21f, 39.3297f, false, 0.0f);

                for (int i = 0; i < 6; i++)
                {
                    if (DOES_OBJECT_EXIST(invisFence[i]))
                        DELETE_OBJECT(ref invisFence[i]);
                }
            }
            if (removeOrNot && !teleportNow)
            {
                if (!IS_HELP_MESSAGE_BEING_DISPLAYED())
                {
                    IVText.TheIVText.ReplaceTextOfTextLabel("TM_2_26", "~s~Press ~INPUT_FRONTEND_ACCEPT~ to stash away your weapons. ~n~~s~Press ~INPUT_PICKUP~ to cancel.");
                    PRINT_HELP_FOREVER("TM_2_26");
                }

                if (IS_CONTROL_JUST_PRESSED(0, (int)GameKey.NavEnter) || IS_CONTROL_JUST_PRESSED(2, (int)GameKey.NavEnter))
                {
                    removeWeapons = true;
                    teleportNow = true;
                    DO_SCREEN_FADE_OUT(1000);
                }
                else if (IS_CONTROL_JUST_PRESSED(0, (int)GameKey.Action) || IS_CONTROL_JUST_PRESSED(2, (int)GameKey.Action))
                {
                    teleportNow = true;
                    DO_SCREEN_FADE_OUT(1000);
                }
            }
            else if (IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("TM_2_26"))
            {
                CLEAR_HELP();
            }

            if (teleportNow)
                TeleportToSafeLoc();
        }
        private static void ProcessReturnWeapons()
        {
            if ((IS_THIS_PRINT_BEING_DISPLAYED("TS_GOAL2", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0) || IS_THIS_PRINT_BEING_DISPLAYED("TS_GOAL3", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0) || canCollectWeaps) && hasCachedWeapons)
            {
                Vector3 weapLoc;
                if (reqdBodyCount < 17)
                    weapLoc = new Vector3(-122.876f, 1323.335f, 20.442f);
                else
                    weapLoc = new Vector3(-53.495f, 1323.335f, 20.442f);

                canCollectWeaps = true;
                if (!DOES_BLIP_EXIST(wBlip))
                {
                    // 1371.646, 621.487, 35.829
                    ADD_BLIP_FOR_COORD(weapLoc.X, weapLoc.Y, weapLoc.Z, out wBlip);

                    NativeBlip pBlip = new NativeBlip(wBlip);

                    pBlip.Icon = BlipIcon.Building_WeaponShop;
                    pBlip.Name = "Stashed Weapons";
                    pBlip.Scale = 0.75f;
                    pBlip.Display = eBlipDisplay.BLIP_DISPLAY_ARROW_AND_MAP;
                    pBlip.ShowOnlyWhenNear = true;
                }
                if (LOCATE_CHAR_ANY_MEANS_3D(Main.PlayerHandle, weapLoc.X, weapLoc.Y, weapLoc.Z, 1.0f, 1.0f, 1.0f, true))
                {
                    if (!IS_HELP_MESSAGE_BEING_DISPLAYED())
                    {
                        IVText.TheIVText.ReplaceTextOfTextLabel("TM_2_27", "~s~Press ~INPUT_PICKUP~ to retrieve your stashed weapons.");
                        PRINT_HELP_FOREVER("TM_2_27");
                    }

                    if (IS_CONTROL_JUST_PRESSED(0, (int)eGameKey.GAME_KEY_ACTION) || IS_CONTROL_JUST_PRESSED(2, (int)eGameKey.GAME_KEY_ACTION))
                    {
                        RestoreWeapons();

                        REMOVE_BLIP(wBlip);

                        hasCachedWeapons = false;
                        canCollectWeaps = false;
                    }
                }
                else if (IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("TM_2_27"))
                {
                    CLEAR_HELP();
                }
            }
        }
        private static void ProcessGunCutscene(Vector3 clarencePos)
        {
            if (cutsceneSeq == 0 && IS_SCREEN_FADED_OUT())
            {
                DISPLAY_HUD(false);
                DISPLAY_RADAR(false);

                SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("cj_gate_3_7r"), -51.0518f, 1306.21f, 39.3297f, false, 0.0f);
                SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("cj_gate_3_7r"), -130.3318f, 1306.21f, 39.3297f, false, 0.0f);

                SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("bm_gate_13"), -57.6508f, 1326.54f, 20.4627f, false, 0.0f);
                SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("bm_gate_13"), -118.663f, 1326.54f, 20.4627f, false, 0.0f);

                CREATE_CHAR((int)ePedType.PED_TYPE_CIV_MALE, GET_HASH_KEY("ig_clarence"), clarencePos.X, clarencePos.Y, clarencePos.Z, out fakeClarence, true);

                //CREATE_CHAR((int)ePedType.PED_TYPE_CIV_MALE, GET_HASH_KEY("ig_clarence"), -50.224f, 1310.268f, 38.855f, out fakeClarence, true);

                SET_CHAR_HEADING(fakeClarence, 180);
                _TASK_PLAY_ANIM(fakeClarence, "copm_searchboot", "cop", 4.0f, 0, 0, 0, 0, -1);

                SET_CHAR_COORDINATES(Main.PlayerHandle, clarencePos.X + 11.498f, clarencePos.Y - 6.651f, clarencePos.Z - 4.162f);
                SET_CHAR_COORDINATES(Main.PlayerHandle, clarencePos.X + 11.498f, clarencePos.Y - 6.651f, clarencePos.Z - 4.162f);

                CREATE_CAM(14, out cam);
                SET_CAM_FOV(cam, 45);
                SET_CAM_POS(cam, clarencePos.X - 0.2f, clarencePos.Y + 1.551f, clarencePos.Z + 0.409f);
                POINT_CAM_AT_COORD(cam, clarencePos.X + 3.021f, clarencePos.Y - 5.921f, clarencePos.Z - 0.246f);

                SET_CAM_ACTIVE(cam, true);
                SET_CAM_PROPAGATE(cam, true);
                ACTIVATE_SCRIPTED_CAMS(true, true);

                //_TASK_CHAR_SLIDE_TO_COORD(Main.PlayerHandle, -126, 1310, 39, 0, 3);
                //_TASK_GO_STRAIGHT_TO_COORD(Main.PlayerHandle, -126.249f, 1309.777f, 38.855f, 4, 45000);

                _TASK_GO_STRAIGHT_TO_COORD(Main.PlayerHandle, clarencePos.X + 3.675f, clarencePos.Y - 6.651f, clarencePos.Z, 4, 45000);

                CREATE_CAM(14, out cam2);

                SET_CAM_POS(cam2, clarencePos.X - 0.2f, clarencePos.Y + 1.551f, clarencePos.Z + 0.409f);
                POINT_CAM_AT_PED(cam2, Main.PlayerHandle);
                SET_CAM_FOV(cam2, 45);

                CREATE_CAM(3, out interpCam);

                GET_GAME_TIMER(out fTimer);

                cutsceneSeq++;
                //startGunCutscene = true;
            }
            else if (Main.gTimer >= fTimer + 500 && cutsceneSeq == 1)
            {
                DO_SCREEN_FADE_IN(1000);
                _TASK_LOOK_AT_CHAR(Main.PlayerHandle, fakeClarence, 5000, 0);
                cutsceneSeq++;
            }
            else if (Main.gTimer >= fTimer + 4000 && cutsceneSeq == 2)
            {
                CLEAR_CHAR_TASKS(Main.PlayerHandle);
                CLEAR_CHAR_TASKS(fakeClarence);

                _TASK_GO_STRAIGHT_TO_COORD(Main.PlayerHandle, clarencePos.X + 3.675f, clarencePos.Y - 0.491f, clarencePos.Z, 4, 45000);

                SAY_AMBIENT_SPEECH(fakeClarence, "Crash_Car", true, false, 1);
                _TASK_GO_STRAIGHT_TO_COORD(fakeClarence, clarencePos.X + 3.675f, clarencePos.Y + 9.509f, clarencePos.Z, 4, 45000);

                SET_CAM_ACTIVE(interpCam, true);
                SET_CAM_PROPAGATE(interpCam, true);
                ACTIVATE_SCRIPTED_CAMS(true, true);
                SET_CAM_INTERP_STYLE_CORE(interpCam, cam, cam2, 2000, false);
                cutsceneSeq++;
            }
            else if (Main.gTimer >= fTimer + 5800 && cutsceneSeq == 3)
            {
                GET_WEAPONTYPE_MODEL(weaponToGive, out uint weapModel);

                CREATE_OBJECT((int)weapModel, clarencePos.X, clarencePos.Y, clarencePos.Z, out fakeGun, true);
                SET_OBJECT_DYNAMIC(fakeGun, true);
                _TASK_LOOK_AT_OBJECT(Main.PlayerHandle, fakeGun, 3000, 0);

                SET_CAM_ACTIVE(cam2, true);
                cutsceneSeq++;
            }
            else if (Main.gTimer >= fTimer + 6000 && cutsceneSeq == 4)
            {
                APPLY_FORCE_TO_OBJECT(fakeGun, 3, new Vector3(0, 0, -2), Vector3.Zero, 0, 1, 1, 1);

                GET_OBJECT_COORDINATES(fakeGun, out Vector3 gunCoord);
                POINT_CAM_AT_COORD(cam, new Vector3(gunCoord.X, gunCoord.Y, gunCoord.Z - 0.9f));
                SET_CAM_ACTIVE(interpCam, true);
                SET_CAM_PROPAGATE(interpCam, true);
                ACTIVATE_SCRIPTED_CAMS(true, true);
                SET_CAM_INTERP_STYLE_CORE(interpCam, cam2, cam, 800, false);
                cutsceneSeq++;
            }
            else if (Main.gTimer >= (fTimer + 8500) && cutsceneSeq == 5)
            {
                DO_SCREEN_FADE_OUT(1000);
                cutsceneSeq++;
            }
            else if (Main.gTimer >= (fTimer + 8500) && cutsceneSeq == 6 && IS_SCREEN_FADED_OUT())
            {
                DISPLAY_HUD(true);
                DISPLAY_RADAR(true);
                SET_PLAYER_CONTROL(Main.PlayerIndex, true);

                ACTIVATE_SCRIPTED_CAMS(false, false);
                DESTROY_ALL_CAMS();

                DELETE_CHAR(ref fakeClarence);
                DELETE_OBJECT(ref fakeGun);

                if (!HAS_CHAR_GOT_WEAPON(Main.PlayerHandle, weaponToGive))
                    GIVE_WEAPON_TO_CHAR(Main.PlayerHandle, weaponToGive, ammoToGive, true);

                cutsceneSeq++;
                missionStage = 7;
                DO_SCREEN_FADE_IN(1000);
            }
        }
        private static void GunCutscene()
        {
            if (cutsceneSeq < 0)
                return;

            if (!HAVE_ANIMS_LOADED("cop"))
            {
                REQUEST_ANIMS("cop");
                return;
            }

            if (reqdBodyCount == 17)
                ProcessGunCutscene(new Vector3(-129.924f, 1310.268f, 38.855f));

            else if (reqdBodyCount == 14)
                ProcessGunCutscene(new Vector3(-50.224f, 1310.268f, 38.855f));
        }
        private static void ProcessStartCutscene()
        {
            if (cutsceneSeq == 0 && IS_SCREEN_FADING_IN())
            {
                DISPLAY_HUD(false);
                DISPLAY_RADAR(false);

                CREATE_CAM(14, out cam);
                SET_CAM_FOV(cam, 45);
                SET_CAM_POS(cam, -62.076f, 1323.723f, 20.749f);
                POINT_CAM_AT_COORD(cam, -56.366f, 1325.491f, 20.376f);

                SET_CAM_ACTIVE(cam, true);
                SET_CAM_PROPAGATE(cam, true);
                ACTIVATE_SCRIPTED_CAMS(true, true);

                GET_GAME_TIMER(out fTimer);

                cutsceneSeq++;
            }
            else if (cutsceneSeq > 0)
            {
                if (IS_CONTROL_JUST_PRESSED(0, (int)GameKey.NavEnter) || IS_CONTROL_JUST_PRESSED(2, (int)GameKey.NavEnter)
                    || IS_CONTROL_JUST_PRESSED(0, (int)GameKey.EnterCar) || IS_CONTROL_JUST_PRESSED(2, (int)GameKey.EnterCar)
                    || IS_CONTROL_JUST_PRESSED(0, (int)GameKey.Jump) || IS_CONTROL_JUST_PRESSED(2, (int)GameKey.Jump))
                {
                    if (IS_SCREEN_FADED_IN() && cutsceneSeq < 4)
                    {
                        cutsceneSeq = 4;
                        fTimer -= 16000;
                    }
                }

                if (cutsceneSeq == 5 && IS_SCREEN_FADED_OUT())
                {
                    CLEAR_HELP();
                    DISPLAY_HUD(true);
                    DISPLAY_RADAR(true);
                    SET_PLAYER_CONTROL(Main.PlayerIndex, true);

                    ACTIVATE_SCRIPTED_CAMS(false, false);
                    DESTROY_ALL_CAMS();

                    DO_SCREEN_FADE_IN(1000);
                    CLEAR_HELP();
                    cutsceneSeq = -1;
                    reqdBodyCount = 99;
                }
                else if (Main.gTimer >= fTimer + 16000 && cutsceneSeq == 4)
                {
                    CLEAR_HELP();
                    DO_SCREEN_FADE_OUT(1000);
                    cutsceneSeq++;
                }
                else if (Main.gTimer >= fTimer + 11000 && cutsceneSeq == 3)
                {
                    CLEAR_HELP();
                    SET_CAM_POS(cam, -50.909f, 1329.163f, 25.892f);
                    POINT_CAM_AT_COORD(cam, -60.150f, 1327.149f, 25.755f);
                    IVText.TheIVText.ReplaceTextOfTextLabel("TM_2_30", "His goons won't take kindly to your presence even if you're allowed in.");
                    PRINT_HELP_FOREVER("TM_2_30");
                    cutsceneSeq++;
                }
                else if (Main.gTimer >= fTimer + 6000 && cutsceneSeq == 2)
                {
                    CLEAR_HELP();
                    IVText.TheIVText.ReplaceTextOfTextLabel("TM_2_30", "Strangers with firearms are not allowed to access the upper levels of the residence.");
                    PRINT_HELP_FOREVER("TM_2_30");
                    cutsceneSeq++;
                }
                else if (cutsceneSeq == 1 && IS_SCREEN_FADED_IN())
                {
                    CLEAR_HELP();
                    IVText.TheIVText.ReplaceTextOfTextLabel("TM_2_30", "Recent police investigations into Clarence's operations have made his security more strict.");
                    PRINT_HELP_FOREVER("TM_2_30");
                    cutsceneSeq++;
                }
            }
        }
        private static void StartCutscene()
        {
            if (LOCATE_CHAR_ANY_MEANS_3D(Main.PlayerHandle, -65.4566f, 1279.167f, 19.4293f, 0.5f, 0.5f, 2.5f, false) && !introCutscene)
            {
                if (!IS_PLAYER_CONTROL_ON(Main.PlayerIndex) && IS_SCREEN_FADING_IN())
                {
                    introCutscene = true;
                }
            }
            else if (introCutscene && reqdBodyCount == 100)
            {
                if (cutsceneSeq < 0 && IS_PLAYER_CONTROL_ON(Main.PlayerIndex))
                {
                    SET_PLAYER_CONTROL(Main.PlayerIndex, false);
                    //DO_SCREEN_FADE_OUT(1000);

                    GET_GAME_TIMER(out fTimer);
                    fTimer += 5000;
                    cutsceneSeq = 0;
                }
                ProcessStartCutscene();
            }
        }
        public static void Tick()
        {
            if (NativeGame.IsScriptRunning("francis3"))
            {
                if (!missionStarted)
                    SpawnObjects();

                StartCutscene();
                ProcessEnemyPeds();
                ProcessRemoveWeapons();
                ProcessReturnWeapons();
                GunCutscene();

                if (missionStage <= 3 && (IS_CHAR_IN_AREA_3D(Main.PlayerHandle, -62.7729f, 1320.307f, 26.72439f, -53.61643f, 1327.459f, 21.4801f, false) || IS_CHAR_IN_AREA_3D(Main.PlayerHandle, -123.8969f, 1320.307f, 21.4801f, -113.9582f, 1327.459f, 26.72439f, false)))
                {
                    for (int i = fistOnly ? 1 : 2; i < 10; i++)
                    {
                        GET_CHAR_WEAPON_IN_SLOT(Main.PlayerHandle, i, out int pWeap, out int pAmmo0, out int pAmmo1);
                        if (pWeap > 0 && (pWeap < 46 || pWeap > 57)) 
                        {
                            missionStage = 1;
                            break;
                        }
                    }
                    if (missionStage != 1)
                    {
                        if (IS_CHAR_IN_AREA_3D(Main.PlayerHandle, -62.7729f, 1320.307f, 26.72439f, -53.61643f, 1327.459f, 21.4801f, false))
                            missionStage = 2;
                        else
                            missionStage = 3;
                    }
                }

                if (missionStage >= 2)
                {
                    if (missionStage < 7 && IS_PLAYER_CLIMBING(Main.PlayerIndex))
                        CLEAR_CHAR_TASKS_IMMEDIATELY(Main.PlayerHandle);

                    if (missionStage == 5 && LOCATE_CHAR_ANY_MEANS_3D(Main.PlayerHandle, -73.624f, 1325.15f, 26.0338f, 2.5f, 2.5f, 2.5f, false) || LOCATE_CHAR_ANY_MEANS_3D(Main.PlayerHandle, -102.464f, 1325.15f, 26.0338f, 2.5f, 2.5f, 2.5f, false))
                    {
                        /*SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("bm_gate_13"), -61.0304f, 1325.86f, 26.0338f, true, 0.0f);
                        SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("bm_gate_13"), -115.287f, 1325.86f, 26.0338f, true, 0.0f);*/
                        missionStage = 6;
                    }
                    else if (LOCATE_CHAR_ANY_MEANS_3D(Main.PlayerHandle, -73.782f, 1303.390f, 26.0338f, 2.5f, 2.5f, 2.5f, false) || LOCATE_CHAR_ANY_MEANS_3D(Main.PlayerHandle, -102.480f, 1303.390f, 26.0338f, 2.5f, 2.5f, 2.5f, false))
                    {
                        DELETE_OBJECT(ref invisFence[0]);
                    }

                    //IVGame.ShowSubtitleMessage(injPedCount.ToString());
                    if (DOES_CHAR_EXIST(clarencePed))
                    {
                        SET_CHAR_MOVE_ANIM_SPEED_MULTIPLIER(clarencePed, 1.25f);
                        if (injPedCount < reqdBodyCount && (LOCATE_CHAR_ANY_MEANS_3D(clarencePed, -48.331f, 1307.785f, 38.855f, 1.0f, 1.0f, 1.0f, false) || LOCATE_CHAR_ANY_MEANS_3D(clarencePed, -127.611f, 1307.785f, 38.855f, 1.0f, 1.0f, 1.0f, false)))
                        {
                            SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("cj_gate_3_7r"), -51.0518f, 1306.21f, 39.3297f, true, 0.0f);
                            SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("cj_gate_3_7r"), -130.3318f, 1306.21f, 39.3297f, true, 0.0f);
                        }
                        else if (injPedCount < 10 && (LOCATE_CHAR_ANY_MEANS_3D(clarencePed, -58.803f, 1326.14f, 34.693f, 1.0f, 1.0f, 1.0f, false) || LOCATE_CHAR_ANY_MEANS_3D(clarencePed, -117.512f, 1326.14f, 34.693f, 1.0f, 1.0f, 1.0f, false)))
                        {
                            SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("cj_ext_door_9"), -57.303f, 1326.84f, 34.9383f, true, 0.0f);
                            SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("cj_ext_door_9"), -119.012f, 1326.84f, 34.9383f, true, 0.0f);
                        }
                    }

                    if (injPedCount >= reqdBodyCount && cutsceneSeq < 0)
                    {
                        SET_PLAYER_CONTROL(Main.PlayerIndex, false);
                        DO_SCREEN_FADE_OUT(1000);

                        GET_GAME_TIMER(out fTimer);
                        fTimer += 5000;

                        cutsceneSeq = 0;
                        /*SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("cj_gate_3_7r"), -51.0518f, 1306.21f, 39.3297f, false, 0.0f);
                        SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("cj_gate_3_7r"), -130.3318f, 1306.21f, 39.3297f, false, 0.0f);

                        SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("bm_gate_13"), -57.6508f, 1326.54f, 20.4627f, false, 0.0f);
                        SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("bm_gate_13"), -118.663f, 1326.54f, 20.4627f, false, 0.0f);

                        if (!HAS_CHAR_GOT_WEAPON(Main.PlayerHandle, weaponToGive))
                            GIVE_WEAPON_TO_CHAR(Main.PlayerHandle, weaponToGive, ammoToGive, true);*/
                    }
                    else if (injPedCount >= 12)
                    {
                        DELETE_OBJECT(ref invisFence[1]);
                        DELETE_OBJECT(ref invisFence[2]);
                        SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("cj_ext_door_9"), -57.303f, 1326.84f, 34.9383f, false, 0.0f);
                        SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("cj_ext_door_9"), -119.012f, 1326.84f, 34.9383f, false, 0.0f);
                        //DELETE_OBJECT(ref invisFence[3]);
                    }
                    else if (injPedCount >= 10)
                    {
                        SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("cj_ext_door_9"), -57.303f, 1326.84f, 34.9383f, false, 0.0f);
                        SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("cj_ext_door_9"), -119.012f, 1326.84f, 34.9383f, false, 0.0f);
                    }
                    else if (injPedCount >= 8)
                    {
                        if (DOES_OBJECT_EXIST(invisFence[5]))
                            DELETE_OBJECT(ref invisFence[5]);
                        SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("bm_gate_13"), -61.0304f, 1325.86f, 30.3858f, false, 0.0f);
                        SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("bm_gate_13"), -115.287f, 1325.86f, 30.3858f, false, 0.0f);
                    }
                    else if (injPedCount >= 7)
                    {
                        SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("bm_gate_13"), -61.0304f, 1325.86f, 30.3858f, false, 0.0f);
                        SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("bm_gate_13"), -115.287f, 1325.86f, 30.3858f, false, 0.0f);
                    }
                    else if (injPedCount >= 3 && missionStage == 4)
                    {
                        DELETE_OBJECT(ref invisFence[3]);
                        DELETE_OBJECT(ref invisFence[4]);
                        SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("bm_gate_13"), -61.0304f, 1325.86f, 26.0338f, false, 0.0f);
                        SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("bm_gate_13"), -115.287f, 1325.86f, 26.0338f, false, 0.0f);

                        missionStage = 5;
                    }
                    else if ((injPedCount >= 1 || inCombat) && (missionStage == 2 || missionStage == 3))
                    {
                        if (LOCATE_CHAR_ANY_MEANS_3D(Main.PlayerHandle, -61.0304f, 1327.25f, 26.2806f, 8.0f, 8.0f, 2.0f, false) || LOCATE_CHAR_ANY_MEANS_3D(Main.PlayerHandle, -115.287f, 1327.25f, 26.2806f, 8.0f, 8.0f, 2.0f, false))
                        {
                            if (LOCATE_CHAR_ANY_MEANS_3D(Main.PlayerHandle, -61.0304f, 1327.25f, 26.2806f, 8.0f, 8.0f, 2.0f, false))
                            {
                                SET_CHAR_COORDINATES(PedTPListA[0], -57.1508f, 1325.15f, 20.2074f);
                                SET_CHAR_COORDINATES(PedTPListA[1], -57.1508f, 1324.15f, 20.2074f);
                                _TASK_COMBAT(PedTPListA[0], Main.PlayerHandle);
                                _TASK_COMBAT(PedTPListA[1], Main.PlayerHandle);
                                SET_CHAR_RELATIONSHIP(PedTPListA[0], (int)eRelationship.RELATIONSHIP_HATE, (int)eRelationshipGroup.RELATIONSHIP_GROUP_PLAYER);
                                SET_CHAR_RELATIONSHIP(PedTPListA[1], (int)eRelationship.RELATIONSHIP_HATE, (int)eRelationshipGroup.RELATIONSHIP_GROUP_PLAYER);
                            }
                            else if (LOCATE_CHAR_ANY_MEANS_3D(Main.PlayerHandle, -115.287f, 1327.25f, 26.2806f, 8.0f, 8.0f, 2.0f, false))
                            {
                                SET_CHAR_COORDINATES(PedTPListB[0], -119.163f, 1325.15f, 20.2074f);
                                SET_CHAR_COORDINATES(PedTPListB[1], -119.163f, 1324.15f, 20.2074f);
                                _TASK_COMBAT(PedTPListB[0], Main.PlayerHandle);
                                _TASK_COMBAT(PedTPListB[1], Main.PlayerHandle);
                                SET_CHAR_RELATIONSHIP(PedTPListB[0], (int)eRelationship.RELATIONSHIP_HATE, (int)eRelationshipGroup.RELATIONSHIP_GROUP_PLAYER);
                                SET_CHAR_RELATIONSHIP(PedTPListB[1], (int)eRelationship.RELATIONSHIP_HATE, (int)eRelationshipGroup.RELATIONSHIP_GROUP_PLAYER);
                            }

                            SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("bm_gate_13"), -57.6508f, 1326.54f, 20.4627f, true, 0.0f);
                            SET_STATE_OF_CLOSEST_DOOR_OF_TYPE((uint)GET_HASH_KEY("bm_gate_13"), -118.663f, 1326.54f, 20.4627f, true, 0.0f);

                            if (missionStage == 2)
                                reqdBodyCount = 17;
                            else
                            {
                                reqdBodyCount = 14;
                                DELETE_OBJECT(ref invisFence[5]);
                            }

                            missionStage = 4;
                        }
                    }
                }
            }
            else if (missionStarted)
            {
                UnInit();
            }
        }
    }
}
