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

namespace PippyLastHit
{
    class Program
    {
        private static Hero me;

        private static bool gameLoad;
        private static bool onLoad;

        private static HKC LHHold;
        private static HKC LHToggle;
        private static HKC DNToggle;
        private static HKC moreTime;
        private static HKC lessTime;

        private static Vector2 LHHDrawPos;
        private static Vector2 LHTDrawPos;
        private static Vector2 DNTDrawPos;
        private static Vector2 LHStatusDrawPos;
        private static Vector2 DNStatusDrawPos;
        private static Vector2 moreTimeDrawPos;
        private static Vector2 lessTimeDrawPos;
        private static Vector2 customTimeDrawPos;

        private static int CustomWaitTime = 150;

        private const FontFlags HQ = FontFlags.AntiAlias & FontFlags.DropShadow;

        private static int lastMoveT;

        static void Main(string[] args)
        {
            gameLoad = false;
            onLoad = false;

            //Events
            Game.OnUpdate += LHUpdate;
            Game.OnIngameUpdate += LHIngameUpdate;
            Drawing.OnDraw += LHDraw;
        }

        private static void LHUpdate(EventArgs args)
        {
            if (!Game.IsInGame)  //Menu
            {
                LHHold = null;
                LHToggle = null;
                DNToggle = null;
                moreTime = null;
                lessTime = null;

                gameLoad = false;
                onLoad = false;
            }
        }

        private static void LHIngameUpdate(EventArgs args)
        {
            if (!onLoad)
            {
                LHHDrawPos = new Vector2(Drawing.Width * 5 / 100, Drawing.Height * 10 / 100);
                LHTDrawPos = new Vector2(Drawing.Width * 5 / 100, Drawing.Height * 12 / 100);
                DNTDrawPos = new Vector2(Drawing.Width * 5 / 100, Drawing.Height * 14 / 100);
                LHStatusDrawPos = new Vector2(Drawing.Width * 7 / 100, Drawing.Height * 18 / 100);
                DNStatusDrawPos = new Vector2(Drawing.Width * 7 / 100, Drawing.Height * 20 / 100);
                moreTimeDrawPos = new Vector2(Drawing.Width * 15 / 100, Drawing.Height * 10 / 100);
                lessTimeDrawPos = new Vector2(Drawing.Width * 15 / 100, Drawing.Height * 12 / 100);
                customTimeDrawPos = new Vector2(Drawing.Width * 17 / 100, Drawing.Height * 14 / 100);

                LHHold = new HKC("LHH", "Last Hit Hold", 65, HKC.KeyMode.HOLD, LHHDrawPos, Color.LightGreen);
                LHToggle = new HKC("LHT", "Last Hit Toggle", 84, HKC.KeyMode.TOGGLE, LHTDrawPos, Color.LightGreen);
                DNToggle = new HKC("DNT", "Deny Toggle", 75, HKC.KeyMode.TOGGLE, DNTDrawPos, Color.Cyan);
                moreTime = new HKC("moreTime", "Add delay", 107, HKC.KeyMode.HOLD, moreTimeDrawPos, Color.LightGreen);
                lessTime = new HKC("lessTime", "Remove delay", 109, HKC.KeyMode.HOLD, lessTimeDrawPos, Color.LightGreen);

                me = ObjectMgr.LocalHero;

                onLoad = true;
            }

            if (moreTime.IsActive && Utils.SleepCheck("moreTimeCheck"))
            {
                CustomWaitTime += 50;
                Utils.Sleep(250, "moreTimeCheck");
            }
            if (lessTime.IsActive && Utils.SleepCheck("lessTimeCheck"))
            {
                CustomWaitTime -= 50;
                Utils.Sleep(250, "lessTimeCheck");
            }

            if (CustomWaitTime < -200)
            {
                CustomWaitTime = -200;
            }
            if (CustomWaitTime > 500)
            {
                CustomWaitTime = 500;
            }

            if (LHHold.IsActive || LHToggle.IsActive)
            {
                LastHit(DNToggle.IsActive);
            }
        }

        private static void LHDraw(EventArgs args)
        {
            if (Game.IsInGame && !Game.IsPaused && !Game.IsWatchingGame && me != null)
            {
                Drawing.DrawText("Last hitting is: " + ((LHHold.IsActive || LHToggle.IsActive) ? "ENABLED" : "DISABLED"), LHStatusDrawPos,
                    ((LHHold.IsActive || LHToggle.IsActive) ? Color.LightGreen : Color.LightYellow), HQ);

                Drawing.DrawText("Denying " + ((DNToggle.IsActive) ? "IS" : "IS NOT") + " included in last hit", DNStatusDrawPos,
                    ((DNToggle.IsActive) ? Color.Cyan : Color.LightYellow), HQ);

                Drawing.DrawText("Current Last Hit/Deny Delay: " + CustomWaitTime, customTimeDrawPos, Color.OrangeRed, HQ);
            }
        }

        private static float GetPhysDamage(Unit source, Unit target)
        {
            var PhysDamage = source.MinimumDamage + source.BonusDamage;

            var _damageMP = 1 - 0.06 * target.Armor / (1 + 0.06 * Math.Abs(target.Armor));

            return (float)(PhysDamage * _damageMP);
        }

