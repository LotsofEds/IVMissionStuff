using CCL;
using CCL.GTAIV;
using IVSDKDotNet;
using IVSDKDotNet.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Numerics;
using System.Runtime;
using System.Windows.Forms;
using System.Xml.Linq;
using static IVSDKDotNet.Native.Natives;

namespace MissionStuff.ivsdk
{
    internal class Pills
    {
        // IniShit
        private static float packieStat;
        private static int antiDepressHeal;
        private static int adrenalineDuration;
        private static int painKillerDuration;
        private static string antiDepressScreenFx;
        private static string adrenalineScreenFx;
        private static string painKillerScreenFx;
        private static int antiDepressCost;
        private static int adrenalineCost;
        private static int painkillerCost;
        private static int maxPills;
        private static int pillCooldown;

        // BooleShit
        private static bool canBuyPills;
        private static bool inMenu;
        private static bool pillActive;
        private static bool takeAntiDepress;
        private static bool takeAdrenaline;
        private static bool takePainkiller;
        private static bool adrenalineActive;
        private static bool painKillerActive;
        private static bool gotHealth;
        private static bool gotPillCount;

        // OtherShit
        private static int time;
        private static int pillCost;
        private static int pillIndex;
        private static string pillName;
        private static string pillDesc;
        private static int pillBlip;
        private static int pillCount;
        private static int aPillCount;
        private static int pPillCount;
        private static int dPillCount;
        private static uint fTimer;
        private static int objHandle;
        private static uint oldHealth;

        private static SimpleMenu pillMenu;
        private static bool CheckCooldown()
        {
            if (Main.gTimer >= fTimer + pillCooldown)
                return true;
            else 
                return false;
        }
        private static void DisplayDoseText()
        {
            IVText.TheIVText.ReplaceTextOfTextLabel("TM_1_6", "~r~You cannot overdose on pills!");

            if (!IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("TM_1_6"))
                PRINT_HELP("TM_1_6");
        }
        public static void UnInit()
        {
            REMOVE_BLIP(pillBlip);
            DELETE_OBJECT(ref objHandle);
            CLEAR_TIMECYCLE_MODIFIER();
            SET_TIME_SCALE(1.0f);
            SET_CHAR_MOVE_ANIM_SPEED_MULTIPLIER(Main.PlayerHandle, 1f);
        }
        public static void IngameStart()
        {
            gotPillCount = false;
            fTimer = 0;
        }

