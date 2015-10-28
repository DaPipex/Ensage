using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.AbilityInfo;
using SharpDX;
using SharpDX.Direct3D9;

using HotKeyChanger;

namespace PippyInvoker
{
    class InvokingTits
    {

        public enum Combos
        {
            TornadoSnapMeteor = 1,
            SpiritSnapSun,
            TornadoEMPMeteorBlast,
            TornadoEMPSnap,
            TornadoBlast,
            EMPMeteorAlacrity
        }

        public enum SpellsInvoker
        {
            //Unknown,
            Cold_Snap,
            Ghost_Walk,
            Ice_Wall,
            EMP,
            Tornado,
            Alacrity,
            Sun_Strike,
            Forge_Spirit,
            Chaos_Meteor,
            Deafening_Blast
        }

        private static Dictionary<SpellsInvoker, float> CurrentCooldowns = new Dictionary<SpellsInvoker, float>();
        private static Dictionary<SpellsInvoker, bool> CurrentCanUse = new Dictionary<SpellsInvoker, bool>();

        private static Ability spellD;
        private static Ability spellF;

        private static Ability Quas;
        private static Ability Wex;
        private static Ability Exort;
        private static Ability Invoke;

        private static Hero me;

        private static Combos CurrentCombo;

        private const ClassID myClassID = ClassID.CDOTA_Unit_Hero_Invoker;
        private const FontFlags HQ = FontFlags.AntiAlias;

        private static HKC comboKey;
        private static HKC harassKey;
        private static HKC comboNext;
        private static HKC comboPrev;

        private static Vector2 comboKeyDrawPos;
        private static Vector2 harassKeyDrawPos;
        private static Vector2 comboNextDrawPos;
        private static Vector2 comboPrevDrawPos;
        private static Vector2 currentComboDrawPos;

        private static float ColdSnapLastT = 0;
        private static float GhostWalkLastT = 0;
        private static float IceWallLastT = 0;
        private static float EMPLastT = 0;
        private static float TornadoLastT = 0;
        private static float AlacrityLastT = 0;
        private static float SunStrikeLastT = 0;
        private static float ForgeSpiritLastT = 0;
        private static float ChaosMeteorLastT = 0;
        private static float DeafeningBlastLastT = 0;

        private static float[] TornadoUpTimes = { 0.8f, 1.1f, 1.4f, 1.7f, 2.0f, 2.3f, 2.6f, 2.9f };

        private static int ColdSnapLastCheck = 0;
        private static int GhostWalkLastCheck = 0;
        private static int IceWallLastCheck = 0;
        private static int EMPLastCheck = 0;
        private static int TornadoLastCheck = 0;
        private static int AlacrityLastCheck = 0;
        private static int SunStrikeLastCheck = 0;
        private static int ForgeSpiritLastCheck = 0;
        private static int ChaosMeteorLastCheck = 0;
        private static int DeafeningBlastLastCheck = 0;

        private static bool onLoad;
        private static bool RunDrawings;

        private static bool DrawQuasLearn;
        private static bool DrawWexLearn;
        private static bool DrawExortLearn;
        private static bool DrawInvokeLearn;

        private static bool TornadoInitiator;


        public static void Init()
        {
            //Events
            Game.OnUpdate += InvokerUpdate;
            Game.OnIngameUpdate += InvokerIngameUpdate;
            Drawing.OnDraw += InvokerDraw;

        }

        private static void InvokerUpdate(EventArgs args)
        {
            if (Game.GameState == GameState.NotInGame)
            {
                onLoad = false;
                RunDrawings = false;

                me = null;
                comboKey = null;
                harassKey = null;
                comboNext = null;
                comboPrev = null;

                CurrentCooldowns.Clear();
                CurrentCanUse.Clear();
            }
        }

