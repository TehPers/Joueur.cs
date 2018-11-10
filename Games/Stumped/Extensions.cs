using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Joueur.cs.Games.Stumped {
    internal static class Extensions {
        internal static int MoveCost(this Beaver b, Tile target) => b.Tile.MoveCost(target);
        internal static int MoveCost(this Tile source, Tile target) {
            // Base movement cost
            int cost = 2;

            // Stream movement effect
            if (source.FlowDirection == "North") {
                if (source.TileNorth == target)
                    cost--;
                if (source.TileSouth == target)
                    cost++;
            } else if (source.FlowDirection == "East") {
                if (source.TileEast == target)
                    cost--;
                if (source.TileWest == target)
                    cost++;
            } else if (source.FlowDirection == "South") {
                if (source.TileSouth == target)
                    cost--;
                if (source.TileNorth == target)
                    cost++;
            } else if (source.FlowDirection == "West") {
                if (source.TileWest == target)
                    cost--;
                if (source.TileEast == target)
                    cost++;
            }

            return cost;
        }
    }
}
