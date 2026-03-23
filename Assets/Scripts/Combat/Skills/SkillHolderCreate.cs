using Liberator.Combat.Controllers;
using Liberator.Combat.UI;
using Liberator.Inventory;
using UnityEngine;

namespace Liberator.Combat.Skills
{
    public class SkillHolderCreate : MonoBehaviour
    {
        [SerializeField] private GameObject _cardPrefab;
        private bool isSwitched = false;

        [SerializeField] private GameObject _cardHolderMelee;
        [SerializeField] private GameObject _cardHolderRanged;

        Vector3 _oldPosition;

        private void Start()
        {
            _oldPosition = _cardHolderMelee.transform.position;
        }

        public SkillHolder CreateSkill(CombatManager sender, Skill skillToTransform, int index,
            SkillEquipableItem sourceWeapon, EntityType entityType)
        {
            SkillHolder skillHolder = null;

            switch (entityType)
            {
                case EntityType.Player:
                    if (sourceWeapon != null)
                    {
                        switch (sourceWeapon.WeaponType)
                        {
                            case WeaponType.Melee:
                                GameObject newCard = Instantiate(_cardPrefab, _cardHolderMelee.transform);
                                skillHolder = newCard.GetComponent<SkillHolder>();
                                SkillHandler skillManager = newCard.GetComponent<SkillHandler>();
                                skillHolder.SetData(skillToTransform, sourceWeapon);
                                skillManager.Index = index;
                                break;
                            case WeaponType.Ranged:
                                GameObject newCardx = Instantiate(_cardPrefab, _cardHolderRanged.transform);
                                skillHolder = newCardx.GetComponent<SkillHolder>();
                                SkillHandler skillManagerx = newCardx.GetComponent<SkillHandler>();
                                skillHolder.SetData(skillToTransform, sourceWeapon);
                                skillManagerx.Index = index;
                                break;
                        }
                    }
                    else
                    {
                        GameObject newCardxx = Instantiate(_cardPrefab, _cardHolderMelee.transform);
                        skillHolder = newCardxx.GetComponent<SkillHolder>();
                        SkillHandler skillManagerxx = newCardxx.GetComponent<SkillHandler>();
                        skillHolder.SetData(skillToTransform, sourceWeapon);
                        skillManagerxx.Index = index;

                        GameObject newCardxxx = Instantiate(_cardPrefab, _cardHolderRanged.transform);
                        skillHolder = newCardxxx.GetComponent<SkillHolder>();
                        SkillHandler skillManagerxxx = newCardxxx.GetComponent<SkillHandler>();
                        skillHolder.SetData(skillToTransform, sourceWeapon);
                        skillManagerxxx.Index = index;
                    }
                    break;

                case EntityType.Enemy:
                    Transform holder = sender.transform.Find("Skills");
                    skillHolder = holder.gameObject.AddComponent<SkillHolder>();
                    skillHolder.SetData(skillToTransform, sourceWeapon);
                    break;
            }

            return skillHolder;
        }

        public void Switch()
        {
            if (isSwitched)
            {
                _cardHolderMelee.transform.position = _oldPosition;
                _cardHolderRanged.transform.position = new Vector3(50, -400, 0);
                isSwitched = false;
            }
            else
            {
                _cardHolderMelee.transform.position = new Vector3(50, -400, 0);
                _cardHolderRanged.transform.position = _oldPosition;
                isSwitched = true;
            }
        }
    }
}