        private static void InvokerIngameUpdate(EventArgs args)
        {

            me = ObjectMgr.LocalHero;
            if (me == null || me.ClassID != myClassID)
            {
                return;
            }

            if (!onLoad)
            {
                CurrentCombo = Combos.TornadoEMPMeteorBlast;

                comboKeyDrawPos = new Vector2(Drawing.Width * 90 / 100, Drawing.Height * 10 / 100);
                harassKeyDrawPos = new Vector2(Drawing.Width * 90 / 100, Drawing.Height * 12 / 100);
                comboNextDrawPos = new Vector2(Drawing.Width * 90 / 100, Drawing.Height * 14 / 100);
                comboPrevDrawPos = new Vector2(Drawing.Width * 90 / 100, Drawing.Height * 16 / 100);
                currentComboDrawPos = new Vector2(Drawing.Width * 87 / 100, Drawing.Height * 19 / 100);

                comboKey = new HKC("combo", "Combo", 32, HKC.KeyMode.HOLD, comboKeyDrawPos, Color.LightBlue);
                harassKey = new HKC("harass", "Harass", 67, HKC.KeyMode.HOLD, harassKeyDrawPos, Color.LightBlue);
                comboNext = new HKC("nextCombo", "Next Combo", 105, HKC.KeyMode.HOLD, comboNextDrawPos, Color.Pink);
                comboPrev = new HKC("prevCombo", "Previous Combo", 103, HKC.KeyMode.HOLD, comboPrevDrawPos, Color.Pink);

                CurrentCooldowns.Add(SpellsInvoker.Cold_Snap, 0);
                CurrentCooldowns.Add(SpellsInvoker.Ghost_Walk, 0);
                CurrentCooldowns.Add(SpellsInvoker.Ice_Wall, 0);
                CurrentCooldowns.Add(SpellsInvoker.EMP, 0);
                CurrentCooldowns.Add(SpellsInvoker.Tornado, 0);
                CurrentCooldowns.Add(SpellsInvoker.Alacrity, 0);
                CurrentCooldowns.Add(SpellsInvoker.Sun_Strike, 0);
                CurrentCooldowns.Add(SpellsInvoker.Forge_Spirit, 0);
                CurrentCooldowns.Add(SpellsInvoker.Chaos_Meteor, 0);
                CurrentCooldowns.Add(SpellsInvoker.Deafening_Blast, 0);

                CurrentCanUse.Add(SpellsInvoker.Cold_Snap, false);
                CurrentCanUse.Add(SpellsInvoker.Ghost_Walk, false);
                CurrentCanUse.Add(SpellsInvoker.Ice_Wall, false);
                CurrentCanUse.Add(SpellsInvoker.EMP, false);
                CurrentCanUse.Add(SpellsInvoker.Tornado, false);
                CurrentCanUse.Add(SpellsInvoker.Alacrity, false);
                CurrentCanUse.Add(SpellsInvoker.Sun_Strike, false);
                CurrentCanUse.Add(SpellsInvoker.Forge_Spirit, false);
                CurrentCanUse.Add(SpellsInvoker.Chaos_Meteor, false);
                CurrentCanUse.Add(SpellsInvoker.Deafening_Blast, false);

                Orbwalking.Load();

                onLoad = true;
            }

            Quas = me.Spellbook.SpellQ;
            Wex = me.Spellbook.SpellW;
            Exort = me.Spellbook.SpellE;
            Invoke = me.Spellbook.SpellR;
            spellD = me.Spellbook.SpellD;
            spellF = me.Spellbook.SpellF;

            /*
            if (spellD != null)
            {
                Console.WriteLine("Spell D: " + spellD.Name);
            }
            if (spellF != null)
            {
                Console.WriteLine("Spell F: " + spellF.Name);
            }
            */

            RunDrawings = true;

            ComboChecks();

            CooldownChecks();

            /*
            foreach (SpellsInvoker spell in Enum.GetValues(typeof(SpellsInvoker)))
            {
                Console.WriteLine("{0} is {1} and it's cooldown is: {2}", spell.ToString(), CanUse(GetSpellsCombination(spell)), CurrentCooldowns[spell]);
            }
            */

        }

        private static void ComboChecks()
        {

            Hero target = null;

            if (comboKey.IsActive)
            {
                switch (CurrentCombo)
                {
                    case Combos.TornadoSnapMeteor:
                        target = GetTargetMode(Combos.TornadoSnapMeteor);
                        if (target != null)
                        {
                            Orbwalking.Orbwalk(target);
                            CastCombo(Combos.TornadoSnapMeteor, target);
                        }
                        else
                        {
                            if (Utils.SleepCheck("moveCheck"))
                            {
                                me.Move(Game.MousePosition);
                                Utils.Sleep(100, "moveCheck");
                            }
                        }
                        break;
                    case Combos.SpiritSnapSun:
                        target = GetTargetMode(Combos.SpiritSnapSun);
                        if (target != null)
                        {
                            Orbwalking.Orbwalk(target);
                            CastCombo(Combos.SpiritSnapSun, target);
                        }
                        else
                        {
                            if (Utils.SleepCheck("moveCheck"))
                            {
                                me.Move(Game.MousePosition);
                                Utils.Sleep(100, "moveCheck");
                            }
                        }
                        break;
                    case Combos.TornadoEMPMeteorBlast:
                        target = GetTargetMode(Combos.TornadoEMPMeteorBlast);
                        if (target != null)
                        {
                            Orbwalking.Orbwalk(target);
                            CastCombo(Combos.TornadoEMPMeteorBlast, target);
                        }
                        else
                        {
                            if (Utils.SleepCheck("moveCheck"))
                            {
                                me.Move(Game.MousePosition);
                                Utils.Sleep(100, "moveCheck");
                            }
                        }
                        break;
                    case Combos.TornadoEMPSnap:
                        target = GetTargetMode(Combos.TornadoEMPSnap);
                        if (target != null)
                        {
                            Orbwalking.Orbwalk(target);
                            CastCombo(Combos.TornadoEMPSnap, target);
                        }
                        else
                        {
                            if (Utils.SleepCheck("moveCheck"))
                            {
                                me.Move(Game.MousePosition);
                                Utils.Sleep(100, "moveCheck");
                            }
                        }
                        break;
                    case Combos.TornadoBlast:
                        target = GetTargetMode(Combos.TornadoBlast);
                        if (target != null)
                        {
                            Orbwalking.Orbwalk(target);
                            CastCombo(Combos.TornadoBlast, target);
                        }
                        else
                        {
                            if (Utils.SleepCheck("moveCHeck"))
                            {
                                me.Move(Game.MousePosition);
                                Utils.Sleep(100, "moveCheck");
                            }
                        }
                        break;
                    case Combos.EMPMeteorAlacrity:
                        target = GetTargetMode(Combos.EMPMeteorAlacrity);
                        if (target != null)
                        {
                            Orbwalking.Orbwalk(target);
                            CastCombo(Combos.EMPMeteorAlacrity, target);
                        }
                        else
                        {
                            if (Utils.SleepCheck("moveCHeck"))
                            {
                                me.Move(Game.MousePosition);
                                Utils.Sleep(100, "moveCheck");
                            }
                        }
                        break;
                }
            }

            if (comboNext.IsActive && Utils.SleepCheck("comboNextCheck"))
            {
                CurrentCombo++;
                if (CurrentCombo == (Combos)7)
                {
                    CurrentCombo = (Combos)1;
                }
                Utils.Sleep(150, "comboNextCheck");
            }

            if (comboPrev.IsActive && Utils.SleepCheck("comboPrevCheck"))
            {
                CurrentCombo--;
                if (CurrentCombo == 0)
                {
                    CurrentCombo = (Combos)6;
                }
                Utils.Sleep(150, "comboPrevCheck");
            }
        }

