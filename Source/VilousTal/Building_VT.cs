using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VilousTal
{
    public class Building_VT : Building
    {
        public new VTThingDef def;
        protected Graphic extraGraphicInt;

        public bool UsesExtraGraphic => def.extraGraphicData != null;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.def = (VTThingDef)base.def;
        }

        public Graphic ExtraGraphic
        {
            get
            {
                if(!UsesExtraGraphic) return BaseContent.BadGraphic;
                return extraGraphicInt ??= def.extraGraphicData.GraphicColoredFor(this);
            }
        }
    }
}
