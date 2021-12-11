using System;
using System.Collections.Generic;
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

    public class VTThingDef : ThingDef
    {
        public List<TextureResource> textures;
        public GraphicData extraGraphicData;

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
