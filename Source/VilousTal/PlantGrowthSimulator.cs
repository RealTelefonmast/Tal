using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace VilousTal
{
    public interface IPlantSimulatorHolder
    {
        public Thing Thing { get; }
        public float ProvidedFertility { get; }
    }

    public class PlantGrowthSimulator
    {
        private IPlantSimulatorHolder parent;
        private Plant curPlant;

        private IntVec3 Position => parent.Thing.Position;
        private Map Map => parent.Thing.Map;
        private Thing ParentThing => parent.Thing;

        //
        public bool ReadyForHarvest => curPlant?.HarvestableNow ?? false;

        //Plant Sim
        public ThingDef Def => curPlant.def;
        public Plant Plant => curPlant;

        private bool DyingBecauseExposedToLight => Def.plant.cavePlant && ParentThing.Spawned && Map.glowGrid.GameGlowAt(Position, true) > 0f;

        private bool HasEnoughLightToGrow => GrowthRateFactor_Light > 0.001f;

        private bool Resting => GenLocalDate.DayPercent(ParentThing) < 0.25f || GenLocalDate.DayPercent(ParentThing) > 0.8f;

        private bool CurrentlyCultivated
        {
            get
            {
                if (!Def.plant.Sowable)
                {
                    return false;
                }

                if (!ParentThing.Spawned)
                {
                    return false;
                }

                return ParentThing.def.building.SupportsPlants;
            }
        }

        private float GrowthRate
        {
            get
            {
                if (curPlant.Blighted)
                {
                    return 0f;
                }

                if (ParentThing.Spawned && !PlantUtility.GrowthSeasonNow(Position, Map))
                {
                    return 0f;
                }

                return GrowthRateFactor_Fertility * GrowthRateFactor_Temperature * GrowthRateFactor_Light;
            }
        }

        private float GrowthPerTick
        {
            get
            {
                if (curPlant.LifeStage != PlantLifeStage.Growing || Resting)
                {
                    return 0f;
                }

                return 1f / (60000f * Def.plant.growDays) * GrowthRate;
            }
        }

        //GrowthFactors
        private float GrowthRateFactor_Fertility => PlantUtility.GrowthRateFactorFor_Fertility(Def, parent.ProvidedFertility);

        private float GrowthRateFactor_Light => PlantUtility.GrowthRateFactorFor_Light(Def, Map.glowGrid.GameGlowAt(Position));

        private float GrowthRateFactor_Temperature
        {
            get
            {
                if (!GenTemperature.TryGetTemperatureForCell(Position, Map, out var cellTemp)) return 1;
                return PlantUtility.GrowthRateFactorFor_Temperature(cellTemp);
            }
        }

        private float CurrentDyingDamagePerTick
        {
            get
            {
                if (!ParentThing.Spawned) return 0;
                float num = 0f;
                if (Def.plant.LimitedLifespan && curPlant.ageInt > Def.plant.LifespanTicks)
                {
                    num = Mathf.Max(num, 0.005f);
                }

                if (!Def.plant.cavePlant && Def.plant.dieIfNoSunlight && curPlant.unlitTicks > 450000)
                {
                    num = Mathf.Max(num, 0.005f);
                }
                if (DyingBecauseExposedToLight)
                {
                    float lerpPct = Map.glowGrid.GameGlowAt(Position, true);
                    num = Mathf.Max(num, Plant.DyingDamagePerTickBecauseExposedToLight.LerpThroughRange(lerpPct));
                }
                return num;
            }
        }

        public PlantGrowthSimulator(IPlantSimulatorHolder parent)
        {
            this.parent = parent;
        }

        public void SetPlant(Plant plant)
        {
            curPlant = plant;
        }

        public void Notify_PlantHarvested(Pawn byPawn)
        {
            curPlant.PlantCollected(byPawn);
            curPlant = null;
        }

        //Plant Sim, mostly a copy of what happens inside Plant.TickLong
        public void TickLong()
        {
            if (curPlant == null) return;
            if (PlantUtility.GrowthSeasonNow(Position, Map))
            {
                float num = curPlant.growthInt;
                bool flag = curPlant.LifeStage == PlantLifeStage.Mature;
                curPlant.growthInt = Mathf.Clamp01(curPlant.growthInt + GrowthPerTick * 2000f);
                if (((!flag && curPlant.LifeStage == PlantLifeStage.Mature) ||
                     (int) (num * 10f) != (int) (curPlant.growthInt * 10f)) && CurrentlyCultivated)
                {
                    Map.mapDrawer.MapMeshDirty(Position, MapMeshFlag.Things);
                }
            }

            if (!HasEnoughLightToGrow)
            {
                curPlant.unlitTicks += 2000;
            }
            else
            {
                curPlant.unlitTicks = 0;
            }

            curPlant.ageInt += 2000;
        }
    }
}
