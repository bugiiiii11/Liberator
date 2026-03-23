using System.Collections.Generic;
using UnityEngine;
using Liberator.Combat.Controllers;
using Liberator.Inventory;

namespace Liberator.Combat
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerCombatManager : CombatManager
    {
        private List<GameObject> _listOfMeleeCards = new List<GameObject>();
        private List<GameObject> _listOfRangedCards = new List<GameObject>();
        private bool isSwitched = false;

        public List<GameObject> ListOfMeleeCards
        {
            get => _listOfMeleeCards;
            set => _listOfMeleeCards = value;
        }

        public List<GameObject> ListOfRangedCards
        {
            get => _listOfRangedCards;
            set => _listOfRangedCards = value;
        }

        protected new void Start()
        {
            base.Start();

            Equipment _playerEquipment = FindObjectOfType<Equipment>();
            AppendBasicSkill();
            _meleeWeapon = _playerEquipment.GetItemInSlot(EquipType.Melee) as SkillEquipableItem;
            AppendSkillFromWeapon(_meleeWeapon);

            _rangedWeapon = _playerEquipment.GetItemInSlot(EquipType.Ranged) as SkillEquipableItem;
            AppendSkillFromWeapon(_rangedWeapon);

            _myself = GetComponent<PlayerController>() as EntityController;

            SwitchSkills();
        }

        public void SwitchSkills()
        {
            if (isSwitched)
            {
                foreach (GameObject item in ListOfMeleeCards)
                    item.SetActive(false);
                foreach (GameObject item in ListOfRangedCards)
                    item.SetActive(true);
                isSwitched = false;
            }
            else
            {
                foreach (GameObject item in ListOfMeleeCards)
                    item.SetActive(true);
                foreach (GameObject item in ListOfRangedCards)
                    item.SetActive(false);
                isSwitched = true;
            }
        }
    }
}