        private static void CastCombo(Combos combo, Hero unit)
        {

            Ability[] SequenceOne = null;
            Ability[] SequenceTwo = null;
            Ability[] SequenceThree = null;
            Ability[] SequenceFour = null;
            //Ability[] SequenceFive; Not used yet

            switch (combo)
            {
                case Combos.TornadoSnapMeteor:
                    SequenceOne = GetSpellsCombination(SpellsInvoker.Tornado);
                    SequenceTwo = GetSpellsCombination(SpellsInvoker.Cold_Snap);
                    SequenceThree = GetSpellsCombination(SpellsInvoker.Chaos_Meteor);
                    break;
                case Combos.SpiritSnapSun:
                    SequenceOne = GetSpellsCombination(SpellsInvoker.Forge_Spirit);
                    SequenceTwo = GetSpellsCombination(SpellsInvoker.Cold_Snap);
                    SequenceThree = GetSpellsCombination(SpellsInvoker.Sun_Strike);
                    break;
                case Combos.TornadoEMPMeteorBlast:
                    SequenceOne = GetSpellsCombination(SpellsInvoker.Tornado);
                    SequenceTwo = GetSpellsCombination(SpellsInvoker.EMP);
                    SequenceThree = GetSpellsCombination(SpellsInvoker.Chaos_Meteor);
                    SequenceFour = GetSpellsCombination(SpellsInvoker.Deafening_Blast);
                    break;
                case Combos.TornadoEMPSnap:
                    SequenceOne = GetSpellsCombination(SpellsInvoker.Tornado);
                    SequenceTwo = GetSpellsCombination(SpellsInvoker.EMP);
                    SequenceThree = GetSpellsCombination(SpellsInvoker.Cold_Snap);
                    break;
                case Combos.TornadoBlast:
                    SequenceOne = GetSpellsCombination(SpellsInvoker.Tornado);
                    SequenceTwo = GetSpellsCombination(SpellsInvoker.Deafening_Blast);
                    break;
                case Combos.EMPMeteorAlacrity:
                    SequenceOne = GetSpellsCombination(SpellsInvoker.EMP);
                    SequenceTwo = GetSpellsCombination(SpellsInvoker.Chaos_Meteor);
                    SequenceThree = GetSpellsCombination(SpellsInvoker.Alacrity);
                    break;
            }

            #region LearnChecks

            if (SequenceOne != null)
            {
                foreach (var spell in SequenceOne)
                {
                    if (spell.AbilityState == AbilityState.NotLearned)
                    {
                        if (spell == Quas)
                        {
                            LearnSpellPlease(1);
                            return;
                        }
                        else if (spell == Wex)
                        {
                            LearnSpellPlease(2);
                            return;

                        }
                        else if (spell == Exort)
                        {
                            LearnSpellPlease(3);
                            return;
                        }
                    }
                }
            }

            if (SequenceTwo != null)
            {
                foreach (var spell in SequenceTwo)
                {
                    if (spell.AbilityState == AbilityState.NotLearned)
                    {
                        if (spell == Quas)
                        {
                            LearnSpellPlease(1);
                            return;
                        }
                        else if (spell == Wex)
                        {
                            LearnSpellPlease(2);
                            return;

                        }
                        else if (spell == Exort)
                        {
                            LearnSpellPlease(3);
                            return;
                        }
                    }
                }
            }

            if (SequenceThree != null)
            {
                foreach (var spell in SequenceThree)
                {
                    if (spell.AbilityState == AbilityState.NotLearned)
                    {
                        if (spell == Quas)
                        {
                            LearnSpellPlease(1);
                            return;
                        }
                        else if (spell == Wex)
                        {
                            LearnSpellPlease(2);
                            return;

                        }
                        else if (spell == Exort)
                        {
                            LearnSpellPlease(3);
                            return;
                        }
                    }
                }
            }

            if (SequenceFour != null)
            {
                foreach (var spell in SequenceFour)
                {
                    if (spell.AbilityState == AbilityState.NotLearned)
                    {
                        if (spell == Quas)
                        {
                            LearnSpellPlease(1);
                            return;
                        }
                        else if (spell == Wex)
                        {
                            LearnSpellPlease(2);
                            return;

                        }
                        else if (spell == Exort)
                        {
                            LearnSpellPlease(3);
                            return;
                        }
                    }
                }
            }

            if (Invoke.AbilityState == AbilityState.NotLearned)
            {
                LearnSpellPlease(4);
                return;
            }

            #endregion

            if (Invoke.CanBeCasted() && Utils.SleepCheck("InvokeCast"))
            {
                if (SequenceOne != null)
                {
                    if (!HasInvokerSpell(SequenceOne) && CanUse(SequenceOne))
                    {
                        if (GetEnumFromSequence(SequenceOne) == SpellsInvoker.Tornado)
                        {
                            TornadoInitiator = true;
                        }

                        foreach (var spell in SequenceOne)
                        {
                            spell.UseAbility();
                        }

                        Invoke.UseAbility();
                        Utils.Sleep(200, "InvokeCast");
                    }
                }

                if (SequenceTwo != null)
                {
                    if (!HasInvokerSpell(SequenceTwo) && CanUse(SequenceTwo))
                    {
                        foreach (var spell in SequenceTwo)
                        {
                            spell.UseAbility();
                        }

                        Invoke.UseAbility();
                        Utils.Sleep(200, "InvokeCast");
                    }
                }

                if (SequenceThree != null)
                {
                    if (!HasInvokerSpell(SequenceThree) && CanUse(SequenceThree))
                    {
                        foreach (var spell in SequenceThree)
                        {
                            spell.UseAbility();
                        }

                        Invoke.UseAbility();
                        Utils.Sleep(200, "InvokeCast");
                    }
                }

                if (SequenceFour != null)
                {
                    if (!HasInvokerSpell(SequenceFour) && CanUse(SequenceFour))
                    {
                        foreach (var spell in SequenceFour)
                        {
                            spell.UseAbility();
                        }

                        Invoke.UseAbility();
                        Utils.Sleep(200, "InvokeCast");
                    }
                }
            }

            var thisDelay = 100f;

            if (Utils.SleepCheck("createSpellCheck"))
            {
                if (HasInvokerSpell(SequenceOne) && GetInvokerAbility(SequenceOne).CanBeCasted())
                {
                    CastInvokerSpell(SequenceOne, unit);
                    Utils.Sleep(thisDelay, "createSpellCheck");
                }

                if (!TornadoInitiator)
                {
                    if (HasInvokerSpell(SequenceTwo) && GetInvokerAbility(SequenceTwo).CanBeCasted())
                    {
                        CastInvokerSpell(SequenceTwo, unit);
                        Utils.Sleep(thisDelay, "createSpellCheck");
                    }

                    if (HasInvokerSpell(SequenceThree) && GetInvokerAbility(SequenceThree).CanBeCasted())
                    {
                        CastInvokerSpell(SequenceThree, unit);
                        Utils.Sleep(thisDelay, "createSpellCheck");
                    }

                    if (HasInvokerSpell(SequenceFour) && GetInvokerAbility(SequenceFour).CanBeCasted())
                    {
                        CastInvokerSpell(SequenceFour, unit);
                        Utils.Sleep(thisDelay, "createSpellCheck");
                    }
                }
            }
        }

