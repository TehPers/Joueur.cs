using System;
using System.Collections.Generic;
using System.Linq;
using Joueur.cs.Helpers;

namespace Joueur.cs.Games.Catastrophe {
    public class UnitLogic {
        public AI AI { get; }
        public Player Player => this.AI.Player;
        public Game Game => this.AI.Game;
        public Player Opponent => this.Player.Opponent;
        public bool OpponentRushing => false; //this.Opponent.Units.Count(u => u.Job.Title == "soldier") > this.Player.Units.Count / 2;

        public List<Func<Unit, bool>> Tasks { get; }

        public UnitLogic(AI ai) {
            this.AI = ai;
            this.Tasks = new List<Func<Unit, bool>>();
        }

        public UnitLogic AddTask(Func<Unit, bool> task) {
            this.Tasks.Add(task);
            return this;
        }

        public UnitLogic AddTasks(params Func<Unit, bool>[] tasks) => this.AddTasks((IEnumerable<Func<Unit, bool>>) tasks);
        public UnitLogic AddTasks(IEnumerable<Func<Unit, bool>> tasks) {
            this.Tasks.AddRange(tasks);
            return this;
        }

        public bool ForceChangeJobs(Unit unit) {
            // Figure out which job to force change to
            string job = null;
            if (this.Player.Units.Count >= this.Player.Opponent.Units.Count * 2)
                job = "soldier"; // Full out assault
            else if (this.AI.CountUnits("missionary") == 0)
                job = "missionary"; // Make sure there's a missionary
            else if (this.OpponentRushing)
                job = "soldier";

            // Make sure a force change is necessary
            if (job == null || unit.Job.Title == job)
                return false;

            // Check if this unit can change jobs, and make sure it can eventually
            if (unit.Energy < 100)
                return this.Rest(unit);

            // Get an array of tiles in range of the cat
            HashSet<Tile> targets = this.Game.Tiles.Where(t => t.InRange(this.Player.Cat, 1) && t.IsPathable()).ToHashSet();

            // If not on a target already
            if (!targets.Contains(unit.Tile)) {
                // Find a path to the nearest target
                Queue<Pathfinder.Node<Tile>> path = new Queue<Pathfinder.Node<Tile>>(Pathfinder.FindPath(new[] { unit.Tile }, targets, this.GetNeighbors, this.GetCost));
                if (!path.Any())
                    return false;

                // First node is the unit's tile
                path.Dequeue();

                // Move along the path
                while (unit.Moves > 0 && path.Any()) {
                    Pathfinder.Node<Tile> cur = path.Dequeue();
                    unit.Move(cur.Value);
                }
            }

            // Check if on a target
            if (!unit.Acted && targets.Contains(unit.Tile)) {
                unit.ChangeJob(job);
            }

            return true;
        }

        public bool PickupMaterials(Unit unit) {
            // Check if this unit can pickup materials
            if (unit.Resting || unit.Energy <= 0 || unit.CarryLeft <= 0)
                return false;

            // Get an array of tiles containing materials on the ground
            HashSet<Tile> targets = this.Game.Tiles.Where(t => t.Materials > 0).ToHashSet();

            // If not next to or on a target
            if (!unit.Tile.GetNeighbors().Concat(new[] { unit.Tile }).Intersect(targets).Any()) {
                // Find a path to the nearest target
                Queue<Pathfinder.Node<Tile>> path = new Queue<Pathfinder.Node<Tile>>(Pathfinder.FindPath(new[] { unit.Tile }, targets, this.GetNeighbors, this.GetCost));
                if (!path.Any())
                    return false;

                // First node is the unit's tile
                path.Dequeue();

                // Move along the path, but don't move onto the target
                while (unit.Moves > 0 && path.Count > 1) {
                    Pathfinder.Node<Tile> cur = path.Dequeue();
                    unit.Move(cur.Value);
                }
            }

            // Check if next to or on a target
            Tile target = unit.Tile.GetNeighbors().Concat(new[] { unit.Tile }).Intersect(targets).FirstOrDefault();
            if (target != null) {
                // Pickup materials
                unit.Pickup(target, "materials");
                return false;
            }

            return true;
        }

