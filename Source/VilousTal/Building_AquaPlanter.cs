using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace VilousTal
{
    public class AquaPlanterSet
    {
        private const int valuePerPart = 100;

        private float internalValue = 0;
        private List<Building_AquaPlanter> parts;

        public List<Building_AquaPlanter> AllParts => parts;

        public int TotalCapacity => parts.Count * valuePerPart;
        public float TotalStored => internalValue;
        public float CurrentLevel => internalValue / TotalCapacity;

        private void AddPart(Building_AquaPlanter part)
        {
            parts ??= new List<Building_AquaPlanter>();
            parts.Add(part);
        }


        public void Notify_AddWater(float amount)
        {
            internalValue = Mathf.Clamp(internalValue + amount, 0, TotalCapacity);
        }

        public static AquaPlanterSet Regenerate(Thing root)
        {
            AquaPlanterSet newSet = new AquaPlanterSet();
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

    public class Building_AquaPlanter : Building_VT
    {
        private AquaPlanterSet assignedSet;

        public AquaPlanterSet AquaPlanterSet
        {
            get => assignedSet;
            set => assignedSet = value;
        }

        public float WaterLevel => assignedSet.CurrentLevel;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.AquaPlanterSet = AquaPlanterSet.Regenerate(this);
            UpdateGraphic();
        }

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
        private VTGraphic_Linked WaterGraphic => ExtraGraphic as VTGraphic_Linked;//graphInt??= new VTGraphic_Linked((ExtraGraphic as Graphic_Linked).subGraphic);

        public override void Draw()
        {
            if(debug_ShowSet)
                GenDraw.DrawFieldEdges(AquaPlanterSet.AllParts.Select(t => t.Position).ToList(), Color.cyan);
        }

        public override void Print(SectionLayer layer)
        {
            base.Print(layer);
            WaterGraphic.Print(layer, this, 0, AltitudeLayer.BuildingOnTop, debug_DrawExtra);
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Water Level: {WaterLevel.ToStringPercent()}");
            sb.AppendLine($"Connected Parts: {AquaPlanterSet.AllParts.Count}");
            return sb.ToString().TrimEndNewlines();
        }

        private bool debug_ShowSet;
        private bool debug_DrawExtra = true;
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

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
