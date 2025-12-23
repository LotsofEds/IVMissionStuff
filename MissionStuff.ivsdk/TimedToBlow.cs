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
    internal class TimedToBlow
    {
        // IniShit
        private static uint timeLimit;
        private static string newMessage;
        private static string warnMessage;
        //private static string startGXT;
        //private static string stopGXT;
        //private static string textToReplace;
        //private static string failText;

        // Booleans
        private static bool startTime;
        private static bool hasWarned;
        private static bool isKeyPressed;

        // OtherShit
        /*private static uint dayLimit;
        private static int hourLimit;
        private static int minuteLimit;
        private static uint currDay;
        private static int currHour;
        private static int currMinute;*/

        private static uint fTimer;

        // MissionStuff
        private static int pPed1;
        private static int pPed2;
        private static int pPed3;
        private static int pVeh;
        public static void Init(SettingsFile settings)
        {
            timeLimit = settings.GetUInteger("MAIN", "TTBTimeLimit", 180000);
            newMessage = settings.GetValue("MAIN", "TTBMessage", "");
            warnMessage = settings.GetValue("MAIN", "TTBWarning", "");
        }
        public static void UnInit()
        {
            MARK_CHAR_AS_NO_LONGER_NEEDED(pPed1);
            MARK_CHAR_AS_NO_LONGER_NEEDED(pPed2);
            MARK_CHAR_AS_NO_LONGER_NEEDED(pPed3);

            MARK_CAR_AS_NO_LONGER_NEEDED(pVeh);
        }
        public static void Tick()
        {
            if (NativeGame.IsScriptRunning("faustin5"))
            {
                if (!startTime && (IS_THIS_PRINT_BEING_DISPLAYED("TRKBRN_04", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)))
                {
                    //IVText.TheIVText.ReplaceTextOfTextLabel("TRKBRN_04", newMessage);
                    GET_GAME_TIMER(out fTimer);
                    startTime = true;
                }
                else if (startTime)
                {
                    if (IS_THIS_PRINT_BEING_DISPLAYED("TRKBRN_04", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0))
                        IVGame.ShowSubtitleMessage(newMessage, 5000);
                    if (IS_THIS_PRINT_BEING_DISPLAYED("TRKBRN_10", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0))
                        startTime = false;
                    //IVGame.ShowSubtitleMessage(gTimer.ToString() + "  " + (fTimer + 20000).ToString());

                    if (!HAS_MODEL_LOADED(GET_HASH_KEY("rebla")))
                        REQUEST_MODEL(GET_HASH_KEY("rebla"));

                    if (!HAS_MODEL_LOADED(GET_HASH_KEY("m_m_gru2_hi_01")))
                        REQUEST_MODEL(GET_HASH_KEY("m_m_gru2_hi_01"));

                    if (Main.gTimer >= fTimer + timeLimit)
                    {
                        IVGame.ShowSubtitleMessage("~s~Some ~r~workers~s~ have arrived to guard the place.", 4000);
                        CREATE_CAR(GET_HASH_KEY("rebla"), 618.162f, 1481.202f, 12.465f, out pVeh, true);
                        /*CREATE_CHAR_INSIDE_CAR(pVeh, (int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, (uint)GET_HASH_KEY("m_m_gru2_hi_01"), out int pPed1);
                        CREATE_CHAR_AS_PASSENGER(pVeh, (int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, (uint)GET_HASH_KEY("m_m_gru2_hi_01"), 0, out int pPed2);
                        CREATE_CHAR_AS_PASSENGER(pVeh, (int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, (uint)GET_HASH_KEY("m_m_gru2_hi_01"), 1, out int pPed3);
                        CREATE_CHAR_AS_PASSENGER(pVeh, (int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, (uint)GET_HASH_KEY("m_m_gru2_hi_01"), 2, out int pPed4);*/

                        CREATE_CHAR((int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, GET_HASH_KEY("m_m_gru2_hi_01"), 622.262f, 1466.863f, 11.914f, out pPed1, true);
                        CREATE_CHAR((int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, GET_HASH_KEY("m_m_gru2_hi_01"), 615.519f, 1466.863f, 11.875f, out pPed2, true);
                        CREATE_CHAR((int)ePedType.PED_TYPE_GANG_RUSSIAN_GANG, GET_HASH_KEY("m_m_gru2_hi_01"), 617.973f, 1472.532f, 12.815f, out pPed3, true);

                        GIVE_WEAPON_TO_CHAR(pPed1, 12, -1, false);
                        GIVE_WEAPON_TO_CHAR(pPed2, 12, -1, false);
                        GIVE_WEAPON_TO_CHAR(pPed3, 10, -1, false);

                        SET_CHAR_RELATIONSHIP_GROUP(pPed1, (int)eRelationshipGroup.RELATIONSHIP_GROUP_MISSION_1);
                        SET_CHAR_RELATIONSHIP_GROUP(pPed2, (int)eRelationshipGroup.RELATIONSHIP_GROUP_MISSION_1);
                        SET_CHAR_RELATIONSHIP_GROUP(pPed2, (int)eRelationshipGroup.RELATIONSHIP_GROUP_MISSION_1);

                        SET_CHAR_RELATIONSHIP(pPed1, (int)eRelationship.RELATIONSHIP_RESPECT, (int)eRelationshipGroup.RELATIONSHIP_GROUP_MISSION_1);
                        SET_CHAR_RELATIONSHIP(pPed2, (int)eRelationship.RELATIONSHIP_RESPECT, (int)eRelationshipGroup.RELATIONSHIP_GROUP_MISSION_1);
                        SET_CHAR_RELATIONSHIP(pPed3, (int)eRelationship.RELATIONSHIP_RESPECT, (int)eRelationshipGroup.RELATIONSHIP_GROUP_MISSION_1);

                        SET_CHAR_RELATIONSHIP(pPed1, (int)eRelationship.RELATIONSHIP_HATE, (int)eRelationshipGroup.RELATIONSHIP_GROUP_PLAYER);
                        SET_CHAR_RELATIONSHIP(pPed2, (int)eRelationship.RELATIONSHIP_HATE, (int)eRelationshipGroup.RELATIONSHIP_GROUP_PLAYER);
                        SET_CHAR_RELATIONSHIP(pPed3, (int)eRelationship.RELATIONSHIP_HATE, (int)eRelationshipGroup.RELATIONSHIP_GROUP_PLAYER);
                        //GIVE_WEAPON_TO_CHAR(pPed4, 7, -1, false);

                        startTime = false;
                        //DAMAGE_CHAR(Main.PlayerHandle, 200, false);
                        // 611.162, 1528.842, 20.465, 270.304
                    }

                    if (Main.gTimer >= fTimer + (timeLimit - 30000) && !hasWarned)
                    {
                        IVText.TheIVText.ReplaceTextOfTextLabel("PLACEHOLDERSL", warnMessage);
                        PRINT_HELP("PLACEHOLDERSL");
                        //IVGame.ShowSubtitleMessage(warnMessage, 4000);
                        hasWarned = true;
                    }
                }
            }
            else if (startTime)
            {
                MARK_CHAR_AS_NO_LONGER_NEEDED(pPed1);
                MARK_CHAR_AS_NO_LONGER_NEEDED(pPed2);
                MARK_CHAR_AS_NO_LONGER_NEEDED(pPed3);

                MARK_CAR_AS_NO_LONGER_NEEDED(pVeh);

                hasWarned = false;
                startTime = false;
            }
        }
    }
}
