using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace VilousTal
{
    public class VTGraphic_LinkedCornerFiller : Graphic_Linked
    {
        private Material filledMat;
        private Material[] filledSubMats = new Material[4];

        public override void Init(GraphicRequest req)
        {
            subGraphic = GraphicDatabase.Get(typeof(Graphic_Single), req.path, req.shader, req.drawSize, req.color, req.colorTwo, req.graphicData, req.shaderParameters, null);
            data = req.graphicData;
            path = req.path;
            color = req.color;
            colorTwo = req.colorTwo;

            MaterialRequest req2 = default(MaterialRequest);
            req2.mainTex = ContentFinder<Texture2D>.Get($"{req.path}_FC", true);
            req2.shader = req.shader;
            req2.color = req.color;
            req2.colorTwo = req.colorTwo;
            req2.renderQueue = req.renderQueue;
            req2.shaderParameters = req.shaderParameters;
            filledMat = MaterialPool.MatFrom(req2);

            Vector2 mainTextureScale = new Vector2(0.375f, 0.375f);
            for (int i = 0; i < 4; i++)
            {
                float x = (float)(i % 2) * 0.5f + 0.0625f;
                float y = (float)(i / 2) * 0.5f + 0.0625f;
                Vector2 mainTextureOffset = new Vector2(x, y);
                Material material = MaterialAllocator.Create(filledMat);
                material.name = filledMat.name + "_ASMF" + i;
                material.mainTextureScale = mainTextureScale;
                material.mainTextureOffset = mainTextureOffset;
                this.filledSubMats[i] = material;
            }

            var copy = new Material[4] { filledSubMats[3], filledSubMats[1], filledSubMats[0], filledSubMats[2] };
            filledSubMats = copy;
        }

        public void Print(SectionLayer layer, Thing thing, float extraRotation, AltitudeLayer? altitudeOverride = null, bool drawExtra = true)
        {
            var drawPos = thing.DrawPos;
            if (altitudeOverride != null)
                drawPos += new Vector3(0, altitudeOverride.Value.AltitudeFor(), 0);

            /*
            if (!IsSurrounded(thing) && drawExtra)
            {
                Material mat = this.LinkedDrawMatFrom(thing, thing.Position);
                Printer_Plane.PrintPlane(layer, drawPos, new Vector2(1f, 1f), mat, extraRotation, false, null, null, 0.01f, 0f);
            }
            */
            if (Building_AquaPlanter.debug_DrawBasic)
            {
                Material mat = this.LinkedDrawMatFrom(thing, thing.Position);
                Printer_Plane.PrintPlane(layer, drawPos, new Vector2(1.0f, 1.0f), mat, extraRotation, false, null, null, 0.01f, 0f);
            }

            if (Building_AquaPlanter.debug_DrawExtra)
            {
                //if (!drawExtra) return;
                IntVec3 position = thing.Position;
                for (int i = 0; i < 4; i++)
                {
                    //SW, NW, NE, SE 
                    IntVec3 intVec = thing.Position + GenAdj.DiagonalDirectionsAround[i];
                    if (ShouldLinkWith(intVec, thing) &&
                        (i != 0 || (ShouldLinkWith(position + IntVec3.West, thing) &&
                                    ShouldLinkWith(position + IntVec3.South, thing))) && //
                        (i != 1 || (ShouldLinkWith(position + IntVec3.West, thing) &&
                                    ShouldLinkWith(position + IntVec3.North, thing))) && //
                        (i != 2 || (ShouldLinkWith(position + IntVec3.East, thing) &&
                                    ShouldLinkWith(position + IntVec3.North, thing))) && //
                        (i != 3 || (ShouldLinkWith(position + IntVec3.East, thing) &&
                                    ShouldLinkWith(position + IntVec3.South, thing)))) //
                    {
                        Vector3 center = drawPos +
                                         GenAdj.DiagonalDirectionsAround[i].ToVector3().normalized *
                                         Graphic_LinkedCornerFiller.CoverOffsetDist + Altitudes.AltIncVect +
                                         new Vector3(0f, 0f, 0.09f);
                        Vector2 size = new Vector2(0.5f, 0.5f);
                        /*
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
                        */
                        if (Find.Selector.IsSelected(thing))
                            Log.Message($"Printing index direction: {i}");
                        Printer_Plane.PrintPlane(layer, drawPos, new Vector2(1.0f, 1.0f), filledSubMats[i], extraRotation);
                        //Printer_Plane.PrintPlane(layer, center, size, this.LinkedDrawMatFrom(thing, thing.Position), extraRotation, false, Graphic_LinkedCornerFiller.CornerFillUVs, null, 0.01f, 0f);
                    }
                }
            }
        }

        public override void Print(SectionLayer layer, Thing thing, float extraRotation)
        {
            Print(layer, thing, extraRotation);
        }
    }
}
