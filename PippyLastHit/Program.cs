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

        private static bool lastHittingHold;
        private static bool lastHittingToggle;

        private static int _lastToggleT;

        private static float testValue;


        static void Main(string[] args)
        {
            gameLoad = false;
            lastHittingHold = false;
            lastHittingToggle = false;
            _lastToggleT = 0;
            testValue = 0f;

            //Events
            Game.OnUpdate += LHUpdate;
            Drawing.OnDraw += LHDrawing;
            Game.OnWndProc += LHWndProc;
            //Game.OnProcessSpell += LHProcSpell;
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

            if (gameLoad && (lastHittingHold || lastHittingToggle))
            {
                LastHit();
            }

        }

        private static float GetPhysDamageOnUnit(Unit unit)
        {
            var PhysDamage = meLulz.MinimumDamage + meLulz.BonusDamage;

            var _damageMP = 1 - 0.06 * unit.Armor / (1 + 0.06 * Math.Abs(unit.Armor));

            var realDamage = PhysDamage * _damageMP;

            return (float)realDamage;
        }

        private static float GetPhysDamageOnUnit(Unit source, Unit target)
        {
            var PhysDamage = source.MinimumDamage + source.BonusDamage;

            var _damageMP = 1 - 0.06 * target.Armor / (1 + 0.06 * Math.Abs(target.Armor));

            var realDamage = PhysDamage * _damageMP;

            return (float)realDamage;
        }

        private static void LHWndProc(WndEventArgs args)
        {
            if (gameLoad)
            {
                if (args.Msg == (ulong)Utils.WindowsMessages.WM_KEYDOWN && args.WParam == 65 && !Game.IsChatOpen)
                {
                    lastHittingHold = true;
                }
                else if (args.Msg == (ulong)Utils.WindowsMessages.WM_KEYUP && args.WParam == 65 && !Game.IsChatOpen)
                {
                    lastHittingHold = false;
                }



                if (args.Msg == (ulong)Utils.WindowsMessages.WM_KEYDOWN && args.WParam == 84 && !Game.IsChatOpen && _lastToggleT + 1000 < Environment.TickCount)
                {
                    _lastToggleT = Environment.TickCount;

                    lastHittingToggle = !lastHittingToggle;
                }
            }

        }

        private static void LastHit()
        {
            var minion = ObjectMgr.GetEntities<Creep>()
                .Where(creep => creep.IsAlive && meLulz.Distance2D(creep) <= meLulz.GetAttackRange())
                .OrderBy(creep => creep.Health).DefaultIfEmpty(null).FirstOrDefault();

            if (minion != null)
            {
                var timeToCheck = UnitDatabase.GetAttackBackswing(meLulz) * 1000 + Game.Ping + meLulz.GetTurnTime(minion) * 1000 + Math.Max(0, ProjSpeedTicks(meLulz, minion)) * 1000;

                testValue = (float)timeToCheck;

                //var predictedHealth = PredictedDamage(minion, (float)timeToCheck);

                if (minion.Health < GetPhysDamageOnUnit(minion))
                {
                    if (meLulz.CanAttack())
                    {
                        meLulz.Attack(minion);
                    }
                }
                else
                {
                    if (lastHittingHold)
                    {
                        meLulz.Move(Game.MousePosition);
                    }
                }
            }
            else
            {
                if (lastHittingHold)
                {
                    meLulz.Move(Game.MousePosition);
                }
            }
        }

        private static float PredictedDamage(Creep creep, float timeToCheck = 1500f)
        {

            
            var allyMinionProjs = ObjectMgr.Projectiles.ToList().FindAll(proj => proj != null && proj.Source is Creep && proj.Target is Creep &&
            proj.Source.Team == meLulz.Team);

            var enemyMinionProjs = ObjectMgr.Projectiles.ToList().FindAll(proj => proj != null && proj.Source is Creep && proj.Target is Creep &&
            proj.Source.Team != meLulz.Team);

            

            if (creep.Team != meLulz.Team) //Enemy Creep
            {
                var TotalMinionDMG = 0f;
                var maxTimeCheck = Environment.TickCount + timeToCheck;

                if (allyMinionProjs.Any())
                {
                    foreach (var allyProj in allyMinionProjs)
                    {
                        var MinionDMG = 0d;

                        if (allyProj.Target == creep)
                        {
                            var projArrivalTime = Environment.TickCount + (allyProj.Position.Distance2D(allyProj.Target.Position) / MinionProjSpeed(allyProj.Source as Creep));
                            if (projArrivalTime < maxTimeCheck)
                            {
                                MinionDMG = GetPhysDamageOnUnit((Unit)allyProj.Source, (Unit)allyProj.Target);
                            }
                        }

                        TotalMinionDMG += (float)MinionDMG;
                    }
                }

                return creep.Health - TotalMinionDMG;
            }

            if (creep.Team == meLulz.Team) //Ally Team
            {
                var TotalMinionDMG = 0f;
                var maxTimeCheck = Environment.TickCount + timeToCheck;

                if (enemyMinionProjs.Any())
                {
                    foreach (var enemyProj in enemyMinionProjs)
                    {
                        var MinionDMG = 0d;

                        if (enemyProj.Target == creep)
                        {
                            var projArrivalTime = Environment.TickCount + (enemyProj.Position.Distance2D(enemyProj.Target.Position) / MinionProjSpeed(enemyProj.Source as Creep));
                            if (projArrivalTime < maxTimeCheck)
                            {
                                MinionDMG = GetPhysDamageOnUnit((Unit)enemyProj.Source, (Unit)enemyProj.Target);
                            }
                        }

                        TotalMinionDMG += (float)MinionDMG;
                    }
                }

                return creep.Health - TotalMinionDMG;
            }

            return 0f;
        }




        private static float ProjSpeedTicks(Hero hero, Unit target)
        {
            var _distance = hero.Distance2D(target);
            var _speed = UnitDatabase.GetByName(meLulz.Name).ProjectileSpeed;

            return _distance / _speed;

            //Not yet used
        }

        private static void LHDrawing(EventArgs args)
        {
            if (gameLoad)
            {
                float fixedWidth = Drawing.Width * 5 / 100;
                float fixedHeight = Drawing.Height * 10 / 100;

                Drawing.DrawText("Last hitting is: " + ((lastHittingHold || lastHittingToggle) ? "enabled" : "disabled"), new Vector2(fixedWidth, fixedHeight), Color.LightGreen,
                    FontFlags.AntiAlias & FontFlags.DropShadow);
                Drawing.DrawText("My hero's projectile speed is: " + UnitDatabase.GetByName(meLulz.Name).ProjectileSpeed.ToString(), new Vector2(fixedWidth, fixedHeight + 20), Color.LightGreen,
                    FontFlags.AntiAlias & FontFlags.DropShadow);
                Drawing.DrawText("My hero's name is: " + meLulz.Name.ToLowerInvariant(), new Vector2(fixedWidth, fixedHeight + 40), Color.LightGreen,
                    FontFlags.AntiAlias & FontFlags.DropShadow);
                Drawing.DrawText("My ping is: " + Game.Ping, new Vector2(fixedWidth, fixedHeight + 60), Color.LightGreen, FontFlags.AntiAlias & FontFlags.DropShadow);
                Drawing.DrawText("Time to arrive: " + testValue, new Vector2(fixedWidth, fixedHeight + 80), Color.LightGreen, FontFlags.AntiAlias & FontFlags.DropShadow);
            }
        }

        /*private static void PippyDrawCircle(double x, double y, double z, double radius = 550f, double width = 1, Color? color = null)
        {
            var CColor = (color == null) ? Color.White : color;
            var heroPosGame = new Vector3((float)x, (float)y, (float)z);
            var screenPos = Drawing.WorldToScreen(heroPosGame);

            var quality = Math.Max(8, (180 / MathUtil.RadiansToDegrees((float)(Math.Asin((100 / (2 * radius)))))));
            quality = (float)(2 * Math.PI / quality);
            radius = radius * .92;

            List<Vector2> points = new List<Vector2>();

            for (double theta = 0; theta < 2 * Math.PI + quality; theta += quality)
            {
                var cPoint = Drawing.WorldToScreen(new Vector3((float)(x + radius * Math.Cos(theta)), (float)y, (float)(z - radius * Math.Sin(theta))));
                points[points.Count + 1] = (Point)new Vector2(cPoint.X, cPoint.Y);
            }

            pippyLine.Draw(points.ToArray(), (ColorBGRA)CColor);
        }*/

        private static float MinionProjSpeed(Creep creep)
        {
            if (creep.ClassID == ClassID.CDOTA_BaseNPC_Creep_Lane && creep.IsRanged)
            {
                return 900f;
            }
            else if (creep.ClassID == ClassID.CDOTA_BaseNPC_Creep_Siege)
            {
                return 1100f;
            }

            return float.PositiveInfinity;
        }
    }
}
