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
    internal class HaveAHeartTimed
    {
        // IniShit
        private static uint timeLimit;
        private static string newMessage;
        private static string warnMessage;

        // Booleans
        private static bool startTime;
        private static bool hasWarned;
        private static bool noDoctor;
        private static bool tpThePlayer;

        // OtherShit
        private static uint fTimer;

        private static int objHandle;
        private static int pVeh;
        public static void Init(SettingsFile settings)
        {
            timeLimit = settings.GetUInteger("MAIN", "HAHTimeLimit", 120000);
            newMessage = settings.GetValue("MAIN", "HAHMessage", "");
            warnMessage = settings.GetValue("MAIN", "HAHWarning", "");
        }
        public static void Tick()
        {
            if (NativeGame.IsScriptRunning("elizabeta4"))
            {
                if (IS_CHAR_SITTING_IN_ANY_CAR(Main.PlayerHandle))
                    GET_CAR_CHAR_IS_USING(Main.PlayerHandle, out pVeh);
                if (!startTime && IS_THIS_PRINT_BEING_DISPLAYED("EB4X01", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0) && !noDoctor)
                {
                    //IVText.TheIVText.ReplaceTextOfTextLabel("EB4X01", newMessage);
                    GET_GAME_TIMER(out fTimer);
                    startTime = true;
                }
                else if (startTime)
                {
                    if (IS_THIS_PRINT_BEING_DISPLAYED("EB4X01", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0))
                        IVGame.ShowSubtitleMessage(newMessage, 5000);
                    if (LOCATE_CHAR_IN_CAR_3D(Main.PlayerHandle, 1223.573f, 694.0489f, 39.02f, 2.5f, 2.5f, 2.5f, false))
                        startTime = false;
                    //IVGame.ShowSubtitleMessage(gTimer.ToString() + "  " + (fTimer + timeLimit).ToString());

                    if (!HAS_MODEL_LOADED(GET_HASH_KEY("p_water_tow_2")))
                        REQUEST_MODEL(GET_HASH_KEY("p_water_tow_2"));

                    if (Main.gTimer >= fTimer + timeLimit)
                    {
                        IVGame.ShowSubtitleMessage("~s~The doctor is out. ~y~Dispose of the bodies~s~ some other way.", 5000);

                        //CREATE_OBJECT(GET_HASH_KEY("p_water_tow_2"), 1223.573f, 694.0489f, 34.02f, out objHandle, true);
                        //FREEZE_OBJECT_POSITION(objHandle, true);
                        
                        noDoctor = true;
                        startTime = false;
                    }

                    if ((Main.gTimer >= fTimer + (timeLimit - 30000)) && !hasWarned)
                    {
                        IVGame.ShowSubtitleMessage(warnMessage, 4000);
                        hasWarned = true;
                    }
                }
                if (noDoctor && !tpThePlayer)
                {
                    if (IS_CHAR_SITTING_IN_ANY_CAR(Main.PlayerHandle) && LOCATE_CHAR_IN_CAR_3D(Main.PlayerHandle, 1223.573f, 694.0489f, 34.02f, 10.0f, 10.0f ,10.0f, false) && IS_CAR_A_MISSION_CAR(pVeh))
                    {
                        SET_PLAYER_CONTROL(Main.PlayerIndex, false);
                        //FREEZE_CAR_POSITION(pVeh, true);

                        DO_SCREEN_FADE_OUT(1000);
                        tpThePlayer = true;
                    }
                }
                if (tpThePlayer && IS_SCREEN_FADED_OUT())
                {
                    //FREEZE_CAR_POSITION(pVeh, false);
                    SET_CAR_COORDINATES(pVeh, 1197.220f, 702.707f, 35.570f);
                    SET_CAR_HEADING(pVeh, 90);
                    SET_PLAYER_CONTROL(Main.PlayerIndex, true);
                    DO_SCREEN_FADE_IN(1000);
                    IVGame.ShowSubtitleMessage("~s~The doctor is out. ~y~Dispose of the bodies~s~ some other way.", 5000);
                    tpThePlayer = false;
                }
            }
            else
            {
                //DELETE_OBJECT(ref objHandle);

                noDoctor = false;
                hasWarned = false;
                startTime = false;
            }
        }
    }
}