        private static void CastInvokerSpell(Ability[] sequence, Hero target)
        {
            var spellOne = sequence[0];
            var spellTwo = sequence[1];
            var spellThree = sequence[2];

            SpellsInvoker spellToCast;
            
            if (spellOne == Quas && spellTwo == Quas && spellThree == Quas)
            {
                spellToCast = SpellsInvoker.Cold_Snap;
            }

            else if (spellOne == Quas && spellTwo == Quas && spellThree == Wex)
            {
                spellToCast = SpellsInvoker.Ghost_Walk;
            }

            else if (spellOne == Quas && spellTwo == Quas && spellThree == Exort)
            {
                spellToCast = SpellsInvoker.Ice_Wall;
            }

            else if (spellOne == Wex && spellTwo == Wex && spellThree == Wex)
            {
                spellToCast = SpellsInvoker.EMP;
            }

            else if (spellOne == Wex && spellTwo == Wex && spellThree == Quas)
            {
                spellToCast = SpellsInvoker.Tornado;
            }

            else if (spellOne == Wex && spellTwo == Wex && spellThree == Exort)
            {
                spellToCast = SpellsInvoker.Alacrity;
            }

            else if (spellOne == Exort && spellTwo == Exort && spellThree == Exort)
            {
                spellToCast = SpellsInvoker.Sun_Strike;
            }

            else if (spellOne == Exort && spellTwo == Exort && spellThree == Quas)
            {
                spellToCast = SpellsInvoker.Forge_Spirit;
            }

            else if (spellOne == Exort && spellTwo == Exort && spellThree == Wex)
            {
                spellToCast = SpellsInvoker.Chaos_Meteor;
            }

            else if (spellOne == Quas && spellTwo == Wex && spellThree == Exort)
            {
                spellToCast = SpellsInvoker.Deafening_Blast;
            }
            else
            {
                spellToCast = SpellsInvoker.Alacrity;
            }

            if (Utils.SleepCheck("spellCastDelay"))
            {
                var delay = 70;

                switch (spellToCast)
                {

                    case SpellsInvoker.Cold_Snap:
                        me.FindSpell("invoker_cold_snap").UseAbility(target);
                        Utils.Sleep(delay, "spellCastDelay");
                        break;
                    case SpellsInvoker.Ghost_Walk:
                        me.FindSpell("invoker_ghost_walk").UseAbility();
                        Utils.Sleep(delay, "spellCastDelay");
                        break;
                    case SpellsInvoker.Ice_Wall:
                        if (Prediction.InFront(me, 200).Distance2D(target) < 105)
                        {
                            me.FindSpell("invoker_ice_wall").UseAbility();
                            Utils.Sleep(delay, "spellCastDelay");
                        }
                        break;
                    case SpellsInvoker.EMP:
                        me.FindSpell("invoker_emp").UseAbility(target.Position);
                        Utils.Sleep(delay, "spellCastDelay");
                        break;
                    case SpellsInvoker.Tornado:
                        if (me.FindSpell("invoker_tornado").CastSkillShot(target))
                        {
                            var HitTime = TornadoHitTime(target) + 200;
                            var UpTime = TornadoUpTime(Quas.Level);
                            Console.WriteLine(HitTime);
                            Console.WriteLine(UpTime);
                            Utils.Sleep(delay + HitTime + UpTime, "spellCastDelay");
                            TornadoInitiator = false;
                        }
                        break;
                    case SpellsInvoker.Alacrity:
                        me.FindSpell("invoker_alacrity").UseAbility(me);
                        Utils.Sleep(delay, "spellCastDelay");
                        break;
                    case SpellsInvoker.Sun_Strike:
                        me.FindSpell("invoker_sun_strike").UseAbility(target.Position);
                        Utils.Sleep(delay, "spellCastDelay");
                        break;
                    case SpellsInvoker.Forge_Spirit:
                        me.FindSpell("invoker_forge_spirit").UseAbility();
                        Utils.Sleep(delay, "spellCastDelay");
                        break;
                    case SpellsInvoker.Chaos_Meteor:
                        if (me.FindSpell("invoker_chaos_meteor").CastSkillShot(target))
                        {
                            Utils.Sleep(delay, "spellCastDelay");
                        }
                        break;
                    case SpellsInvoker.Deafening_Blast:
                        if (me.FindSpell("invoker_deafening_blast").CastSkillShot(target))
                        {
                            Utils.Sleep(delay, "spellCastDelay");
                        }
                        break;
                }
            }
        }