        public bool Deconstruct(Unit unit) {
            // Check if this unit can gather materials
            if (unit.Resting || unit.Energy <= unit.Job.ActionCost || unit.CarryLeft <= 0)
                return false;

            // Get an array of tiles containing structures to deconstruct
            HashSet<Tile> targets = this.Game.Tiles.Where(t => t.Structure != null && t.Structure.Type != "road" && t.Structure.Owner != this.Player).ToHashSet();

            // If not next to a target
            if (!unit.Tile.GetNeighbors().Intersect(targets).Any()) {
                // Find a path to the nearest target
                Queue<Pathfinder.Node<Tile>> path = new Queue<Pathfinder.Node<Tile>>(Pathfinder.FindPath(new[] { unit.Tile }, targets, this.GetNeighbors, this.GetCost));
                if (!path.Any())
                    return false;

                // First node is the unit's tile
                path.Dequeue();

                // Move along the path, but don't move onto the target
                while (unit.Moves > 0 && path.Count > 1) {
                    Pathfinder.Node<Tile> cur = path.Dequeue();
                    unit.Move(cur.Value);
                }
            }

            // Check if next to a target
            Tile target = unit.Tile.GetNeighbors().Intersect(targets).FirstOrDefault();
            if (!unit.Acted && target != null) {
                // Deconstruct
                unit.Deconstruct(target);
            } else if (target == null) {
                return true;
            }

            return false;
        }

        public bool ConstructShelter(Unit unit) {
            // Check if this unit can construct a shelter
            int shelters = this.Game.Tiles.Count(t => t.Structure != null && t.Structure.Type == "shelter" && t.Structure.Owner == this.Player);
            int monuments = this.Game.Tiles.Count(t => t.Structure != null && t.Structure.Type == "monuments" && t.Structure.Owner == this.Player);
            if (unit.Resting || unit.Energy <= unit.Job.ActionCost || unit.Materials < unit.Job.CarryLimit) // || shelters > monuments)
                return false;

            // Sort all the tiles a shelter can be constructed on by how good of a spot it is. Lower score is better
            IEnumerable<Tile> shelterSpots = (from t in this.Game.Tiles
                                              where t.Structure == null && t.Unit == null && t.HarvestRate == 0
                                              orderby this.GetShelterScore(t)
                                              select t).Take(3);

            HashSet<Tile> targets = shelterSpots.ToHashSet();

            //foreach (Tile t in targets)
            //    t.Log("(shelter)");

            // If not next to a target
            if (!unit.Tile.GetNeighbors().Intersect(targets).Any()) {
                // Find a path to the nearest target
                Queue<Pathfinder.Node<Tile>> path = new Queue<Pathfinder.Node<Tile>>(Pathfinder.FindPath(new[] { unit.Tile }, targets, this.GetNeighbors, this.GetCost));
                if (!path.Any())
                    return false;

                // First node is the unit's tile
                path.Dequeue();

                // Move along the path, but don't move onto the target
                while (unit.Moves > 0 && path.Count > 1) {
                    Pathfinder.Node<Tile> cur = path.Dequeue();
                    unit.Move(cur.Value);
                }
            }

            // Check if next to a target
            Tile target = unit.Tile.GetNeighbors().Intersect(targets).FirstOrDefault();
            if (!unit.Acted && target != null) {
                // Construct a shelter
                unit.Drop(target, "materials", this.Game.ShelterMaterials - target.Materials);
                if (target.Materials >= this.Game.ShelterMaterials)
                    unit.Construct(target, "shelter");
            } else if (target == null) {
                return true;
            }

            return false;
        }

