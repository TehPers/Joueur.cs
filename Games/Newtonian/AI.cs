// This is where you build your AI for the Newtonian game.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
// <<-- Creer-Merge: usings -->> - Code you add between this comment and the end comment will be preserved between Creer re-runs.
// you can add additional using(s) here
using System.Runtime.CompilerServices;
using Joueur.cs.Conflux.Collections;
using Joueur.cs.Conflux.Matching;
using Joueur.cs.Games.Catastrophe;
using Joueur.cs.Games.Pirates;
using Joueur.cs.Helpers;

// <<-- /Creer-Merge: usings -->>

namespace Joueur.cs.Games.Newtonian {
    /// <summary>
    /// This is where you build your AI for Newtonian.
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
        private readonly Dictionary<string, NewtonianUnitLogic> _unitLogics = new Dictionary<string, NewtonianUnitLogic>();
        public Player Opponent => this.Player.Opponent;
        public Queue<Unit> UnitsToAct { get; } = new Queue<Unit>();

#if DEBUG
        private const bool DEBUG = true;
#else
        private const bool DEBUG = false;
#endif
        // <<-- /Creer-Merge: properties -->>
        #endregion


        #region Methods
        /// <summary>
        /// This returns your AI's name to the game server. Just replace the string.
        /// </summary>
        /// <returns>Your AI's name</returns>
        public override string GetName() {
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
        public override void Start() {
            // <<-- Creer-Merge: start -->> - Code you add between this comment and the end comment will be preserved between Creer re-runs.
            base.Start();

            NewtonianUnitLogic curLogic;

            this._unitLogics.Add("intern", curLogic = new NewtonianUnitLogic(this));
            curLogic.AddTask(curLogic.ShowDebugInfo);
            curLogic.AddTask(curLogic.CheckState);
            curLogic.AddTask(curLogic.HealIfNeeded);
            curLogic.AddTask(curLogic.AttackNeighbors);
            curLogic.AddTask(curLogic.CollectFactory(new (string, Func<Tile, int>)[] { ("blueium ore", t => CountOre(t, t.BlueiumOre, this.Player.Pressure <= this.Player.Heat)), ("redium ore", t => CountOre(t, t.RediumOre, this.Player.Heat <= this.Player.Pressure)) }));
            curLogic.AddTask(curLogic.DropOreFactory("blueium ore", "blueium", u => u.BlueiumOre, t => t.BlueiumOre));
            curLogic.AddTask(curLogic.DropOreFactory("redium ore", "redium", u => u.RediumOre, t => t.RediumOre));
            curLogic.AddTask(curLogic.StunEnemies);

            this._unitLogics.Add("physicist", curLogic = new NewtonianUnitLogic(this));
            curLogic.AddTask(curLogic.ShowDebugInfo);
            curLogic.AddTask(curLogic.CheckState);
            curLogic.AddTask(curLogic.HealIfNeeded);
            curLogic.AddTask(curLogic.AttackNeighbors);
            curLogic.AddTask(curLogic.WorkMachine);
            curLogic.AddTask(curLogic.StunEnemies);

            this._unitLogics.Add("manager", curLogic = new NewtonianUnitLogic(this));
            curLogic.AddTask(curLogic.ShowDebugInfo);
            curLogic.AddTask(curLogic.CheckState);
            curLogic.AddTask(curLogic.HealIfNeeded);
            curLogic.AddTask(curLogic.AttackNeighbors);
            curLogic.AddTask(curLogic.CollectFactory(new (string, Func<Tile, int>)[] { ("blueium", t => t.Blueium), ("redium", t => t.Redium) }));
            curLogic.AddTask(curLogic.DropRefined);
            curLogic.AddTask(curLogic.StunEnemies);

            Console.Clear();

            int CountOre(Tile t, int tileCount, bool condition) {
                if (t.Machine != null) {
                    return 0;
                }

                if (!condition) {
                    return 0;
                }

                return tileCount;
            }
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
            /*this.DisplayMap(); // be careful using this as it will probably cause your client to time out in this function.
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;*/
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

            if (Debugger.IsAttached) {
                Console.ReadLine();
            }
            // <<-- /Creer-Merge: ended -->>
        }


        /// <summary>
        /// This is called every time it is this AI.player's turn.
        /// </summary>
        /// <returns>Represents if you want to end your turn. True means end your turn, False means to keep your turn going and re-call this function.</returns>
        public bool RunTurn() {
            // <<-- Creer-Merge: runTurn -->> - Code you add between this comment and the end comment will be preserved between Creer re-runs.
#if !DEBUG
            try {
#endif

            // Debug info
            if (AI.DEBUG) {
                this.DisplayMap();
                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.Black;

                Logger.Log($"--[ Turn {this.Game.CurrentTurn} / {this.Game.MaxTurns} ]--");
                Logger.Log($"Heat: {this.Player.Heat}, Pressure: {this.Player.Pressure}, Points: {this.Player.Heat * this.Player.Pressure} vs. Heat: {this.Opponent.Heat}, Pressure: {this.Opponent.Pressure}, Points: {this.Opponent.Heat * this.Opponent.Pressure}");
                Logger.Log($"Units: {string.Join(", ", this.Player.Units.GroupBy(u => u.Job.Title, (job, units) => $"{job}: {units.Count()}"))}");
                Logger.Log($"Opponent's Units: {string.Join(", ", this.Opponent.Units.GroupBy(u => u.Job.Title, (job, units) => $"{job}: {units.Count()}"))}");

                Logger.Log("");
                Logger.Log("-- Machines --");
                foreach (Machine machine in this.Game.Machines) {
                    Logger.Log($"Machine at ({machine.Tile.X}, {machine.Tile.Y})> Ore: {(machine.OreType == "blueium" ? machine.Tile.BlueiumOre : machine.Tile.RediumOre)} / {machine.RefineInput}");
                }
            }

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
                if (!this._unitLogics.TryGetValue(unit.Job.Title, out NewtonianUnitLogic logic)) {
                    Console.WriteLine($"No logic for {unit.Job.Title}");
                    continue;
                }

                // Perform tasks
                foreach (Func<Unit, bool> task in logic.Tasks) {
                    if (task(unit)) {
                        break;
                    }
                }
            }

            // Stupid shit
            if (this.Game.CurrentTurn < 2 && this.Opponent.Name.ToUpper().Contains("CHRISTIAN")) {
                foreach (GameObject obj in this.Game.GameObjects.Values.Select(obj => obj as GameObject).Where(obj => obj != null).ToArray()) {
                    obj.Log("Heck.");
                }
            }

#if !DEBUG
            } catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }
#endif

