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
    public class VTGraphic_Linked : Graphic_Linked
    {
        public VTGraphic_Linked(Graphic subGraphic)
        {
            this.subGraphic = subGraphic;
            this.data = subGraphic.data;
        }

        public override void Print(SectionLayer layer, Thing thing, float extraRotation)
        {
            Material mat = this.LinkedDrawMatFrom(thing, thing.Position);
            Printer_Plane.PrintPlane(layer, thing.Position.ToVector3ShiftedWithAltitude(thing.def.Altitude+Altitudes.AltInc), new Vector2(1f, 1f), mat, extraRotation, false, null, null, 0.01f, 0f);
            if (base.ShadowGraphic != null && thing != null)
            {
                base.ShadowGraphic.Print(layer, thing, 0f);
            }
        }

	}
}