        public bool ConstructMonument(Unit unit) {
            // Check if this unit can construct a monument
            if (unit.Resting || unit.Energy <= unit.Job.ActionCost || unit.Materials < unit.Job.CarryLimit)
                return false;

            // Sort all the tiles a monument can be constructed on by how good of a spot it is. Lower score is better
            IEnumerable<Tile> monumentSpots = this.Game.Tiles.Where(t => t.Materials > 0).ToHashSet();

            if (!monumentSpots.Any()) {
                monumentSpots = (from t in this.Game.Tiles
                                 where t.Structure == null && t.Unit == null && t.HarvestRate == 0
                                 orderby this.GetMonumentScore(t)
                                 select t).Take(3);
            }

            HashSet<Tile> targets = monumentSpots.ToHashSet();

            foreach (Tile t in targets)
                t.Log("(monument)");

            // If not next to a target
            if (!unit.Tile.GetNeighbors().Intersect(targets).Any()) {
                // Find a path to the nearest target
                Queue<Pathfinder.Node<Tile>> path = new Queue<Pathfinder.Node<Tile>>(Pathfinder.FindPath(new[] { unit.Tile }, targets, this.GetNeighbors, this.GetCost));
                if (!path.Any())
                    return false;

                // First node is the unit's tile
                path.Dequeue();

                // Move along the path, but don't move onto the target
                while (unit.Moves > 0 && path.Count > 1) {
                    Pathfinder.Node<Tile> cur = path.Dequeue();
                    unit.Move(cur.Value);
                }
            }

            // Check if next to a target
            Tile target = unit.Tile.GetNeighbors().Intersect(targets).FirstOrDefault();
            if (!unit.Acted && target != null) {
                // Construct a shelter
                unit.Drop(target, "materials", this.Game.MonumentMaterials - target.Materials);
                if (target.Materials >= this.Game.MonumentMaterials)
                    unit.Construct(target, "monument");
            } else if (target == null) {
                return true;
            }

            return false;
        }

        public bool ConstructWall(Unit unit) {
            // TODO
            return false;
        }

        public bool MoveToRoadShelter(Unit unit) {
            // Check if this unit can move
            if (unit.Resting || unit.Moves == 0)
                return false;

            // Get the shelter closest to the road
            HashSet<Tile> targets = (from s in this.Player.Structures
                                     where s.Type == "shelter" && s.Tile != null
                                     orderby Math.Abs(this.Game.MapHeight / 2F - s.Tile.Y)
                                     select s.Tile)
                              .SelectMany(t => t.GetNeighbors())
                              .Where(t => (t.Unit == unit || t.IsPathable()) && t.Structure == null)
                              .Take(1)
                              .ToHashSet();

            // If not on a target already
            if (!targets.Contains(unit.Tile)) {
                // Find a path to the nearest target
                Queue<Pathfinder.Node<Tile>> path = new Queue<Pathfinder.Node<Tile>>(Pathfinder.FindPath(new[] { unit.Tile }, targets, this.GetNeighbors, this.GetCost));
                if (!path.Any())
                    return false;

                // First node is the unit's tile
                path.Dequeue();

                // Move along the path
                while (unit.Moves > 0 && path.Any()) {
                    Pathfinder.Node<Tile> cur = path.Dequeue();
                    unit.Move(cur.Value);
                }
            }

            return false;
        }

        public bool CheckState(Unit unit) {
            // Check if the unit still needs to rest
            if (unit.Energy >= 100) {
                unit.Resting = false;

                // If done resting, try to store food
                if (this.StoreFood(unit)) {
                    return true;
                }
            }

            // Make sure to keep going through the possible actions for this unit
            return false;
        }

        public bool StoreFood(Unit unit) {
            // Check if this unit can store food
            if (unit.Resting || unit.Food == 0)
                return false;

            // Get an array of tiles containing friendly shelters
            HashSet<Tile> targets = this.Game.Tiles.Where(t => t.Structure?.Owner == this.Player && t.Structure?.Type == "shelter").ToHashSet();

            // If not next to a target
            if (!unit.Tile.GetNeighbors().Concat(new[] { unit.Tile }).Intersect(targets).Any()) {
                // Find a path to the nearest target
                Queue<Pathfinder.Node<Tile>> path = new Queue<Pathfinder.Node<Tile>>(Pathfinder.FindPath(new[] { unit.Tile }, targets, this.GetNeighbors, this.GetCost));
                if (!path.Any())
                    return false;

                // First node is the unit's tile
                path.Dequeue();

                // Move along the path, but don't move onto the target
                while (unit.Moves > 0 && path.Count > 1) {
                    Pathfinder.Node<Tile> cur = path.Dequeue();
                    unit.Move(cur.Value);
                }
            }

            // Check if next to or on a target
            Tile target = unit.Tile.GetNeighbors().Concat(new[] { unit.Tile }).Intersect(targets).FirstOrDefault();
            if (target != null) {
                // Drop food
                unit.Drop(target, "food");
                return false;
            }

            return true;
        }

