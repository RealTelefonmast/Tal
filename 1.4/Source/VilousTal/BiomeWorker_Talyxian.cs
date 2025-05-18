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
            if (tile.temperature < 20)
            {
                return 0f;
            }

            //var val = 15f + (tile.temperature - 7f) + (tile.rainfall - 600f) / 180f;
            return 8 + tile.temperature + (tile.rainfall - 600) / 180;
		}
    }
}
