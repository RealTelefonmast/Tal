using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace VilousTal
{
    public class AquaPlanterSet : IPlantToGrowSettable
    {
        private const int valuePerPart = 100;

        private Map map;
        private List<IntVec3> cells = new();

        private ThingDef currentPlant;

        private float internalValue = 0;
        private List<Building_AquaPlanter> parts;

        public List<Building_AquaPlanter> AllParts => parts;

        public int TotalCapacity => parts.Count * valuePerPart;
        public float TotalStored => internalValue;
        public float CurrentLevel => internalValue / TotalCapacity;

        public Map Map => map;
        public IEnumerable<IntVec3> Cells => cells;

        public AquaPlanterSet(Map map)
        {
            this.map = map;
        }

        private void AddPart(Building_AquaPlanter part)
        {
            parts ??= new List<Building_AquaPlanter>();
            parts.Add(part);
            cells.Add(part.Position);
        }

        public void Notify_AddWater(float amount)
        {
            internalValue = Mathf.Clamp(internalValue + amount, 0, TotalCapacity);
        }

        //PlantGrower
        public IEnumerable<Plant> PlantsOnMe
        {
            get
            {
                foreach (IntVec3 c in Cells)
                {
                    List<Thing> thingList = Map.thingGrid.ThingsListAt(c);
                    int num;
                    for (int i = 0; i < thingList.Count; i = num + 1)
                    {
                        if (thingList[i] is Plant plant)
                        {
                            yield return plant;
                        }
                        num = i;
                    }
                }
            }
        }

        public ThingDef GetPlantDefToGrow()
        {
            return currentPlant;
        }

        public void SetPlantDefToGrow(ThingDef plantDef)
        {
            currentPlant = plantDef;
        }

        public bool CanAcceptSowNow()
        {
            return CurrentLevel > 0.10f;
        }

        //
        public static AquaPlanterSet Regenerate(Thing root)
        {
            AquaPlanterSet newSet = new AquaPlanterSet(root.Map);
            HashSet<AquaPlanterSet> oldSets = new HashSet<AquaPlanterSet>();

            HashSet<Thing> closedSet = new HashSet<Thing>();
            HashSet<Thing> openSet = new HashSet<Thing>() { root };
            HashSet<Thing> currentSet = new HashSet<Thing>();
            while (openSet.Count > 0)
            {
                foreach (var thing in openSet)
                {
                    if (thing is Building_AquaPlanter planter)
                    {
                        if (planter.AquaPlanterSet != null)
                        {
                            oldSets.Add(planter.AquaPlanterSet);
                        }
                        planter.AquaPlanterSet = newSet;
                        newSet.AddPart(planter);
                        closedSet.Add(planter);
                    }
                }

                //
                (currentSet, openSet) = (openSet, currentSet);

                openSet.Clear();
                foreach (var thing in currentSet)
                {
                    foreach (var c in GenAdjFast.AdjacentCellsCardinal(thing))
                    {
                        Thing ofDef = c.GetFirstThing(root.Map, root.def);
                        if (ofDef != null && !closedSet.Contains(ofDef))
                        {
                            openSet.Add(ofDef);
                        }
                    }
                }
            }

            foreach (var aquaPlanterSet in oldSets)
            {
                newSet.Notify_AddWater(aquaPlanterSet.TotalStored);
            }

            return newSet;
        }
    }

    public class Building_AquaPlanter : Building_VT, IPlantToGrowSettable, IPlantSimulatorHolder
    {
        private AquaPlanterSet assignedSet;

        //
        private PlantGrowthSimulator internalPlantSimulator;

        public AquaPlanterSet AquaPlanterSet
        {
            get => assignedSet;
            set => assignedSet = value;
        }

        public float WaterLevel => assignedSet.CurrentLevel;
        public IEnumerable<IntVec3> Cells => AquaPlanterSet.Cells;

        //
        public Thing Thing => this;
        public Plant HeldPlant => internalPlantSimulator.Plant;

        public bool HasPlant => internalPlantSimulator.Plant != null;
        public bool ReadyForHarvest => internalPlantSimulator.ReadyForHarvest;
        public float ProvidedFertility => (float)Math.Round(Mathf.Log10(Mathf.Lerp(1,10,WaterLevel)) * def.fertility, 2);

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.AquaPlanterSet = AquaPlanterSet.Regenerate(this);
            AquaPlanterSet.SetPlantDefToGrow(def.building.defaultPlantToGrow);
            internalPlantSimulator = new PlantGrowthSimulator(this);
            UpdateGraphic();
        }

        public override void PostMake()
        {
            base.PostMake();
        }

        public override void TickRare()
        {
            base.TickRare();
            TryCollectRainWater();
            internalPlantSimulator.TickLong();
        }

        private void TryCollectRainWater()
        {
            if (Position.Roofed(Map)) return;
            if (Map.weatherManager.RainRate > 0.01f && Rand.Value < 6f)
            {
                Notify_AddedWater(Map.weatherManager.RainRate * 2);
            }
        }

        //
        public ThingDef GetPlantDefToGrow()
        {
            return AquaPlanterSet.GetPlantDefToGrow();
        }

        public void SetPlantDefToGrow(ThingDef plantDef)
        {
            AquaPlanterSet.SetPlantDefToGrow(plantDef);
        }

        public bool CanAcceptSowNow()
        {
            return !HasPlant && AquaPlanterSet.CanAcceptSowNow();
        }

        public void Notify_DoSow()
        {
            var def = GetPlantDefToGrow();
            if (def != null)
            {
                var internalPlant = (Plant)ThingMaker.MakeThing(def);
                internalPlantSimulator.SetPlant(internalPlant);
            }
        }

        public void Notify_DoHarvest(Pawn byPawn)
        {
            internalPlantSimulator.Notify_PlantHarvested(byPawn);
            AquaPlanterSet.Notify_AddWater(-2f);
        }

        //
        private void UpdateGraphic()
        {
            AquaPlanterSet.AllParts.ForEach(t =>
            {
                t.graphics[0].SetNewColor(new Color(1, 1, 1, WaterLevel));
                Map.mapDrawer.MapMeshDirty(t.Position, MapMeshFlag.Terrain);
                Map.mapDrawer.MapMeshDirty(t.Position, MapMeshFlag.Buildings);
                Map.mapDrawer.MapMeshDirty(t.Position, MapMeshFlag.Things);
            });
        }

        private void Notify_AddedWater(float amount)
        {
            AquaPlanterSet.Notify_AddWater(amount);
            UpdateGraphic();
        }

        //public VTGraphic_Linked graphInt;
        private VTGraphic WaterGraphic => ExtraGraphic(0);
        private VTGraphic WallGraphic => ExtraGraphic(1);

        public override void Draw()
        {
            if(debug_ShowSet)
                GenDraw.DrawFieldEdges(AquaPlanterSet.AllParts.Select(t => t.Position).ToList(), Color.cyan);
        }

        //
        protected static Vector2[] UVS = new Vector2[] {new(0f, 0f), new(0f, 1f), new(1f, 1f), new(1f, 0f)};

        public override void Print(SectionLayer layer)
        {
            base.Print(layer);
            //
            DrawPlants(layer, AltitudeLayer.Building.AltitudeFor(0.25f));
            //            
            WaterGraphic.Print(layer, this, 0);
            WallGraphic.Print(layer, this, 0);
        }

        private void DrawPlants(SectionLayer layer, float altitudeOverride)
        {
            if (HeldPlant is null) return;

            Rand.PushState();
            Rand.Seed = base.Position.GetHashCode();
            var drawPos = new Vector3(DrawPos.x, altitudeOverride, DrawPos.z);
            var mat = HeldPlant.Graphic.MatSingle;
            var randBool = Rand.Bool;
            Graphic.TryGetTextureAtlasReplacementInfo(mat, HeldPlant.def.category.ToAtlasGroup(), randBool, true, out mat, out var _, out _);
            
            Printer_Plane.PrintPlane(layer, drawPos, new Vector2(0.35f, 0.35f), mat, 0, randBool, UVS, new Color32[4], 0.01f, 0f);
            Rand.PopState();
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Plant: {HeldPlant}: {HeldPlant?.Growth}");
            sb.AppendLine($"LifeStage: {HeldPlant?.LifeStage} | Resting: {HeldPlant?.Resting}");
            //sb.AppendLine($"GrowthPT: {internalPlant?.GrowthPerTick} | GrowthRate: {internalPlant?.GrowthRate}");
            sb.AppendLine($"Water Level: {WaterLevel.ToStringPercent()}");
            sb.AppendLine($"CanAcceptSow: {CanAcceptSowNow()}");
            sb.AppendLine($"Fertility: {ProvidedFertility.ToStringPercent()}");
            /*
            sb.AppendLine($"Connected Parts: {AquaPlanterSet.AllParts.Count}");
            sb.AppendLine($"CanAcceptSowNow: {CanAcceptSowNow()}");
            sb.AppendLine($"Cells: {Cells.Count()}");
            sb.AppendLine($"ToGrow: {GetPlantDefToGrow()}");
            var report = PlantUtility.CanEverPlantAt(GetPlantDefToGrow(), Position, Map, out Thing blocker);
            sb.AppendLine($"CanPlantEverAt: {report.Accepted} '{report.Reason}' Blocker: {blocker}");
            sb.AppendLine($"CanPlantNowAt: {PlantUtility.CanNowPlantAt(GetPlantDefToGrow(), Position, Map)}");
            sb.AppendLine($"{(PlantUtility.GrowthSeasonNow(base.Position, base.Map, true) ? "GrowSeasonHereNow".Translate() : "CannotGrowBadSeasonTemperature".Translate())}");
            */
            return sb.ToString().TrimEndNewlines();
        }

        public bool debug_ShowSet;
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            yield return PlantToGrowSettableUtility.SetPlantToGrowCommand(this);

            if (DebugSettings.godMode)
            {
                yield return new Command_Action()
                {
                    defaultLabel = "Fill Water",
                    action = delegate { Notify_AddedWater(100f); }
                };
                yield return new Command_Action()
                {
                    defaultLabel = "Remove Water",
                    action = delegate { Notify_AddedWater(-100f); }
                };
                yield return new Command_Action()
                {
                    defaultLabel = "Show Set",
                    action = delegate
                    {
                        debug_ShowSet = !debug_ShowSet;
                    }
                };
                yield return new Command_Action()
                {
                    defaultLabel = "Add Plant",
                    action = delegate
                    {
                        Notify_DoSow();
                        UpdateGraphic();
                    }
                };
            }
        }
    }
}