        public bool Harvest(Unit unit) {
            // Check if this unit can gather food
            if (unit.Resting || unit.Energy <= unit.Job.ActionCost || unit.CarryLeft <= 0)
                return false;

            // Get an array of food tiles
            HashSet<Tile> targets = this.Game.Tiles.Where(t => (t.HarvestRate > 0 && t.TurnsToHarvest == 0) || (t.Structure?.Type == "shelter" && t.Structure?.Owner == this.Player.Opponent)).ToHashSet();

            // If not next to or on a target already
            if (!unit.Tile.GetNeighbors().Concat(new[] { unit.Tile }).Intersect(targets).Any()) {
                // Find a path to the nearest target
                Queue<Pathfinder.Node<Tile>> path = new Queue<Pathfinder.Node<Tile>>(Pathfinder.FindPath(new[] { unit.Tile }, targets, this.GetNeighbors, this.GetCost));
                if (!path.Any())
                    return false;

                // First node is the unit's tile
                path.Dequeue();

                // Move along the path, but don't move onto the target
                while (unit.Moves > 0 && path.Count > 1) {
                    Pathfinder.Node<Tile> cur = path.Dequeue();
                    unit.Move(cur.Value);
                }
            }

            // Check if next to or on a target
            Tile target = unit.Tile.GetNeighbors().Concat(new[] { unit.Tile }).Intersect(targets).FirstOrDefault();
            if (!unit.Acted && target != null) {
                // Harvest
                unit.Harvest(target);
            } else if (target == null) {
                return true;
            }

            return false;
        }

        public bool PickupFood(Unit unit) {
            // Check if this unit can pickup food
            if (unit.Resting || unit.Energy <= 0 || unit.CarryLeft <= 0)
                return false;

            // Get an array of tiles containing food on the ground
            HashSet<Tile> targets = this.Game.Tiles.Where(t => t.Food > 0).ToHashSet();

            // If not next to or on a target
            if (!unit.Tile.GetNeighbors().Concat(new[] { unit.Tile }).Intersect(targets).Any()) {
                // Find a path to the nearest target
                Queue<Pathfinder.Node<Tile>> path = new Queue<Pathfinder.Node<Tile>>(Pathfinder.FindPath(new[] { unit.Tile }, targets, this.GetNeighbors, this.GetCost));
                if (!path.Any())
                    return false;

                // First node is the unit's tile
                path.Dequeue();

                // Move along the path, but don't move onto the target
                while (unit.Moves > 0 && path.Count > 1) {
                    Pathfinder.Node<Tile> cur = path.Dequeue();
                    unit.Move(cur.Value);
                }
            }

            // Check if next to or on a target
            Tile target = unit.Tile.GetNeighbors().Concat(new[] { unit.Tile }).Intersect(targets).FirstOrDefault();
            if (target != null) {
                // Pickup food
                unit.Pickup(target, "food");
                return false;
            }

            return true;
        }

