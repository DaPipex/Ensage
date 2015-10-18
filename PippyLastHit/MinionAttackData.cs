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
    class MinionAttackData
    {

        public string CreepName { get; private set; }
        public ClassID CreepClassID { get; private set; }
        public float ProjSpeed { get; private set; }

        public MinionAttackData(string creepName, ClassID creepClassID, float projSpeed)
        {
            CreepName = creepName;
            CreepClassID = creepClassID;
            ProjSpeed = projSpeed;
        }

        //Will use this class later to organize stuff when it's ready.
    }
}
