using System.Collections.Generic;
using UnityEngine;

namespace Liberator.Combat.Skills
{
    public enum SkillType
    {
        Melee,
        Ranged,
        Heal
    }

    public enum Debuff
    {
        Poison,
        Bleeding,
        Stun
    }

    public enum AreaOfEffect
    {
        Single,
        Line,
        All,
        Cross,
        Column,
        Myself
    }

    [CreateAssetMenu(fileName = "Skill", menuName = "Liberator/Create Skill", order = 0)]
    public class Skill : ScriptableObject
    {
        [SerializeField] private string _name;

        [Header("Skill Base Attributes")]
        [SerializeField] private int _manaCost;
        [SerializeField] private int _damagePercentage;
        [SerializeField] private int _cooldown;
        [SerializeField] private SkillType _skillType;
        [SerializeField] private AreaOfEffect _areaOfEffect;

        [Header("Skill Additional Attributes")]
        [SerializeField] private int _numberOfAttacks;
        [SerializeField] private float _delay;
        [SerializeField] private List<Debuff> _activeDebuffs;

        [Header("Skill UI")]
        [SerializeField] private string _description;

        public int Mana => _manaCost;
        public float Delay => _delay;
        public int DamagePercentage => _damagePercentage;
        public int Cooldown => _cooldown;
        public SkillType SkillType => _skillType;
        public AreaOfEffect AreaOfEffect => _areaOfEffect;
        public int NumberOfAttacks => _numberOfAttacks;
        public List<Debuff> ActiveDebuffs => _activeDebuffs;
        public string Name => _name;
        public string Description => _description;
    }
}
