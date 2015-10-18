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

        private static List<Projectile> creepShots = new List<Projectile>();

        static void Main(string[] args)
        {
            gameLoad = false;
            lastHittingHold = false;
            lastHittingToggle = false;
            _lastToggleT = 0;

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
                LastHit();
            }
            else
            {
                gameLoad = false;
            }

        }

        private static double GetPhysDamageOnUnit(Unit unit)
        {
            var PhysDamage = meLulz.MinimumDamage + meLulz.BonusDamage;

            double _damageMP = 1 - 0.06 * unit.Armor / (1 + 0.06 * Math.Abs(unit.Armor));

            double realDamage = PhysDamage * _damageMP;

            return realDamage;
        }

        private static double GetPhysDamageOnUnit(Unit source, Unit target)
        {
            var PhysDamage = source.MinimumDamage + source.BonusDamage;

            double _damageMP = 1 - 0.06 * target.Armor / (1 + 0.06 * Math.Abs(target.Armor));

            double realDamage = PhysDamage * _damageMP;

            return realDamage;
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
            if (lastHittingHold || lastHittingToggle)
            {
                var minion = ObjectMgr.GetEntities<Creep>()
                    .Where(creep => creep.IsAlive && meLulz.Distance2D(creep) <= meLulz.GetAttackRange())
                    .OrderBy(creep => creep.Health).DefaultIfEmpty(null).FirstOrDefault();

                if (minion != null)
                {
                    var timetoCheck = (UnitDatabase.GetAttackBackswing(meLulz) + meLulz.GetTurnTime(minion)) + meLulz.Distance2D(minion) / UnitDatabase.GetByName(meLulz.Name).ProjectileSpeed;

                    var predictedHealth = PredictedDamage(minion, (float)timetoCheck);

                    if (predictedHealth > 0 && predictedHealth < GetPhysDamageOnUnit(minion))
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
        }

        private static float PredictedDamage(Creep creep, float timeToCheck = 1500f)
        {
            var myTimeToArrive = ProjSpeedTicks(meLulz, creep) + Environment.TickCount + (Game.Ping / 1000) + UnitDatabase.GetAttackBackswing(meLulz);

            var myDmgOnMinion = GetPhysDamageOnUnit(creep);

            var minionProjectiles = ObjectMgr.Projectiles.Where(projectile => projectile.Source.GetType() == typeof(Creep) && projectile.Target.GetType() == typeof(Creep)).ToList();

            var enemyMinionProjs = minionProjectiles.Where(projectile => projectile.Source.Team != meLulz.Team).ToList();

            var allyMinionProjs = minionProjectiles.Where(projectile => projectile.Source.Team == meLulz.Team).ToList();

            if (creep.Team != meLulz.Team) //Enemy Creep
            {
                var TotalMinionDMG = 0d;
                var maxTimeCheck = Environment.TickCount + timeToCheck;

                foreach (var allyProj in allyMinionProjs)
                {
                    var MinionDMG = 0d;

                    if (allyProj.Target == creep)
                    {
                        var projArrivalTime = Environment.TickCount + (allyProj.Position.Distance2D(allyProj.Target.Position) / MinionProjSpeed((Creep)allyProj.Source));
                        if (projArrivalTime < maxTimeCheck)
                        {
                            MinionDMG = GetPhysDamageOnUnit((Unit)allyProj.Source, (Unit)allyProj.Target);
                        }
                    }

                    TotalMinionDMG += MinionDMG;
                }

                return creep.Health - (int)TotalMinionDMG;
            }

            if (creep.Team == meLulz.Team) //Ally Team
            {
                var TotalMinionDMG = 0d;
                var maxTimeCheck = Environment.TickCount + timeToCheck;

                foreach (var enemyProj in enemyMinionProjs)
                {
                    var MinionDMG = 0d;

                    if (enemyProj.Target == creep)
                    {
                        var projArrivalTime = Environment.TickCount + (enemyProj.Position.Distance2D(enemyProj.Target.Position) / MinionProjSpeed((Creep)enemyProj.Source));
                        if (projArrivalTime < maxTimeCheck)
                        {
                            MinionDMG = GetPhysDamageOnUnit((Unit)enemyProj.Source, (Unit)enemyProj.Target);
                        }
                    }

                    TotalMinionDMG += MinionDMG;
                }

                return creep.Health - (int)TotalMinionDMG;
            }

            return 0f;

        }



        private static float ProjSpeedTicks(Hero hero, Unit target)
        {
            var _distance = hero.Distance2D(target);
            var _speed = ProjSpeed(meLulz);

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

        private static void LHProcSpell(Unit sender, Ability ability)
        {
            if (sender is Creep)
            {
                //Do proc stuff here when it gets implemented
            }
        }

        private static float MinionProjSpeed(Creep creep)
        {
            if (creep.ClassID == ClassID.CDOTA_BaseNPC_Creep_Lane && creep.IsRanged)
            {
                return 900f;
            }
            else if (creep.ClassID == ClassID.CDOTA_BaseNPC_Creep_Siege)
            {
                return 1100;
            }

            return float.PositiveInfinity;
        }

        private static float ProjSpeed(Hero hero)
        {
            var projSpeed = 0f;

            switch (hero.Name.ToLowerInvariant())
            {
                case "npc_dota_hero_ancient_apparition":
                    projSpeed = 1250;
                    break;
                case "npc_dota_hero_bane":
                case "npc_dota_hero_bat_rider":
                    projSpeed = 900;
                    break;
                case "npc_dota_hero_chen":
                    projSpeed = 1100;
                    break;
                case "npc_dota_hero_clinkz":
                case "npc_dota_hero_crystal_maiden":
                    projSpeed = 900;
                    break;
                case "npc_dota_hero_dazzle":
                    projSpeed = 1200;
                    break;
                case "npc_dota_hero_death_prophet":
                    projSpeed = 1000;
                    break;
                case "npc_dota_hero_disruptor":
                    projSpeed = 1200;
                    break;
                case "npc_dota_hero_drow_ranger":
                    projSpeed = 1250;
                    break;
                case "npc_dota_hero_enchantress":
                case "npc_dota_hero_enigma":
                    projSpeed = 900;
                    break;
                case "npc_dota_hero_gyrocopter":
                    projSpeed = 3000;
                    break;
                case "npc_dota_hero_huskar":
                    projSpeed = 1400;
                    break;
                case "npc_dota_hero_invoker":
                    projSpeed = 900;
                    break;
                case "npc_dota_hero_io":
                    projSpeed = 1200;
                    break;
                case "npc_dota_hero_jakiro":
                    projSpeed = 1100;
                    break;
                case "npc_dota_hero_keeper_of_the_light":
                    projSpeed = 900;
                    break;
                case "npc_dota_hero_leshrac":
                case "npc_dota_hero_lich":
                    projSpeed = 900;
                    break;
                case "npc_dota_hero_lina":
                case "npc_dota_hero_lion":
                    projSpeed = 1000;
                    break;
                case "npc_dota_hero_lone_druid":
                case "npc_dota_hero_luna":
                    projSpeed = 900;
                    break;
                case "npc_dota_hero_medusa":
                    projSpeed = 1200;
                    break;
                case "npc_dota_hero_mirana":
                    projSpeed = 900;
                    break;
                case "npc_dota_hero_morphling":
                    projSpeed = 1300;
                    break;
                case "npc_dota_hero_natures_prophet":
                    projSpeed = 1125;
                    break;
                case "npc_dota_hero_necrophos":
                    projSpeed = 900;
                    break;
                case "npc_dota_hero_oracle":
                case "npc_dota_hero_outworld_devourer":
                    projSpeed = 900;
                    break;
                case "npc_dota_hero_phoenix":
                    projSpeed = 1100;
                    break;
                case "npc_dota_hero_puck":
                case "npc_dota_hero_pugna":
                    projSpeed = 900;
                    break;
                case "npc_dota_hero_queen_of_pain":
                    projSpeed = 1500;
                    break;
                case "npc_dota_hero_razor":
                    projSpeed = 2000;
                    break;
                case "npc_dota_hero_rubick":
                    projSpeed = 1125;
                    break;
                case "npc_dota_hero_shadow_demon":
                    projSpeed = 900;
                    break;
                case "npc_dota_hero_shadow_fiend":
                    projSpeed = 1200;
                    break;
                case "npc_dota_hero_shadow_shaman":
                    projSpeed = 900;
                    break;
                case "npc_dota_hero_silencer":
                case "npc_dota_hero_skywrath_mage":
                    projSpeed = 1000;
                    break;
                case "npc_dota_hero_sniper":
                    projSpeed = 3000;
                    break;
                case "npc_dota_hero_storm_spirit":
                    projSpeed = 1100;
                    break;
                case "npc_dota_hero_techies":
                case "npc_dota_hero_templar_assassin":
                case "npc_dota_hero_tinker":
                    projSpeed = 900;
                    break;
                case "npc_dota_hero_troll_warlord":
                    projSpeed = 1200;
                    break;
                case "npc_dota_hero_vengeful_spirit":
                    projSpeed = 1500;
                    break;
                case "npc_dota_hero_venomancer":
                    projSpeed = 900;
                    break;
                case "npc_dota_hero_viper":
                    projSpeed = 1200;
                    break;
                case "npc_dota_hero_visage":
                    projSpeed = 900;
                    break;
                case "npc_dota_hero_warlock":
                    projSpeed = 1200;
                    break;
                case "npc_dota_hero_weaver":
                    projSpeed = 900;
                    break;
                case "npc_dota_hero_wind_ranger":
                    projSpeed = 1200;
                    break;
                case "npc_dota_hero_winter_wyvern":
                    projSpeed = 700;
                    break;
                case "npc_dota_hero_witch_doctor":
                    projSpeed = 1200;
                    break;
                case "npc_dota_hero_zeus":
                    projSpeed = 1100;
                    break;
                default:
                    projSpeed = float.PositiveInfinity;
                    break;
            }

            return projSpeed;
        }
    }
}