        public bool Convert(Unit unit) {
            // Check if this unit can convert
            if (unit.Resting || unit.Energy <= unit.Job.ActionCost)
                return false;

            // Get an array of tiles containing neutral fresh humans
            HashSet<Tile> targets = this.Game.Tiles.Where(t => t.Unit != null && t.Unit.Owner == null).ToHashSet();

            // If not next to a target
            if (!unit.Tile.GetNeighbors().Intersect(targets).Any()) {
                // Find a path to the nearest target
                Queue<Pathfinder.Node<Tile>> path = new Queue<Pathfinder.Node<Tile>>(Pathfinder.FindPath(new[] { unit.Tile }, targets, this.GetNeighbors, this.GetCost));
                if (!path.Any())
                    return false;

                // First node is the unit's tile
                path.Dequeue();

                // Move along the path, but don't move onto the target
                while (unit.Moves > 0 && path.Count > 1) {
                    Pathfinder.Node<Tile> cur = path.Dequeue();
                    unit.Move(cur.Value);
                }
            }

            // Check if next to a target
            Tile target = unit.Tile.GetNeighbors().Intersect(targets).FirstOrDefault();
            if (!unit.Acted && target != null) {
                // Convert
                unit.Convert(target);
            } else if (target == null) {
                return true;
            }

            return false;
        }

        public bool ConvertRoad(Unit unit) {
            // Check if this unit can convert
            if (unit.Resting || unit.Energy <= unit.Job.ActionCost)
                return false;

            // Get an array of pathable road tiles
            HashSet<Tile> targets = this.Game.Tiles.Where(t => (t == unit.Tile || t.IsPathable()) && t.Structure?.Type == "road").ToHashSet();

            // If not on a target already
            if (!targets.Contains(unit.Tile)) {
                // Find a path to the nearest target
                Queue<Pathfinder.Node<Tile>> path = new Queue<Pathfinder.Node<Tile>>(Pathfinder.FindPath(new[] { unit.Tile }, targets, this.GetNeighbors, this.GetCost));
                if (!path.Any())
                    return false;

                // First node is the unit's tile
                path.Dequeue();

                // Move along the path
                while (unit.Moves > 0 && path.Any()) {
                    Pathfinder.Node<Tile> cur = path.Dequeue();
                    unit.Move(cur.Value);
                }
            }

            // Try to convert a unit
            Tile target = unit.Tile.GetNeighbors().FirstOrDefault(t => t.Unit != null && t.Unit.Owner == null);
            if (!unit.Acted && target != null) {
                unit.Convert(target);
                this.AI.UnitsToAct.Enqueue(target.Unit);
            } else if (target == null) {
                return true;
            }

            return false;
        }

        public bool ConvertDefeated(Unit unit) {
            // Check if this unit can convert
            if (unit.Resting || unit.Energy <= unit.Job.ActionCost)
                return false;

            // Get an array of tiles containing units to convert
            HashSet<Tile> targets = this.Game.Tiles.Where(t => t.Unit != null && t.Unit.Owner == null && t.Unit.MovementTarget == null).ToHashSet();

            // If not next to a target
            if (!unit.Tile.GetNeighbors().Intersect(targets).Any()) {
                // Find a path to the nearest target
                Queue<Pathfinder.Node<Tile>> path = new Queue<Pathfinder.Node<Tile>>(Pathfinder.FindPath(new[] { unit.Tile }, targets, this.GetNeighbors, this.GetCost));
                if (!path.Any())
                    return false;

                // First node is the unit's tile
                path.Dequeue();

                // Move along the path, but don't move onto the target
                while (unit.Moves > 0 && path.Count > 1) {
                    Pathfinder.Node<Tile> cur = path.Dequeue();
                    unit.Move(cur.Value);
                }
            }

            // Check if next to a target
            Tile target = unit.Tile.GetNeighbors().Intersect(targets).FirstOrDefault();
            if (!unit.Acted && target != null) {
                // Convert
                unit.Convert(target);
                this.AI.UnitsToAct.Enqueue(target.Unit);
            } else if (target == null) {
                return true;
            }

            return false;
        }

