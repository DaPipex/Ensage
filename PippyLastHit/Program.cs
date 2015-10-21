﻿using System;
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

namespace PippyLastHit
{
    class Program
    {

        private static Hero meLulz;

        private static Creep tminion;

        private static bool gameLoad;

        //private static bool lastHittingHold;
        //private static bool lastHittingToggle;

        private static int _lastToggleT;

        private static float testValue;

        private static bool onLoad;

        private static HKC lastHitHold;
        private static HKC lastHitToggle;

        private static List<Projectile> allyMinionProjs = new List<Projectile>();
        private static List<Projectile> enemMinionProjs = new List<Projectile>();

        private static List<Creep> allyCreeps = new List<Creep>();
        private static List<Creep> enemyCreeps = new List<Creep>();

        private static float predDamage;


        static void Main(string[] args)
        {
            gameLoad = false;
            //lastHittingHold = false;
            //lastHittingToggle = false;
            _lastToggleT = 0;
            testValue = 0f;
            onLoad = false;

            //Events
            Game.OnUpdate += LHUpdate;
            Drawing.OnDraw += LHDrawing;
            //Game.OnWndProc += LHWndProc;
            //Game.OnProcessSpell += LHProcSpell;
        }

        private static void LHUpdate(EventArgs args)
        {
            if (Game.IsInGame && !Game.IsPaused)
            {
                gameLoad = true;
                meLulz = ObjectMgr.LocalHero;

                allyCreeps = ObjectMgr.GetEntities<Creep>().Where(creep => creep.Team == meLulz.Team).ToList();
                enemyCreeps = ObjectMgr.GetEntities<Creep>().Where(creep => creep.Team == meLulz.GetEnemyTeam()).ToList();
            }
            else
            {
                gameLoad = false;
                onLoad = false;

                lastHitHold = null;
                lastHitToggle = null;

                allyMinionProjs = null;
                enemMinionProjs = null;
            }

            if (gameLoad && !onLoad)
            {
                lastHitHold = new HKC("lasthithold", "Last Hit Hold", 65, HKC.KeyMode.HOLD, new Vector2((Drawing.Width * 5 / 100) + 20, (Drawing.Height * 10 / 100) - 50), Color.LightGreen);
                lastHitToggle = new HKC("lasthittoggle", "Last Hit Toggle", 84, HKC.KeyMode.TOGGLE, new Vector2((Drawing.Width * 5 / 100) + 20, (Drawing.Height * 10 / 100) - 30), Color.LightGreen);
                onLoad = true;
            }

            if (gameLoad && (lastHitHold.IsActive || lastHitToggle.IsActive))
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

        /*
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
        */

        private static void LastHit()
        {
            var minion = ObjectMgr.GetEntities<Creep>()
                .Where(creep => creep.IsAlive && meLulz.Distance2D(creep) <= meLulz.GetAttackRange())
                .OrderBy(creep => creep.Health).DefaultIfEmpty(null).FirstOrDefault();

            if (minion != null)
            {
                tminion = minion;
            }
            else
            {
                tminion = null;
            }

            if (minion != null)
            {
                var timeToCheck = UnitDatabase.GetAttackBackswing(meLulz) * 1000 + (Game.Ping > 125 ? (Game.Ping / 2 + 60) : Game.Ping) + meLulz.GetTurnTime(minion) * 1000 + Math.Max(0, meLulz.Distance2D(minion) - meLulz.HullRadius) / UnitDatabase.GetByName(meLulz.Name).ProjectileSpeed * 1000;

                testValue = (float)timeToCheck;

                var predictedHealth = PredictedDamage(minion, (float)timeToCheck);

                predDamage = predictedHealth;

                if (predictedHealth < GetPhysDamageOnUnit(minion))
                {
                    if (meLulz.CanAttack())
                    {
                        meLulz.Attack(minion);
                    }
                    else
                    {
                        meLulz.Move(Game.MousePosition);
                    }
                }
                else
                {
                    if (lastHitHold.IsActive)
                    {
                        meLulz.Move(Game.MousePosition);
                    }
                }
            }
            else
            {
                if (lastHitHold.IsActive)
                {
                    meLulz.Move(Game.MousePosition);
                }
            }
        }

        private static float PredictedDamage(Creep creep, float timeToCheck = 1500f)
        {

            //Ranged Creep Logic

            allyMinionProjs = ObjectMgr.Projectiles.ToList().FindAll(proj => proj != null && proj.Source is Creep && proj.Target is Creep &&
            proj.Source.Team == meLulz.Team);

            enemMinionProjs = ObjectMgr.Projectiles.ToList().FindAll(proj => proj != null && proj.Source is Creep && proj.Target is Creep &&
            proj.Source.Team != meLulz.Team);



            if (creep.Team != meLulz.Team) //Enemy Creep
            {
                var TotalMinionDMG = 0f;
                var maxTimeCheck = Environment.TickCount + timeToCheck;

                var rangedDamage = 0f;
                var meleeDamage = 0f;

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

                        rangedDamage += (float)MinionDMG;
                    }

                }

                if (allyCreeps.Any())
                {
                    foreach (var alCreep in allyCreeps)
                    {
                        var minionDMG = 0f;

                        if (alCreep.IsAlive && alCreep.IsMelee && alCreep.Distance2D(creep) <= alCreep.AttackRange && alCreep.IsAttacking())
                        {
                            if (MinionAAData.GetAttackBackswing(alCreep) < maxTimeCheck)
                            {
                                minionDMG = GetPhysDamageOnUnit(alCreep, creep);
                            }
                        }

                        meleeDamage += minionDMG;
                    }
                }

                TotalMinionDMG = rangedDamage + meleeDamage;

                return Math.Max(0, creep.Health - TotalMinionDMG);
            }

