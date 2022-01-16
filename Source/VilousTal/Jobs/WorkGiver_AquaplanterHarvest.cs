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
    public class WorkGiver_AquaplanterHarvest : WorkGiver_Aquaplanter
    {
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            List<Building> buildings = pawn.Map.listerBuildings.allBuildingsColonist;
            for (int i = 0; i < buildings.Count; i++)
            {
                if (buildings[i] is Building_AquaPlanter {ReadyForHarvest: true} planter && pawn.CanReserveAndReach(planter, PathEndMode, pawn.NormalMaxDanger()))
                    yield return planter;
            }
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return base.HasJobOnThing(pawn, t, forced);
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Job job = JobMaker.MakeJob(TalDefOf.AquaplanterHarvest);
            Room room = t.GetRoom();
            var c = t.Position;
            float num = 0f;
            for (int i = 0; i < 40; i++)
            {
                IntVec3 intVec = c + GenRadial.RadialPattern[i];
                var nextBuilding = intVec.GetFirstThing(t.Map, t.def) as Building_AquaPlanter;
                if(nextBuilding is null) continue;
                if(nextBuilding.GetRoom() == room && nextBuilding.ReadyForHarvest)
                {
                    Plant plant = nextBuilding.HeldPlant;
                    num += plant.def.plant.harvestWork;
                    if (intVec != c && num > 2400f)
                    {
                        break;
                    }
                    job.AddQueuedTarget(TargetIndex.A, nextBuilding);
                }
            }
            if (job.targetQueueA is {Count: >= 3})
            {
                job.targetQueueA.SortBy(targ => targ.Cell.DistanceToSquared(pawn.Position));
            }
            return job;
        }
    }
}