        public bool ChangeJobs(Unit unit) {
            // Check if this unit can change jobs
            if (unit.Energy < 100)
                return false;

            // Get an array of tiles in range of the cat
            HashSet<Tile> targets = this.Game.Tiles.Where(t => t.InRange(this.Player.Cat, 1) && t.IsPathable()).ToHashSet();

            // If not on a target already
            if (!targets.Contains(unit.Tile)) {
                // Find a path to the nearest target
                Queue<Pathfinder.Node<Tile>> path = new Queue<Pathfinder.Node<Tile>>(Pathfinder.FindPath(new[] { unit.Tile }, targets, this.GetNeighbors, this.GetCost));
                if (!path.Any())
                    return false;

                // First node is the unit's tile
                path.Dequeue();

                // Move along the path
                while (unit.Moves > 0 && path.Any()) {
                    Pathfinder.Node<Tile> cur = path.Dequeue();
                    unit.Move(cur.Value);
                }
            }

            // Check if on a target
            if (!unit.Acted && targets.Contains(unit.Tile)) {
                if (this.AI.CountUnits("gatherer") < this.Player.Units.Count / this.AI.UnitsPerGatherer)
                    unit.ChangeJob("gatherer"); // Need gatherers
                else if (this.AI.CountUnits("soldier") < 1)
                    unit.ChangeJob("soldier"); // Need at least one soldier
                else if (this.AI.CountUnits("missionary") < 1)
                    unit.ChangeJob("missionary"); // Need at least one missionary
                else if (this.AI.CountUnits("builder") == 0)
                    unit.ChangeJob("builder"); // Need at least one builder, but not as important
                else if (this.AI.CountUnits("missionary") < 2)
                    unit.ChangeJob("missionary"); // Need at least one missionary
                else
                    unit.ChangeJob("soldier"); // Rest are soldiers
            }

            return true;
        }

        public bool Rest(Unit unit) {
            // Check if this unit should rest
            if (Math.Abs(unit.Energy - 100) < 1)
                return false;

            // If not already in range of a shelter
            if (!unit.Tile.InRange("shelter", this.Player)) {
                // Find a path to the nearest tile in range of a shelter
                Queue<Pathfinder.Node<Tile>> path = new Queue<Pathfinder.Node<Tile>>(Pathfinder.FindPath(new[] { unit.Tile }, this.Game.Tiles.Where(t => t.InRange("shelter", this.Player) && t.IsPathable()), this.GetNeighbors, this.GetCost));
                if (!path.Any())
                    return false;

                // First node is the unit's tile
                path.Dequeue();

                // Move along the path
                while (unit.Moves > 0 && path.Any()) {
                    Pathfinder.Node<Tile> cur = path.Dequeue();
                    unit.Move(cur.Value);
                }

                // Check if near the shelter, and return if not
                if (path.Any()) {
                    return true;
                }
            }

            // Rest
            unit.Resting = true;
            double energyBefore = unit.Energy;
            unit.Rest();
            Logger.Log($"{unit} rested for {unit.Energy - energyBefore} energy, normal is {unit.Job.RegenRate}.");

            return true;
        }

        public bool DefendCat(Unit unit) {
            // Check if this unit should defend
            if (unit.Resting || unit.Energy <= 25)
                return false;

            // Ignore these checks if opponent is rushing
            if (!this.OpponentRushing) {
                const int defenders = 1;

                // Make sure the right number of units defend the cat
                if (this.Player.Cat.Squad.Count > defenders + 1)
                    return false;

                // Make sure if the cat is defended and this unit isn't defending it, don't defend it
                if (this.Player.Cat.Squad.Count == defenders + 1 && !this.Player.Cat.Squad.Contains(unit))
                    return false;
            }

            // Get an array with the cat's tile (needs to be an IEnumerable<Tile>)
            HashSet<Tile> targets = this.Player.Cat.Tile.GetNeighbors().Where(t => t.Structure?.Owner != this.Player && (t.Unit == unit || t.IsPathable()) && t.InRange("shelter", this.Player)).ToHashSet();
            if (!targets.Any())
                targets = this.Player.Cat.Tile.GetNeighbors().Where(t => t.Structure?.Owner != this.Player && (t.Unit == unit || t.IsPathable())).ToHashSet();
            if (!targets.Any())
                targets = this.Player.Units.Where(u => this.Player.Cat.Squad.Contains(u)).SelectMany(u => u.Tile.GetNeighbors().Where(t => t.Unit == unit || t.IsPathable())).ToHashSet();

            // If not on a target already
            if (!targets.Contains(unit.Tile)) {
                // Find a path to the nearest target
                Queue<Pathfinder.Node<Tile>> path = new Queue<Pathfinder.Node<Tile>>(Pathfinder.FindPath(new[] { unit.Tile }, targets, this.GetNeighbors, this.GetCost));
                if (!path.Any())
                    return false;

                // First node is the unit's tile
                path.Dequeue();

                // Move along the path
                while (unit.Moves > 0 && path.Any()) {
                    Pathfinder.Node<Tile> cur = path.Dequeue();
                    unit.Move(cur.Value);
                }
            }

            return true;
        }

