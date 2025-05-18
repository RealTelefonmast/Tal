using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace VilousTal
{
    public class VTGraphic_LinkedSelf : Graphic_Linked
    {
        public VTGraphic_LinkedSelf() { }

        public override void Init(GraphicRequest req)
        {
            subGraphic = GraphicDatabase.Get(typeof(Graphic_Single), req.path, req.shader, req.drawSize, req.color, req.colorTwo, req.graphicData, req.shaderParameters, null);
            data = req.graphicData;
            path = req.path;
            color = req.color;
            colorTwo = req.colorTwo;
        }

        public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
        {
            return GraphicDatabase.Get<VTGraphic_LinkedSelf>(this.path, newShader, this.drawSize, newColor, newColorTwo, this.data, null);
        }

        public override Material LinkedDrawMatFrom(Thing parent, IntVec3 cell)
        {
            return base.LinkedDrawMatFrom(parent, cell);
        }

        public override bool ShouldLinkWith(IntVec3 c, Thing parent)
        {
            return c.GetFirstThing(parent.Map, parent.def) != null;
        }

        public override Material MatSingleFor(Thing thing)
        {
            return LinkedDrawMatFrom(thing, thing.Position);
        }

        public void Print(SectionLayer layer, Thing thing, float extraRotation, AltitudeLayer? altitudeOverride)
        {
            Print(layer, thing, thing.DrawPos, extraRotation, altitudeOverride?.AltitudeFor() ?? 0);
        }

        public void Print(SectionLayer layer, Thing thing, Vector3 drawPos, float extraRotation, float altitudeOverride)
        {
            Material mat = LinkedDrawMatFrom(thing, thing.Position);
            Printer_Plane.PrintPlane(layer, new Vector3(drawPos.x, altitudeOverride, drawPos.z), Vector2.one, mat, extraRotation);
        }

        public override void Print(SectionLayer layer, Thing thing, float extraRotation)
        {
            Print(layer, thing, extraRotation, null);
        }

    }
}
