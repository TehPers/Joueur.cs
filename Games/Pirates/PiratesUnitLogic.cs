using System;
using System.Collections.Generic;
using System.Linq;
using Joueur.cs.Games.Pirates.Helpers;
using static Joueur.cs.Games.Pirates.Helpers.Pathfinder;

namespace Joueur.cs.Games.Pirates {
    public class PiratesUnitLogic : UnitLogic<AI, Unit> {
        public Player Player => this.AI.Player;
        public Game Game => this.AI.Game;
        public Player Opponent => this.Player.Opponent;

        public PiratesUnitLogic(AI ai) : base(ai) { }

        public bool HealIfNeeded(Unit unit) {
            bool shipNeedsHeal = unit.ShipHealth <= this.Game.ShipHealth / 4F;
            bool crewNeedsHeal = unit.CrewHealth <= unit.Crew * this.Game.CrewHealth / 2F;
            bool crewFull = unit.CrewHealth >= unit.Crew * this.Game.CrewHealth;
            bool shipFull = unit.ShipHealth <= 0 || unit.ShipHealth >= this.Game.ShipHealth;
            if (crewFull && shipFull) {
                unit.Healing = false;
            } else if (shipNeedsHeal || crewNeedsHeal) {
                unit.Healing = true;
            }

            if (!unit.Healing)
                return false;

            // Get valid targets
            HashSet<Tile> targets = this.Game.Tiles.Where(t => t.PathableBy(unit) && t.InRange(this.Player.Port.Tile, this.Game.RestRange)).ToHashSet();

            // If not on a target
            if (!targets.Contains(unit.Tile)) {
                // Find a path to the nearest target
                Queue<Node<Tile>> path = new Queue<Node<Tile>>(Pathfinder.FindPath(new[] { unit.Tile }, targets, this.GetNeighbors, this.GetCostFunc(unit)));
                if (!path.Any())
                    return false;

                // First node is the unit's tile
                path.Dequeue();

                // Move along the path
                while (unit.Moves > 0 && path.Any()) {
                    unit.Move(path.Dequeue().Value);
                }
            }

            if (targets.Contains(unit.Tile)) {
                unit.Rest();
            }

            return false;
        }

        public bool DepositGold(Unit unit) {
            if (unit.Gold == 0 || unit.Acted)
                return false;

            // Get valid targets
            Tile target = this.Player.Port.Tile;

            // If not on a target
            if (!target.GetNeighbors().Contains(unit.Tile)) {
                // Find a path to the nearest target
                Queue<Node<Tile>> path = new Queue<Node<Tile>>(Pathfinder.FindPath(new[] { unit.Tile }, new[] { target }, this.GetNeighbors, this.GetCostFunc(unit)));
                if (!path.Any())
                    return false;

                // First node is the unit's tile
                path.Dequeue();

                // Move along the path
                while (unit.Moves > 0 && path.Any()) {
                    unit.Move(path.Dequeue().Value);
                }
            }

            if (target.GetNeighbors().Contains(unit.Tile)) {
                unit.Deposit();
            }

            return true;
        }

        public bool AttackMerchantCrew(Unit unit) {
            if (unit.ShipHealth <= 0 || unit.Acted)
                return false;

            // Get valid targets
            HashSet<Tile> targets = (from u in this.Game.Units
                                     where u.Tile != null && u.TargetPort != null && u.Tile.Port == null
                                     select u.Tile).ToHashSet();

            if (!targets.Any())
                return false;

            // If not on a target
            if (!targets.Any(u => u.InRange(unit.Tile, this.Game.CrewRange))) {
                // Find a path to the nearest target
                Queue<Node<Tile>> path = new Queue<Node<Tile>>(Pathfinder.FindPath(new[] { unit.Tile }, targets, this.GetNeighbors, this.GetCostFunc(unit)));
                if (!path.Any())
                    return false;

                // First node is the unit's tile
                path.Dequeue();

                // Move along the path, except the last node
                while (unit.Moves > 0 && path.Count > 0) {
                    unit.Move(path.Dequeue().Value);
                }
            }

            // Check if at a target
            Tile target = targets.FirstOrDefault(u => u.InRange(unit.Tile, this.Game.CrewRange));
            if (target != null) {
                unit.Attack(target, "crew");
            }

            return true;
        }

