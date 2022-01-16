using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace VilousTal
{
    public class JobDriver_AquaplanterHarvest : JobDriver
    {
        private float workDone;
        private float xpPerTick = 0.085f;

		private Building_AquaPlanter Planter => TargetA.Thing as Building_AquaPlanter;
        private Plant Plant => (this.job.targetA.Thing as Building_AquaPlanter).HeldPlant;

		public static float WorkDonePerTick(Pawn actor, Plant plant)
        {
            return actor.GetStatValue(StatDefOf.PlantWorkSpeed, true) * Mathf.Lerp(3.3f, 1f, plant.Growth);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            LocalTargetInfo target = job.GetTarget(TargetIndex.A);
            if (target.IsValid && !this.pawn.Reserve(target, job, 1, -1, null, errorOnFailed))
            {
                return false;
            }
            pawn.ReserveAsManyAsPossible(this.job.GetTargetQueue(TargetIndex.A), this.job, 1, -1, null);
			return true;
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_JobTransforms.MoveCurrentTargetIntoQueue(TargetIndex.A);
			Toil clearToil = new Toil();
            clearToil.initAction = delegate ()
            {
                Pawn actor = clearToil.actor;
                actor.jobs.curJob.GetTargetQueue(TargetIndex.A).RemoveAll((ta) => !ta.HasThing || !ta.Thing.Spawned || ta.Thing.IsForbidden(actor));
            };
            yield return clearToil;
            yield return Toils_JobTransforms.SucceedOnNoTargetInQueue(TargetIndex.A);
            yield return Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.A, true);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).JumpIfDespawnedOrNullOrForbidden(TargetIndex.A, clearToil);

			Toil cut = new Toil();
            cut.tickAction = delegate ()
			{
				Pawn actor = cut.actor;
                actor.skills?.Learn(SkillDefOf.Plants, xpPerTick, false);

                Plant plant2 = Planter.HeldPlant;
				workDone += WorkDonePerTick(actor, plant2);
				if (workDone >= plant2.def.plant.harvestWork)
				{
					if (plant2.def.plant.harvestedThingDef != null)
					{
						StatDef stat = plant2.def.plant.harvestedThingDef.IsDrug ? StatDefOf.DrugHarvestYield : StatDefOf.PlantHarvestYield;
						float statValue = actor.GetStatValue(stat, true);
						if (actor.RaceProps.Humanlike && plant2.def.plant.harvestFailable && !plant2.Blighted && Rand.Value > statValue)
						{
							MoteMaker.ThrowText((this.pawn.DrawPos + plant2.DrawPos) / 2f, this.Map, "TextMote_HarvestFailed".Translate(), 3.65f);
						}
						else
						{
							int num = plant2.YieldNow();
							if (statValue > 1f)
							{
								num = GenMath.RoundRandom((float)num * statValue);
							}
							if (num > 0)
							{
								Thing thing = ThingMaker.MakeThing(plant2.def.plant.harvestedThingDef, null);
								thing.stackCount = num;
								if (actor.Faction != Faction.OfPlayer)
								{
									thing.SetForbidden(true, true);
								}
								Find.QuestManager.Notify_PlantHarvested(actor, thing);
								GenPlace.TryPlaceThing(thing, actor.Position, this.Map, ThingPlaceMode.Near, null, null, default(Rot4));
								actor.records.Increment(RecordDefOf.PlantsHarvested);
							}
						}
					}
					plant2.def.plant.soundHarvestFinish.PlayOneShot(actor);
                    Planter.Notify_DoHarvest(actor);
                    workDone = 0f;
                    job.GetTargetQueue(TargetIndex.A).Remove(Planter);
					ReadyForNextToil();
                }
			};
			//cut.AddPreInitAction(() => plantDef = Planter.HeldPlant.def);
			cut.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            cut.FailOnCannotTouch(TargetIndex.A, PathEndMode.ClosestTouch);
            cut.FailOn(t => !Planter.HasPlant);
			cut.defaultCompleteMode = ToilCompleteMode.Never;
            cut.WithEffect(EffecterDefOf.Harvest_Plant, TargetIndex.A);
            cut.WithProgressBar(TargetIndex.A, () => this.workDone / Plant.def.plant.harvestWork, true, -0.5f, false);
			cut.PlaySustainerOrSound(() => Plant.def.plant.soundHarvesting, 1f);
			cut.activeSkill = (() => SkillDefOf.Plants);
			yield return cut;

            yield return Toils_Jump.Jump(clearToil);
        }
    }
}
