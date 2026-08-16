using System;
using System.Collections.Generic;
using CCL.GTAIV;
using static IVSDKDotNet.Native.Natives;
using IVSDKDotNet;
using MissionStuff.ivsdk;

// Credits to catsmackaroo

namespace MissionStuff
{
    internal class SimpleMenu
    {
        public class MenuItem
        {
            public virtual string Label { get; }
            public Action OnSelect { get; }
            public Func<string> Description { get; }
            public Action OnLeft { get; }
            public Action OnRight { get; }

            public MenuItem(string label, Action onSelect, Func<string> description = null, Action onLeft = null, Action onRight = null)
            {
                Label = label;
                OnSelect = onSelect;
                Description = description;
                OnLeft = onLeft;
                OnRight = onRight;
            }
        }

        public List<MenuItem> _items = new List<MenuItem>();
        public int _selectedIndex;
        private string _title;

        public string Title
        {
            get => _title;
            set => _title = value;
        }

        private bool _active;
        private bool _keyPressed;
        private static readonly Stack<SimpleMenu> _menuStack = new Stack<SimpleMenu>();
        public Action OnHide { get; set; }
        public SimpleMenu(string title)
        {
            _title = title;
        }

        public void AddItem(Func<string> label, Action onSelect, Func<string> description = null, Action onLeft = null, Action onRight = null)
        {
            _items.Add(new MenuItemWithDynamicLabel(label, onSelect, description, onLeft, onRight));
        }

        private class MenuItemWithDynamicLabel : MenuItem
        {
            private readonly Func<string> _labelFunc;
            public MenuItemWithDynamicLabel(Func<string> labelFunc, Action onSelect, Func<string> description = null, Action onLeft = null, Action onRight = null)
                : base(null, onSelect, description, onLeft, onRight)
            {
                _labelFunc = labelFunc;
            }
            public override string Label => _labelFunc();
        }

        public void Show()
        {
            if (_active)
                return;

            if (_menuStack.Count > 0 && _menuStack.Peek() != this)
                _menuStack.Peek()._active = false;

            _menuStack.Push(this);
            _active = true;
            _selectedIndex = 0;
        }
        public void Hide()
        {
            if (!_active)
                return;

            _active = false;
            OnHide?.Invoke();
            IVPhoneInfo.ThePhoneInfo.DisablePhone = 0;
            //CLEAR_HELP();
            //CLEAR_PRINTS();

            if (_menuStack.Count > 0 && _menuStack.Peek() == this)
                _menuStack.Pop();

            if (_menuStack.Count > 0)
            {
                var previousMenu = _menuStack.Peek();
                previousMenu._active = true;
            }
        }
        public void ClearItems()
        {
            _items.Clear();
            _selectedIndex = 0;
        }

        public bool IsActive => _active;
        public void Tick()
        {
            if (!_active || _items.Count == 0)
                return;

            if (_selectedIndex >= _items.Count)
                _selectedIndex = 0;

            IVPhoneInfo.ThePhoneInfo.DisablePhone = 1;

            // Navigation
            if (!_keyPressed || !IS_PLAYER_CONTROL_ON(Main.PlayerIndex))
            {
                if (IS_CONTROL_JUST_PRESSED(0, (int)GameKey.NavUp))
                {
                    _selectedIndex = (_selectedIndex - 1 + _items.Count) % _items.Count;
                    PLAY_SOUND_FRONTEND(-1, "FRONTEND_MENU_TOGGLE_MT");
                    _keyPressed = true;
                }
                else if (IS_CONTROL_JUST_PRESSED(0, (int)GameKey.NavDown))
                {
                    _selectedIndex = (_selectedIndex + 1) % _items.Count;
                    PLAY_SOUND_FRONTEND(-1, "FRONTEND_MENU_TOGGLE_MT");
                    _keyPressed = true;
                }
                else if (IS_CONTROL_JUST_PRESSED(0, (int)GameKey.NavEnter))
                {
                    _items[_selectedIndex].OnSelect?.Invoke();
                    PLAY_SOUND_FRONTEND(-1, "FRONTEND_MENU_TOGGLE_ON");
                    if (!_active) return;
                    _keyPressed = true;
                }
                else if (IS_CONTROL_JUST_PRESSED(0, (int)GameKey.NavBack) || !IS_PLAYER_CONTROL_ON(Main.PlayerIndex))
                {
                    PLAY_SOUND_FRONTEND(-1, "FRONTEND_MENU_BACK");
                    _keyPressed = true;
                    Hide();
                    return;
                }
                else if (IS_CONTROL_JUST_PRESSED(0, (int)GameKey.NavLeft))
                {
                    var onLeft = _items[_selectedIndex].OnLeft;
                    if (onLeft != null)
                    {
                        onLeft.Invoke();
                        PLAY_SOUND_FRONTEND(-1, "FRONTEND_MENU_TOGGLE_MT");
                    }
                    _keyPressed = true;
                }
                else if (IS_CONTROL_JUST_PRESSED(0, (int)GameKey.NavRight))
                {
                    var onRight = _items[_selectedIndex].OnRight;
                    if (onRight != null)
                    {
                        onRight.Invoke();
                        PLAY_SOUND_FRONTEND(-1, "FRONTEND_MENU_TOGGLE_MT");
                    }
                    _keyPressed = true;
                }
            }
            else
                _keyPressed = false;
            
            string menuText = $"~n~~h~~w~{_title}~s~~m~~n~";
            for (int i = 0; i < _items.Count; i++)
            {
                if (i == _selectedIndex)
                    menuText += $"~h~ ~y~{_items[i].Label}~s~~m~~n~";
                else
                    menuText += $"{_items[i].Label}~n~";
            }

            var desc = _items[_selectedIndex].Description?.Invoke();
            if (!string.IsNullOrEmpty(desc))
                menuText += $"~n~~w~{desc}";

            DRAW_RECT(0.15f, 0.6f, 0.2f, 0.25f, 0, 0, 0, 128);

            IVText.TheIVText.ReplaceTextOfTextLabel("PLACEHOLDERSL", menuText);

            SET_TEXT_SCALE(0.25f, 0.25f);
            SET_TEXT_DROPSHADOW(false, 0, 0, 0, 0);
            SET_TEXT_FONT(6);
            SET_TEXT_CENTRE(false);
            SET_TEXT_COLOUR(255, 255, 255, 255);
            SET_TEXT_WRAP(0.06f, 0.24f);

            DISPLAY_TEXT(0.15f, 0.475f, "PLACEHOLDERSL");
        }

        public static SimpleMenu CurrentMenu
        {
            get
            {
                return _menuStack.Count > 0 ? _menuStack.Peek() : null;
            }
        }

    }
}
