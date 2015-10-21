using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;


namespace PippyLastHit
{
    class MinionAAData
    {
        public static float GetAttackSpeed(Creep creep)
        {
            var attackSpeed = Math.Min(creep.AttacksPerSecond * 1 / 0.01, 600);

            return (float)attackSpeed;
        }

        public static float GetAttackPoint(Creep creep)
        {
            var animationPoint = 0f;

            var attackSpeed = GetAttackSpeed(creep);

            if (creep.IsRanged)
            {
                animationPoint = 0.5f;
            }
            else
            {
                animationPoint = 0.467f;
            }

            return animationPoint / (1 + (attackSpeed - 100) / 100);
        }

        public static float GetAttackRate(Creep creep)
        {
            var attackSpeed = GetAttackSpeed(creep);

            return 1 / (1 + (attackSpeed - 100) / 100);
        }

        public static float GetAttackBackswing(Creep creep)
        {
            var attackRate = GetAttackRate(creep);

            var attackPoint = GetAttackPoint(creep);

            return attackRate - attackPoint;
        }
    }
}
