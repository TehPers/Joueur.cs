// A Tile in the game that makes up the 2D map grid.

// DO NOT MODIFY THIS FILE
// Never try to directly create an instance of this class, or modify its member variables.
// Instead, you should only be reading its variables and calling its functions.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// <<-- Creer-Merge: usings -->> - Code you add between this comment and the end comment will be preserved between Creer re-runs.
// you can add additional using(s) here
// <<-- /Creer-Merge: usings -->>

namespace Joueur.cs.Games.Catastrophe
{
    /// <summary>
    /// A Tile in the game that makes up the 2D map grid.
    /// </summary>
    public class Tile : Catastrophe.GameObject
    {
        #region Properties
        /// <summary>
        /// The number of food dropped on this Tile.
        /// </summary>
        public int Food { get; protected set; }

        /// <summary>
        /// The amount of food that can be harvested from this Tile per turn.
        /// </summary>
        public int HarvestRate { get; protected set; }

        /// <summary>
        /// The number of materials dropped on this Tile.
        /// </summary>
        public int Materials { get; protected set; }

        /// <summary>
        /// The Structure on this Tile if present, otherwise null.
        /// </summary>
        public Catastrophe.Structure Structure { get; protected set; }

        /// <summary>
        /// The Tile to the 'East' of this one (x+1, y). Null if out of bounds of the map.
        /// </summary>
        public Catastrophe.Tile TileEast { get; protected set; }

        /// <summary>
        /// The Tile to the 'North' of this one (x, y-1). Null if out of bounds of the map.
        /// </summary>
        public Catastrophe.Tile TileNorth { get; protected set; }

        /// <summary>
        /// The Tile to the 'South' of this one (x, y+1). Null if out of bounds of the map.
        /// </summary>
        public Catastrophe.Tile TileSouth { get; protected set; }

        /// <summary>
        /// The Tile to the 'West' of this one (x-1, y). Null if out of bounds of the map.
        /// </summary>
        public Catastrophe.Tile TileWest { get; protected set; }

        /// <summary>
        /// The amount of turns before this resource can be harvested.
        /// </summary>
        public int TurnsToHarvest { get; protected set; }

        /// <summary>
        /// The Unit on this Tile if present, otherwise null.
        /// </summary>
        public Catastrophe.Unit Unit { get; protected set; }

        /// <summary>
        /// The x (horizontal) position of this Tile.
        /// </summary>
        public int X { get; protected set; }

        /// <summary>
        /// The y (vertical) position of this Tile.
        /// </summary>
        public int Y { get; protected set; }


        // <<-- Creer-Merge: properties -->> - Code you add between this comment and the end comment will be preserved between Creer re-runs.
        // you can add additional properties(s) here. None of them will be tracked or updated by the server.
        // <<-- /Creer-Merge: properties -->>
        #endregion


        #region Methods
        /// <summary>
        /// Creates a new instance of Tile. Used during game initialization, do not call directly.
        /// </summary>
        protected Tile() : base()
        {
        }


        /// <summary>
        /// Gets the neighbors of this Tile
        /// </summary>
        /// <returns>The neighboring (adjacent) Tiles to this tile</returns>
        public List<Tile> GetNeighbors()
        {
            var list = new List<Tile>();

            if (this.TileNorth != null)
            {
                list.Add(this.TileNorth);
            }

            if (this.TileEast != null)
            {
                list.Add(this.TileEast);
            }

            if (this.TileSouth != null)
            {
                list.Add(this.TileSouth);
            }

            if (this.TileWest != null)
            {
                list.Add(this.TileWest);
            }

            return list;
        }

        /// <summary>
        /// Checks if a Tile is pathable to units
        /// </summary>
        /// <returns>True if pathable, false otherwise</returns>
        public bool IsPathable()
        {
            // <<-- Creer-Merge: is_pathable_builtin -->> - Code you add between this comment and the end comment will be preserved between Creer re-runs.
            return this.Unit == null && this.IsPathableIgnoringUnits();
            // <<-- /Creer-Merge: is_pathable_builtin -->>
        }

        /// <summary>
        /// Checks if this Tile has a specific neighboring Tile
        /// </summary>
        /// <param name="tile">Tile to check against</param>
        /// <returns>true if the tile is a neighbor of this Tile, false otherwise</returns>
        public bool HasNeighbor(Tile tile)
        {
            if (tile == null)
            {
                return false;
            }

            return this.TileNorth == tile || this.TileEast == tile || this.TileSouth == tile || this.TileWest == tile;
        }

        // <<-- Creer-Merge: methods -->> - Code you add between this comment and the end comment will be preserved between Creer re-runs.
        public bool InRange(string structure, Player owner)
        {
            return owner.Structures.Any(s => s.Tile != null && s.Type == structure && Math.Abs(this.X - s.Tile.X) <= s.EffectRadius && Math.Abs(this.Y - s.Tile.Y) <= s.EffectRadius);
        }

        public bool InRange(Unit unit, int radius)
        {
            return Math.Abs(this.X - unit.Tile.X) <= radius && Math.Abs(this.Y - unit.Tile.Y) <= radius;
        }

        public bool IsPathableIgnoringUnits()
        {
            return this.Structure == null || this.Structure.Type == "shelter" || this.Structure.Type == "road";
        }

        public int Manhattan(Tile other)
        {
            return Math.Abs(this.X - other.X) + Math.Abs(this.Y - other.Y);
        }

        public float Euclidean(Tile other)
        {
            int dx = this.X - other.X;
            int dy = this.Y - other.Y;
            return (float) Math.Sqrt(dx * dx + dy * dy);
        }
        // <<-- /Creer-Merge: methods -->>
        #endregion
    }
}
