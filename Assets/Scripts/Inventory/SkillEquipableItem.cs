using System.Collections.Generic;
using Liberator.Combat.Skills;
using UnityEngine;

namespace Liberator.Inventory
{
    public enum WeaponType
    {
        Melee,
        Ranged
    }

    [CreateAssetMenu(menuName = ("Liberator/Inventory/Weapon Equipable Item"))]
    public class SkillEquipableItem : StatsEquipableItem
    {
        [Header("Weapon specific stats")]
        [SerializeField] private WeaponType _weaponType;
        [SerializeField] private int _damage;
        [SerializeField] private int _critChance;
        [SerializeField] private int _CritDamage;
        [SerializeField] private int _range;

        public int Damage => _damage;
        public int CritChance => _critChance;
        public int CriDamage => _CritDamage;
        public int Range => _range;
        public WeaponType WeaponType => _weaponType;

        [Header("Weapon skills")]
        [SerializeField] private List<Skill> _skills;

        public List<Skill> Skills => _skills;
    }
}
