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

        private static Font pippyFont; //Temporary Solution, I like Drawing.DrawText better.

        private static bool lastHittingHold;
        private static bool lastHittingToggle;

        private static int _lastToggleT;

        static void Main(string[] args)
        {
            gameLoad = false;
            lastHittingHold = false;
            lastHittingToggle = false;
            _lastToggleT = 0;

            pippyFont = new Font(Drawing.Direct3DDevice9, 20, 0, FontWeight.Bold, 0, false, FontCharacterSet.Default, FontPrecision.Default, FontQuality.Antialiased, FontPitchAndFamily.Default,
                "Tahoma");

            //Events
            Game.OnUpdate += LHUpdate;
            Drawing.OnEndScene += LHDrawing;
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


                if (minion != null && minion.Health < GetPhysDamageOnUnit(minion))
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
        }

        private static float PredictedDamage(Unit unit)
        {
            var myTimeToArrive = ProjSpeedTicks(meLulz, unit) + Environment.TickCount + (Game.Ping / 1000) + UnitDatabase.GetAttackBackswing(meLulz);

            var myDmgOnMinion = GetPhysDamageOnUnit(unit);

            return 15f;

            //Not yet used
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
                float fixedWidth = /*Drawing.Width * 5 / 100;*/ 15f;
                float fixedHeight = /*Drawing.Height * 10 / 100;*/ 120f;

                //Drawing.DrawText("Last Hitting is: " + (Game.IsKeyDown(84) ? "enabled" : "disabled"), new Vector2(fixedWidth, fixedHeight), new Vector2(18), Color.LightGreen, FontFlags.DropShadow);
                pippyFont.DrawText(null, "Last hitting is: " + ((lastHittingHold || lastHittingToggle) ? "enabled" : "disabled"), (int)fixedWidth, (int)fixedHeight, Color.LightGreen);
                pippyFont.DrawText(null, "My hero's projectile speed is: " + ProjSpeed(meLulz).ToString(), (int)fixedWidth, (int)fixedHeight + 20, Color.DarkGreen);
                pippyFont.DrawText(null, "My hero's name is: " + meLulz.Name.ToLowerInvariant(), (int)fixedWidth, (int)fixedHeight + 40, Color.OrangeRed);
            }
        }

        public static void PippyDrawCircle(float x, float y, float z, float radius, Color color)
        {
            var heroPosGame = new Vector3(x, y, z);
            //Draw circle with SharpDx once I get Camera Position Values
        }

        private static void LHProcSpell(Unit sender, Ability ability)
        {
            if (sender is Creep)
            {
                //Do proc stuff here when it gets implemented
            }
        }

        private static float ProjSpeed(Hero hero)
        {
            var projSpeed = 0f;

            switch (hero.Name.ToLowerInvariant())
            {
                case "npc_dota_hero_ancientapparition":
                    projSpeed = 1250;
                    break;
                case "npc_dota_hero_bane":
                case "npc_dota_hero_batrider":
                    projSpeed = 900;
                    break;
                case "npc_dota_hero_chen":
                    projSpeed = 1100;
                    break;
                case "npc_dota_hero_clinkz":
                case "npc_dota_hero_crystalmaiden":
                    projSpeed = 900;
                    break;
                case "npc_dota_hero_dazzle":
                    projSpeed = 1200;
                    break;
                case "npc_dota_hero_deathprophet":
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
                case "npc_dota_hero_keeperofthelight":
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
                case "npc_dota_hero_lonedruid":
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
                case "npc_dota_hero_naturesprophet":
                    projSpeed = 1125;
                    break;
                case "npc_dota_hero_necrophos":
                    projSpeed = 900;
                    break;
                case "npc_dota_hero_oracle":
                case "npc_dota_hero_outworlddevourer":
                    projSpeed = 900;
                    break;
                case "npc_dota_hero_phoenix":
                    projSpeed = 1100;
                    break;
                case "npc_dota_hero_puck":
                case "npc_dota_hero_pugna":
                    projSpeed = 900;
                    break;
                case "npc_dota_hero_queenofpain":
                    projSpeed = 1500;
                    break;
                case "npc_dota_hero_razor":
                    projSpeed = 2000;
                    break;
                case "npc_dota_hero_rubick":
                    projSpeed = 1125;
                    break;
                case "npc_dota_hero_shadowdemon":
                    projSpeed = 900;
                    break;
                case "npc_dota_hero_shadowfiend":
                    projSpeed = 1200;
                    break;
                case "npc_dota_hero_shadowshaman":
                    projSpeed = 900;
                    break;
                case "npc_dota_hero_silencer":
                case "npc_dota_hero_skywrathmage":
                    projSpeed = 1000;
                    break;
                case "npc_dota_hero_sniper":
                    projSpeed = 3000;
                    break;
                case "npc_dota_hero_stormspirit":
                    projSpeed = 1100;
                    break;
                case "npc_dota_hero_techies":
                case "npc_dota_hero_templarassassin":
                case "npc_dota_hero_tinker":
                    projSpeed = 900;
                    break;
                case "npc_dota_hero_trollwarlord":
                    projSpeed = 1200;
                    break;
                case "npc_dota_hero_vengefulspirit":
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
                case "npc_dota_hero_windranger":
                    projSpeed = 1200;
                    break;
                case "npc_dota_hero_winterwyvern":
                    projSpeed = 700;
                    break;
                case "npc_dota_hero_witchdoctor":
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
