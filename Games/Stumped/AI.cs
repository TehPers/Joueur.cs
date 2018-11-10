// This is where you build your AI for the Stumped game.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// <<-- Creer-Merge: usings -->> - Code you add between this comment and the end comment will be preserved between Creer re-runs.
// you can add additional using(s) here
// <<-- /Creer-Merge: usings -->>

namespace Joueur.cs.Games.Stumped
{
    /// <summary>
    /// This is where you build your AI for Stumped.
    /// </summary>
    public class AI : BaseAI
    {
        #region Properties
        #pragma warning disable 0169 // the never assigned warnings between here are incorrect. We set it for you via reflection. So these will remove it from the Error List.
        #pragma warning disable 0649
        /// <summary>
        /// This is the Game object itself. It contains all the information about the current game.
        /// </summary>
        public readonly Game Game;
        /// <summary>
        /// This is your AI's player. It contains all the information about your player's state.
        /// </summary>
        public readonly Player Player;
        #pragma warning restore 0169
        #pragma warning restore 0649

        // <<-- Creer-Merge: properties -->> - Code you add between this comment and the end comment will be preserved between Creer re-runs.

        private Job Builder { get; set; }
        private Job Chopper { get; set; }
        private Job Muncher { get; set; }
        private Job Attacker { get; set; }

        private List<Behavior> BuilderActions { get; } = new List<Behavior>();
        private List<Behavior> ChopperActions { get; } = new List<Behavior>();
        private List<Behavior> MuncherActions { get; } = new List<Behavior>();
        private List<Behavior> AttackerActions { get; } = new List<Behavior>();

        private int Builders => this.Player.Beavers.Count(b => b.Job == this.Builder);
        private int Choppers => this.Player.Beavers.Count(b => b.Job == this.Chopper);
        private int Fishers => this.Player.Beavers.Count(b => b.Job == this.Muncher);
        private int Attackers => this.Player.Beavers.Count(b => b.Job == this.Attacker);

        private int TotalBranches => this.Player.Beavers.Sum(b => b.Branches) + this.Player.Lodges.Sum(t => t.Branches);
        private int TotalFood => this.Player.Beavers.Sum(b => b.Food) + this.Player.Lodges.Sum(t => t.Food);
        private int TotalCarryLimit => this.Player.Beavers.Sum(b => b.Job.CarryLimit);

        private Tile NextLodge { get; set; }
        private HashSet<string> ClaimedResources { get; } = new HashSet<string>();


        public static bool Verbose => true;

        // <<-- /Creer-Merge: properties -->>
        #endregion


        #region Methods
        /// <summary>
        /// This returns your AI's name to the game server. Just replace the string.
        /// </summary>
        /// <returns>Your AI's name</returns>
        public override string GetName()
        {
            // <<-- Creer-Merge: get-name -->> - Code you add between this comment and the end comment will be preserved between Creer re-runs.
            return "Not A Dank Meme";
            // <<-- /Creer-Merge: get-name -->>
        }

