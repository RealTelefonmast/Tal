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
            return CurrentLevel > 0.5f;
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

    public class Building_AquaPlanter : Building_VT, IPlantToGrowSettable
    {
        private AquaPlanterSet assignedSet;

        public AquaPlanterSet AquaPlanterSet
        {
            get => assignedSet;
            set => assignedSet = value;
        }

        public float WaterLevel => assignedSet.CurrentLevel;
        public IEnumerable<IntVec3> Cells => AquaPlanterSet.Cells;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.AquaPlanterSet = AquaPlanterSet.Regenerate(this);
            AquaPlanterSet.SetPlantDefToGrow(def.building.defaultPlantToGrow);
            UpdateGraphic();
        }

        public override void PostMake()
        {
            base.PostMake();
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
            return AquaPlanterSet.CanAcceptSowNow();
        }

        //
        private void UpdateGraphic()
        {
            AquaPlanterSet.AllParts.ForEach(t =>
            {
                t.extraGraphicInt = ExtraGraphic.GetColoredVersion(ExtraGraphic.Shader, new Color(1, 1, 1, WaterLevel), Color.white);
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
        private VTGraphic_LinkedSelf WaterGraphic => ExtraGraphic as VTGraphic_LinkedSelf;//graphInt??= new VTGraphic_Linked((ExtraGraphic as Graphic_Linked).subGraphic);

        public override void Draw()
        {
            if(debug_ShowSet)
                GenDraw.DrawFieldEdges(AquaPlanterSet.AllParts.Select(t => t.Position).ToList(), Color.cyan);
        }

        public override void Print(SectionLayer layer)
        {
            base.Print(layer);
            WaterGraphic.Print(layer, this, 0, AltitudeLayer.BuildingOnTop);
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Water Level: {WaterLevel.ToStringPercent()}");
            sb.AppendLine($"Connected Parts: {AquaPlanterSet.AllParts.Count}");
            sb.AppendLine($"CanAcceptSowNow: {CanAcceptSowNow()}");
            sb.AppendLine($"Cells: {Cells.Count()}");
            sb.AppendLine($"ToGrow: {GetPlantDefToGrow()}");
            var report = PlantUtility.CanEverPlantAt(GetPlantDefToGrow(), Position, Map, out Thing blocker);
            sb.AppendLine($"CanPlantEverAt: {report.Accepted} '{report.Reason}' Blocker: {blocker}");
            sb.AppendLine($"CanPlantNowAt: {PlantUtility.CanNowPlantAt(GetPlantDefToGrow(), Position, Map)}");
            sb.AppendLine($"{(PlantUtility.GrowthSeasonNow(base.Position, base.Map, true) ? "GrowSeasonHereNow".Translate() : "CannotGrowBadSeasonTemperature".Translate())}");
            return sb.ToString().TrimEndNewlines();
        }

        public bool debug_ShowSet;
        public static bool debug_DrawBasic = true;
        public static bool debug_DrawExtra = true;
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
                    defaultLabel = "Switch Basic",
                    action = delegate
                    {
                        debug_DrawBasic = !debug_DrawBasic;
                        UpdateGraphic();
                    }
                };
                yield return new Command_Action()
                {
                    defaultLabel = "Switch Extra",
                    action = delegate
                    {
                        debug_DrawExtra = !debug_DrawExtra;
                        UpdateGraphic();
                    }
                };
            }
        }
    }
}
