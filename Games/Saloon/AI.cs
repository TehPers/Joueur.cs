// This is where you build your AI for the Saloon game.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// <<-- Creer-Merge: usings -->> - Code you add between this comment and the end comment will be preserved between Creer re-runs.
// you can add additional using(s) here
// <<-- /Creer-Merge: usings -->>

namespace Joueur.cs.Games.Saloon
{
    /// <summary>
    /// This is where you build your AI for Saloon.
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
        // you can add additional properties here for your AI to use
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
            Console.WriteLine("Game started");
            Console.WriteLine("Max cowboys per job: " + this.Game.MaxCowboysPerJob);
            Console.WriteLine("Map size: " + this.Game.MapWidth + ", " + this.Game.MapHeight);
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
            // <<-- /Creer-Merge: ended -->>
        }


        /// <summary>
        /// This is called every time it is this AI.player's turn.
        /// </summary>
        /// <returns>Represents if you want to end your turn. True means end your turn, False means to keep your turn going and re-call this function.</returns>
        public bool RunTurn()
        {
            // <<-- Creer-Merge: runTurn -->> - Code you add between this comment and the end comment will be preserved between Creer re-runs.
            // Put your game logic here for runTurn
            Player player = this.Player;
            HashSet<int> playedPianos = new HashSet<int>();

            // Do cowboy logic
            foreach (Cowboy cowboy in this.Player.Cowboys) {
                string job = cowboy.Job;
                if (cowboy.IsDrunk) {
                    Console.WriteLine(job + ": I swear to drunk I'm not god!");
                    continue;
                }

                switch (job) {
                    case "Brawler": // Attack nearest bartender
                        if (!this.MoveToPiano(cowboy, playedPianos))
                            this.MoveToEnemies(cowboy);
                        break;
                    case "Sharpshooter":
                        if (!this.MoveToPiano(cowboy, playedPianos))
                            this.RunFromHazards(cowboy);
                        break;
                    case "Bartender":
                        if (this.MoveToPiano(cowboy, playedPianos))
                            break;

                        if (cowboy.TurnsBusy > 0) {
                            if (!this.MoveToPiano(cowboy, playedPianos))
                                this.RunFromHazards(cowboy);
                        } else {
                            string actDir;

                            bool r = this.MoveToLineOfSight(cowboy, tile => {
                                // Only sucessful if enemy cowboy found > 1 tile away
                                if (tile.Cowboy != null) {
                                    if (tile.Cowboy.Owner == player /*|| Math.abs(cowboy.tile.x - tile.x) + Math.abs(cowboy.tile.y - tile.y) <= 1*/)
                                        return -1;
                                    if (tile.TileNorth?.Furnishing?.IsPiano == true)
                                        return 1;
                                    if (tile.TileEast?.Furnishing?.IsPiano == true)
                                        return 1;
                                    if (tile.TileSouth?.Furnishing?.IsPiano == true)
                                        return 1;
                                    if (tile.TileWest?.Furnishing?.IsPiano == true)
                                        return 1;
                                } else if (tile.Furnishing != null || tile.IsBalcony)
                                    return -1;
                                return 0;
                            }, out actDir);

                            if (actDir != null) {
                                Console.WriteLine($"{cowboy.Job}: Acting in this direction: {actDir}");
                                Tile actTile = null;
                                switch (actDir) {
                                    case "North":
                                        actTile = cowboy.Tile.TileNorth;
                                        break;
                                    case "East":
                                        actTile = cowboy.Tile.TileEast;
                                        break;
                                    case "South":
                                        actTile = cowboy.Tile.TileSouth;
                                        break;
                                    case "West":
                                        actTile = cowboy.Tile.TileWest;
                                        break;
                                }

                                if (actTile != null)
                                    cowboy.Act(actTile, actDir == "North" ? "South" : actDir == "South" ? "North" : actDir == "East" ? "West" : actDir == "West" ? "East" : "North");
                                else
                                    Console.WriteLine($"{cowboy.Job}: Coudln't figure out direction to Act!");
                            } else if (!r)
                                this.RunFromHazards(cowboy);
                        }
                        break;
                }
            }

            // Add cowboy if possible
            this.SpawnIfAble();

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
        private void SpawnIfAble() {
            if (!this.Player.YoungGun.CanCallIn)
                return;

            // Check if spawning cowboy on friendly unit
            if (this.Player.YoungGun.CallInTile == null || this.Player.YoungGun.CallInTile.Furnishing != null)
                return;

            foreach (string job in this.Game.Jobs) {
                if (this.Player.Cowboys.Count(cowboy => cowboy.Job == job) >= this.Game.MaxCowboysPerJob)
                    continue;

                Console.WriteLine($"Calling in {job}");
                this.Player.YoungGun.CallIn(job);
                break;
            }
        }

        private bool MoveToPiano(Cowboy cowboy, HashSet<int> playedPianos) {
            Console.WriteLine($"{cowboy.Job}: Looking for piano starting at {cowboy.Tile.X}, {cowboy.Tile.Y}");
            Node path = this.PathToNearest(cowboy.Tile, t => t.Furnishing != null && t.Furnishing.IsPiano && !playedPianos.Contains(this.CoordsToIndex(t.X, t.Y)), null, true);

            if (path == null) {
                Console.WriteLine($"{cowboy.Job}: Path not found");
            } else {
                Console.WriteLine($"{cowboy.Job}: Found path! Target at {path.Tile.X}, {path.Tile.Y}");

                Node moveNode = path;
                while (moveNode.Parent?.Parent != null)
                    moveNode = moveNode.Parent;

                if (moveNode != path) {
                    if (cowboy.Move(moveNode.Tile))
                        return true;
                    Console.WriteLine($"{cowboy.Job}: Couldn't Move. Pathfinding is broken");
                } else {
                    Console.WriteLine($"{cowboy.Job}: Playing piano");
                    cowboy.Play(moveNode.Tile.Furnishing);
                    playedPianos.Add(this.CoordsToIndex(moveNode.Tile.X, moveNode.Tile.Y));
                    return true;
                }
            }

            return false;
        }

        private void MoveToEnemies(Cowboy cowboy) {
            Console.WriteLine($"{cowboy.Job}: Looking for enemies starting at {cowboy.Tile.X}, {cowboy.Tile.Y}");
            Node path = this.PathToNearest(cowboy.Tile, t => t.Cowboy?.Owner == this.Player.Opponent, t => {
                int cost = this.GetMovementCost(t);
                // Try to go to another side of enemy if tile would damage friendly unit
                //if (t.tileNorth && t.tileNorth.cowboy && t.tileNorth.cowboy.owner == this.player) cost += 7;
                //if (t.tileEast && t.tileEast.cowboy && t.tileEast.cowboy.owner == this.player) cost += 7;
                //if (t.tileSouth && t.tileSouth.cowboy && t.tileSouth.cowboy.owner == this.player) cost += 7;
                //if (t.tileWest && t.tileWest.cowboy && t.tileWest.cowboy.owner == this.player) cost += 7;
                return cost;
            }, true);

            if (path != null) {
                Console.WriteLine($"{cowboy.Job}: Found path! Target at ({path.Tile.X}, {path.Tile.Y}).");

                Node moveNode = path;
                while (moveNode.Parent?.Parent != null)
                    moveNode = moveNode.Parent;

                if (moveNode != path) {
                    if (cowboy.Move(moveNode.Tile))
                        return;
                    Console.WriteLine($"{cowboy.Job}: Couldn't Move. Pathfinding is broken.");
                } else {
                    Console.WriteLine($"{cowboy.Job}: Already here!");
                }
            } else
                Console.WriteLine($"{cowboy.Job}: Path not found.");
        }

        // validSpot(tile) => -# for break, 0 for continue, +# for match
        private bool MoveToLineOfSight(Cowboy cowboy, Func<Tile, int> validSpot, out string actDir) {
            Console.WriteLine($"{cowboy.Job}: Looking for line of sight spot to enemies starting at {cowboy.Tile.X}, {cowboy.Tile.Y}");
            string dir = null;
            Node path = this.PathToNearest(cowboy.Tile, tile => {
                // Check if valid spot
                int cost = this.GetMovementCost(tile);
                if (cost < 0 && tile.Cowboy != cowboy)
                    return false;
                // Check if enemy piano-playing cowboy in line of sight
                Tile t;

                // Check +x line of sight
                dir = "East";
                int x = tile.X;
                int y = tile.Y;
                while ((t = this.GetTile(++x, y)) != null) {
                    int r = validSpot(t);
                    if (r < 0)
                        break;
                    if (r > 0)
                        return true;
                }
                // Check -x line of sight
                dir = "West";
                x = tile.X;
                y = tile.Y;
                while ((t = this.GetTile(--x, y)) != null) {
                    int r = validSpot(t);
                    if (r < 0)
                        break;
                    if (r > 0)
                        return true;
                }

                // Check +y line of sight
                dir = "South";
                x = tile.X;
                y = tile.Y;
                while ((t = this.GetTile(x, ++y)) != null) {
                    int r = validSpot(t);
                    if (r < 0)
                        break;
                    if (r > 0)
                        return true;
                }

                // Check -y line of sight
                dir = "North";
                x = tile.X;
                y = tile.Y;
                while ((t = this.GetTile(x, --y)) != null) {
                    int r = validSpot(t);
                    if (r < 0)
                        break;
                    if (r > 0)
                        return true;
                }

                dir = null;
                return false;
            });

            actDir = dir;
            if (path != null) {
                Console.WriteLine($"{cowboy.Job}: Found path! Target at {path.Tile.X}, {path.Tile.Y}");
                Node moveNode = path;
                while (moveNode.Parent?.Parent != null)
                    moveNode = moveNode.Parent;

                if (moveNode.Tile != cowboy.Tile) {
                    if (cowboy.Move(moveNode.Tile))
                        return true;
                    Console.WriteLine($"{cowboy.Job}: Couldn't move. Pathfinding is broken.");
                } else
                    return true;
            } else
                Console.WriteLine($"{cowboy.Job}: No decent spots found.");
            return false;
        }

        private void RunFromHazards(Cowboy cowboy) {
            if (this.GetMovementCost(cowboy.Tile) != 1)
                return;

            Console.WriteLine($"{cowboy.Job}: Running from hazards starting at {cowboy.Tile.X}, {cowboy.Tile.Y}");
            Node path = this.PathToNearest(cowboy.Tile, tile => this.GetMovementCost(tile) == 1, null, true);

            if (path != null) {
                Console.WriteLine($"{cowboy.Job}: Found path! Target at {path.Tile.X}, {path.Tile.Y}");

                Node moveNode = path;
                while (moveNode.Parent?.Parent != null) {
                    moveNode = moveNode.Parent;
                }

                if (moveNode != path) {
                    if (cowboy.Move(moveNode.Tile))
                        return;
                    Console.WriteLine("{cowboy.job}: Couldn't move. Pathfinding is broken");
                } else {
                    Console.WriteLine("{cowboy.job}: Already safe!");
                }
            } else
                Console.WriteLine("{cowboy.job}: Path not found");
        }

        private bool IsValidTile(int x, int y) => x >= 0 && y >= 0 && x < this.Game.MapWidth && y < this.Game.MapHeight;

        private Tile GetTile(int x, int y) => !this.IsValidTile(x, y) ? null : this.Game.Tiles[this.CoordsToIndex(x, y)];

        private int CoordsToIndex(int x, int y) => x + y * this.Game.MapWidth;

        private Node PathToNearest(Tile startTile, Func<Tile, bool> success, Func<Tile, int> getMovementCost = null, bool ignoreCostIfSuccessful = false) {
            if (getMovementCost == null)
                getMovementCost = this.GetMovementCost;

            List<Node> openNodes = new List<Node>();
            HashSet<int> closedNodes = new HashSet<int>();
            Dictionary<int, Node> nodesByLocation = new Dictionary<int, Node>();

            //console.log("PF: Adding starting node at " + startTile.x + ", " + startTile.y);
            Node start = this.CreateNode(startTile);
            //if (success.call(this, startTile)) return start;
            openNodes.Add(start);
            nodesByLocation[this.CoordsToIndex(startTile.X, startTile.Y)] = start;

            int timeout = 1024; // Stops after this many nodes are checked
            while (openNodes.Any()) {
                openNodes.Sort((a, b) => a.G - b.G);
                Node node = openNodes.First();
                openNodes.RemoveAt(0);
                Tile nodeTile = node.Tile;
                int x = nodeTile.X;
                int y = nodeTile.Y;

                if (success(nodeTile))
                    return node;

                //console.log("PF: Closing node at " + x + ", " + y + " (" + this.coordsToIndex(x, y) + ")");
                closedNodes.Add(this.CoordsToIndex(x, y));

                HashSet<Tile> neighbors = new HashSet<Tile> {
                    this.GetTile(x - 1, y),
                    this.GetTile(x + 1, y),
                    this.GetTile(x, y - 1),
                    this.GetTile(x, y + 1)
                };

                foreach (Tile neighbor in neighbors) {
                    // Tile doesn't exist
                    if (neighbor == null)
                        continue;
                    int neighborLocation = this.CoordsToIndex(neighbor.X, neighbor.Y);

                    // Node has already been checked
                    if (closedNodes.Contains(neighborLocation))
                        continue;

                    int cost = getMovementCost(neighbor);
                    Node neighborNode = this.CreateNode(neighbor, node, cost);
                    Node curNeighborNode;
                    if (!nodesByLocation.TryGetValue(neighborLocation, out curNeighborNode)) {
                        nodesByLocation[neighborLocation] = neighborNode;
                    } else if (curNeighborNode.G > neighborNode.G) {
                        curNeighborNode.Parent = neighborNode.Parent;
                        curNeighborNode.G = neighborNode.G;
                    }

                    // Check if can pass through it, or if final destination
                    if (cost < 0 && !(ignoreCostIfSuccessful && success(neighbor)))
                        continue;

                    // Add to open list
                    if (curNeighborNode == null)
                        openNodes.Add(neighborNode);
                }

                if (--timeout == 0) {
                    //console.log("PF: Pathfinding took too long");
                    return null;
                }
            }

            return null;
        }

        private Node CreateNode(Tile tile, Node parent = null, int? cost = null) {
            return new Node {
                Parent = parent,
                G = parent == null ? 0 : parent.G + (cost ?? 1),
                Tile = tile
            };
        }

        // Returns cost for movement on tile, or negative number for impassible
        private int GetMovementCost(Tile tile) {
            if (tile.Furnishing != null || tile.IsBalcony || tile.Cowboy != null)
                return -1;

            int g = 1;

            // Avoid pathing through hazards
            if (tile.HasHazard)
                return -1; //g += 100;
            if (tile.Bottle != null)
                return -1; //g += 1000;

            // Discourage walking into bottles
            if (tile.TileNorth?.Bottle != null && tile.TileNorth.Bottle.Direction == "South")
                return -1; //g += 1000;
            if (tile.TileEast?.Bottle != null && tile.TileEast.Bottle.Direction == "West")
                return -1; //g += 1000;
            if (tile.TileSouth?.Bottle != null && tile.TileSouth.Bottle.Direction == "North")
                return -1; //g += 1000;
            if (tile.TileWest?.Bottle != null && tile.TileWest.Bottle.Direction == "East")
                return -1; //g += 1000;

            // Discourage walking next to brawlers
            if (tile.TileNorth?.Cowboy != null && tile.TileNorth.Cowboy.Job == "Brawler")
                g += 100;
            if (tile.TileEast?.Cowboy != null && tile.TileEast.Cowboy.Job == "Brawler")
                g += 100;
            if (tile.TileSouth?.Cowboy != null && tile.TileSouth.Cowboy.Job == "Brawler")
                g += 100;
            if (tile.TileWest?.Cowboy != null && tile.TileWest.Cowboy.Job == "Brawler")
                g += 100;

            // Discourage walking into line of fire of sharpshooters

            return g;
        }

        private class Node {
            public Node Parent { get; set; }
            public Tile Tile { get; set; }
            public int G { get; set; }
        }

        // <<-- /Creer-Merge: methods -->>
        #endregion
    }
}
