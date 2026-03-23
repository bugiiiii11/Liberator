using System;
using System.Collections.Generic;
using UnityEngine;

namespace Liberator.Inventory
{
    public class Equipment : MonoBehaviour
    {
        Dictionary<EquipType, EquipableItem> equippedItems = new Dictionary<EquipType, EquipableItem>();

        public event Action equipmentUpdated;

        public EquipableItem GetItemInSlot(EquipType equipType)
        {
            if (!equippedItems.ContainsKey(equipType))
                return null;
            return equippedItems[equipType];
        }

        public SkillEquipableItem GetSkillEquipableItem(EquipType equipType)
        {
            if (!equippedItems.ContainsKey(equipType))
                return null;
            return equippedItems[equipType] as SkillEquipableItem;
        }

        public static Equipment GetPlayerEquipment()
        {
            var player = GameObject.FindWithTag("Player");
            return player.GetComponent<Equipment>();
        }

        public void AddItem(EquipType slot, EquipableItem item)
        {
            Debug.Assert(item.GetAllowedEquipLocation() == slot);
            equippedItems[slot] = item;
            equipmentUpdated?.Invoke();
        }

        public void RemoveItem(EquipType slot)
        {
            equippedItems.Remove(slot);
            equipmentUpdated?.Invoke();
        }

        public IEnumerable<EquipType> GetAllPopulatedSlots()
        {
            return equippedItems.Keys;
        }
    }
}
