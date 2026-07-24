using System;
using System.Collections.Generic;
using System.Linq;

namespace Ecalpon.Combat
{
    public class CombatManager
    {
        public CombatState CurrentState { get; private set; }

        private List<Combatant> Combatants = new List<Combatant>();

        private int CurrentCombatantIndex = 0;

        private List<string> DisplayLog = new List<string>();

        public IReadOnlyList<string> RecentMessages => DisplayLog;

        private System.Random RNG = new System.Random();

        private const int GRID_ROWS = 16;
        private const int GRID_COLS = 16;

        public int CursorRow { get; private set; }
        public int CursorCol { get; private set; }

        public bool TryConfirmTarget(out Combatant target)
        {
            target = GetCombatantAt(CursorRow, CursorCol);

            if (target == null)
            {
                AddMessage("No target there.");
                return false;
            }

            if (!target.IsAlive)
            {
                AddMessage(target.Name + " is already down.");
                target = null;
                return false;
            }

            if (target.IsPlayerControlled)
            {
                AddMessage("You can't attack " + target.Name + ".");
                target = null;
                return false;
            }

            Combatant attacker = CurrentCombatant();
            int rowDistance = Math.Abs(target.GridRow - attacker.GridRow);
            int colDistance = Math.Abs(target.GridCol - attacker.GridCol);

            if (rowDistance > 1 || colDistance > 1)
            {
                AddMessage(target.Name + " is out of melee range.");
                target = null;
                return false;
            }

            return true;
        }

        private Combatant GetCombatantAt(int row, int col)
        {
            return Combatants.FirstOrDefault(c => c.IsAlive && c.GridRow == row && c.GridCol == col);
        }

        public void BeginMeleeTargeting(Combatant attacker)
        {
            CursorRow = attacker.GridRow;
            CursorCol = attacker.GridCol;
        }

        public void MoveCursor(int rowDelta, int colDelta)
        {
            int newRow = CursorRow + rowDelta;
            int newCol = CursorCol + colDelta;

            if (newRow < 0)
                newRow = 0;
            else if (newRow >= GRID_ROWS)
                newRow = GRID_ROWS - 1;

            if (newCol < 0)
                newCol = 0;
            else if (newCol >= GRID_COLS)
                newCol = GRID_COLS - 1;

            CursorRow = newRow;
            CursorCol = newCol;
        }

        public void StartCombat(List<Combatant> playerParty, List<Combatant> enemies)
        {
            Combatants.Clear();
            Combatants.AddRange(playerParty);
            Combatants.AddRange(enemies);

            RollInitiative();
            CurrentCombatantIndex = 0;

            TransitionTo(CombatState.Initializing);

            if (CurrentCombatant().IsPlayerControlled)
                TransitionTo(CombatState.PlayerTurn);
            else
                TransitionTo(CombatState.EnemyTurn);
        }

        public void TransitionTo(CombatState newState)
        {
            CurrentState = newState;

            switch (CurrentState)
            {
                case CombatState.PlayerTurn:
                    PrepareCurrentCombatantTurn();
                    break;

                case CombatState.EnemyTurn:
                    PrepareCurrentCombatantTurn();
                    break;

                case CombatState.Victory:
                    AddMessage("Victory! The enemy has been defeated.");
                    break;

                case CombatState.Defeat:
                    AddMessage("Your party has fallen...");
                    break;

                case CombatState.SelectingTarget:
                    BeginMeleeTargeting(CurrentCombatant());
                    break;
            }
        }

        // =====================================================
        // TURN ORDER
        // =====================================================

        private void RollInitiative()
        {
            foreach (var combatant in Combatants)
                combatant.Initiative = RNG.Next(1, 20) + combatant.Level;

            Combatants = Combatants
                .OrderByDescending(c => c.Initiative)
                .ToList();
        }

        private void PrepareCurrentCombatantTurn()
        {
            Combatant current = CurrentCombatant();
            if (current == null)
                return;

            current.StartTurn();

            if (current.IsPlayerControlled)
                AddMessage(current.Name + "'s turn.");
            else
                AddMessage(current.Name + " acts...");
        }

        public Combatant CurrentCombatant()
        {
            if (Combatants.Count == 0)
                return null;

            return Combatants[CurrentCombatantIndex];
        }

        public void EndCurrentTurn()
        {
            if (AllEnemiesDefeated())
            {
                TransitionTo(CombatState.Victory);
                return;
            }

            if (AllPlayerCombatantsDefeated())
            {
                TransitionTo(CombatState.Defeat);
                return;
            }

            AdvanceToNextCombatant();
        }

        public string LastMessage()
        {
            if (DisplayLog.Count == 0)
                return "";

            return DisplayLog[DisplayLog.Count - 1];
        }

        private void AdvanceToNextCombatant()
        {
            int attempts = 0;

            do
            {
                CurrentCombatantIndex++;

                if (CurrentCombatantIndex >= Combatants.Count)
                    CurrentCombatantIndex = 0;

                attempts++;

            } while (!Combatants[CurrentCombatantIndex].IsAlive
                     && attempts <= Combatants.Count);

            Combatant next = CurrentCombatant();

            System.Diagnostics.Debug.WriteLine("Next combatant: " + next.Name + " index: " + CurrentCombatantIndex);

            if (next.IsPlayerControlled)
                TransitionTo(CombatState.PlayerTurn);
            else
                TransitionTo(CombatState.EnemyTurn);
        }

        // =====================================================
        // COMBAT RESOLUTION — AD&D THAC0
        // =====================================================

        public bool RollToHit(Combatant attacker, Combatant defender)
        {
            int roll = RNG.Next(1, 21);
            int needed = attacker.ThacO - defender.ArmorClass;

            if (roll >= needed)
            {
                AddMessage(attacker.Name + " hits " + defender.Name
                    + "! (rolled " + roll + ", needed " + needed + ")");
                return true;
            }
            else
            {
                AddMessage(attacker.Name + " misses " + defender.Name
                    + ". (rolled " + roll + ", needed " + needed + ")");
                return false;
            }
        }

        public int RollDamage(Combatant attacker)
        {
            int damage = RNG.Next(attacker.DamageMin, attacker.DamageMax + 1)
                         + attacker.DamageBonus;

            return damage;
        }

        public void ApplyDamage(Combatant target, int damage)
        {
            target.HitPoints -= damage;

            if (!target.IsAlive)
            {
                AddMessage(target.Name + " has been slain!");
            }
            else
            {
                AddMessage(target.Name + " takes " + damage + " damage."
                    + " (" + target.HitPoints + "/" + target.MaxHitPoints + " HP)");
            }
        }

        // =====================================================
        // VICTORY / DEFEAT CONDITIONS
        // =====================================================

        private bool AllEnemiesDefeated()
        {
            return Combatants
                .Where(c => c.Type == CombatantType.Enemy)
                .All(c => !c.IsAlive);
        }

        private bool AllPlayerCombatantsDefeated()
        {
            return Combatants
                .Where(c => c.IsPlayerControlled)
                .All(c => !c.IsAlive);
        }

        // =====================================================
        // MESSAGE LOG
        // =====================================================

        private void AddMessage(string message)
        {
            DisplayLog.Add(message);

            if (DisplayLog.Count > 8)
                DisplayLog.RemoveAt(0);
        }

        public IEnumerable<Combatant> GetAliveCombatants()
        {
            return Combatants.Where(c => c.IsAlive);
        }

        public void CancelTargeting()
        {
            CurrentState = CombatState.PlayerTurn;
        }
    }
}