            return true;
            // <<-- /Creer-Merge: runTurn -->>
        }

        // <<-- Creer-Merge: methods -->> - Code you add between this comment and the end comment will be preserved between Creer re-runs.
        public override void Invalid(string message) {
            base.Invalid(message);
        }

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
                    if (t.Machine != null) {
                        Console.BackgroundColor = (t.Machine.OreType == "redium") ? ConsoleColor.DarkRed : ConsoleColor.DarkBlue;
                    } else if (t.IsWall == true) {
                        if (t.Decoration == 1 || t.Decoration == 2) {
                            Console.BackgroundColor = ConsoleColor.DarkGray;  // Black;
                        } else {
                            Console.BackgroundColor = ConsoleColor.DarkGray;
                        }
                    } else {
                        if (t.Decoration == 1 || t.Decoration == 2) {
                            Console.BackgroundColor = ConsoleColor.DarkYellow;
                        } else {
                            Console.BackgroundColor = ConsoleColor.Gray;
                        }
                    }

                    // Character to display
                    char foreground = t.Machine == null ? '·' : 'M';
                    Console.ForegroundColor = ConsoleColor.White;

                    // Tile specific stuff
                    if (t.Unit != null) {
                        Console.ForegroundColor = t.Unit.Owner == this.Player ? ConsoleColor.Green : ConsoleColor.Red;
                        foreground = t.Unit.Job.Title[0] == 'i' ? 'I' : t.Unit.Job.Title[0] == 'm' ? 'M' : 'P'; //t.Unit.ShipHealth > 0 ? 'S' : 'C';
                    }
                    if (t.Blueium > 0 || t.Redium > 0) {
                        Console.BackgroundColor = t.Blueium >= t.Redium ? ConsoleColor.DarkBlue : ConsoleColor.DarkRed;
                        if (foreground == '·') {
                            foreground = 'R';
                        }
                    } else if (t.BlueiumOre > 0 || t.RediumOre > 0) {
                        Console.BackgroundColor = t.BlueiumOre >= t.RediumOre ? ConsoleColor.DarkBlue : ConsoleColor.DarkRed;
                        if (foreground == '·') {
                            foreground = 'O';
                        }
                    } else if (t.Owner != null) {
                        if (t.Type == "spawn") {
                            Console.BackgroundColor = t.Owner == this.Player ? ConsoleColor.Cyan : ConsoleColor.Magenta;
                        } else if (t.Type == "generator") {
                            Console.BackgroundColor = t.Owner == this.Player ? ConsoleColor.DarkCyan : ConsoleColor.DarkMagenta;
                        }
                    } else if (t.Type == "conveyor") {
                        if (t.Direction == "north") {
                            foreground = '^';
                        } else if (t.Direction == "east") {
                            foreground = '>';
                        } else if (t.Direction == "west") {
                            foreground = '<';
                        } else if (t.Direction == "blank") {
                            foreground = '_';
                        } else {
                            foreground = 'V';
                        }
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
