using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;

namespace VilousTal
{
    public class JobDriver_AquaplanterHarvest : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            throw new NotImplementedException();
        }
    }
}
