using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VilousTal
{
    public class TalConfigDef : Def
    {
        public SergalProperties sergProps;


        public static TalConfigDef Def => TalDefOf.MainConfigDef;
    }
}
