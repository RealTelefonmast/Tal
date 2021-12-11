using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace VilousTal
{
    public class Building_AquaPlanter : Building_VT
    {
        private float internalWater = 0;
        private float maxWaterValue = 100;

        public float WaterLevel => internalWater/maxWaterValue;

        private void Notify_AddedWater(float amount)
        {
            internalWater = Mathf.Clamp(internalWater + amount, 0, maxWaterValue);

            extraGraphicInt = ExtraGraphic.GetColoredVersion(ExtraGraphic.Shader, new Color(1, 1, 1, WaterLevel), Color.white);

            Map.mapDrawer.MapMeshDirty(Position, MapMeshFlag.Buildings);
        }

        //public VTGraphic_Linked graphInt;
        private Graphic_Linked WaterGraphic => ExtraGraphic as Graphic_Linked;//graphInt??= new VTGraphic_Linked((ExtraGraphic as Graphic_Linked).subGraphic);

        public override void Print(SectionLayer layer)
        {
            base.Print(layer);

            PrintOverlay(layer);
        }

        private void PrintOverlay(SectionLayer layer)
        {
            IntVec3 position = Position;
            for (int i = 0; i < 4; i++)
            {
                IntVec3 intVec = Position + GenAdj.DiagonalDirectionsAround[i];
                var thisG = WaterGraphic;
                if (thisG.ShouldLinkWith(intVec, this) && 
                    (i != 0 || (thisG.ShouldLinkWith(position + IntVec3.West, this) && thisG.ShouldLinkWith(position + IntVec3.South, this))) && 
                    (i != 1 || (thisG.ShouldLinkWith(position + IntVec3.West, this) && thisG.ShouldLinkWith(position + IntVec3.North, this))) && 
                    (i != 2 || (thisG.ShouldLinkWith(position + IntVec3.East, this) && thisG.ShouldLinkWith(position + IntVec3.North, this))) && 
                    (i != 3 || (thisG.ShouldLinkWith(position + IntVec3.East, this) && thisG.ShouldLinkWith(position + IntVec3.South, this))))
                {
                    Vector3 center = this.DrawPos + Altitudes.AltIncVect + GenAdj.DiagonalDirectionsAround[i].ToVector3().normalized * Graphic_LinkedCornerFiller.CoverOffsetDist + Altitudes.AltIncVect + new Vector3(0f, 0f, 0.09f);
                    Vector2 size = new Vector2(0.5f, 0.5f);
                    if (!intVec.InBounds(Map))
                    {
                        if (intVec.x == -1)
                        {
                            center.x -= 1f;
                            size.x *= 5f;
                        }
                        if (intVec.z == -1)
                        {
                            center.z -= 1f;
                            size.y *= 5f;
                        }
                        if (intVec.x == Map.Size.x)
                        {
                            center.x += 1f;
                            size.x *= 5f;
                        }
                        if (intVec.z == Map.Size.z)
                        {
                            center.z += 1f;
                            size.y *= 5f;
                        }
                    }
                    Printer_Plane.PrintPlane(layer, center, size, thisG.LinkedDrawMatFrom(this, Position), 0, false, Graphic_LinkedCornerFiller.CornerFillUVs, null, 0.01f, 0f);
                }
            }
        }

        public override string GetInspectString()
        {
            return $"Water Level: {WaterLevel.ToStringPercent()}";
        }

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
                    action = delegate { Notify_AddedWater(25f); }
                };
                yield return new Command_Action()
                {
                    defaultLabel = "Remove Water",
                    action = delegate { Notify_AddedWater(-25f); }
                };
            }
        }
    }
}
