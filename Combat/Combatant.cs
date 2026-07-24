using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecalpon.Combat
{
    public enum CombatantType
    {
        PlayerCharacter,    // the scientist — player controlled
        Companion,          // recruited NPC — player controlled
        Enemy               // AI controlled
    }

    public class Combatant
    {

        
        public string Name { get; set; }
        public CombatantType Type { get; set; }
        public int GridRow { get; set; }
        public int GridCol { get; set; }
        public WeaponType WeaponType { get; set; }
        public int MaxAttacks { get; set; }
        public int AttacksRemainingThisTurn { get; set;  }
        public int HitPoints { get; set; }
        public int MaxHitPoints { get; set; }
        public int ArmorClass { get; set; }
        public int Level { get; set; }
        public int ThacO { get; set; }          // "To Hit Armor Class Zero"
        public int DamageMin { get; set; }
        public int DamageMax { get; set; }
        public int DamageBonus { get; set; }

        // --- Neoscience / Power Points ---
        public int PowerPoints { get; set; }
        public int MaxPowerPoints { get; set; }

        // --- Turn management ---
        public int Initiative { get; set; }
        public bool HasActedThisTurn 
        { 
            get
            {
                return AttacksRemainingThisTurn <= 0;
            }
        }

        public int MovesRemaining { get; set; }
        public int MaxMoves { get; set; }

        // --- State flags ---
        public bool IsAlive => HitPoints > 0;
        public bool IsPlayerControlled =>
            Type == CombatantType.PlayerCharacter ||
            Type == CombatantType.Companion;

        // --- Experience reward when killed ---
        public int ExperienceValue { get; set; }

        public Combatant()
        {
            MaxAttacks = 1;
            WeaponType = WeaponType.Melee;
        }

        public void StartTurn()
        {
            AttacksRemainingThisTurn = MaxAttacks;
            MovesRemaining = MaxMoves;
        }
    }

}
