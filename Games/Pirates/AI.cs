// This is where you build your AI for the Pirates game.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// <<-- Creer-Merge: usings -->> - Code you add between this comment and the end comment will be preserved between Creer re-runs.
using System.Diagnostics;
using Joueur.cs.Helpers;

// <<-- /Creer-Merge: usings -->>

namespace Joueur.cs.Games.Pirates {
    /// <summary>
    /// This is where you build your AI for Pirates.
    /// </summary>
    public class AI : BaseAI {
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

        public Player Opponent => this.Player.Opponent;

        public int Crew => this.Player.Units.Sum(u => u.Crew);
        public int Ships => this.Player.Units.Count(u => u.ShipHealth > 0);

        public Dictionary<Roles, PiratesUnitLogic> UnitLogics { get; } = new Dictionary<Roles, PiratesUnitLogic>();
        public Queue<Unit> UnitsToAct { get; } = new Queue<Unit>();

        private bool ShowMap => true;

        // <<-- /Creer-Merge: properties -->>
        #endregion


        #region Methods
        /// <summary>
        /// This returns your AI's name to the game server. Just replace the string.
        /// </summary>
        /// <returns>Your AI's name</returns>
        public override string GetName() {
            // <<-- Creer-Merge: get-name -->> - Code you add between this comment and the end comment will be preserved between Creer re-runs.
            return "Not A Dank Meme"; // REPLACE THIS WITH YOUR TEAM NAME!
            // <<-- /Creer-Merge: get-name -->>
        }

        /// <summary>
        /// This is automatically called when the game first starts, once the Game and all GameObjects have been initialized, but before any players do anything.
        /// </summary>
        /// <remarks>
        /// This is a good place to initialize any variables you add to your AI or start tracking game objects.
        /// </remarks>
        public override void Start() {
            // <<-- Creer-Merge: start -->> - Code you add between this comment and the end comment will be preserved between Creer re-runs.
            base.Start();

            // Setup unit logic
            PiratesUnitLogic curLogic;

            // None
            this.UnitLogics.Add(Roles.NONE, curLogic = new PiratesUnitLogic(this));

            // Recrew
            this.UnitLogics.Add(Roles.RECREW, curLogic = new PiratesUnitLogic(this));
            curLogic.AddTask(curLogic.HealIfNeeded);
            curLogic.AddTask(curLogic.DepositGold);
            curLogic.AddTask(curLogic.ReturnHome);
            curLogic.AddTask(curLogic.PassiveFlee);
            curLogic.AddTask(curLogic.PissOff);

            // Merchant Hijackers
            this.UnitLogics.Add(Roles.MERCHANT_HIJACKER, curLogic = new PiratesUnitLogic(this));
            //curLogic.AddTask(curLogic.CheckCrew);
            curLogic.AddTask(curLogic.HealIfNeeded);
            curLogic.AddTask(curLogic.DepositGold);
            curLogic.AddTask(curLogic.HijackShip);
            curLogic.AddTask(curLogic.DefendShip);
            //curLogic.AddTask(curLogic.AttackMerchantCrew);
            curLogic.AddTask(curLogic.AttackMerchantShip);
            curLogic.AddTask(curLogic.AttackMerchantPort);
            //curLogic.AddTask(curLogic.AttackEnemyCrew);
            curLogic.AddTask(curLogic.PassiveFlee);
            curLogic.AddTask(curLogic.PissOff);

            // Enemy Hijackers
            this.UnitLogics.Add(Roles.ENEMY_HIJACKER, curLogic = new PiratesUnitLogic(this));
            //curLogic.AddTask(curLogic.CheckCrew);
            curLogic.AddTask(curLogic.HealIfNeeded);
            curLogic.AddTask(curLogic.DepositGold);
            curLogic.AddTask(curLogic.HijackShip);
            //curLogic.AddTask(curLogic.AttackEnemyCrew);
            curLogic.AddTask(curLogic.AttackEnemyShip);
            curLogic.AddTask(curLogic.PassiveFlee);

            Console.Clear();
            // <<-- /Creer-Merge: start -->>
        }