        public bool AttackMerchantPort(Unit unit) {
            if (unit.ShipHealth <= 0 || unit.Acted)
                return false;

            // Get valid targets
            HashSet<Tile> targets = (from p in this.Game.Ports
                                     where p.Owner == null
                                     select p.Tile).ToHashSet();

            if (!targets.Any())
                return false;

            // If not on a target
            if (!targets.Any(u => u.InRange(unit.Tile, 1))) {
                // Find a path to the nearest target
                Queue<Node<Tile>> path = new Queue<Node<Tile>>(Pathfinder.FindPath(new[] { unit.Tile }, targets, this.GetNeighbors, this.GetCostFunc(unit)));
                if (!path.Any())
                    return false;

                // First node is the unit's tile
                path.Dequeue();

                // Move along the path, except the last node
                while (unit.Moves > 0 && path.Count > 0) {
                    unit.Move(path.Dequeue().Value);
                }
            }

            // Check if at a target
            Tile target = targets.FirstOrDefault(u => u.InRange(unit.Tile, 1));
            if (target != null) {
                return false;
            }

            return true;
        }

        public bool AttackMerchantShip(Unit unit) {
            if (unit.ShipHealth <= 0 || unit.Acted)
                return false;

            // Get valid targets
            HashSet<Tile> targets = (from u in this.Game.Units
                where u.Tile != null && u.TargetPort != null && u.Tile.Port == null
                select u.Tile).ToHashSet();

            if (!targets.Any())
                return false;

            // If not on a target
            if (!targets.Any(u => u.InRange(unit.Tile, this.Game.ShipRange))) {
                // Find a path to the nearest target
                Queue<Node<Tile>> path = new Queue<Node<Tile>>(Pathfinder.FindPath(new[] { unit.Tile }, targets, this.GetNeighbors, this.GetCostFunc(unit)));
                if (!path.Any())
                    return false;

                // First node is the unit's tile
                path.Dequeue();

                // Move along the path, except the last node
                while (unit.Moves > 0 && path.Count > 0) {
                    unit.Move(path.Dequeue().Value);
                }
            }

            // Check if at a target
            Tile target = targets.FirstOrDefault(u => u.InRange(unit.Tile, this.Game.ShipRange));
            if (target != null) {
                unit.Attack(target, "ship");
            }

            return true;
        }

        public bool HijackShip(Unit unit) {
            if (unit.ShipHealth <= 0 || unit.Acted || unit.Crew == 1)
                return false;

            // Get valid targets
            HashSet<Tile> targets = (from u in this.Game.Units
                                     where u.Owner == null && u.TargetPort == null
                                     select u.Tile).ToHashSet();

            if (!targets.Any())
                return false;

            // If not on a target
            if (!targets.Any(u => u.InRange(unit.Tile, 1))) {
                // Find a path to the nearest target
                Queue<Node<Tile>> path = new Queue<Node<Tile>>(Pathfinder.FindPath(new[] { unit.Tile }, targets, this.GetNeighbors, this.GetCostFunc(unit)));
                if (!path.Any())
                    return false;

                // First node is the unit's tile
                path.Dequeue();

                // Move along the path, except the last node
                while (unit.Moves > 0 && path.Count > 0) {
                    unit.Move(path.Dequeue().Value);
                }
            }

            // Check if at a target
            Tile target = targets.FirstOrDefault(t => t.InRange(unit.Tile, 1));
            if (target != null) {
                unit.Split(target);
                this.AI.AssignRole(target.Unit);
            }

            return true;
        }