        public static void Init (SettingsFile settings)
        {
            packieStat = settings.GetFloat("PACKIE'S PILLS", "LikeRequirement", 80);
            antiDepressHeal = settings.GetInteger("PACKIE'S PILLS", "AntiDepressantHealAmount", 25);
            adrenalineDuration = settings.GetInteger("PACKIE'S PILLS", "AdrenalineDuration", 20000);
            painKillerDuration = settings.GetInteger("PACKIE'S PILLS", "PainkillerDuration", 20000);
            antiDepressScreenFx = settings.GetValue("PACKIE'S PILLS", "AntiDepressantScreenFilter", "");
            adrenalineScreenFx = settings.GetValue("PACKIE'S PILLS", "AdrenalineScreenFilter", "");
            painKillerScreenFx = settings.GetValue("PACKIE'S PILLS", "PainkillerScreenFilter", "");
            antiDepressCost = settings.GetInteger("PACKIE'S PILLS", "AntiDepressantCost", 200);
            adrenalineCost = settings.GetInteger("PACKIE'S PILLS", "AdrenalineCost", 200);
            painkillerCost = settings.GetInteger("PACKIE'S PILLS", "PainkilllerCost", 200);
            maxPills = settings.GetInteger("PACKIE'S PILLS", "MaxPills", 3);
            pillCooldown = settings.GetInteger("PACKIE'S PILLS", "PillCooldown", 300000);

            InitMenu();
        }
        public static void InitMenu()
        {
            pillMenu = new SimpleMenu("Pills");

            Func<string> menuName = () => "Pills";
            Action pillAction = pillMenu.Show;
            Func<string> pillDescription = () => "Take performance-enhancing pills.";

            Main.actionMenu.AddItem(menuName, pillAction, pillDescription, null, null);
            
            menuName = () => "Adrenaline Pills";
            pillAction = Adrenaline;
            pillDescription = () => "Slows down time and increases movement speed for a short period of time.";

            pillMenu.AddItem(menuName, pillAction, pillDescription, null, null);

            menuName = () => "Painkillers";
            pillAction = PainKiller;
            pillDescription = () => "Reduces damage taken by half for a short period of time.";

            pillMenu.AddItem(menuName, pillAction, pillDescription, null, null);

            menuName = () => "Anti-Depressants";
            pillAction = AntiDepressant;
            pillDescription = () => "Replenishes a small amount of health immediately.";

            pillMenu.AddItem(menuName, pillAction, pillDescription, null, null);
        }
        public static void SavePillCount(SettingsFile settings)
        {
            if (!settings.DoesSectionExists(IVGenericGameStorage.ValidSaveName))
                settings.AddSection(IVGenericGameStorage.ValidSaveName);
            if (!settings.DoesKeyExists(IVGenericGameStorage.ValidSaveName, "AdrenalineCount"))
                settings.AddKeyToSection(IVGenericGameStorage.ValidSaveName, "AdrenalineCount");
            if (!settings.DoesKeyExists(IVGenericGameStorage.ValidSaveName, "PainkillerCount"))
                settings.AddKeyToSection(IVGenericGameStorage.ValidSaveName, "PainkillerCount");
            if (!settings.DoesKeyExists(IVGenericGameStorage.ValidSaveName, "AntiDepressantCount"))
                settings.AddKeyToSection(IVGenericGameStorage.ValidSaveName, "AntiDepressantCount");

            settings.SetInteger(IVGenericGameStorage.ValidSaveName, "AdrenalineCount", aPillCount);
            settings.SetInteger(IVGenericGameStorage.ValidSaveName, "PainkillerCount", pPillCount);
            settings.SetInteger(IVGenericGameStorage.ValidSaveName, "AntiDepressantCount", dPillCount);

            settings.Save();
            settings.Load();
        }
        private static void ProcessBuying()
        {
            if (!inMenu)
            {
                if (GET_FLOAT_STAT(20) >= packieStat)
                    canBuyPills = true;
                else
                {
                    REMOVE_BLIP(pillBlip);
                    canBuyPills = false;
                }
            }
            //canBuyPills = true;
            if (canBuyPills)
            {
                if (!DOES_BLIP_EXIST(pillBlip))
                {
                    // 1371.646, 621.487, 35.829
                    ADD_BLIP_FOR_COORD(1371.646f, 621.487f, 35.829f, out pillBlip);

                    NativeBlip pBlip = new NativeBlip(pillBlip);

                    pBlip.Icon = BlipIcon.Pickup_Health;
                    pBlip.Name = "Pills";
                    pBlip.Display = eBlipDisplay.BLIP_DISPLAY_ARROW_AND_MAP;
                    pBlip.Scale = 0.5f;
                    pBlip.ShowOnlyWhenNear = true;
                }

                if (LOCATE_CHAR_ON_FOOT_3D(Main.PlayerHandle, 1371.646f, 621.487f, 35.829f, 1.0f, 1.0f, 1.0f, true) || inMenu)
                {
                    if (!IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("TM_2_20") && !IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("TM_1_5") && !inMenu)
                    {
                        IVText.TheIVText.ReplaceTextOfTextLabel("TM_2_20", "~s~Press ~INPUT_PICKUP~ to buy pills.");
                        PRINT_HELP_FOREVER("TM_2_20");
                    }
                    if (IS_CONTROL_JUST_PRESSED(0, (int)GameKey.Action) || IS_CONTROL_JUST_PRESSED(2, (int)GameKey.Action))
                    {
                        pillIndex = 0;
                        inMenu = !inMenu;
                        if (inMenu)
                        {
                            CLEAR_HELP();
                            SET_PLAYER_CONTROL(Main.PlayerIndex, false);
                        }
                        else
                        {
                            CLEAR_HELP();
                            SET_PLAYER_CONTROL(Main.PlayerIndex, true);
                        }
                    }

                    if (inMenu)
                    {
                        if (pillIndex == 0)
                        {
                            pillName = "Adrenaline pills";
                            pillCost = adrenalineCost;
                            pillCount = aPillCount;
                            pillDesc = "~s~Slows down time and increases movement speed for a short period of time.";
                        }
                        else if (pillIndex == 1)
                        {
                            pillName = "Painkillers";
                            pillCost = painkillerCost;
                            pillCount = pPillCount;
                            pillDesc = "~s~Reduces damage taken by half for a short period of time.";
                        }
                        else if (pillIndex == 2)
                        {
                            pillName = "Anti-depressants";
                            pillCost = antiDepressCost;
                            pillCount = dPillCount;
                            pillDesc = "~s~Replenishes a small amount of health immediately.";
                        }

                        if (!IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("TM_1_5") && !IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("TM_2_20") && !IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("TM_2_21") && !IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("TM_2_22"))
                        {
                            IVText.TheIVText.ReplaceTextOfTextLabel("TM_1_5", "~s~Use ~PAD_LEFT~ and ~PAD_RIGHT~ to browse pills. ~n~~s~Press ~INPUT_PICKUP~ to cancel. ~n~~s~Press ~INPUT_FRONTEND_ACCEPT~ to buy. ~n~~g~" + pillName + " $" + pillCost.ToString() + " ~n~~s~Currently have " + pillCount.ToString());
                            PRINT_HELP_FOREVER("TM_1_5");
                            // Description
                            //IVGame.ShowSubtitleMessage(pillDesc);
                        }
                        IVText.TheIVText.ReplaceTextOfTextLabel("PLACEHOLDER_1", pillDesc);
                        PRINT_NOW("PLACEHOLDER_1", 100, false);
                        //IVGame.ShowSubtitleMessage(pillDesc);

                        if (IS_CONTROL_JUST_PRESSED(0, (int)GameKey.NavEnter) || IS_CONTROL_JUST_PRESSED(2, (int)GameKey.NavEnter))
                        {
                            STORE_SCORE(Main.PlayerIndex, out uint pMoney);
                            if (pMoney >= pillCost && (pPillCount + aPillCount + dPillCount) < maxPills)
                            {
                                ADD_SCORE(Main.PlayerIndex, -pillCost);
                                if (pillIndex == 0)
                                    aPillCount++;

                                else if (pillIndex == 1)
                                    pPillCount++;

                                else if (pillIndex == 2)
                                    dPillCount++;
                            }
                            else if ((pPillCount + aPillCount + dPillCount) >= maxPills)
                            {
                                CLEAR_HELP();
                                IVText.TheIVText.ReplaceTextOfTextLabel("TM_2_21", "~s~~r~You cannot carry any more pills!");
                                PRINT_HELP("TM_2_21");
                            }
                            else if (pMoney < pillCost)
                            {
                                CLEAR_HELP();
                                IVText.TheIVText.ReplaceTextOfTextLabel("TM_2_22", "~s~~r~You don't have enough money!");
                                PRINT_HELP("TM_2_22");
                            }
                        }

                        else if (IS_CONTROL_JUST_PRESSED(0, (int)GameKey.NavLeft) || IS_CONTROL_JUST_PRESSED(2, (int)GameKey.NavLeft))
                        {
                            if (IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("TM_2_21") || IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("TM_2_22"))
                                CLEAR_HELP();
                            if (pillIndex > 0)
                                pillIndex--;
                            else
                                pillIndex = 2;

                            CLEAR_HELP();
                        }

                        else if (IS_CONTROL_JUST_PRESSED(0, (int)GameKey.NavRight) || IS_CONTROL_JUST_PRESSED(2, (int)GameKey.NavRight))
                        {
                            if (IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("TM_2_21") || IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("TM_2_22"))
                                CLEAR_HELP();
                            if (pillIndex < 2)
                                pillIndex++;
                            else
                                pillIndex = 0;

                            CLEAR_HELP();
                        }
                    }
                }
                else if (IS_THIS_HELP_MESSAGE_BEING_DISPLAYED("TM_2_20"))
                    CLEAR_HELP();
            }
        }
        public static void Tick()
        {
            if (!gotPillCount)
            {
                aPillCount = Main.savefileSettings.GetInteger(IVGenericGameStorage.ValidSaveName, "AdrenalineCount", 0);
                pPillCount = Main.savefileSettings.GetInteger(IVGenericGameStorage.ValidSaveName, "PainkillerCount", 0);
                dPillCount = Main.savefileSettings.GetInteger(IVGenericGameStorage.ValidSaveName, "AntiDepressantCount", 0);
                gotPillCount = true;
            }
            pillMenu.Tick();
            ProcessBuying();

            if (!HAVE_ANIMS_LOADED("amb@sprunk_plyr"))
                REQUEST_ANIMS("amb@sprunk_plyr");

            if (takeAdrenaline)
            {
                GET_CHAR_ANIM_CURRENT_TIME(Main.PlayerHandle, "amb@sprunk_plyr", "partial_drink", out float animTime);
                if (animTime > 0.85f || !IS_CHAR_PLAYING_ANIM(Main.PlayerHandle, "amb@sprunk_plyr", "partial_drink"))
                {
                    takeAdrenaline = false;
                    GET_GAME_TIMER(out fTimer);

                    DELETE_OBJECT(ref objHandle);

                    time = adrenalineDuration;

                    SET_TIMECYCLE_MODIFIER(adrenalineScreenFx);
                    SET_TIME_SCALE(0.75f);
                    SET_CHAR_MOVE_ANIM_SPEED_MULTIPLIER(Main.PlayerHandle, (float)(1 / 0.75));
                    adrenalineActive = true;
                }
            }

            else if (takePainkiller)
            {
                GET_CHAR_ANIM_CURRENT_TIME(Main.PlayerHandle, "amb@sprunk_plyr", "partial_drink", out float animTime);
                if (animTime > 0.85f || !IS_CHAR_PLAYING_ANIM(Main.PlayerHandle, "amb@sprunk_plyr", "partial_drink"))
                {
                    takePainkiller = false;
                    GET_GAME_TIMER(out fTimer);

                    DELETE_OBJECT(ref objHandle);

                    time = painKillerDuration;

                    SET_TIMECYCLE_MODIFIER(painKillerScreenFx);
                    painKillerActive = true;
                }
            }

            else if (takeAntiDepress)
            {
                GET_CHAR_ANIM_CURRENT_TIME(Main.PlayerHandle, "amb@sprunk_plyr", "partial_drink", out float animTime);
                if (animTime > 0.85f || !IS_CHAR_PLAYING_ANIM(Main.PlayerHandle, "amb@sprunk_plyr", "partial_drink"))
                {
                    DELETE_OBJECT(ref objHandle);
                    CLEAR_TIMECYCLE_MODIFIER();
                    takeAntiDepress = false;
                    pillActive = false;
                }
            }

            if (adrenalineActive && (Main.gTimer >= fTimer + time || !IS_PLAYER_CONTROL_ON(Main.PlayerIndex)))
            {
                CLEAR_TIMECYCLE_MODIFIER();
                SET_TIME_SCALE(1.0f);
                SET_CHAR_MOVE_ANIM_SPEED_MULTIPLIER(Main.PlayerHandle, 1f);
                adrenalineActive = false;
                pillActive = false;
            }

            else if (painKillerActive)
            {
                GET_CHAR_HEALTH(Main.PlayerHandle, out uint pHealth);

                if (!gotHealth)
                {
                    oldHealth = pHealth;
                    gotHealth = true;
                }

                if (oldHealth > pHealth && gotHealth)
                {
                    SET_CHAR_HEALTH(Main.PlayerHandle, pHealth + ((oldHealth - pHealth) / 2));
                    gotHealth = false;
                }

                if ((Main.gTimer >= fTimer + time) || !IS_PLAYER_CONTROL_ON(Main.PlayerIndex))
                {
                    CLEAR_TIMECYCLE_MODIFIER();
                    painKillerActive = false;
                    pillActive = false;
                }
            }
        }
        private static void TakePill()
        {
            pillActive = true;
            CREATE_OBJECT(GET_HASH_KEY("cspillbottle"), Main.PlayerPos.X, Main.PlayerPos.Y, Main.PlayerPos.Z + 10f, out objHandle, true);
            SET_OBJECT_COLLISION(objHandle, false);
            ATTACH_OBJECT_TO_PED(objHandle, Main.PlayerHandle, (uint)eBone.BONE_RIGHT_HAND, 0.1f, 0.02f, -0.02f, 0f, 0f, 0f, 0);
            _TASK_PLAY_ANIM_SECONDARY_NO_INTERRUPT(Main.PlayerHandle, "partial_drink", "amb@sprunk_plyr", 4, 0, 0, 0, 0, -1);
        }
        private static void Adrenaline()
        {
            if (CheckCooldown())
            {
                if (!pillActive)
                {
                    TakePill();
                    aPillCount--;
                    takeAdrenaline = true;
                }
            }
            else
                DisplayDoseText();
        }
        private static void PainKiller()
        {
            if (CheckCooldown())
            {
                if (!pillActive)
                {
                    TakePill();
                    pPillCount--;
                    takePainkiller = true;
                }
            }
            else
                DisplayDoseText();
        }
        private static void AntiDepressant()
        {
            if (CheckCooldown())
            {
                if (!pillActive)
                {
                    TakePill();
                    dPillCount--;
                    GET_CHAR_HEALTH(Main.PlayerHandle, out uint pHealth);
                    SET_CHAR_HEALTH(Main.PlayerHandle, pHealth + (uint)antiDepressHeal);
                    SET_TIMECYCLE_MODIFIER(antiDepressScreenFx);
                    GET_GAME_TIMER(out fTimer);
                    takeAntiDepress = true;
                }
            }
            else
                DisplayDoseText();
        }
    }
}
