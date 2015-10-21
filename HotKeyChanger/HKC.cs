using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using SharpDX;
using SharpDX.Direct3D9;

namespace HotKeyChanger
{
    public class HKC
    {

        public enum KeyMode
        {
            HOLD,
            TOGGLE,
        }

        public string VarToCheck { get; private set; }
        public string DisplayText { get; private set; }
        public KeyMode SetMode { get; private set; }
        public Vector2 BoxPosition { get; private set; }
        public Color BoxColor { get; private set; }
        public uint Key { get; private set; }
        private uint OrigKey { get; set; }

        private bool _isActive;

        private int keyToggleT;

        private bool _choosingKey;
        private bool _isUnderCheckbox;
        private bool _gameLoad;

        private static readonly Vector2 _textSize = new Vector2(20);


        public HKC(string variable, string displayText, uint key, KeyMode mode, Vector2 pos, Color boxColor)
        {
            VarToCheck = variable;
            DisplayText = displayText;
            SetMode = mode;
            BoxPosition = pos;
            BoxColor = boxColor;
            Key = key;
            OrigKey = key;

            keyToggleT = 0;

            //Event subs
            Game.OnWndProc += HKC_WndProc;
            Drawing.OnDraw += HKC_Draw;
            Game.OnUpdate += HKC_Update;
        }

        private void HKC_Draw(EventArgs args)
        {
            if (_gameLoad)
            {
                Drawing.DrawRect(BoxPosition, new Vector2(15, 15), BoxColor, false);
                Drawing.DrawText(DisplayText + " [" + KeyToChar(Key) + "]", new Vector2(BoxPosition.X + 20, BoxPosition.Y), BoxColor, FontFlags.AntiAlias & FontFlags.DropShadow);

                if (_choosingKey)
                {
                    Drawing.DrawText("Press a new key for this bind!", new Vector2(Drawing.Width / 2, Drawing.Height / 2), Color.Orange, FontFlags.AntiAlias & FontFlags.DropShadow);
                }
            }
        }

        private void HKC_Update(EventArgs args)
        {
            if (Game.IsInGame && !Game.IsPaused && ObjectMgr.LocalHero != null)
            {
                _gameLoad = true;
            }
            else
            {
                Key = OrigKey;
            }

            if (_gameLoad)
            {
                if (Game.MouseScreenPosition.X >= BoxPosition.X && Game.MouseScreenPosition.X <= BoxPosition.X + 15 &&
                    Game.MouseScreenPosition.Y >= BoxPosition.Y && Game.MouseScreenPosition.Y <= BoxPosition.Y + 15)
                {
                    _isUnderCheckbox = true;
                }
                else
                {
                    _isUnderCheckbox = false;
                }
            }

            //Console.WriteLine("Key " + KeyToChar(Key) + " is: " + IsActive);
        }

        private void HKC_WndProc(WndEventArgs args)
        {
            if (_gameLoad)
            {
                if (args.Msg == (uint)Utils.WindowsMessages.WM_LBUTTONDOWN && !_choosingKey && _isUnderCheckbox && !Game.IsChatOpen)
                {
                    _choosingKey = true;
                }

                if (_choosingKey && !Game.IsChatOpen)
                {
                    if (args.Msg == (uint)Utils.WindowsMessages.WM_KEYDOWN)
                    {
                        Key = (uint)args.WParam;
                        _choosingKey = false;
                    }
                }
            }



            if (args.Msg == (uint)Utils.WindowsMessages.WM_KEYDOWN && !_choosingKey && SetMode == KeyMode.HOLD && !Game.IsChatOpen && args.WParam == Key)
            {
                _isActive = true;
            }
            else if (args.Msg == (uint)Utils.WindowsMessages.WM_KEYUP && !_choosingKey && SetMode == KeyMode.HOLD && !Game.IsChatOpen && args.WParam == Key)
            {
                _isActive = false;
            }



            if (args.Msg == (uint)Utils.WindowsMessages.WM_KEYDOWN && !_choosingKey && SetMode == KeyMode.TOGGLE && !Game.IsChatOpen && args.WParam == Key &&
                keyToggleT + 1000 < Environment.TickCount)
            {
                keyToggleT = Environment.TickCount;
                _isActive = !_isActive;
            }
        }

        private static string KeyToChar(uint key)
        {
            if (key >= 65 && key <= 90)
            {
                return ((char)key).ToString();
            }

            if (key == 32)
            {
                return "SPACE";
            }

            return key.ToString();
        }

        public bool IsActive { get { return _isActive; } }
    }
}
