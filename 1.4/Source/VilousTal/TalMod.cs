using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;

namespace VilousTal
{
    public class TalMod : Mod
    {
        private static TalMod instance;
        private static Harmony talHarmony;

        public static TalMod TalModRef => instance;
        public static Harmony VilousTal => talHarmony ?? new Harmony("Ammonite616.VilousTal");

        public TalMod(ModContentPack content) : base(content)
        {
            Log.Message("[Tal] - Init");
            instance = this;
            VilousTal.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
