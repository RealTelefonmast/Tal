using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using RimWorld;
using Verse;

namespace VilousTal
{
    public class SergCapacityOffset
    {
        public PawnCapacityDef capacity;
        public float offset;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "capacity", xmlRoot.Name, null, null);
            this.offset = ParseHelper.FromString<float>(xmlRoot.FirstChild.Value);
        }
    }

    public class SergalProperties
    {
        public List<SergCapacityOffset> capacityOffsets;
        public float meleeCDMultiplier = 1;
        public float canonBodySize = 1.0f;
        public float fanonBodySize = 1.2f;

        public float GetSizeUsed => true ? fanonBodySize : canonBodySize;

        public SergCapacityOffset GetCapModFor(PawnCapacityDef capacity)
        {
            return capacityOffsets.Find(c => c.capacity == capacity);
        }
    }
}
