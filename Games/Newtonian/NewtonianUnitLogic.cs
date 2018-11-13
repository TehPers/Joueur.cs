using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Joueur.cs.Conflux.Convenience;
using Joueur.cs.Helpers;

namespace Joueur.cs.Games.Newtonian {
    public class NewtonianUnitLogic : UnitLogic<AI, Unit> {
        public Player Player => this.AI.Player;
        public Game Game => this.AI.Game;
        public Player Opponent => this.Player.Opponent;

        public NewtonianUnitLogic(AI ai) : base(ai) { }

#if DEBUG
        public bool ShowDebugInfo(Unit unit) {
            unit.Log(unit.Job.Title.FirstOrDefault().ToString().ToUpper());
            return false;
        }
#else
        public bool ShowDebugInfo(Unit unit) {
            return false;
        }
#endif

        public bool CheckState(Unit unit) {
            // Check if healed fully while healing
            if (unit.Healing && unit.Health >= unit.Job.Health) {
                unit.Healing = false;
            }

            // Don't do logic for stunned units
            if (unit.StunTime > 0) {
                return true;
            }

            /*
            // Assign this unit to a machine
            if (unit.TargetMachine == null) {
                // Get all machines without a unit of this job type assigned to it ordered by the number of units assigned to it descending
                IOrderedEnumerable<Machine> possibleTargets = from machine in this.Game.Machines
                                                              where this.Player.Units.FirstOrDefault(u => u.Job.Title == unit.Job.Title && u.TargetMachine == machine) == null
                                                              orderby this.Player.Units.Count(u => u.TargetMachine == machine) descending
                                                              select machine;

                // Grab the first one if any
                unit.TargetMachine = possibleTargets.FirstOrDefault();

                if (unit.TargetMachine != null) {
                    Logger.Log($"{unit} assigned to {unit.TargetMachine} at ({unit.TargetMachine.Tile.X}, {unit.TargetMachine.Tile.Y})");
                }
            }
            */

            return false;
        }

        public bool HealIfNeeded(Unit unit) {
            if (!unit.Healing && unit.Health > unit.Job.Health / 2) {
                return false;
            }

            // Get valid targets
            HashSet<Tile> targets = this.Game.Tiles.Where(t => t.IsPathable(this.Player) && t.Owner == this.Player).ToHashSet();

            // If not on a target
            if (!targets.Contains(unit.Tile) && !unit.Acted) {
                // Find a path to the nearest target
                Queue<Pathfinder.Node<Tile>> path = new Queue<Pathfinder.Node<Tile>>(Pathfinder.FindPath(unit.Tile.Yield(), targets, this.GetNeighbors, this.GetCostFunc(unit)));
                if (!path.Any())
                    return false;

                // First node is the unit's tile
                path.Dequeue();

                // Move along the path
                while (unit.Moves > 0 && path.Any()) {
                    unit.Move(path.Dequeue().Value);
                }
            }

            // Unit should not perform anymore actions
            return true;
        }

        public bool WorkMachine(Unit unit) {
            if (unit.Acted) {
                return false;
            }

            // Get valid targets
            HashSet<Machine> workableMachines = (from machine in this.Game.Machines
                                                 let progress = machine.OreType == "blueium" ? machine.Tile.BlueiumOre : machine.Tile.RediumOre
                                                 where progress >= machine.RefineInput
                                                       && !machine.Tile.GetNeighbors().Any(t => t.Unit != null && t.Unit.Owner == this.Player && t.Unit.Job.Title == "physicist")
                                                 select machine).ToHashSet();
            HashSet<Tile> targets = (from machine in workableMachines
                                     from neighbor in machine.Tile.GetNeighbors()
                                     where neighbor.IsPathable(this.Player)
                                     select neighbor).ToHashSet();

            // If not on a target
            if (!targets.Contains(unit.Tile) && !unit.Acted) {
                // Find a path to the nearest target
                Queue<Pathfinder.Node<Tile>> path = new Queue<Pathfinder.Node<Tile>>(Pathfinder.FindPath(unit.Tile.Yield(), targets, this.GetNeighbors, this.GetCostFunc(unit)));
                if (!path.Any())
                    return false;

                // First node is the unit's tile
                path.Dequeue();

                // Move along the path
                while (unit.Moves > 0 && path.Any()) {
                    unit.Move(path.Dequeue().Value);
                }
            }

            // Work machine if near one
            Tile target = unit.Tile.GetNeighbors().FirstOrDefault(t => t.Machine != null && workableMachines.Contains(t.Machine));
            if (target != null) {
                unit.Act(target);
            }

            return false;
        }

        public bool AttackNeighbors(Unit unit) {
            if (unit.Acted || unit.Moves < unit.Job.Moves) {
                return false;
            }

            // Attack if near an enemy for some reason
            Unit target = unit.Tile.GetNeighbors().Select(t => t.Unit).FirstOrDefault(u => u?.Owner == this.Opponent);
            if (target != null) {
                if (target.StunTime <= 0 && target.StunImmune <= 0 && unit.Job.GetStunnableJob() == target.Job.Title) {
                    unit.Act(target.Tile);
                }

                unit.Attack(target.Tile);
            }

            return false;
        }