        private static float GetPhysDamage(Unit target)
        {
            return GetPhysDamage(me, target);
        }

        private static void LastHit(bool deny)
        {
            var PossibleMinion = (deny) ? (ObjectMgr.GetEntities<Creep>()
                .Where(creep => creep.IsAlive && creep.IsValid && me.Distance2D(creep) < me.GetAttackRange()).OrderBy(creep => creep.Health).DefaultIfEmpty(null).FirstOrDefault()) :
                (ObjectMgr.GetEntities<Creep>()
                .Where(creep => creep.IsAlive && creep.IsValid && creep.Team == me.GetEnemyTeam() && me.Distance2D(creep) < me.GetAttackRange()).OrderBy(creep => creep.Health).DefaultIfEmpty(null).FirstOrDefault());

            if (PossibleMinion != null)
            {
                var CheckTime = UnitDatabase.GetAttackPoint(me) * 1000 - CustomWaitTime + Game.Ping + me.GetTurnTime(PossibleMinion) * 1000 + Math.Max(0,
                    me.Distance2D(PossibleMinion) / UnitDatabase.GetProjectileSpeed(me) * 1000);

                var predHealth = PredictedHealth(PossibleMinion, (int)CheckTime);

                if (predHealth > 0 && predHealth <= GetPhysDamage(PossibleMinion))
                {
                    if (me.CanAttack())
                    {
                        me.Attack(PossibleMinion);
                    }
                }
                else
                {
                    if (LHHold.IsActive && lastMoveT + 80 < Environment.TickCount)
                    {
                        lastMoveT = Environment.TickCount;
                        me.Move(Game.MousePosition);
                    }
                }
            }
            else
            {
                if (LHHold.IsActive && lastMoveT + 80 < Environment.TickCount)
                {
                    lastMoveT = Environment.TickCount;
                    me.Move(Game.MousePosition);
                }
            }
        }

        private static float PredictedHealth(Unit unit, int time)
        {
            var allyMinions = ObjectMgr.GetEntities<Creep>().Where(creep => creep.IsAlive && creep.IsValid && creep.Team == me.Team && creep.IsMelee).ToList();
            var enemMinions = ObjectMgr.GetEntities<Creep>().Where(creep => creep.IsAlive && creep.IsValid && creep.Team == me.GetEnemyTeam() && creep.IsMelee).ToList();

            var allyMinionProjectiles = ObjectMgr.Projectiles.Where(proj => proj.Source is Creep && proj.Source.Team == me.Team).ToList();
            var enemyMinionProjectiles = ObjectMgr.Projectiles.Where(proj => proj.Source is Creep && proj.Source.Team == me.GetEnemyTeam()).ToList();

            var MaxTime = Environment.TickCount + time;

            if (unit.Team == me.GetEnemyTeam())
            {
                var rangedDamage = 0f;
                var meleeDamage = 0f;

                foreach (var proj in allyMinionProjectiles)
                {
                    var projDamage = 0f;

                    if (proj.Target == unit)
                    {
                        var arrivalTime = Environment.TickCount + proj.Distance2D(unit) / proj.Speed;

                        if (arrivalTime < MaxTime)
                        {
                            projDamage = GetPhysDamage(proj.Source as Creep, unit);
                        }
                    }

                    rangedDamage += projDamage;
                }

                foreach (var creep in allyMinions)
                {
                    var hitDamage = 0f;

                    if (creep.Distance2D(unit) <= creep.AttackRange && creep.NetworkActivity == NetworkActivity.AttackEvent)
                    {
                        var arrivalTime = Environment.TickCount + MinionAAData.GetAttackPoint(creep);

                        if (arrivalTime < MaxTime)
                        {
                            hitDamage = GetPhysDamage(creep, unit);
                        }
                    }

                    meleeDamage += hitDamage;
                }

                return Math.Max(0, unit.Health - (rangedDamage + meleeDamage));
            }

            if (unit.Team == me.Team)
            {
                var rangedDamage = 0f;
                var meleeDamage = 0f;

                foreach (var proj in enemyMinionProjectiles)
                {
                    var projDamage = 0f;

                    if (proj.Target == unit)
                    {
                        var arrivalTime = Environment.TickCount + proj.Distance2D(unit) / proj.Speed;

                        if (arrivalTime < MaxTime)
                        {
                            projDamage = GetPhysDamage(proj.Source as Creep, unit);
                        }
                    }

                    rangedDamage += projDamage;
                }

                foreach (var creep in enemMinions)
                {
                    var hitDamage = 0f;

                    if (creep.Distance2D(unit) <= creep.AttackRange && creep.NetworkActivity == NetworkActivity.AttackEvent)
                    {
                        var arrivalTime = Environment.TickCount + MinionAAData.GetAttackPoint(creep);

                        if (arrivalTime < MaxTime)
                        {
                            hitDamage = GetPhysDamage(creep, unit);
                        }
                    }

                    meleeDamage += hitDamage;
                }

                return Math.Max(0, unit.Health - (rangedDamage + meleeDamage));
            }

            return unit.Health;
        }
    }
}