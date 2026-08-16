using CCL;
using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Mail;
using System.Numerics;
using System.Runtime;
using System.Windows.Forms;
using System.Xml.Linq;
using static IVSDKDotNet.Native.Natives;

namespace MissionStuff.ivsdk
{
    internal class RealPoliceCorruption
    {
        // IniShit
        private static int oneStarBribeNormal;
        private static int twoStarBribeNormal;
        private static int threeStarBribeNormal;

        private static int oneStarBribeRecession;
        private static int twoStarBribeRecession;
        private static int threeStarBribeRecession;

        private static bool bribeOnDeath;
        private static bool prisonEnable;
        private static bool romanRescue;

        // OtherShit
        private static int bribeAmount;
        private static bool deathArrest;
        private static bool isDead;
        private static bool inPrison;
        public static void UnInit()
        {
            deathArrest = false;
            isDead = false;
            inPrison = false;
        }
        public static void Init(SettingsFile settings)
        {
            oneStarBribeNormal = settings.GetInteger("ACTUAL BRIBES", "BribeAmountOneStar", 10000);
            twoStarBribeNormal = settings.GetInteger("ACTUAL BRIBES", "BribeAmountTwoStar", 25000);
            threeStarBribeNormal = settings.GetInteger("ACTUAL BRIBES", "BribeAmountThreeStar", 50000);

            if (Main.recessionEnable)
            {
                oneStarBribeRecession = settings.GetInteger("2008 RECESSION SIMULATOR", "BribeAmountOneStar", 15000);
                twoStarBribeRecession = settings.GetInteger("2008 RECESSION SIMULATOR", "BribeAmountTwoStar", 40000);
                threeStarBribeRecession = settings.GetInteger("2008 RECESSION SIMULATOR", "BribeAmountThreeStar", 75000);
            }

            bribeOnDeath = settings.GetBoolean("ACTUAL BRIBES", "NoEasyWayOut", false);
            prisonEnable = settings.GetBoolean("ACTUAL BRIBES", "PrisonBitch", false);
            romanRescue = settings.GetBoolean("ACTUAL BRIBES", "RomanFreebie", false);
        }
        public static void Tick()
        {
            //if (HAS_CHAR_BEEN_ARRESTED(Main.PlayerHandle))
                //IVGame.ShowSubtitleMessage("ass");

            if ((HAS_CHAR_BEEN_ARRESTED(Main.PlayerHandle) && !deathArrest) || (IS_CHAR_DEAD(Main.PlayerHandle) && bribeOnDeath && !isDead))
            {
                if (GET_FLOAT_STAT(22) < 80 || !Main.recessionEnable)
                {
                    if (IS_WANTED_LEVEL_GREATER(Main.PlayerIndex, 2))
                        bribeAmount = threeStarBribeNormal;
                    else if (IS_WANTED_LEVEL_GREATER(Main.PlayerIndex, 1))
                        bribeAmount = twoStarBribeNormal;
                    else if (IS_WANTED_LEVEL_GREATER(Main.PlayerIndex, 0))
                        bribeAmount = oneStarBribeNormal;
                }
                else
                {
                    if (IS_WANTED_LEVEL_GREATER(Main.PlayerIndex, 2))
                        bribeAmount = threeStarBribeRecession;
                    else if (IS_WANTED_LEVEL_GREATER(Main.PlayerIndex, 1))
                        bribeAmount = twoStarBribeRecession;
                    else if (IS_WANTED_LEVEL_GREATER(Main.PlayerIndex, 0))
                        bribeAmount = oneStarBribeRecession;
                }
                if (romanRescue && GET_INT_STAT(261) <= 1)
                    bribeAmount = 0;

                if (IS_CHAR_DEAD(Main.PlayerHandle))
                    isDead = true;

                else
                    deathArrest = true;
            }
            if ((deathArrest && !HAS_CHAR_BEEN_ARRESTED(Main.PlayerHandle) && IS_SCREEN_FADING_IN()) || (bribeOnDeath && isDead && !IS_CHAR_DEAD(Main.PlayerHandle)))
            {
                STORE_SCORE(Main.PlayerIndex, out uint currMoney);
                if (currMoney < bribeAmount && prisonEnable)
                {
                    SET_CHAR_COORDINATES(Main.PlayerHandle, -1070.378f, -456.960f, 1.762f);
                    inPrison = true;
                }
                else
                    ADD_SCORE(Main.PlayerIndex, -bribeAmount);

                deathArrest = false;
                isDead = false;
            }

            if (inPrison)
            {
                if (IS_CHAR_IN_AREA_3D(Main.PlayerHandle, -1100.879f, -484.835f, 1.262f, -1046.963f, -433.598f, 9.262f, false))
                {
                    CLEAR_WANTED_LEVEL(Main.PlayerIndex);
                    SET_ALL_RANDOM_PEDS_FLEE(Main.PlayerIndex, true);
                }
                else
                {
                    SET_ALL_RANDOM_PEDS_FLEE(Main.PlayerIndex, false);
                    inPrison = false;
                }
            }
        }
    }
}
