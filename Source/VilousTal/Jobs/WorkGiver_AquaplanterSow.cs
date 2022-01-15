using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;

namespace VilousTal
{
    public class WorkGiver_AquaplanterSow : WorkGiver_Scanner
    {
        protected static ThingDef wantedPlantDef;

        public override PathEndMode PathEndMode => PathEndMode.ClosestTouch;

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            List<Building> buildings = pawn.Map.listerBuildings.allBuildingsColonist;
            for (int i = 0; i < buildings.Count; i++)
            {
                if (buildings[i] is Building_AquaPlanter planter && planter.CanAcceptSowNow() && pawn.CanReach(planter, PathEndMode, pawn.NormalMaxDanger()))
                    yield return planter;
            }
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (t is not Building_AquaPlanter p || p.GetPlantDefToGrow() == null) return false;
            return base.HasJobOnThing(pawn, t, forced);
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Map map = pawn.Map;
            Job job = JobMaker.MakeJob(TalDefOf.AquaplanterSow, t);
            return job;
        }
    }
}