        private static int TornadoHitTime(Hero hero)
        {
            var TornadoSpeed = 1000;
            var DistanceToTarget = me.Distance2D(hero);

            Console.WriteLine(DistanceToTarget);

            return (int)((DistanceToTarget / TornadoSpeed) * 1000);
        }

        private static float TornadoUpTime(uint level)
        {
            return (int)(TornadoUpTimes[level - 1] * 1000);
        }

        private static void InvokerDraw(EventArgs args)
        {
            if (!RunDrawings)
            {
                return;
            }

            if (!comboKey.IsActive)
            {
                DrawQuasLearn = false;
                DrawWexLearn = false;
                DrawExortLearn = false;
                DrawInvokeLearn = false;
            }

            if (DrawQuasLearn)
            {
                Drawing.DrawText("Please learn Quas first", new Vector2(Drawing.Width / 2, Drawing.Height / 2), Color.LightBlue, HQ);
            }
            else if (DrawWexLearn)
            {
                Drawing.DrawText("Please learn Wex first", new Vector2(Drawing.Width / 2, Drawing.Height / 2), Color.HotPink, HQ);
            }
            else if (DrawExortLearn)
            {
                Drawing.DrawText("Please learn Exort first", new Vector2(Drawing.Width / 2, Drawing.Height / 2), Color.Orange, HQ);
            }
            else if (DrawInvokeLearn)
            {
                Drawing.DrawText("Please learn Invoke first", new Vector2(Drawing.Width / 2, Drawing.Height / 2), Color.White, HQ);
            }

            Drawing.DrawText("Current Combo: " + CurrentCombo, currentComboDrawPos, Color.White, HQ);
        }

        private static Ability[] GetSpellsCombination(SpellsInvoker spellToCreate)
        {
            switch (spellToCreate)
            {
                case SpellsInvoker.Cold_Snap:
                    return new Ability[] { me.Spellbook.SpellQ, me.Spellbook.SpellQ, me.Spellbook.SpellQ };
                case SpellsInvoker.Ghost_Walk:
                    return new Ability[] { me.Spellbook.SpellQ, me.Spellbook.SpellQ, me.Spellbook.SpellW };
                case SpellsInvoker.Ice_Wall:
                    return new Ability[] { me.Spellbook.SpellQ, me.Spellbook.SpellQ, me.Spellbook.SpellE };
                case SpellsInvoker.EMP:
                    return new Ability[] { me.Spellbook.SpellW, me.Spellbook.SpellW, me.Spellbook.SpellW };
                case SpellsInvoker.Tornado:
                    return new Ability[] { me.Spellbook.SpellW, me.Spellbook.SpellW, me.Spellbook.SpellQ };
                case SpellsInvoker.Alacrity:
                    return new Ability[] { me.Spellbook.SpellW, me.Spellbook.SpellW, me.Spellbook.SpellE };
                case SpellsInvoker.Sun_Strike:
                    return new Ability[] { me.Spellbook.SpellE, me.Spellbook.SpellE, me.Spellbook.SpellE };
                case SpellsInvoker.Forge_Spirit:
                    return new Ability[] { me.Spellbook.SpellE, me.Spellbook.SpellE, me.Spellbook.SpellQ };
                case SpellsInvoker.Chaos_Meteor:
                    return new Ability[] { me.Spellbook.SpellE, me.Spellbook.SpellE, me.Spellbook.SpellW };
                case SpellsInvoker.Deafening_Blast:
                    return new Ability[] { me.Spellbook.SpellQ, me.Spellbook.SpellW, me.Spellbook.SpellE };
                default:
                    return new Ability[0];
            }
        }