        public bool AttackEnemyCrew(Unit unit) {
            if (unit.ShipHealth <= 0 || unit.Acted)
                return false;

            // Get valid targets
            HashSet<Tile> targets = (from u in this.Game.Units
                                     where u.Tile != null && u.Owner == this.Player.Opponent && u.ShipHealth > 0
                                     select u.Tile).ToHashSet();

            if (!targets.Any())
                return false;

            // If not on a target
            if (!targets.Any(u => u.InRange(unit.Tile, this.Game.CrewRange))) {
                // Find a path to the nearest target
                Queue<Node<Tile>> path = new Queue<Node<Tile>>(Pathfinder.FindPath(new[] { unit.Tile }, targets, this.GetNeighbors, this.GetCostFunc(unit)));
                if (!path.Any())
                    return false;

                // First node is the unit's tile
                path.Dequeue();

                // Move along the path, except the last node
                while (unit.Moves > 0 && path.Count > 0) {
                    unit.Move(path.Dequeue().Value);
                }
            }

            // Check if at a target
            Tile target = targets.FirstOrDefault(u => u.InRange(unit.Tile, this.Game.CrewRange));
            if (target != null) {
                unit.Attack(target, "crew");
            }

            return true;
        }

        public bool AttackEnemyShip(Unit unit) {
            if (unit.ShipHealth <= 0 || unit.Acted)
                return false;

            // Get valid targets
            HashSet<Tile> targets = (from u in this.Game.Units
                where u.Tile != null && u.Owner == this.Player.Opponent && u.ShipHealth > 0
                select u.Tile).ToHashSet();

            if (!targets.Any())
                return false;

            // If not on a target
            if (!targets.Any(u => u.InRange(unit.Tile, this.Game.ShipRange))) {
                // Find a path to the nearest target
                Queue<Node<Tile>> path = new Queue<Node<Tile>>(Pathfinder.FindPath(new[] { unit.Tile }, targets, this.GetNeighbors, this.GetCostFunc(unit)));
                if (!path.Any())
                    return false;

                // First node is the unit's tile
                path.Dequeue();

                // Move along the path, except the last node
                while (unit.Moves > 0 && path.Count > 0) {
                    unit.Move(path.Dequeue().Value);
                }
            }

            // Check if at a target
            Tile target = targets.FirstOrDefault(u => u.InRange(unit.Tile, this.Game.ShipRange));
            if (target != null) {
                unit.Attack(target, "ship");
            }

            return true;
        }

        public bool BuryGold(Unit unit) {
            if (unit.ShipHealth > 0 || unit.Acted)
                return false;

            Tile[] options = unit.Tile.GetNeighbors().Where(t => t.Type == "land").ToArray();
            Random rnd = new Random();
            unit.Move(options[rnd.Next(options.Length)]);

            if (unit.Gold == 0) {
                // Withdraw some gold

            }

            return true;
        }

        public bool DefendShip(Unit unit) {
            if (unit.ShipHealth == 0 || unit.Acted)
                return false;

            Unit target = this.Game.Units.FirstOrDefault(u => u.Tile != null && u.Owner == this.Player.Opponent && unit.Tile.InRange(u.Tile, this.Game.ShipRange));
            if (target != null) {
                unit.Attack(target.Tile, "ship");
                return true;
            }

            return false;
        }

        public bool CheckCrew(Unit unit) {
            if (unit.Crew < 3) {
                unit.Role = Roles.RECREW;
                this.AI.UnitsToAct.Enqueue(unit);
                return true;
            }

            return false;
        }

        public bool ReturnHome(Unit unit) {
            if (unit.Acted)
                return false;

            // Get valid targets
            Tile target = this.Player.Port.Tile;

            // If not on a target
            if (unit.Tile != target) {
                // Find a path to the nearest target
                Queue<Node<Tile>> path = new Queue<Node<Tile>>(Pathfinder.FindPath(new[] { unit.Tile }, new[] { target }, this.GetNeighbors, this.GetCostFunc(unit)));
                if (!path.Any())
                    return false;

                // First node is the unit's tile
                path.Dequeue();

                // Move along the path
                while (unit.Moves > 0 && path.Any()) {
                    unit.Move(path.Dequeue().Value);
                }
            }

            return true;
        }