            if (creep.Team == meLulz.Team) //Ally Team
            {
                var TotalMinionDMG = 0f;
                var maxTimeCheck = Environment.TickCount + timeToCheck;

                var rangedDamage = 0f;
                var meleeDamage = 0f;

                if (enemMinionProjs.Any())
                {
                    foreach (var enemyProj in enemMinionProjs)
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

                        rangedDamage += (float)MinionDMG;
                    }
                }

                if (enemyCreeps.Any())
                {
                    foreach (var enCreep in enemyCreeps)
                    {
                        var minionDMG = 0f;

                        if (enCreep.IsAlive && enCreep.IsMelee && enCreep.Distance2D(creep) <= enCreep.AttackRange && enCreep.IsAttacking())
                        {
                            if (MinionAAData.GetAttackBackswing(enCreep) < maxTimeCheck)
                            {
                                minionDMG = GetPhysDamageOnUnit(enCreep, creep);
                            }
                        }

                        meleeDamage += minionDMG;
                    }
                }

                TotalMinionDMG = rangedDamage + meleeDamage;

                return Math.Max(0, creep.Health - TotalMinionDMG);
            }

            return 0f;
        }

        private static float PredictedDamageMelee(Creep creep, float timeToCheck = 1500f)
        {
            //Melee creep Logic

            var maxTimeCheck = Environment.TickCount + timeToCheck;

            if (enemyCreeps.Any())
            {
                var totalMinionDMG = 0f;

                if (creep.Team == meLulz.Team)
                {
                    foreach (var enCreep in enemyCreeps)
                    {
                        var minionDMG = 0f;

                        if (enCreep.IsAlive && enCreep.IsMelee && enCreep.IsAttacking())
                        {
                            if (enCreep.Distance2D(creep) <= enCreep.AttackRange)
                            {
                                if (MinionAAData.GetAttackBackswing(enCreep) < maxTimeCheck)
                                {
                                    minionDMG = GetPhysDamageOnUnit(enCreep, creep);
                                }
                            }
                        }

                        totalMinionDMG += minionDMG;
                    }
                }
            }

            if (allyCreeps.Any())
            {
                var totalMinionDMG = 0f;

                if (creep.Team == meLulz.GetEnemyTeam())
                {
                    foreach (var alCreep in allyCreeps)
                    {
                        var minionDMG = 0f;

                        if (alCreep.IsAlive && alCreep.IsMelee && alCreep.IsAttacking())
                        {
                            if (MinionAAData.GetAttackBackswing(alCreep) < maxTimeCheck)
                            {
                                minionDMG = GetPhysDamageOnUnit(alCreep, creep);
                            }
                        }

                        totalMinionDMG += minionDMG;
                    }
                }
            }

            return 0f;
        }


        private static float ProjSpeedTicks(Hero hero, Unit target)
        {
            var _distance = hero.Distance2D(target) - hero.HullRadius;
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

                Drawing.DrawText("Last hitting is: " + ((lastHitHold.IsActive || lastHitToggle.IsActive) ? "enabled" : "disabled"), new Vector2(fixedWidth, fixedHeight), Color.LightGreen,
                    FontFlags.AntiAlias & FontFlags.DropShadow);
                Drawing.DrawText("My hero's projectile speed is: " + UnitDatabase.GetByName(meLulz.Name).ProjectileSpeed.ToString(), new Vector2(fixedWidth, fixedHeight + 20), Color.LightGreen,
                    FontFlags.AntiAlias & FontFlags.DropShadow);
                Drawing.DrawText("My hero's name is: " + meLulz.Name.ToLowerInvariant(), new Vector2(fixedWidth, fixedHeight + 40), Color.LightGreen,
                    FontFlags.AntiAlias & FontFlags.DropShadow);
                Drawing.DrawText("My ping is: " + Game.Ping, new Vector2(fixedWidth, fixedHeight + 60), Color.LightGreen, FontFlags.AntiAlias & FontFlags.DropShadow);
                Drawing.DrawText("Time to arrive: " + testValue, new Vector2(fixedWidth, fixedHeight + 80), Color.LightGreen, FontFlags.AntiAlias & FontFlags.DropShadow);
                Drawing.DrawText("My backswing " + UnitDatabase.GetAttackBackswing(meLulz), new Vector2(fixedWidth, fixedHeight + 100), Color.LightGreen, FontFlags.AntiAlias & FontFlags.DropShadow);
                Drawing.DrawText("My Turntime to minion: " + (tminion != null ? meLulz.GetTurnTime(tminion).ToString() : 0.ToString()), new Vector2(fixedWidth, fixedHeight + 120), Color.LightGreen, FontFlags.AntiAlias & FontFlags.DropShadow);
                Drawing.DrawText("My ProjTick: " + (tminion != null ? ProjSpeedTicks(meLulz, tminion).ToString() : 0.ToString()), new Vector2(fixedWidth, fixedHeight + 140), Color.LightGreen, FontFlags.AntiAlias & FontFlags.DropShadow);
                Drawing.DrawText("Pred Damage: " + (tminion != null ? predDamage : 0), new Vector2(fixedWidth, fixedHeight + 160), Color.LightGreen, FontFlags.AntiAlias & FontFlags.DropShadow);

                foreach (Projectile allyProj in allyMinionProjs)
                {
                    Drawing.DrawText("Proj speed is: " + allyProj.Speed.ToString(), Drawing.WorldToScreen(allyProj.Position), Color.LightGreen, FontFlags.AntiAlias & FontFlags.DropShadow);
                }

                foreach (Projectile enemyProj in enemMinionProjs)
                {
                    Drawing.DrawText("Proj speed is: " + enemyProj.Speed.ToString(), Drawing.WorldToScreen(enemyProj.Position), Color.Orange, FontFlags.AntiAlias & FontFlags.DropShadow);
                }

                foreach (Creep creep in allyCreeps)
                {
                    if (creep.IsAlive && creep.IsVisible)
                    {
                        //Drawing.DrawText("This creep is: " + creep.ClassID + " - " + creep.IsRanged, Drawing.WorldToScreen(creep.Position), Color.LightGreen, FontFlags.AntiAlias & FontFlags.DropShadow);
                    }
                }

                foreach (Creep creep in enemyCreeps)
                {
                    if (creep.IsAlive && creep.IsVisible)
                    {
                        //Drawing.DrawText("This creep is: " + creep.ClassID + " - " + creep.IsRanged, Drawing.WorldToScreen(creep.Position), Color.Orange, FontFlags.AntiAlias & FontFlags.DropShadow);
                    }
                }

                if (tminion != null)
                {
                    Drawing.DrawText(string.Format("This minion HP: {0} - Pred Damage: {1}",
                        tminion.Health, predDamage), Drawing.WorldToScreen(tminion.Position), Color.LightPink,
                        FontFlags.AntiAlias & FontFlags.DropShadow);
                    Drawing.DrawText("My AA damage on creep: " + GetPhysDamageOnUnit(tminion), new Vector2(Drawing.WorldToScreen(tminion.Position).X, Drawing.WorldToScreen(tminion.Position).Y - 20),
                        Color.LightCoral, FontFlags.AntiAlias & FontFlags.DropShadow);
                }
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