        public bool AttackEnemies(Unit unit) {
            // Check if this unit can/should attack
            if (unit.Resting || unit.Job.Title != "soldier" || unit.Energy <= 50 || unit.Acted)
                return false;

            // Get an array of food tiles
            HashSet<Tile> targets = unit.Owner.Opponent.Units.Select(u => u.Tile).ToHashSet();

            // If not next to a target already
            if (!unit.Tile.GetNeighbors().Intersect(targets).Any()) {
                // Find a path to the nearest target
                Queue<Pathfinder.Node<Tile>> path = new Queue<Pathfinder.Node<Tile>>(Pathfinder.FindPath(new[] { unit.Tile }, targets, this.GetNeighbors, this.GetCost));
                if (!path.Any())
                    return false;

                // First node is the unit's tile
                path.Dequeue();

                // Move along the path, but don't move onto the target
                while (unit.Moves > 0 && path.Count > 1) {
                    Pathfinder.Node<Tile> cur = path.Dequeue();
                    unit.Move(cur.Value);
                }
            }

            // Check if next to or on a target
            Tile target = unit.Tile.GetNeighbors().Concat(new[] { unit.Tile }).Intersect(targets).FirstOrDefault();
            if (!unit.Acted && target != null) {
                // Attack
                return unit.Attack(target);
            }

            return false;
        }

        public float GetShelterScore(Tile tile) {
            float score = 0;
            const int checkDist = 10;
            float gathererCapacity = this.Game.Jobs.First(j => j.Title == "gatherer").CarryLimit;

            // Add distance from cat
            score += tile.Euclidean(this.Player.Cat.Tile) * 0.5F;

            // Subtract food tiles nearby based on distance
            IEnumerable<Tile> sourcesInRange = this.Game.Tiles.Where(t => t.HarvestRate > 0 && t.Manhattan(tile) <= checkDist);
            score = sourcesInRange.Aggregate(score, (s, t) => s - Math.Min(1, t.HarvestRate / gathererCapacity) * (checkDist + t.Manhattan(tile)));

            // Add nearby shelters
            IEnumerable<Tile> sheltersInRange = this.Game.Tiles.Where(t => t.Structure?.Owner == this.Player && t.Structure?.Type == "shelter" && t.Manhattan(tile) <= checkDist);
            score = sheltersInRange.Aggregate(score, (s, t) => s + checkDist - t.Manhattan(tile));

            return score;
        }

        public float GetMonumentScore(Tile tile) {
            float score = 0;

            // Subtract distance from road
            Tile[] roadTiles = this.Game.Tiles.Where(t => t.Structure?.Type == "road").ToArray();
            score = roadTiles.Aggregate(score, (s, t) => s - tile.Manhattan(t));

            // Subtract distance from enemy cat
            score -= this.Player.Opponent.Cat.Tile.Manhattan(tile) * roadTiles.Length;

            return score;
        }

        public IEnumerable<Tile> GetNeighbors(Tile tile) {
            return tile.GetNeighbors();
        }

        public float? GetCost(Tile tile) {
            return tile.IsPathable() ? 1 : (float?) null;
        }

        public Func<Tile, float?> GetCostIncluding(IEnumerable<Tile> validTiles) {
            HashSet<Tile> validSet = new HashSet<Tile>(validTiles);
            return tile => (validSet.Contains(tile) || tile.IsPathable()) ? 1 : (float?) null;
        }
    }
}
