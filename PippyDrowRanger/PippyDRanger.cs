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

namespace PippyDrowRanger
{
    class PippyDRanger
    {

        private static Hero meLulz;
        private const string heroName = "npc_dota_hero_drow_ranger";
        private static Hero _target;

        private static Ability theQ, theW, theE;

        private static Font pippyFont;

        private static bool gameLoad, isInCombo, isAABest, lastHitting;

        private static int _lastToggleTarget = 0, _lastToggleLastHit = 0;

        private static ParticleEffect rangesAA;
        private static ParticleEffect rangesW;

        public static void Init()
        {

            gameLoad = false;
            isInCombo = false;
            isAABest = false;
            lastHitting = false;
            rangesAA = null;
            rangesW = null;

            Orbwalking.Load();

            /*pippyFont = new Font(Drawing.Direct3DDevice9, 
                20, 
                0, 
                FontWeight.Bold, 
                0, 
                false, 
                FontCharacterSet.Default, 
                FontPrecision.Default, 
                FontQuality.Antialiased, 
                FontPitchAndFamily.Default,
                "Tahoma");
            */

            //Events
            Game.OnUpdate += DRangerUpdate;
            Game.OnWndProc += DRangerWnd;
            Drawing.OnDraw += DRangerDraw;

        }

        private static bool IsDRanger()
        {
            if (meLulz.Name.ToLowerInvariant() == heroName)
            {
                return true;
            }

            return false;
        }

        private static string GetTargetMode()
        {
            if (isAABest)
            {
                return "Best AA Target";
            }

            return "Closest to mouse";
        }

        private static void DRangerUpdate(EventArgs args)
        {
            if (Game.IsInGame && !Game.IsPaused && !Game.IsWatchingGame)
            {
                gameLoad = true;
                meLulz = ObjectMgr.LocalHero;
            }
            else
            {
                gameLoad = false;
                rangesAA = null;
                rangesW = null;
            }

            if (!gameLoad || !IsDRanger())
            {
                return;
            }

            theQ = meLulz.Spellbook.SpellQ;
            theW = meLulz.Spellbook.SpellW;
            theE = meLulz.Spellbook.SpellE;

            _target = isAABest ? TargetSelector.BestAutoAttackTarget(meLulz) : TargetSelector.ClosestToMouse(meLulz);

            Combo();

            //Console.WriteLine("Q Spell name is: " + theQ.Name);
        }

        private static void DRangerDraw(EventArgs args)
        {
            if (!gameLoad || !IsDRanger())
            {
                return;
            }

            var fixedWidth = Drawing.Width * 10 / 100;
            var fixedHeight = Drawing.Height * 35 / 100;

            Drawing.DrawText("Pippy Drow Ranger - Loaded", new Vector2(fixedWidth, fixedHeight), Color.LightBlue, 
                (FontFlags)((int)FontFlags.DropShadow + (int)FontFlags.AntiAlias));
            Drawing.DrawText("Targeting Mode: " + GetTargetMode(), new Vector2(fixedWidth, fixedHeight + 20), Color.LightBlue, 
                (FontFlags)((int)FontFlags.DropShadow + (int)FontFlags.AntiAlias));
            Drawing.DrawText("Lasthitting Mode: " + (lastHitting ? "Enabled" : "Disabled"), new Vector2(fixedWidth, fixedHeight + 40), Color.LightGreen,
                (FontFlags)((int)FontFlags.DropShadow + (int)FontFlags.AntiAlias));

            if (rangesAA == null)
            {
                rangesAA = meLulz.AddParticleEffect(@"particles\ui_mouseactions\range_display.vpcf");
            }
            rangesAA.SetControlPoint(1, new Vector3(meLulz.GetAttackRange() + meLulz.HullRadius, 0, 0));

            if (rangesW == null && theW.Level != 0)
            {
                rangesW = meLulz.AddParticleEffect(@"particles\ui_mouseactions\range_display.vpcf");
            }
            rangesW.SetControlPoint(1, new Vector3(theW.CastRange, 0, 0));
        }

        private static void DRangerWnd(WndEventArgs args)
        {
            if (gameLoad && IsDRanger())
            {
                if (args.Msg == (uint)Utils.WindowsMessages.WM_KEYDOWN && args.WParam == 32 && !Game.IsChatOpen)
                {
                    isInCombo = true;
                }
                else if (args.Msg == (uint)Utils.WindowsMessages.WM_KEYUP && args.WParam == 32 && !Game.IsChatOpen)
                {
                    isInCombo = false;
                }

                if (args.Msg == (uint)Utils.WindowsMessages.WM_KEYDOWN && args.WParam == 71 && !Game.IsChatOpen && _lastToggleTarget + 1000 < Environment.TickCount)
                {
                    _lastToggleTarget = Environment.TickCount;
                    isAABest = !isAABest;
                }

                if (args.Msg == (uint)Utils.WindowsMessages.WM_KEYDOWN && args.WParam == 84 && !Game.IsChatOpen && _lastToggleLastHit + 1000 < Environment.TickCount)
                {
                    _lastToggleLastHit = Environment.TickCount;
                    lastHitting = !lastHitting;
                }
            }
        }

        private static void Combo()
        {
            if (_target == null)
            {
                if (theQ.IsAutoCastEnabled)
                {
                    theQ.ToggleAutocastAbility();
                }
                return;
            }

            if (meLulz.Distance2D(_target) < meLulz.GetAttackRange() && theQ.CanBeCasted() && !theQ.IsAutoCastEnabled)
            {
                theQ.ToggleAutocastAbility();
            }

            if (theW.Level != 0 && meLulz.Distance2D(_target) < theW.CastRange && theW.CanBeCasted())
            {
                
            }
        }
    }
}