        private static bool HasInvokerSpell(Ability[] sequence)
        {
            var spellOne = sequence[0];
            var spellTwo = sequence[1];
            var spellThree = sequence[2];

            if (spellOne == Quas && spellTwo == Quas && spellThree == Quas)
            {
                return (spellD.Name == "invoker_cold_snap" || spellF.Name == "invoker_cold_snap");
            }

            else if (spellOne == Quas && spellTwo == Quas && spellThree == Wex)
            {
                return (spellD.Name == "invoker_ghost_walk" || spellF.Name == "invoker_ghost_walk");
            }

            else if (spellOne == Quas && spellTwo == Quas && spellThree == Exort)
            {
                return (spellD.Name == "invoker_ice_wall" || spellF.Name == "invoker_ice_wall");
            }

            else if (spellOne == Wex && spellTwo == Wex && spellThree == Wex)
            {
                return (spellD.Name == "invoker_emp" || spellF.Name == "invoker_emp");
            }

            else if (spellOne == Wex && spellTwo == Wex && spellThree == Quas)
            {
                return (spellD.Name == "invoker_tornado" || spellF.Name == "invoker_tornado");
            }

            else if (spellOne == Wex && spellTwo == Wex && spellThree == Exort)
            {
                return (spellD.Name == "invoker_alacrity" || spellF.Name == "invoker_alacrity");
            }

            else if (spellOne == Exort && spellTwo == Exort && spellThree == Exort)
            {
                return (spellD.Name == "invoker_sun_strike" || spellF.Name == "invoker_sun_strike");
            }

            else if (spellOne == Exort && spellTwo == Exort && spellThree == Quas)
            {
                return (spellD.Name == "invoker_forge_spirit" || spellF.Name == "invoker_forge_spirit");
            }

            else if (spellOne == Exort && spellTwo == Exort && spellThree == Wex)
            {
                return (spellD.Name == "invoker_chaos_meteor" || spellF.Name == "invoker_chaos_meteor");
            }

            else if (spellOne == Quas && spellTwo == Wex && spellThree == Exort)
            {
                return (spellD.Name == "invoker_deafening_blast" || spellF.Name == "invoker_deafening_blast");
            }

            return false;
        }

        private static Ability GetInvokerAbility(Ability[] sequence)
        {
            var spellOne = sequence[0];
            var spellTwo = sequence[1];
            var spellThree = sequence[2];

            Ability ability = null;

            if (spellOne == Quas && spellTwo == Quas && spellThree == Quas)
            {
                ability = me.FindSpell("invoker_cold_snap");
            }

            else if (spellOne == Quas && spellTwo == Quas && spellThree == Wex)
            {
                ability = me.FindSpell("invoker_ghost_walk");
            }

            else if (spellOne == Quas && spellTwo == Quas && spellThree == Exort)
            {
                ability = me.FindSpell("invoker_ice_wall");
            }

            else if (spellOne == Wex && spellTwo == Wex && spellThree == Wex)
            {
                ability = me.FindSpell("invoker_emp");
            }

            else if (spellOne == Wex && spellTwo == Wex && spellThree == Quas)
            {
                ability = me.FindSpell("invoker_tornado");
            }

            else if (spellOne == Wex && spellTwo == Wex && spellThree == Exort)
            {
                ability = me.FindSpell("invoker_alacrity");
            }

            else if (spellOne == Exort && spellTwo == Exort && spellThree == Exort)
            {
                ability = me.FindSpell("invoker_sun_strike");
            }

            else if (spellOne == Exort && spellTwo == Exort && spellThree == Quas)
            {
                ability = me.FindSpell("invoker_forge_spirit");
            }

            else if (spellOne == Exort && spellTwo == Exort && spellThree == Wex)
            {
                ability = me.FindSpell("invoker_chaos_meteor");
            }

            else if (spellOne == Quas && spellTwo == Wex && spellThree == Exort)
            {
                ability = me.FindSpell("invoker_deafening_blast");
            }

            return ability;
        }

        private static Hero GetMyCustomTarget(float range)
        {
            var customTarget = ObjectMgr.GetEntities<Hero>().Where(x => x.IsAlive && x.Team == me.GetEnemyTeam() && !x.IsIllusion && x.IsValid && x.IsVisible && x.Distance2D(me) < range)
                .OrderBy(x => x.Health / x.MaximumHealth).ThenBy(x => x.Distance2D(me)).DefaultIfEmpty(null).FirstOrDefault();

            return customTarget;
        }

        private static Hero GetTargetMode(Combos combo)
        {
            switch (combo)
            {
                case Combos.TornadoSnapMeteor:
                    return GetMyCustomTarget(1000);
                case Combos.SpiritSnapSun:
                    return GetMyCustomTarget(1000);
                case Combos.TornadoEMPMeteorBlast:
                    return GetMyCustomTarget(1000);
                case Combos.TornadoEMPSnap:
                    return GetMyCustomTarget(1000);
                case Combos.TornadoBlast:
                    return GetMyCustomTarget(1000);
                case Combos.EMPMeteorAlacrity:
                    return GetMyCustomTarget(1000);
                default:
                    return null;
            }
        }

        private static void LearnSpellPlease(int number)
        {
            switch (number)
            {
                case 1:
                    DrawQuasLearn = true;
                    break;
                case 2:
                    DrawWexLearn = true;
                    break;
                case 3:
                    DrawExortLearn = true;
                    break;
                case 4:
                    DrawInvokeLearn = true;
                    break;
            }
        }

        private static bool CanUse(Ability[] sequence)
        {
            var EnumVar = GetEnumFromSequence(sequence);

            return CurrentCanUse[EnumVar];
        }