        /// <summary>
        /// This is automatically called when the game first starts, once the Game and all GameObjects have been initialized, but before any players do anything.
        /// </summary>
        /// <remarks>
        /// This is a good place to initialize any variables you add to your AI or start tracking game objects.
        /// </remarks>
        public override void Start()
        {
            // <<-- Creer-Merge: start -->> - Code you add between this comment and the end comment will be preserved between Creer re-runs.
            base.Start();
            if (AI.Verbose) {
                Console.Clear();
                Console.WindowHeight = Console.LargestWindowHeight;
            }

            // Calculate roles for each job
            // Basically a beaver can move X resources Y spaces, so total resource movement is X * Y per turn
            // It can also harvest Z resources, so it fills at most Z / X of its capacity per turn
            // Beavers also have A health and deal B damage, so they can deal A * B damage per point of health per turn if taking 1 damage per turn
            HashSet<Job> availableJobs = new HashSet<Job>(this.Game.Jobs);

            // Builder needs to build and move, so should be able to build fast and move fast
            this.Builder = availableJobs.OrderByDescending(j => j.Moves * j.CarryLimit).First();
            availableJobs.Remove(this.Builder);
            this.BuilderActions.AddRange(new Behavior[] { this.Build, this.PickupBranches, this.Build, this.HarvestBranches, this.Build, this.Attack });

            // Harvester needs to get wood and take it back to the base
            this.Chopper = availableJobs.OrderByDescending(j => j.Moves * j.CarryLimit + j.Chopping).First();
            availableJobs.Remove(this.Chopper);
            this.ChopperActions.AddRange(new Behavior[] { this.HarvestBranches, this.Store, this.Attack });

            // Muncher needs to find food spots and harvest from them
            this.Muncher = availableJobs.OrderByDescending(j => j.Moves * j.CarryLimit + j.Munching).First();
            availableJobs.Remove(this.Muncher);
            this.MuncherActions.AddRange(new Behavior[] { this.HarvestFood, this.Store, this.Attack });

            // Attacker needs to hit hard and tank damage, so should have high damage and health
            this.Attacker = availableJobs.OrderByDescending(j => j.Damage * j.Actions).First();
            availableJobs.Remove(this.Attacker);
            this.AttackerActions.Add(this.Attack);

            // <<-- /Creer-Merge: start -->>
        }

        /// <summary>
        /// This is automatically called every time the game (or anything in it) updates.
        /// </summary>
        /// <remarks>
        /// If a function you call triggers an update, this will be called before that function returns.
        /// </remarks>
        public override void GameUpdated()
        {
            // <<-- Creer-Merge: game-updated -->> - Code you add between this comment and the end comment will be preserved between Creer re-runs.
            base.GameUpdated();
            // <<-- /Creer-Merge: game-updated -->>
        }

        /// <summary>
        /// This is automatically called when the game ends.
        /// </summary>
        /// <remarks>
        /// You can do any cleanup of you AI here, or do custom logging. After this function returns, the application will close.
        /// </remarks>
        /// <param name="won">True if your player won, false otherwise</param>
        /// <param name="reason">A string explaining why you won or lost</param>
        public override void Ended(bool won, string reason)
        {
            // <<-- Creer-Merge: ended -->> - Code you add between this comment and the end comment will be preserved between Creer re-runs.
            base.Ended(won, reason);

            // Don't pause for clients run in CMD
            if (AI.Verbose)
                Console.ReadKey();
            // <<-- /Creer-Merge: ended -->>
        }


