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

namespace PippyLastHit
{
    class Program
    {

        private static Hero meLulz;

        private static bool gameLoad;
        private static bool lastHitting;

        static void Main(string[] args)
        {
            gameLoad = false;
            lastHitting = false;

            //Events
            Game.OnUpdate += LHUpdate;
            Drawing.OnDraw += LHDrawing;
            Game.OnWndProc += LHWndProc;
        }

        private static void LHUpdate(EventArgs args)
        {
            if (Game.IsInGame && !Game.IsPaused)
            {
                gameLoad = true;
                meLulz = ObjectMgr.LocalHero;
            }
            else
            {
                gameLoad = false;
            }

        }

        private static double GetPhysDamageOnUnit(Unit unit)
        {
            var PhysDamage = meLulz.DamageAverage;

            double _damageMP = 1 - 0.06 * unit.Armor / (1 + 0.06 * Math.Abs(unit.Armor));

            double realDamage = PhysDamage * _damageMP;

            return realDamage;
        }

        private static void LHWndProc(WndEventArgs args)
        {
            if (args.Msg == (ulong)Utils.WindowsMessages.WM_KEYDOWN && args.WParam == 84 && !Game.IsChatOpen)
            {
                LastHit();
            }
        }

        private static void LastHit()
        {
            if (gameLoad)
            {
                var minion = ObjectMgr.GetEntities<Creep>()
                    .Where(creep => creep.IsAlive && meLulz.Distance2D(creep) <= meLulz.GetAttackRange())
                    .OrderBy(creep => creep.Health).DefaultIfEmpty(null).FirstOrDefault();


                if (minion != null && minion.Health < GetPhysDamageOnUnit(minion))
                {
                    if (meLulz.CanAttack())
                    {
                        meLulz.Attack(minion);
                    }
                }
                else
                {
                    meLulz.Move(Game.MousePosition);
                }
            }
        }

        private static void LHDrawing(EventArgs args)
        {
            if (gameLoad)
            {
                float fixedWidth = Drawing.Width * 5 / 100;
                float fixedHeight = Drawing.Height * 10 / 100;

                Drawing.DrawText("Last Hitting is: " + (Game.IsKeyDown(84) ? "enabled" : "disabled"), new Vector2(fixedWidth, fixedHeight), new Vector2(18), Color.LightGreen, FontFlags.DropShadow);
            }
        }
    }
}
