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
    public class VTGraphic_Linked : Graphic_Linked
    {
        public VTGraphic_Linked() { }

        /*
        public VTGraphic_Linked(Graphic subGraphic)
        {
            this.subGraphic = subGraphic;
            this.data = subGraphic.data;


            MaterialRequest req = default(MaterialRequest);
            req.mainTex = ContentFinder<Texture2D>.Get($"{base.subGraphic.path}_FC", true);
            req.shader = subGraphic.Shader;
            req.color = subGraphic.Color;
            req.colorTwo = subGraphic.ColorTwo;
            req.renderQueue = data.renderQueue;
            req.shaderParameters = data.shaderParameters;

            this.filledMat = MaterialPool.MatFrom(req);
        }
        */

        public override void Init(GraphicRequest req)
        {
            subGraphic = GraphicDatabase.Get(typeof(Graphic_Single), req.path, req.shader, req.drawSize, req.color, req.colorTwo, req.graphicData, req.shaderParameters, null);
            data = req.graphicData;
            path = req.path;
            color = req.color;
            colorTwo = req.colorTwo;

            //
            /*
            MaterialRequest req2 = default(MaterialRequest);
            req2.mainTex = ContentFinder<Texture2D>.Get($"{req.path}_FC", true);
            req2.shader = req.shader;
            req2.color = req.color;
            req2.colorTwo = req.colorTwo;
            req2.renderQueue = req.renderQueue;
            req2.shaderParameters = req.shaderParameters;

            filledMat = MaterialPool.MatFrom(req2);
            */
        }

        public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
        {
            return GraphicDatabase.Get<VTGraphic_Linked>(this.path, newShader, this.drawSize, newColor, newColorTwo, this.data, null);
        }

        public override Material LinkedDrawMatFrom(Thing parent, IntVec3 cell)
        {
            return base.LinkedDrawMatFrom(parent, cell);
        }

        public override bool ShouldLinkWith(IntVec3 c, Thing parent)
        {
            return c.GetFirstThing(parent.Map, parent.def) != null;
        }

        public bool IsSurrounded(Thing thing)
        {
            return GenAdjFast.AdjacentCells8Way(thing).All(t => t.GetFirstThing(thing.Map, thing.def) != null);
        }

        public void Print(SectionLayer layer, Thing thing, float extraRotation, AltitudeLayer? altitudeOverride = null, bool drawExtra = true)
        {
            var drawPos = thing.DrawPos;
            if (altitudeOverride != null)
                drawPos += new Vector3(0, altitudeOverride.Value.AltitudeFor(), 0);

            if (!IsSurrounded(thing) && drawExtra)
            {
                Material mat = this.LinkedDrawMatFrom(thing, thing.Position);
                Printer_Plane.PrintPlane(layer, drawPos, new Vector2(1f, 1f), mat, extraRotation, false, null, null, 0.01f, 0f);
            }

            IntVec3 position = thing.Position;
            for (int i = 0; i < 4; i++)
            {
                IntVec3 intVec = thing.Position + GenAdj.DiagonalDirectionsAround[i];
                if (ShouldLinkWith(intVec, thing) &&
                    (i != 0 || (ShouldLinkWith(position + IntVec3.West, thing) &&
                                ShouldLinkWith(position + IntVec3.South, thing))) &&
                    (i != 1 || (ShouldLinkWith(position + IntVec3.West, thing) &&
                                ShouldLinkWith(position + IntVec3.North, thing))) &&
                    (i != 2 || (ShouldLinkWith(position + IntVec3.East, thing) &&
                                ShouldLinkWith(position + IntVec3.North, thing))) && (i != 3 ||
                               (ShouldLinkWith(position + IntVec3.East, thing) &&
                                ShouldLinkWith(position + IntVec3.South, thing))))
                {
                    Vector3 center = drawPos + GenAdj.DiagonalDirectionsAround[i].ToVector3().normalized * Graphic_LinkedCornerFiller.CoverOffsetDist + Altitudes.AltIncVect + new Vector3(0f, 0f, 0.09f);
                    Vector2 size = new Vector2(0.5f, 0.5f);
                    if (!intVec.InBounds(thing.Map))
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

                        if (intVec.x == thing.Map.Size.x)
                        {
                            center.x += 1f;
                            size.x *= 5f;
                        }

                        if (intVec.z == thing.Map.Size.z)
                        {
                            center.z += 1f;
                            size.y *= 5f;
                        }
                    }
                    Printer_Plane.PrintPlane(layer, center, size, this.LinkedDrawMatFrom(thing, thing.Position), extraRotation, false, Graphic_LinkedCornerFiller.CornerFillUVs, null, 0.01f, 0f);
                }
            }
        }

        public override void Print(SectionLayer layer, Thing thing, float extraRotation)
        {
            Print(layer, thing, extraRotation);
        }

    }
}