        /// <summary>
        /// This is called every time it is this AI.player's turn.
        /// </summary>
        /// <returns>Represents if you want to end your turn. True means end your turn, False means to keep your turn going and re-call this function.</returns>
        public bool RunTurn()
        {
            // <<-- Creer-Merge: runTurn -->> - Code you add between this comment and the end comment will be preserved between Creer re-runs.

            // Basic info
            if (AI.Verbose) {
                this.DisplayMap();
                Console.WriteLine($"Turn: {this.Game.CurrentTurn + 1}/{this.Game.MaxTurns}");
                Console.WriteLine($"Lodges: {this.Player.Lodges.Count} ({this.Player.Lodges.Sum(t => t.Branches)} branches, {this.Player.Lodges.Sum(t => t.Food)} food) | Cost: {this.Player.BranchesToBuildLodge}");
                Console.WriteLine($"Beavers: {this.Player.Beavers.Count} | Chopper: {this.Chopper.Title} ({this.Choppers}) | Muncher: {this.Muncher.Title} ({this.Fishers}) | Builder: {this.Builder.Title} ({this.Builders}) | Attacker: {this.Attacker.Title} ({this.Attackers})");
                Console.WriteLine($"Owned resources: {this.TotalBranches} branches, {this.TotalFood} food");
            }

            // For each lodge, spawn beavers if possible
            foreach (Tile lodge in this.Player.Lodges) {
                lodge.Log("Lodge");
                if (lodge.Beaver != null)
                    continue;

                // Select which job to spawn
                Job job;
                IEnumerable<Beaver> attackBeavers = this.Player.Beavers.Where(b => b.Job == this.Attacker);
                IEnumerable<Beaver> otherBeavers = this.Game.Beavers.Where(b => b.Owner != this.Player);
                if (this.Builders * this.Builder.CarryLimit < this.Game.Tiles.Sum(t => t.Branches)) // Not enough builders for the branches here
                    job = this.Builder;
                else if (this.Fishers == 0 || this.TotalCarryLimit < lodge.Branches) // Not enough beavers for the number of branches
                    job = this.Muncher;
                else if (attackBeavers.Sum(b => b.Job.Damage * b.Job.Actions) < otherBeavers.Sum(b => b.Health)) // Check that there are enough attackers for genocide
                    job = this.Attacker;
                else // Default job and choppers
                    job = this.Chopper;

                if (this.Player.Beavers.Count < this.Game.FreeBeaversCount || lodge.Food >= job.Cost) {
                    if (AI.Verbose) {
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine($"Recruiting {job.Title}");
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                    job.Recruit(lodge);
                }
            }

            // For each beaver, take actions
            foreach (Beaver b in this.Player.Beavers) {
                // Build first lodge
                if (!this.Player.Lodges.Any())
                    b.BuildLodge();

                // Display which beaver this is
                b.Log($"{b.Job.Title}");

                /* ACTIONS (returns true if acted or moved) */

                List<Behavior> actions;
                if (!this.Player.Lodges.Any()) {
                    // Build a lodge. Should be free.
                    actions = this.BuilderActions;
                } else if (b.Job == this.Chopper) {
                    actions = this.ChopperActions;
                } else if (b.Job == this.Muncher) {
                    actions = this.MuncherActions;
                } else if (b.Job == this.Builder) {
                    actions = this.BuilderActions;
                } else if (b.Job == this.Attacker) {
                    actions = this.AttackerActions;
                } else {
                    // Default, harvest food I guess?
                    actions = this.MuncherActions;
                }

                // Perform each action
                foreach (Behavior action in actions)
                    while (action(b)) { }
                while (this.GetOffLodge(b)) { }
            }

            return true;
            // <<-- /Creer-Merge: runTurn -->>
        }

        /// <summary>
        /// A very basic path finding algorithm (Breadth First Search) that when given a starting Tile, will return a valid path to the goal Tile.
        /// </summary>
        /// <remarks>
        /// This is NOT an optimal pathfinding algorithm. It is intended as a stepping stone if you want to improve it.
        /// </remarks>
        /// <param name="start">the starting Tile</param>
        /// <param name="goal">the goal Tile</param>
        /// <returns>A list of Tiles representing the path where the first element is a valid adjacent Tile to the start, and the last element is the goal. Or an empty list if no path found.</returns>
        List<Tile> FindPath(Tile start, Tile goal)
        {
            // no need to make a path to here...
            if (start == goal)
            {
                return new List<Tile>();
            }

            // the tiles that will have their neighbors searched for 'goal'
            Queue<Tile> fringe = new Queue<Tile>();

            // How we got to each tile that went into the fringe.
            Dictionary<Tile, Tile> cameFrom = new Dictionary<Tile, Tile>();

            // Enqueue start as the first tile to have its neighbors searched.
            fringe.Enqueue(start);

            // keep exploring neighbors of neighbors... until there are no more.
            while (fringe.Any())
            {
                // the tile we are currently exploring.
                Tile inspect = fringe.Dequeue();

                // cycle through the tile's neighbors.
                foreach (Tile neighbor in inspect.GetNeighbors())
                {
                    if (neighbor == goal)
                    {
                        // Follow the path backward starting at the goal and return it.
                        List<Tile> path = new List<Tile>();
                        path.Add(goal);

                        // Starting at the tile we are currently at, insert them retracing our steps till we get to the starting tile
                        for (Tile step = inspect; step != start; step = cameFrom[step])
                        {
                            path.Insert(0, step);
                        }

                        return path;
                    }

                    // if the tile exists, has not been explored or added to the fringe yet, and it is pathable
                    if (neighbor != null && !cameFrom.ContainsKey(neighbor) && neighbor.IsPathable())
                    {
                        // add it to the tiles to be explored and add where it came from.
                        fringe.Enqueue(neighbor);
                        cameFrom.Add(neighbor, inspect);
                    }

                } // foreach(neighbor)

            } // while(fringe not empty)

            // if you're here, that means that there was not a path to get to where you want to go.
            //   in that case, we'll just return an empty path.
            return new List<Tile>();
        }

        // <<-- Creer-Merge: methods -->> - Code you add between this comment and the end comment will be preserved between Creer re-runs.

        private void DisplayMap() {
            Console.SetCursorPosition(0, 0);
            Console.BackgroundColor = ConsoleColor.White;
            Console.Write(new string(' ', this.Game.MapWidth + 2));
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine();
            for (int y = 0; y < this.Game.MapHeight; y++) {
                Console.BackgroundColor = ConsoleColor.White;
                Console.Write(' ');
                for (int x = 0; x < this.Game.MapWidth; x++) {
                    Tile t = this.Game.Tiles[y * this.Game.MapWidth + x];

                    // Background color
                    Console.BackgroundColor = (t.Type == "water") ? ConsoleColor.Blue : ConsoleColor.DarkYellow;

                    // Character to display
                    char foreground = ' ';
                    Console.ForegroundColor = ConsoleColor.White;

                    // Water flow direction
                    switch (t.FlowDirection) {
                        case "North":
                            foreground = '^';
                            break;
                        case "South":
                            foreground = 'v';
                            break;
                        case "East":
                            foreground = '>';
                            break;
                        case "West":
                            foreground = '<';
                            break;
                    }

                    // Tile specific stuff
                    if (t.Beaver != null) {
                        Console.ForegroundColor = t.Beaver.Owner == this.Player ? ConsoleColor.Green : ConsoleColor.Red;
                        foreground = '?';
                        if (t.Beaver.Owner == this.Player) {
                            if (t.Beaver.Job == this.Muncher) {
                                foreground = 'F';
                            } else if (t.Beaver.Job == this.Builder) {
                                foreground = 'B';
                            } else if (t.Beaver.Job == this.Chopper) {
                                foreground = 'C';
                            } else if (t.Beaver.Job == this.Attacker) {
                                foreground = 'A';
                            }
                        }
                    } else if (t.Spawner != null) {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        foreground = '$';
                    } else if (t.LodgeOwner != null) {
                        Console.ForegroundColor = t.LodgeOwner == this.Player ? ConsoleColor.Green : ConsoleColor.Red;
                        foreground = '#';
                    }

                    Console.Write(foreground);
                }
                Console.BackgroundColor = ConsoleColor.White;
                Console.Write(' ');
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(y);
                Console.WriteLine();
            }
            Console.BackgroundColor = ConsoleColor.White;
            Console.Write(new string(' ', this.Game.MapWidth + 2));
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine();

            // Clear everything past here
            int left = Console.CursorLeft;
            int top = Console.CursorTop;
            Console.Write(new string(' ', Console.WindowWidth * (Console.WindowHeight - top) - 1));
            Console.SetCursorPosition(left, top);
        }

        #region Behaviors

        /// <summary>Takes an action with <paramref name="b"/> and returns whether it should be called again.</summary>
        private delegate bool Behavior(Beaver b);

        private bool HarvestBranches(Beaver b) => this.Harvest(b, "branches");
        private bool HarvestFood(Beaver b) => this.Harvest(b, "food");

        /// <summary>Harvests resource</summary>
        private bool Harvest(Beaver b, string resource) {
            // Don't bother harvesting if it can't hold anymore stuff
            if (b.Branches + b.Food >= b.Job.CarryLimit)
                return false;

            bool keepGoing = false;

            // Function for finding movement targets
            Func<Tile, bool> isTarget = t => t.Spawner != null && t.Spawner.Type == resource && !t.Spawner.HasBeenHarvested && t.Spawner.Health > 3;

            // Find nearest resource, skipping current node
            IEnumerable<Tile> basePath = this.FindPathCustom(b.Tile,
                    (t, parent) => {
                        double cost = this.GetCost(t, parent, b);
                        if (cost < 0)
                            return cost;
                        return this.IsNextTo(t, isTarget) ? 0 : cost;
                    })
                .Skip(1);

            // Traverse path
            LinkedList<Tile> path = new LinkedList<Tile>(basePath);
            while (path.Any()) {
                // If can't move anymore
                if (b.Moves < b.MoveCost(path.First()) || !b.Move(path.First()))
                    break;
                path.RemoveFirst();
                keepGoing = true;
            }

            // Try to harvest if next to the target spawner
            Tile target = this.GetNeighborWhere(b.Tile, isTarget);
            if (target != null && b.Actions > 0) {
                if (AI.Verbose)
                    Console.WriteLine("Harvesting resource");
                b.Harvest(target.Spawner);
                keepGoing = true;
            }

            return keepGoing;
        }

        /// <summary>Stores carried resources in lodges</summary>
        private bool Store(Beaver b) {
            // Don't bother storing if it doesn't have a full inventory
            if (b.Branches + b.Food < b.Job.CarryLimit)
                return false;

            bool retValue = false;

            // Function for finding movement targets
            Func<Tile, bool> isTarget = t => t.LodgeOwner == this.Player;

            // Find nearest lodge, skipping current node
            IEnumerable<Tile> basePath = this.FindPathCustom(b.Tile,
                    (t, parent) => {
                        double cost = this.GetCost(t, parent, b);
                        if (cost < 0)
                            return cost;
                        return this.IsNextTo(t, isTarget) ? 0 : cost;
                    })
                .Skip(1);

            // Traverse path
            LinkedList<Tile> path = new LinkedList<Tile>(basePath);
            while (path.Any()) {
                // If can't move anymore
                if (b.Moves < b.MoveCost(path.First()) || !b.Move(path.First()))
                    break;
                path.RemoveFirst();
                retValue = true;
            }

            // Drop off resources at lodge
            Tile target = this.GetNeighborWhere(b.Tile, isTarget);
            if (target != null && b.Actions > 0) {
                if (AI.Verbose)
                    Console.WriteLine("Dropping resources on lodge");
                if (b.Branches > 0)
                    b.Drop(target, "b"); // branches
                if (b.Food > 0)
                    b.Drop(target, "f"); // food
                retValue = true;
            }

            return retValue;
        }

        private bool PickupBranches(Beaver b) => this.Pickup(b, "b");

        /// <summary>Picks up resource from owned lodges</summary>
        private bool Pickup(Beaver b, string resource) {
            if (b.Branches + b.Food >= b.Job.CarryLimit)
                return false;

            bool keepGoing = false;

            // Go to nearest lodge with the given resource
            Func<Tile, bool> isTarget = t => t.LodgeOwner == this.Player && resource == "branches" ? t.Branches > 0 : t.Food > 0;

            // Find nearest resource, skipping current node
            IEnumerable<Tile> basePath = this.FindPathCustom(b.Tile,
                    (t, parent) => {
                        double cost = this.GetCost(t, parent, b);
                        if (cost < 0)
                            return cost;
                        if (this.IsNextTo(t, isTarget))
                            return 0;
                        return cost;
                    })
                .Skip(1);

            // Traverse path
            LinkedList<Tile> path = new LinkedList<Tile>(basePath);
            while (path.Any()) {
                // If can't move anymore
                if (b.Moves < b.MoveCost(path.First()) || !b.Move(path.First()))
                    break;
                path.RemoveFirst();
                keepGoing = true;
            }

            // Try to pick up resources if next to the target lodge
            Tile target = this.GetNeighborWhere(b.Tile, isTarget);
            if (target != null && b.Actions > 0) {
                int amount = resource == "food" ? target.Food : target.Branches;
                if (target.LodgeOwner == this.Player) {
                    amount--;
                }
                amount = Math.Min(amount, b.Job.CarryLimit - b.Branches - b.Food);
                if (amount > 0) {
                    if (AI.Verbose)
                        Console.WriteLine("Picking up resource");
                    b.Pickup(target, resource, amount);
                    keepGoing = true;
                }
            }

            return keepGoing;
        }

        /// <summary>Builds lodges</summary>
        private bool Build(Beaver b) {
            // Check if it can even build a lodge
            if (b.Branches <= 0)
                return false;

            bool retValue = false;

            // Find suitable building spot
            bool foodOnly = true;

            // ReSharper disable once AccessToModifiedClosure
            Func<Tile, bool> isTarget = t => t.LodgeOwner == null && t.Spawner == null && t.FlowDirection == null && this.IsNextTo(t, n1 => this.IsNextTo(n1, n2 => n2.Spawner != null && (!foodOnly || n2.Spawner.Type == "food") && !this.ClaimedResources.Contains(n2.Id)));

            IEnumerable<Tile> basePath;

            if (this.NextLodge == null) {
                // Look for a spot two spaces away from an unclaimed food spot
                basePath = this.FindPathCustom(b.Tile,
                        (t, parent) => {
                            double cost = this.GetCost(t, parent, b);
                            if (cost < 0)
                                return cost;
                            return isTarget(t) ? 0 : cost;
                        })
                    .Skip(1);

                // Look for a spot two spaces away from any unclaimed resource
                if (!basePath.Any()) {
                    foodOnly = false;
                    basePath = this.FindPathCustom(b.Tile,
                            (t, parent) => {
                                double cost = this.GetCost(t, parent, b);
                                if (cost < 0)
                                    return cost;
                                return isTarget(t) ? 0 : cost;
                            })
                        .Skip(1);
                }

                // Set this as the next lodge spot
                if (basePath.Any())
                    this.NextLodge = basePath.Last();
            } else {
                basePath = this.FindPathCustom(b.Tile, this.NextLodge).Skip(1);
            }

            // Traverse path
            LinkedList<Tile> path = new LinkedList<Tile>(basePath);
            while (path.Any()) {
                // If can't move
                if (b.Moves < b.MoveCost(path.First()) || !b.Move(path.First()))
                    break;
                path.RemoveFirst();
                retValue = true;
            }

            // Try to build a lodge
            Tile target = b.Tile;
            if (isTarget(b.Tile)) {
                if (b.Actions > 0 && target.Branches >= this.Player.BranchesToBuildLodge) {
                    if (AI.Verbose)
                        Console.WriteLine("Building lodge");
                    b.BuildLodge();
                    this.NextLodge = null;
                    retValue = true;

                    // Set nearby food source as "claimed"
                    // TODO: If the lodge is destroyed, unclaim those sources

                    IEnumerable<Tile> sources = from n1 in new[] { target.TileNorth, target.TileEast, target.TileSouth, target.TileWest }
                                                where n1 != null
                                                from n2 in new[] { n1.TileNorth, n1.TileEast, n1.TileSouth, n1.TileWest }
                                                where n2?.Spawner != null
                                                select n2;
                    foreach (Tile source in sources) {
                        this.ClaimedResources.Add(source.Id);
                    }
                } else if (b.Actions > 0 && b.Branches > 0) {
                    if (AI.Verbose)
                        Console.WriteLine("Dropping branches for lodge");
                    b.Drop(target, nameof(b)); // branches
                    retValue = true;
                }
            }

            return retValue;
        }

        /// <summary>Attacks enemy beavers</summary>
        private bool Attack(Beaver b) {
            if (b.Actions <= 0)
                return false;

            bool retValue = false;

            // Find suitable building spot
            Func<Tile, bool> isTarget = t => t.Beaver != null && t.Beaver.Owner != this.Player && t.Beaver.Recruited;

            IEnumerable<Tile> pfResult;
            if (this.NextLodge == null) {
                // Look for a spot two spaces away from an unclaimed food spot
                pfResult = this.FindPathCustom(b.Tile,
                        (t, parent) => {
                            double cost = this.GetCost(t, parent, b);
                            if (cost < 0)
                                return cost;
                            return this.IsNextTo(t, isTarget) ? 0 : cost;
                        })
                    .Skip(1);

                // Look for a spot two spaces away from any unclaimed resource
                if (!pfResult.Any()) {
                    pfResult = this.FindPathCustom(b.Tile,
                            (t, parent) => {
                                double cost = this.GetCost(t, parent, b);
                                if (cost < 0)
                                    return cost;
                                if (isTarget(t))
                                    return 0;
                                return cost;
                            })
                        .Skip(1);
                }

                // Set this as the next lodge spot
                if (pfResult.Any())
                    this.NextLodge = pfResult.Last();
            } else {
                pfResult = this.FindPathCustom(b.Tile, this.NextLodge).Skip(1);
            }

            // Traverse path
            LinkedList<Tile> path = new LinkedList<Tile>(pfResult);
            while (path.Any()) {
                // If can't move
                if (b.Moves < b.MoveCost(path.First()) || !b.Move(path.First()))
                    break;
                path.RemoveFirst();
                retValue = true;
            }

            // Try to attack a beaver
            Tile target = this.GetNeighborWhere(b.Tile, isTarget);
            if (target != null)
                while (b.Actions > 0 && b.Health > target.Beaver.Job.Damage)
                    b.Attack(target.Beaver);

            return retValue;
        }

        /// <summary>Gets off the lodge if it's on one</summary>
        private bool GetOffLodge(Beaver b) {
            if (b.Tile.LodgeOwner == null)
                return false;

            Tile target = this.GetNeighborWhere(b.Tile, t => t.LodgeOwner == null && b.Moves < b.MoveCost(t));
            return target != null && b.Move(target);
        }

        #endregion

        #region Pathfinding

        private double GetCost(Tile t, Tile parent, Beaver self = null) {
            // Don't move over friendly lodges or solids
            if (t.LodgeOwner != null)
                return -1;
            if (t.Beaver != null && t.Beaver != self)
                return -1;
            if (t.Spawner != null)
                return -1;

            return parent?.MoveCost(t) ?? 2;
        }

        private double Manhattan(Tile t, Tile finish) {
            // Manhatten distance. Nothing complicated needed really.
            double da = Math.Abs(t.X - finish.X) + Math.Abs(t.Y - finish.Y);
            double db = Math.Abs(t.X - finish.X) + Math.Abs(t.Y - finish.Y);
            return da - db;
        }

        private bool IsNextTo(Tile t, Func<Tile, bool> checkTile) => this.GetNeighborWhere(t, checkTile) != null;

        private Tile GetNeighborWhere(Tile t, Func<Tile, bool> checkTile) {
            if (t.TileNorth != null && checkTile(t.TileNorth))
                return t.TileNorth;
            if (t.TileEast != null && checkTile(t.TileEast))
                return t.TileEast;
            if (t.TileSouth != null && checkTile(t.TileSouth))
                return t.TileSouth;
            if (t.TileWest != null && checkTile(t.TileWest))
                return t.TileWest;
            return null;
        }

        private IEnumerable<Tile> FindPathCustom(Tile start, Tile finish) {
            return this.FindPathCustom(start,
                (t, parent) => {
                    double cost = this.GetCost(t, parent);
                    if (cost < 0)
                        return cost;
                    if (t == finish)
                        return 0;
                    return cost;
                },
                t => this.Manhattan(t, finish));
        }

        #region Base Pathfinding

        private double NoHeuristic(Tile t) => 0;

        /// <summary>Dijkstra pathfinder</summary>
        /// <param name="start">Starting tile</param>
        /// <param name="getG">Cost function for the tile, also tells pathfinder when to stop and where walls are</param>
        /// <returns>The path, or <seealso cref="Enumerable.Empty{Tile}"/> if no path found</returns>
        private IEnumerable<Tile> FindPathCustom(Tile start, CostFunc getG) => this.FindPathCustom(start, getG, this.NoHeuristic);

        /// <summary>Returns the heuristic cost of the tile</summary>
        /// <param name="t">The tile</param>
        /// <returns>Heuristic cost of the tile</returns>
        private delegate double HeuristicFunc(Tile t);

        /// <summary>Returns the cost/type of the tile</summary>
        /// <param name="t">The tile</param>
        /// <returns>Positive value for movement cost, 0 for valid ending node, negative for wall</returns>
        private delegate double CostFunc(Tile t, Tile parent);

        /// <summary>Dijkstra pathfinder with heuristic</summary>
        /// <param name="start">Starting tile</param>
        /// <param name="getCost">Cost function for the tile, also tells pathfinder when to stop and where walls are</param>
        /// <param name="getH">Heuristic function for the tile</param>
        /// <returns>The path, or <seealso cref="Enumerable.Empty{Tile}"/> if no path found</returns>
        private IEnumerable<Tile> FindPathCustom(Tile start, CostFunc getCost, HeuristicFunc getH) {
            Dictionary<Tile, Node> nodes = new Dictionary<Tile, Node>();
            List<Node> openList = new List<Node> {
                this.GetNode(start, null, nodes, getCost, getH)
            };
            HashSet<string> closedList = new HashSet<string>();

            while (openList.Any()) {
                // Get current tile and close it
                Node curNode = openList.First();
                Tile curTile = curNode.Value;
                openList.Remove(curNode);
                closedList.Add(curTile.Id);

                // Skip it if it's a wall and not the first tile
                if (curNode.Cost < 0 && curTile != start)
                    continue;

                // Check if done
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (curNode.Cost == 0) {
                    Stack<Tile> path = new Stack<Tile>();
                    while (curNode != null) {
                        path.Push(curNode.Value);
                        curNode = curNode.Parent;
                    }
                    return path;
                }

                // Add neighbors
                Tile neighbor = curTile.TileNorth;
                if (neighbor != null && !closedList.Contains(neighbor.Id)) {
                    openList.RemoveAll(n => n.Value.Id == neighbor.Id);
                    openList.Add(this.GetNode(neighbor, curNode, nodes, getCost, getH));
                }
                neighbor = curTile.TileEast;
                if (neighbor != null && !closedList.Contains(neighbor.Id)) {
                    openList.RemoveAll(n => n.Value.Id == neighbor.Id);
                    openList.Add(this.GetNode(neighbor, curNode, nodes, getCost, getH));
                }
                neighbor = curTile.TileSouth;
                if (neighbor != null && !closedList.Contains(neighbor.Id)) {
                    openList.RemoveAll(n => n.Value.Id == neighbor.Id);
                    openList.Add(this.GetNode(neighbor, curNode, nodes, getCost, getH));
                }
                neighbor = curTile.TileWest;
                if (neighbor != null && !closedList.Contains(neighbor.Id)) {
                    openList.RemoveAll(n => n.Value.Id == neighbor.Id);
                    openList.Add(this.GetNode(neighbor, curNode, nodes, getCost, getH));
                }

                // Sort openList
                openList.Sort((a, b) => (int) (a.F - b.F));
            }

            return Enumerable.Empty<Tile>();
        }

        private Node GetNode(Tile t, Node parent, IDictionary<Tile, Node> nodes, CostFunc getCost, HeuristicFunc getH) {
            Node newNode = new Node(parent, t, getCost(t, parent?.Value), getH(t));
            Node existingNode;
            if (nodes.TryGetValue(t, out existingNode) && existingNode.F <= newNode.F)
                return existingNode;
            nodes[t] = newNode;
            return newNode;
        }

        private class Node {
            public Node Parent { get; }
            public Tile Value { get; }
            public double Cost { get; }
            private readonly double _g;
            private readonly double _h;
            public double F => this._g + this._h;

            public Node(Node parent, Tile value, double cost, double h) {
                this.Parent = parent;
                this.Value = value;
                this.Cost = cost;
                this._g = parent?._g + this.Cost ?? this.Cost;
                this._h = h;
            }
        }

        #endregion

        #endregion

        // <<-- /Creer-Merge: methods -->>
        #endregion
    }
}
