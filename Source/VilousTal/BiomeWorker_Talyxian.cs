using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VilousTal
{
    public class BiomeWorker_Talyxian : BiomeWorker
    {
        public override float GetScore(Tile tile, int tileID)
        {
			if (tile.WaterCovered)
            {
                return -100f;
            }
            if (tile.temperature < -10f)
            {
                return 0f;
            }
            if (tile.rainfall < 600f)
            {
                return 0f;
            }

            var val = 15f + (tile.temperature - 7f) + (tile.rainfall - 600f) / 180f;
            return Rand.Chance(0.5f) ? val + 999 : val;
		}
    }
}
