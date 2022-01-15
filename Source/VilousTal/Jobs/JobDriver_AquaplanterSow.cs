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
    public class JobDriver_AquaplanterSow : JobDriver
    {
        private ThingDef plantDef;
        private float sowWorkDone;

        private Building_AquaPlanter Planter => TargetA.Thing as Building_AquaPlanter;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);

            Toil sow = new Toil();
            sow.initAction = delegate
            {
                plantDef = Planter.GetPlantDefToGrow();
            };
            sow.tickAction = delegate
            {
                Pawn actor = sow.actor;
                if (actor.skills != null)
                {
                    actor.skills.Learn(SkillDefOf.Plants, 0.085f, false);
                }
                float statValue = actor.GetStatValue(StatDefOf.PlantWorkSpeed, true);
                sowWorkDone += statValue;
                if (sowWorkDone >= plantDef.plant.sowWork)
                {
                    Planter.Notify_DoSow();
                    actor.records.Increment(RecordDefOf.PlantsSown);
                    Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.SowedPlant, actor.Named(HistoryEventArgsNames.Doer)), true);
                    if (plantDef.plant.humanFoodPlant)
                        Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.SowedHumanFoodPlant, actor.Named(HistoryEventArgsNames.Doer)), true);
                    ReadyForNextToil();
                }
            };
            sow.defaultCompleteMode = ToilCompleteMode.Never;
            sow.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            sow.FailOnCannotTouch(TargetIndex.A, PathEndMode.ClosestTouch);
            sow.WithEffect(EffecterDefOf.Sow, TargetIndex.A, null);
            sow.WithProgressBar(TargetIndex.A, () => sowWorkDone / plantDef.plant.sowWork, true, -0.5f, false);
            sow.PlaySustainerOrSound(() => SoundDefOf.Interact_Sow, 1f);
            sow.activeSkill = (() => SkillDefOf.Plants);
            yield return sow;
        }
    }
}
