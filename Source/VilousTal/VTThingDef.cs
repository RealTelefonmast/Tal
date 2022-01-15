using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using Verse;

namespace VilousTal
{
    public abstract class TaggedResource<T>
    {
        protected T resource;

        public string resourceTag;
        public string resourceData;

        public T Resource => resource;

        protected virtual T GetResource()
        {
            throw new NotImplementedException();
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            resourceTag = xmlRoot.Name;
            resourceData = xmlRoot.FirstChild.Value;
            GetResource();
        }
    }

    public class TextureResource : TaggedResource<Texture2D>
    {
        protected override Texture2D GetResource()
        {
            return resource = ContentFinder<Texture2D>.Get(resourceData);
        }
    }

    public class VTGraphic
    {
        private Thing parent;
        private VTThingDef parentDef;
        private Graphic graphicInt;
        private GraphicData data;
        private float altitude;
        private int index = 0;

        public VTGraphic(Thing parent, GraphicData data, int index)
        {
            this.parent = parent;
            this.parentDef = parent.def as VTThingDef;
            this.data = data;
            this.index = index;

            altitude = parentDef.altitudeLayer.AltitudeFor((index + 1) / 2f);
        }

        public void SetNewColor(Color newColor)
        {
            graphicInt = Graphic.GetColoredVersion(graphicInt.Shader, newColor, graphicInt.ColorTwo);
        }

        public Graphic Graphic
        {
            get
            {
                if (graphicInt == null)
                {
                    if (parentDef.graphicData.Graphic is Graphic_Random random)
                    {
                        var path = data.texPath;
                        var parentName = random.SubGraphicFor(parent).path.Split('/').Last();
                        var lastPart = path.Split('/').Last();
                        path += "/" + lastPart;
                        path += "_" + parentName.Split('_').Last();
                        graphicInt = GraphicDatabase.Get(typeof(Graphic_Single), path, data.shaderType.Shader, data.drawSize, data.color, data.colorTwo);
                    }
                    else if (data != null)
                    {
                        graphicInt = data.GraphicColoredFor(parent);
                    }

                    /*
                    if (!data.textureParams.NullOrEmpty())
                    {
                        foreach (var param in data.textureParams)
                        {
                            param.ApplyOn(graphicInt);
                        }
                    }
                    */
                }
                return graphicInt;
            }
        }

        public void Draw()
        {

        }

        public void Print(SectionLayer layer, Thing thing, float extraRotation)
        {
            if (Graphic is VTGraphic_LinkedSelf linkedSelf)
            {
                linkedSelf.Print(layer, thing, thing.DrawPos, extraRotation, altitude);
            }
        }
    }

    public class VTThingDef : ThingDef
    {
        public List<TextureResource> textures;
        public List<GraphicData> extraGraphics;

        public Texture2D GetTextureResource(string tag)
        {
            for (var i = 0; i < textures.Count; i++)
            {
                var texture = textures[i];
                if (texture.resourceTag.Equals(tag))
                    return texture.Resource;
            }
            return null;
        }
    }
}
