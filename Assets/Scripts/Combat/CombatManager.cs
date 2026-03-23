using System.Collections.Generic;
using Liberator.Combat.Controllers;
using Liberator.Combat.Skills;
using Liberator.Inventory;
using UnityEngine;

namespace Liberator.Combat
{
    public class CombatManager : MonoBehaviour
    {
        [SerializeField] private List<Skill> _basicSkills = new List<Skill>();

        protected SkillEquipableItem _meleeWeapon = null;
        protected SkillEquipableItem _rangedWeapon = null;
        protected EntityController _myself;

        private Dictionary<int, SkillHolder> _availableSkills;
        private SkillHolderCreate _skillCreator;

        protected void Awake()
        {
            _myself = GetComponent<EntityController>();
        }

        protected void Start()
        {
            _availableSkills = new Dictionary<int, SkillHolder>();
            _skillCreator = FindObjectOfType<SkillHolderCreate>();
        }

        protected void AppendBasicSkill()
        {
            SkillEquipableItem weapon = null;
            foreach (Skill skillToTransform in _basicSkills)
            {
                int index = _availableSkills.Count + 1;
                SkillHolder skill = CreateSkillHolder(skillToTransform, index, weapon);
                _availableSkills.Add(index, skill);
            }
        }

        protected void AppendSkillFromWeapon(SkillEquipableItem weapon)
        {
            if (weapon == null) return;

            foreach (Skill skillToTransform in weapon.Skills)
            {
                int index = _availableSkills.Count + 1;
                SkillHolder skill = CreateSkillHolder(skillToTransform, index, weapon);
                _availableSkills.Add(index, skill);
            }
        }

        private SkillHolder CreateSkillHolder(Skill skillToTransform, int index, SkillEquipableItem sourceWeapon = null)
        {
            SkillHolder skillHolder = _skillCreator.CreateSkill(this, skillToTransform, index, sourceWeapon, _myself.EntityType);
            return skillHolder;
        }

        public SkillHolder GetSkillOnIndex(int skillIndex)
        {
            return _availableSkills[skillIndex];
        }

        public void Attack(EntityController target, int skillIndex, GameObject placement = null)
        {
            _availableSkills[skillIndex].ExecuteSkill(target, _myself, placement);
        }
    }
}