        private static SpellsInvoker GetEnumFromSequence(Ability[] sequence)
        {

            var SpellOne = sequence[0];
            var SpellTwo = sequence[1];
            var SpellThree = sequence[2];

            if (SpellOne == GetSpellsCombination(SpellsInvoker.Cold_Snap)[0]
                && SpellTwo == GetSpellsCombination(SpellsInvoker.Cold_Snap)[1]
                && SpellThree == GetSpellsCombination(SpellsInvoker.Cold_Snap)[2])
            {
                return SpellsInvoker.Cold_Snap;
            }
            else if (SpellOne == GetSpellsCombination(SpellsInvoker.Ghost_Walk)[0]
                && SpellTwo == GetSpellsCombination(SpellsInvoker.Ghost_Walk)[1]
                && SpellThree == GetSpellsCombination(SpellsInvoker.Ghost_Walk)[2])
            {
                return SpellsInvoker.Ghost_Walk;
            }
            else if (SpellOne == GetSpellsCombination(SpellsInvoker.Ice_Wall)[0]
                && SpellTwo == GetSpellsCombination(SpellsInvoker.Ice_Wall)[1]
                && SpellThree == GetSpellsCombination(SpellsInvoker.Ice_Wall)[2])
            {
                return SpellsInvoker.Ice_Wall;
            }
            else if (SpellOne == GetSpellsCombination(SpellsInvoker.EMP)[0]
                && SpellTwo == GetSpellsCombination(SpellsInvoker.EMP)[1]
                && SpellThree == GetSpellsCombination(SpellsInvoker.EMP)[2])
            {
                return SpellsInvoker.EMP;
            }
            else if (SpellOne == GetSpellsCombination(SpellsInvoker.Tornado)[0]
                && SpellTwo == GetSpellsCombination(SpellsInvoker.Tornado)[1]
                && SpellThree == GetSpellsCombination(SpellsInvoker.Tornado)[2])
            {
                return SpellsInvoker.Tornado;
            }
            else if (SpellOne == GetSpellsCombination(SpellsInvoker.Alacrity)[0]
                && SpellTwo == GetSpellsCombination(SpellsInvoker.Alacrity)[1]
                && SpellThree == GetSpellsCombination(SpellsInvoker.Alacrity)[2])
                {
                return SpellsInvoker.Alacrity;
            }
            else if (SpellOne == GetSpellsCombination(SpellsInvoker.Sun_Strike)[0]
                && SpellTwo == GetSpellsCombination(SpellsInvoker.Sun_Strike)[1]
                && SpellThree == GetSpellsCombination(SpellsInvoker.Sun_Strike)[2])
                {
                return SpellsInvoker.Sun_Strike;
            }
            else if (SpellOne == GetSpellsCombination(SpellsInvoker.Forge_Spirit)[0]
                && SpellTwo == GetSpellsCombination(SpellsInvoker.Forge_Spirit)[1]
                && SpellThree == GetSpellsCombination(SpellsInvoker.Forge_Spirit)[2])
                {
                return SpellsInvoker.Forge_Spirit;
            }
            else if (SpellOne == GetSpellsCombination(SpellsInvoker.Chaos_Meteor)[0]
                && SpellTwo == GetSpellsCombination(SpellsInvoker.Chaos_Meteor)[1]
                && SpellThree == GetSpellsCombination(SpellsInvoker.Chaos_Meteor)[2])
                    {
                return SpellsInvoker.Chaos_Meteor;
            }
            else if (SpellOne == GetSpellsCombination(SpellsInvoker.Deafening_Blast)[0]
                && SpellTwo == GetSpellsCombination(SpellsInvoker.Deafening_Blast)[1]
                && SpellThree == GetSpellsCombination(SpellsInvoker.Deafening_Blast)[2])
                {
                return SpellsInvoker.Deafening_Blast;
            }
            else
            {
                return SpellsInvoker.Cold_Snap;
            }
        }

