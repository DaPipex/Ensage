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
using HotKeyChanger;

namespace PippyShadowFiend
{
    public class PipFiendOfShadow
    {

        private static Hero SF;
        private static Hero _target;

        private static ClassID SF_ClassID = ClassID.CDOTA_Unit_Hero_Nevermore;

        private static bool gameLoad;
        private static bool loadOnce;

        private static Ability theQ, theW, theE;

        private static HKC ComboKey;

        private static Vector2 MenuCoords;


        public static void Init()
        {
            gameLoad = false;
            loadOnce = false;

            //Events
            Game.OnUpdate += SFUpdate;
            Drawing.OnDraw += SFDrawing;
        }

        private static void SFUpdate(EventArgs args)
        {
            if (Game.IsInGame && !Game.IsPaused && !Game.IsWatchingGame)
            {
                gameLoad = true;
                SF = ObjectMgr.LocalHero;
            }
            else
            {
                SF = null;
                gameLoad = false;
                loadOnce = false;
            }

            if (gameLoad && SF != null)
            {
                if (SF.ClassID != SF_ClassID)
                {
                    return;
                }

                MenuCoords = new Vector2((Drawing.Width * 10 / 100) + 40, (Drawing.Height * 10 / 100) - 50);

                theQ = SF.Spellbook.SpellQ;
                theW = SF.Spellbook.SpellW;
                theE = SF.Spellbook.SpellE;

                _target = ObjectMgr.GetEntities<Hero>().Where(enemy => enemy.Team == SF.GetEnemyTeam() && enemy.IsAlive && enemy.IsVisible && !enemy.IsIllusion && !enemy.IsInvul() && SF.Distance2D(enemy) < GetFarthestRange())
                    .OrderBy(enemy => enemy.Health / enemy.MaximumHealth).DefaultIfEmpty(null).FirstOrDefault();

                if (!loadOnce)
                {
                    ComboKey = new HKC("SFcombo", "Combo Key", 32, HKC.KeyMode.HOLD, MenuCoords, Color.IndianRed);
                    Console.WriteLine("Pippy Shadow Fiend - Loaded!");

                    loadOnce = true;
                }

                if (ComboKey.IsActive && !Game.IsChatOpen)
                {
                    Combo();
                }
            }
        }

        private static void Combo()
        {
            if (_target == null && Utils.SleepCheck("moveDelay"))
            {
                SF.Move(Game.MousePosition);
                Utils.Sleep(80, "moveDelay");
                return;
            }

            if (SF.Distance2D(_target) <= GetFarthestRange() && !SF.IsAttacking())
            {
                SF.Attack(_target);
            }

            var targetPredPos = Prediction.PredictedXYZ(_target, 670);

            var meInFrontE = Prediction.InFront(SF, 700);
            var meInFrontW = Prediction.InFront(SF, 450);
            var meInFrontQ = Prediction.InFront(SF, 200);

            if (targetPredPos.Distance2D(meInFrontQ) < 250 && theQ.CanBeCasted() && !SF.IsAttacking() && !SF.IsChanneling())
            {
                theQ.UseAbility();
            }
            else if (targetPredPos.Distance2D(meInFrontW) < 250 && theW.CanBeCasted() && !SF.IsAttacking() && !SF.IsChanneling())
            {
                theW.UseAbility();
            }
            else if (targetPredPos.Distance2D(meInFrontE) < 250 && theE.CanBeCasted() && !SF.IsAttacking() && !SF.IsChanneling())
            {
                theE.UseAbility();
            }
        }

        private static void SFDrawing(EventArgs args)
        {
            if (gameLoad)
            {
                /*
                if (theQ != null && theQ.Cooldown <= 0 && theQ.ManaCost <= SF.Mana)
                {
                    PippyDrawCircle(SF, 200, 69, Color.Red);
                }
                if (theW != null && theW.Cooldown <= 0 && theW.ManaCost <= SF.Mana)
                {
                    PippyDrawCircle(SF, 450, 69, Color.Violet);
                }
                if (theE != null && theE.Cooldown <= 0 && theE.ManaCost <= SF.Mana)
                {
                    PippyDrawCircle(SF, 700, 69, Color.Blue);
                }
                
                Drawing.DrawLine(Drawing.WorldToScreen(SF.Position), Drawing.WorldToScreen(Prediction.InFront(SF, 200)), Color.Red);
                Drawing.DrawLine(Drawing.WorldToScreen(SF.Position), Drawing.WorldToScreen(Prediction.InFront(SF, 450)), Color.Violet);
                Drawing.DrawLine(Drawing.WorldToScreen(SF.Position), Drawing.WorldToScreen(Prediction.InFront(SF, 700)), Color.Blue);
                */
            }
        }

        private static float GetFarthestRange()
        {
            if (theE.CanBeCasted())
            {
                return 700f;
            }

            return SF.GetAttackRange();
        }

        private static void PippyDrawCircle(double x, double y, double z, double radius = 550f, float width = 1, Color? color = null)
        {
            var Position = Drawing.WorldToScreen(new Vector3((float)(x - radius), (float)y, (float)(z + radius)));
            var Radius = radius * 0.70;
            var Width = width; //DrawLine doesnt use this yet D:
            var ColorC = color != null ? color : Color.White;

            var Fid = Math.PI * 2 / 40;

            var points = new List<Vector2>();

            var startPoint = Drawing.WorldToScreen(new Vector3((float)(x + Radius * Math.Cos(0)), (float)y, (float)(z - radius * Math.Sin(0))));

            for (var theta = Fid; theta < Math.PI * 2 + Fid / 2; theta += Fid)
            {
                var endPoint = Drawing.WorldToScreen(new Vector3((float)(x + Radius * Math.Cos(theta)), (float)y, (float)(z - radius * Math.Sin(theta))));
                Drawing.DrawLine(startPoint, endPoint, (Color)ColorC);
                startPoint = endPoint;
            }
        }

        private static void PippyDrawCircle(Unit unit, double radius = 550f, float width = 1, Color? color = null)
        {
            PippyDrawCircle(unit.Position.X, unit.Position.Y, unit.Position.Z, radius, width, color);
        }
    }
}