        public bool StunEnemies(Unit unit) {
            if (unit.Acted) {
                return false;
            }

            // Get valid targets
            HashSet<Tile> targets = (from enemy in this.Opponent.Units
                                     where enemy.Tile != null && unit.Job.GetStunnableJob() == enemy.Job.Title
                                     from neighborTile in enemy.Tile.GetNeighbors()
                                     where neighborTile.IsPathable(this.Player)
                                     select neighborTile).ToHashSet();

            // If not on a target
            if (!targets.Contains(unit.Tile) && !unit.Acted) {
                // Find a path to the nearest target
                Queue<Pathfinder.Node<Tile>> path = new Queue<Pathfinder.Node<Tile>>(Pathfinder.FindPath(unit.Tile.Yield(), targets, this.GetNeighbors, this.GetCostFunc(unit)));
                if (!path.Any())
                    return false;

                // First node is the unit's tile
                path.Dequeue();

                // Move along the path
                while (unit.Moves > 0 && path.Any()) {
                    unit.Move(path.Dequeue().Value);
                }
            }

            Tile target = unit.Tile.GetNeighbors().FirstOrDefault(t => t.Unit?.Owner == this.Opponent && t.Unit?.Job.Title == unit.Job.GetStunnableJob());
            if (target != null) {
                unit.Act(target);
            }

            return false;
        }

        public bool DropRefined(Unit unit) {
            if (unit.Blueium <= 0 || unit.Redium <= 0) {
                return false;
            }

            // Get valid targets
            HashSet<Tile> targets = this.Player.GeneratorTiles.SelectMany(t => t.GetNeighbors()).Where(t => t.IsPathable(this.Player)).ToHashSet();

            // If not on a target
            if (!targets.Contains(unit.Tile) && !unit.Acted) {
                // Find a path to the nearest target
                Queue<Pathfinder.Node<Tile>> path = new Queue<Pathfinder.Node<Tile>>(Pathfinder.FindPath(unit.Tile.Yield(), targets, this.GetNeighbors, this.GetCostFunc(unit)));
                if (!path.Any())
                    return false;

                // First node is the unit's tile
                path.Dequeue();

                // Move along the path
                while (unit.Moves > 0 && path.Any()) {
                    unit.Move(path.Dequeue().Value);
                }
            }

            Tile target = unit.Tile.GetNeighbors().FirstOrDefault(t => this.Player.GeneratorTiles.Contains(t));
            if (target != null) {
                unit.Drop(target, 0, "blueium");
                unit.Drop(target, 0, "redium");
            }

            return false;
        }

        public Func<Unit, bool> DropOreFactory(string oreType, string outputName, Func<Unit, int> unitCount, Func<Tile, int> tileCount) {
            return unit => {
                if (unitCount(unit) <= 0) {
                    return false;
                }

                // Get valid targets
                HashSet<Tile> targets = (from machine in this.Game.Machines
                                         where machine.OreType == outputName && tileCount(machine.Tile) < machine.RefineInput
                                         from neighbor in machine.Tile.GetNeighbors()
                                         where neighbor.IsPathable(this.Player)
                                         select neighbor).ToHashSet();

                // If not on a target
                if (!targets.Contains(unit.Tile) && !unit.Acted) {
                    // Find a path to the nearest target
                    Queue<Pathfinder.Node<Tile>> path = new Queue<Pathfinder.Node<Tile>>(Pathfinder.FindPath(unit.Tile.Yield(), targets, this.GetNeighbors, this.GetCostFunc(unit)));
                    if (!path.Any())
                        return false;

                    // First node is the unit's tile
                    path.Dequeue();

                    // Move along the path
                    while (unit.Moves > 0 && path.Any()) {
                        unit.Move(path.Dequeue().Value);
                    }
                }

                // Drop off ore if near a target machine
                Tile target = unit.Tile.GetNeighbors().FirstOrDefault(t => t.Machine != null && t.Machine.OreType == outputName && tileCount(t) < t.Machine.RefineInput);
                if (target != null) {
                    unit.Drop(target, 0, oreType);
                }

                return false;
            };
        }

        public Func<Unit, bool> CollectFactory(IEnumerable<(string name, Func<Tile, int> tileCount)> resources) {
            (string name, Func<Tile, int> tileCount)[] resourceArray = resources.ToArray();
            return unit => {
                if (unit.CapacityRemaining() <= 0) {
                    return false;
                }

                // Get valid targets
                HashSet<Tile> targets = (from resource in resourceArray
                                         from tile in this.Game.Tiles
                                         where resource.tileCount(tile) > 0
                                         from neighbor in tile.GetNeighbors()
                                         where neighbor.IsPathable(this.Player)
                                         select neighbor).ToHashSet();

                // If not on a target
                if (!targets.Contains(unit.Tile) && !unit.Acted) {
                    // Find a path to the nearest target
                    Queue<Pathfinder.Node<Tile>> path = new Queue<Pathfinder.Node<Tile>>(Pathfinder.FindPath(unit.Tile.Yield(), targets, this.GetNeighbors, this.GetCostFunc(unit)));
                    if (!path.Any())
                        return false;

                    // First node is the unit's tile
                    path.Dequeue();

                    // Move along the path
                    while (unit.Moves > 0 && path.Any()) {
                        unit.Move(path.Dequeue().Value);
                    }
                }

                // Check if next to a target resource
                var target = (from resource in resourceArray
                              from tile in unit.Tile.GetNeighbors()
                              where resource.tileCount(tile) > 0
                              select new { Tile = tile, Resource = resource.name }).FirstOrDefault();

                // Pick it up if so
                if (target != null) {
                    unit.Pickup(target.Tile, 0, target.Resource);
                    //this.AI.UnitsToAct.Enqueue(unit);
                }

                return false;
            };
        }

        private IEnumerable<Tile> GetNeighbors(Tile tile) {
            return tile.GetNeighbors();
        }

        private Func<Tile, Pathfinder.Node<Tile>, float?> GetCostFunc(Unit unit) {
            return (tile, node) => {
                // Don't pathfind over non-pathable tiles
                if (!tile.IsPathable(this.Player))
                    return null;

                // Don't path through tiles that could result in this unit dying

                // Avoid enemy units if possible

                return 1;
            };
        }
    }
}
