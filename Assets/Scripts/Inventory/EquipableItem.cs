using UnityEngine;

namespace Liberator.Inventory
{
    [CreateAssetMenu(menuName = ("Liberator/Inventory/Equipable Item"))]
    public class EquipableItem : InventoryItem
    {
        [Header("Equip type")]
        [SerializeField] EquipType _allowedEquipType;

        public EquipType GetAllowedEquipLocation()
        {
            return _allowedEquipType;
        }
    }
}
