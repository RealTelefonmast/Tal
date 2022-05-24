using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;

namespace VilousTal
{
    public class BiomeWorker_Talyxian : BiomeWorker
    {
        public override float GetScore(Tile tile, int tileID)
        {
            return 1;
        }
    }
}
