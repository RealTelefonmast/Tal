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
        protected VTGraphic[] graphics;

        public bool HasExtraGraphics => !def.extraGraphics.NullOrEmpty();

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.def = (VTThingDef)base.def;
            if (HasExtraGraphics)
            {
                graphics = new VTGraphic[def.extraGraphics.Count];
                for (var i = 0; i < def.extraGraphics.Count; i++)
                {
                    var extraGraphic = def.extraGraphics[i];
                    graphics[i] = new VTGraphic(this, extraGraphic, i);
                }
            }
        }

        public VTGraphic ExtraGraphic(int index)
        {
            //if (!HasExtraGraphics || graphics.Length-1 < index) return BaseContent.BadGraphic;
            return graphics[index];
        }
    }
}
