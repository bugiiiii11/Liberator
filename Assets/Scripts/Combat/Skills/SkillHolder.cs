using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;
using System.Linq;
using Liberator.Combat.Controllers;
using Liberator.Inventory;

namespace Liberator.Combat.Skills
{
    public class SkillHolder : MonoBehaviour
    {
        public void SetData(Skill skill, SkillEquipableItem weaponSource)
        {
            Name = skill.Name;
            Description = skill.Description;

            ManaCost = skill.Mana;
            DamagePercent = skill.DamagePercentage;
            Cooldown = skill.Cooldown;
            SkillType = skill.SkillType;
            AreaOfEffect = skill.AreaOfEffect;

            Debuffs = skill.ActiveDebuffs;
            NumberOfAttacks = skill.NumberOfAttacks;
            Delay = skill.Delay;

            SourceWeaponDamage = weaponSource == null ? 0 : weaponSource.Damage;
        }

        // UI
        public string Name { get; set; }
        public string Description { get; set; }

        // Skill attributes
        public int ManaCost { get; set; }
        public float DamagePercent { get; set; }
        public int Cooldown { get; set; }
        public SkillType SkillType { get; set; }
        public AreaOfEffect AreaOfEffect { get; set; }

        // Skill Additions
        public List<Debuff> Debuffs { get; set; }
        public int NumberOfAttacks { get; set; }
        public float SourceWeaponDamage { get; set; }
        public float SourceWeaponRange { get; set; }
        public float Delay { get; set; }

        GameObject[] _placements;
        EntityController[] _attackableEnemies;

        public void ExecuteSkill(EntityController target, EntityController myself, GameObject placement)
        {
            _placements = GameObject.FindGameObjectsWithTag("Placement");

            _attackableEnemies = CheckAreaOfEffect(AreaOfEffect, target, myself, placement).ToArray();

            foreach (EntityController item in _attackableEnemies)
            {
                myself.StartCoroutine(MultiSwingDelayed(item, myself));
            }

            myself.Mana.SpendMana(ManaCost);

            foreach (EntityController item in _attackableEnemies)
            {
                Debuffed(Debuffs, item, myself);
            }
        }

        private IEnumerator MultiSwingDelayed(EntityController target, EntityController myself)
        {
            for (int i = 0; i < NumberOfAttacks; i++)
            {
                switch (SkillType)
                {
                    case SkillType.Melee:
                        if (target != null)
                            target.Health.Hurt(CalculateRealDamage(target, myself));
                        break;
                    case SkillType.Ranged:
                        if (target != null)
                            target.Health.Hurt(CalculateRealDamage(target, myself));
                        break;
                    case SkillType.Heal:
                        if (target != null)
                            target.Health.Heal(myself.Attributes.Damage * (DamagePercent / 100));
                        break;
                }
                yield return new WaitForSeconds(Delay);
            }
        }

        public void Debuffed(List<Debuff> debuff, EntityController target, EntityController myself)
        {
            foreach (Debuff item in debuff)
            {
                int dodge = Random.Range(0, 100);
                if (dodge < target.Attributes.DodgeChance)
                {
                    GameObject newHurtText = Instantiate(target.Health.HurtText,
                        target.transform.position - target.Health.BleedPosition, target.transform.rotation);
                    newHurtText.GetComponentInChildren<TextMeshPro>().text = "Effect Dodged";
                }
                else
                {
                    switch (item)
                    {
                        case Debuff.Poison:
                            if (target.gameObject.GetComponent<Poison>())
                            {
                                Poison _poison = target.gameObject.GetComponent<Poison>();
                                _poison.TurnCount += 3;
                                _poison._text.text = _poison.TurnCount.ToString();
                            }
                            else
                            {
                                Poison poison = target.gameObject.AddComponent<Poison>();
                                poison.TurnCount = 3;
                                poison._damage += myself.Attributes.Damage * (DamagePercent / 100);
                            }
                            break;
                        case Debuff.Bleeding:
                            Debug.Log("Bleeding");
                            break;
                        case Debuff.Stun:
                            Stun stun = target.gameObject.AddComponent<Stun>();
                            break;
                    }
                }
            }
        }

        public List<EntityController> CheckAreaOfEffect(AreaOfEffect areaOfEffect, EntityController target,
            EntityController myself, GameObject placement = null)
        {
            EnemyHolder placementEnemyHolder = null;
            if (placement != null)
                placementEnemyHolder = placement.GetComponent<EnemyHolder>();

            List<EntityController> listOfEnemies = new List<EntityController>();

            switch (areaOfEffect)
            {
                case AreaOfEffect.Single:
                    listOfEnemies.Add(target);
                    return listOfEnemies;

                case AreaOfEffect.Line:
                    foreach (GameObject item in _placements)
                    {
                        EnemyHolder enemyHolder = item.GetComponent<EnemyHolder>();
                        if (enemyHolder.GetEnemyController() != null)
                        {
                            if (enemyHolder.GetEnemyController().RowIndex == target.RowIndex)
                                listOfEnemies.Add(enemyHolder.GetEnemyController());
                        }
                    }
                    listOfEnemies = listOfEnemies.Distinct().ToList();
                    return listOfEnemies;

                case AreaOfEffect.Column:
                    foreach (GameObject item in _placements)
                    {
                        EnemyHolder enemyHolder = item.GetComponent<EnemyHolder>();
                        if (placementEnemyHolder.ColumnIndex == enemyHolder.ColumnIndex)
                            listOfEnemies.Add(enemyHolder.GetEnemyController());
                    }
                    listOfEnemies = listOfEnemies.Distinct().ToList();
                    return listOfEnemies;

                case AreaOfEffect.Cross:
                    foreach (GameObject item in _placements)
                    {
                        EnemyHolder enemyHolder = item.GetComponent<EnemyHolder>();
                        if (placementEnemyHolder.ColumnIndex == enemyHolder.ColumnIndex ||
                            placementEnemyHolder.RowIndex == enemyHolder.RowIndex)
                            listOfEnemies.Add(enemyHolder.GetEnemyController());
                    }
                    listOfEnemies = listOfEnemies.Distinct().ToList();
                    return listOfEnemies;

                case AreaOfEffect.All:
                    listOfEnemies.Clear();
                    foreach (GameObject item in _placements)
                    {
                        EnemyHolder enemyHolder = item.GetComponent<EnemyHolder>();
                        listOfEnemies.Add(enemyHolder.GetEnemyController());
                    }
                    listOfEnemies = listOfEnemies.Distinct().ToList();
                    return listOfEnemies;

                case AreaOfEffect.Myself:
                    listOfEnemies.Clear();
                    listOfEnemies.Add(myself);
                    return listOfEnemies;
            }

            return listOfEnemies;
        }

        public int CalculateRealDamage(EntityController target, EntityController myself)
        {
            float realDamage;
            int cri = Random.Range(0, 100);
            if (cri < myself.Attributes.CritChance)
            {
                realDamage =
                    ((myself.Attributes.Damage + SourceWeaponDamage) * (DamagePercent / 100) *
                        (myself.Attributes.CritDamage / 100) - target.Attributes.Defense);
            }
            else
            {
                realDamage = (myself.Attributes.Damage + SourceWeaponDamage) * (DamagePercent / 100) -
                             target.Attributes.Defense;
            }

            return (int)realDamage;
        }
    }
}