        private static void CooldownChecks()
        {
            foreach (SpellsInvoker spell in Enum.GetValues(typeof(SpellsInvoker)))
            {

                if (HasInvokerSpell(GetSpellsCombination(spell)))
                {
                    if (GetInvokerAbility(GetSpellsCombination(spell)).AbilityState == AbilityState.OnCooldown)
                    {
                        CurrentCooldowns[spell] = GetInvokerAbility(GetSpellsCombination(spell)).Cooldown;
                        CurrentCanUse[spell] = false;
                    }
                    else if (GetInvokerAbility(GetSpellsCombination(spell)).CanBeCasted())
                    {
                        CurrentCooldowns[spell] = 0;
                        CurrentCanUse[spell] = true;
                    }
                }
                else
                {
                    if (CurrentCooldowns[spell] > 0)
                    {
                        switch (spell)
                        {
                            case SpellsInvoker.Cold_Snap:
                                if (ColdSnapLastT == 0)
                                {
                                    ColdSnapLastT = CurrentCooldowns[spell];
                                    ColdSnapLastCheck = Environment.TickCount + (int)ColdSnapLastT * 1000;
                                }
                                else
                                {
                                    if (Environment.TickCount > ColdSnapLastCheck)
                                    {
                                        ColdSnapLastT = 0;
                                        ColdSnapLastCheck = 0;
                                        CurrentCooldowns[spell] = 0;

                                        CurrentCanUse[spell] = true;
                                    }
                                    else
                                    {
                                        CurrentCanUse[spell] = false;
                                    }
                                }
                                break;
                            case SpellsInvoker.Ghost_Walk:
                                if (GhostWalkLastT == 0)
                                {
                                    GhostWalkLastT = CurrentCooldowns[spell];
                                    GhostWalkLastCheck = Environment.TickCount + (int)GhostWalkLastT * 1000;
                                }
                                else
                                {
                                    if (Environment.TickCount > GhostWalkLastCheck)
                                    {
                                        GhostWalkLastT = 0;
                                        GhostWalkLastCheck = 0;
                                        CurrentCooldowns[spell] = 0;

                                        CurrentCanUse[spell] = true;
                                    }
                                    else
                                    {
                                        CurrentCanUse[spell] = false;
                                    }
                                }
                                break;
                            case SpellsInvoker.Ice_Wall:
                                if (IceWallLastT == 0)
                                {
                                    IceWallLastT = CurrentCooldowns[spell];
                                    IceWallLastCheck = Environment.TickCount + (int)IceWallLastT * 1000;
                                }
                                else
                                {
                                    if (Environment.TickCount > IceWallLastCheck)
                                    {
                                        IceWallLastT = 0;
                                        IceWallLastCheck = 0;
                                        CurrentCooldowns[spell] = 0;

                                        CurrentCanUse[spell] = true;
                                    }
                                    else
                                    {
                                        CurrentCanUse[spell] = false;
                                    }
                                }
                                break;
                            case SpellsInvoker.EMP:
                                if (EMPLastT == 0)
                                {
                                    EMPLastT = CurrentCooldowns[spell];
                                    EMPLastCheck = Environment.TickCount + (int)EMPLastT * 1000;
                                }
                                else
                                {
                                    if (Environment.TickCount > EMPLastCheck)
                                    {
                                        EMPLastT = 0;
                                        EMPLastCheck = 0;
                                        CurrentCooldowns[spell] = 0;

                                        CurrentCanUse[spell] = true;
                                    }
                                    else
                                    {
                                        CurrentCanUse[spell] = false;
                                    }
                                }
                                break;
                            case SpellsInvoker.Tornado:
                                if (TornadoLastT == 0)
                                {
                                    TornadoLastT = CurrentCooldowns[spell];
                                    TornadoLastCheck = Environment.TickCount + (int)TornadoLastT * 1000;
                                }
                                else
                                {
                                    if (Environment.TickCount > TornadoLastCheck)
                                    {
                                        TornadoLastT = 0;
                                        TornadoLastCheck = 0;
                                        CurrentCooldowns[spell] = 0;

                                        CurrentCanUse[spell] = true;
                                    }
                                    else
                                    {
                                        CurrentCanUse[spell] = false;
                                    }
                                }
                                break;
                            case SpellsInvoker.Alacrity:
                                if (AlacrityLastT == 0)
                                {
                                    AlacrityLastT = CurrentCooldowns[spell];
                                    AlacrityLastCheck = Environment.TickCount + (int)AlacrityLastT * 1000;
                                }
                                else
                                {
                                    if (Environment.TickCount > AlacrityLastCheck)
                                    {
                                        AlacrityLastT = 0;
                                        AlacrityLastCheck = 0;
                                        CurrentCooldowns[spell] = 0;

                                        CurrentCanUse[spell] = true;
                                    }
                                    else
                                    {
                                        CurrentCanUse[spell] = false;
                                    }
                                }
                                break;
                            case SpellsInvoker.Sun_Strike:
                                if (SunStrikeLastT == 0)
                                {
                                    SunStrikeLastT = CurrentCooldowns[spell];
                                    SunStrikeLastCheck = Environment.TickCount + (int)SunStrikeLastT * 1000;
                                }
                                else
                                {
                                    if (Environment.TickCount > SunStrikeLastCheck)
                                    {
                                        SunStrikeLastT = 0;
                                        SunStrikeLastCheck = 0;
                                        CurrentCooldowns[spell] = 0;

                                        CurrentCanUse[spell] = true;
                                    }
                                    else
                                    {
                                        CurrentCanUse[spell] = false;
                                    }
                                }
                                break;
                            case SpellsInvoker.Forge_Spirit:
                                if (ForgeSpiritLastT == 0)
                                {
                                    ForgeSpiritLastT = CurrentCooldowns[spell];
                                    ForgeSpiritLastCheck = Environment.TickCount + (int)ForgeSpiritLastT * 1000;
                                }
                                else
                                {
                                    if (Environment.TickCount > ForgeSpiritLastCheck)
                                    {
                                        ForgeSpiritLastT = 0;
                                        ForgeSpiritLastCheck = 0;
                                        CurrentCooldowns[spell] = 0;

                                        CurrentCanUse[spell] = true;
                                    }
                                    else
                                    {
                                        CurrentCanUse[spell] = false;
                                    }
                                }
                                break;
                            case SpellsInvoker.Chaos_Meteor:
                                if (ChaosMeteorLastT == 0)
                                {
                                    ChaosMeteorLastT = CurrentCooldowns[spell];
                                    ChaosMeteorLastCheck = Environment.TickCount + (int)ChaosMeteorLastT * 1000;
                                }
                                else
                                {
                                    if (Environment.TickCount > ChaosMeteorLastCheck)
                                    {
                                        ChaosMeteorLastT = 0;
                                        ChaosMeteorLastCheck = 0;
                                        CurrentCooldowns[spell] = 0;

                                        CurrentCanUse[spell] = true;
                                    }
                                    else
                                    {
                                        CurrentCanUse[spell] = false;
                                    }
                                }
                                break;
                            case SpellsInvoker.Deafening_Blast:
                                if (DeafeningBlastLastT == 0)
                                {
                                    DeafeningBlastLastT = CurrentCooldowns[spell];
                                    DeafeningBlastLastCheck = Environment.TickCount + (int)DeafeningBlastLastT * 1000;
                                }
                                else
                                {
                                    if (Environment.TickCount > DeafeningBlastLastCheck)
                                    {
                                        DeafeningBlastLastT = 0;
                                        DeafeningBlastLastCheck = 0;
                                        CurrentCooldowns[spell] = 0;

                                        CurrentCanUse[spell] = true;
                                    }
                                    else
                                    {
                                        CurrentCanUse[spell] = false;
                                    }
                                }
                                break;
                        }
                    }

                    if (CurrentCooldowns[spell] == 0)
                    {
                        CurrentCanUse[spell] = true;
                    }
                    else
                    {
                        CurrentCanUse[spell] = false;
                    }
                }
            }
        }
    }
}