        public bool PassiveFlee(Unit unit) {
            if (unit.Acted || !this.IsDangerous(unit.Tile))
                return false;

            // Get valid targets
            HashSet<Tile> targets = this.Game.Tiles.Where(t => t.PathableBy(unit) && !this.IsDangerous(t)).ToHashSet();

            // If not on a target
            if (!targets.Contains(unit.Tile)) {
                // Find a path to the nearest target
                Queue<Node<Tile>> path = new Queue<Node<Tile>>(Pathfinder.FindPath(new[] { unit.Tile }, targets, this.GetNeighbors, this.GetCostFunc(unit)));
                if (!path.Any())
                    return false;

                // First node is the unit's tile
                path.Dequeue();

                // Move along the path
                while (unit.Moves > 0 && path.Any()) {
                    unit.Move(path.Dequeue().Value);
                }
            }

            return true;
        }

        public bool ManageBury(Unit unit) {
            if (unit.ShipHealth > 0)
                return false;


        }

        public bool PissOff(Unit unit) {
            if (unit.Acted || !this.IsDangerous(unit.Tile))
                return false;

            // Get valid targets
            HashSet<Tile> targets = this.Game.Tiles.Where(t => t.PathableBy(unit) && !t.InRange(this.Player.Port.Tile, 10)).ToHashSet();

            // If not on a target
            if (targets.Contains(unit.Tile)) {
                return false;
            } else {
                // Find a path to the nearest target
                Queue<Node<Tile>> path = new Queue<Node<Tile>>(Pathfinder.FindPath(new[] { unit.Tile }, targets, this.GetNeighbors, this.GetCostFunc(unit)));
                if (!path.Any())
                    return false;

                // First node is the unit's tile
                path.Dequeue();

                // Move along the path
                while (unit.Moves > 0 && path.Any()) {
                    unit.Move(path.Dequeue().Value);
                }
            }

            return true;
        }

        private IEnumerable<Tile> GetNeighbors(Tile tile) {
            return tile.GetNeighbors();
        }

        private Func<Tile, Node<Tile>, float?> GetCostFunc(Unit unit) {
            return (tile, node) => {
                // Don't pathfind over non-pathable tiles
                if (!tile.PathableBy(unit))
                    return null;

                // Don't path through tiles that could result in this unit dying
                //if (unit.ShipHealth > 0 && this.PotentialDamage(tile) >= unit.ShipHealth)
                //    return null;

                // Avoid enemy units if possible
                //if (this.Game.Units.Any(t => t.Unit != null && t.Unit.Owner != this.Player))
                //return 10;

                return 1;
            };
        }

        public int PotentialDamage(Tile tile) {
            IEnumerable<Unit> dangerousShips = this.Game.Units.Where(u => {
                if (u.Tile == null)
                    return false;
                if (u.Owner == this.Player)
                    return false;
                if (u.ShipHealth == 0)
                    return false;
                return u.Tile.InRange(u.Tile, this.Game.ShipRange + this.Game.ShipMoves);
            });

            return dangerousShips.Count() * this.Game.ShipDamage;
        }

        public bool IsDangerous(Tile tile) {
            return this.Opponent.Units.Any(u => {
                if (u.Owner == this.Player)
                    return false;
                if (u.Tile == null)
                    return false;
                if (u.ShipHealth > 0)
                    return tile.InRange(u.Tile, this.Game.ShipMoves + this.Game.ShipMoves);
                if (u.Crew > 0)
                    return tile.InRange(u.Tile, this.Game.CrewRange + this.Game.CrewMoves);
                return false;
            });
        }
    }
}