        /// <summary>
        /// This is automatically called every time the game (or anything in it) updates.
        /// </summary>
        /// <remarks>
        /// If a function you call triggers an update, this will be called before that function returns.
        /// </remarks>
        public override void GameUpdated() {
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
        public override void Ended(bool won, string reason) {
            // <<-- Creer-Merge: ended -->> - Code you add between this comment and the end comment will be preserved between Creer re-runs.
            base.Ended(won, reason);

            if (Debugger.IsAttached)
                Console.ReadLine();
            // <<-- /Creer-Merge: ended -->>
        }


        /// <summary>
        /// This is called every time it is this AI.player's turn.
        /// </summary>
        /// <returns>Represents if you want to end your turn. True means end your turn, False means to keep your turn going and re-call this function.</returns>
        public bool RunTurn() {
            // <<-- Creer-Merge: runTurn -->> - Code you add between this comment and the end comment will be preserved between Creer re-runs.
            // Put your game logic here for runTurn
            this.DisplayMap();
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            Logger.Log($"--[ Turn {this.Game.CurrentTurn} / {this.Game.MaxTurns} ]--");
            Logger.Log($"Infamy: {this.Player.Infamy} vs. {this.Opponent.Infamy}");
            Logger.Log($"Gold: {this.Player.Gold} ({this.Player.NetWorth(this)}) vs. {this.Opponent.Gold} ({this.Player.Opponent.NetWorth(this)})");
            Logger.Log($"Units: {string.Join(", ", this.Player.Units.GroupBy(u => u.Role, (r, units) => $"{r}: {units.Count()}"))}");

            // Turn logic
            this.UnitsToAct.Clear();
            foreach (Unit unit in this.Player.Units) {
                if (unit.Tile != null) {
                    this.UnitsToAct.Enqueue(unit);
                }
            }

            while (this.UnitsToAct.Any()) {
                Unit unit = this.UnitsToAct.Dequeue();
                if (unit.Owner != this.Player)
                    continue;

                // Get unit logic
                if (!this.UnitLogics.TryGetValue(unit.Role, out PiratesUnitLogic logic)) {
                    Console.WriteLine($"No logic for {unit.Role}");
                    continue;
                }

                // Perform tasks
                foreach (Func<Unit, bool> task in logic.Tasks) {
                    unit.Log(new string('a', 100));

                    if (task(unit)) {
                        break;
                    }
                }
            }

            // Try to spawn some units
            Port port = this.Player.Port;
            Unit portUnit = port.Tile.Unit;

            if (portUnit == null && this.Player.Units.All(u => u.Role != Roles.BURY)) {

            } else if (portUnit == null || portUnit.Crew < 3) {
                // Spawn as much crew as possible
                while (port.Gold >= this.Game.CrewCost && this.Player.Gold >= this.Game.CrewCost) {
                    port.Spawn("Crew");
                }

                if (port.Tile.Unit != null) {
                    port.Tile.Unit.Role = Roles.NONE;
                }
            } else if (portUnit.ShipHealth > 0 && (portUnit.Role == Roles.NONE || portUnit.Role == Roles.RECREW)) {
                this.AssignRole(portUnit);
            } else if (portUnit.ShipHealth > 0 && portUnit.Moves > 0 && !portUnit.Acted && portUnit.Tile.GetNeighbors().Any(t => t.PathableBy(portUnit))) {
                portUnit.Move(portUnit.Tile.GetNeighbors().First(t => t.PathableBy(portUnit)));
            } else if (this.Player.Gold >= this.Game.ShipCost && port.Gold >= this.Game.ShipCost) {
                // Spawn a ship if there's crew there
                port.Spawn("Ship");
                this.AssignRole(portUnit);
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
        List<Tile> FindPath(Tile start, Tile goal) {
            // no need to make a path to here...
            if (start == goal) {
                return new List<Tile>();
            }

            // the tiles that will have their neighbors searched for 'goal'
            Queue<Tile> fringe = new Queue<Tile>();

            // How we got to each tile that went into the fringe.
            Dictionary<Tile, Tile> cameFrom = new Dictionary<Tile, Tile>();

            // Enqueue start as the first tile to have its neighbors searched.
            fringe.Enqueue(start);

            // keep exploring neighbors of neighbors... until there are no more.
            while (fringe.Any()) {
                // the tile we are currently exploring.
                Tile inspect = fringe.Dequeue();

                // cycle through the tile's neighbors.
                foreach (Tile neighbor in inspect.GetNeighbors()) {
                    if (neighbor == goal) {
                        // Follow the path backward starting at the goal and return it.
                        List<Tile> path = new List<Tile>();
                        path.Add(goal);

                        // Starting at the tile we are currently at, insert them retracing our steps till we get to the starting tile
                        for (Tile step = inspect; step != start; step = cameFrom[step]) {
                            path.Insert(0, step);
                        }

                        return path;
                    }

                    // if the tile exists, has not been explored or added to the fringe yet, and it is pathable
                    if (neighbor != null && !cameFrom.ContainsKey(neighbor) && neighbor.IsPathable()) {
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

        public void AssignRole(Unit unit) {
            int merchs = this.Player.Units.Count(u => u.Role == Roles.MERCHANT_HIJACKER);
            int enems = this.Player.Units.Count(u => u.Role == Roles.ENEMY_HIJACKER);

            if (merchs < 3) {
                unit.Role = Roles.MERCHANT_HIJACKER;
            } else {
                unit.Role = Roles.ENEMY_HIJACKER;
            }
        }

        private void DisplayMap() {
            if (!this.ShowMap)
                return;

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
                    if (t.Port != null) {
                        Console.BackgroundColor = t.Port.Owner == this.Player ? ConsoleColor.DarkGreen : t.Port.Owner == null ? ConsoleColor.DarkYellow : ConsoleColor.DarkRed;
                    } else if (t.Type == "land") {
                        Console.BackgroundColor = t.Decoration ? ConsoleColor.DarkGray : ConsoleColor.Gray;
                    } else {
                        Console.BackgroundColor = t.Decoration ? ConsoleColor.DarkBlue : ConsoleColor.Blue;
                    }

                    // Character to display
                    char foreground = ' ';
                    Console.ForegroundColor = ConsoleColor.White;

                    // Tile specific stuff
                    if (t.Unit != null) {
                        Console.ForegroundColor = t.Unit.Owner == this.Player ? ConsoleColor.Green : t.Unit.Owner == null ? ConsoleColor.Yellow : ConsoleColor.Red;
                        foreground = t.Unit.ShipHealth > 0 ? 'S' : 'C';
                    } else if (t.Gold > 0) {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        foreground = '$';
                    } else if (false && this.Game.Units.Any(u => u.Path.Contains(t))) {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        foreground = '*';
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
            Console.Write(new string(' ', Math.Max(Console.WindowHeight, Console.WindowWidth * (Console.WindowHeight - top) - 1)));
            Console.SetCursorPosition(left, top);
        }

        // <<-- /Creer-Merge: methods -->>
        #endregion
    }
}
