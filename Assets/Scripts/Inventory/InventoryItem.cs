using System;
using System.Collections.Generic;
using UnityEngine;

namespace Liberator.Inventory
{
    public abstract class InventoryItem : ScriptableObject, ISerializationCallbackReceiver
    {
        [Tooltip("Auto-generated UUID for saving/loading.")]
        [SerializeField] private string _itemID = null;

        [Header("Information")]
        [SerializeField] private string _displayName = null;
        [SerializeField][TextArea] private string _description = null;
        [SerializeField] private Sprite _icon = null;

        static Dictionary<string, InventoryItem> _itemLookupCache;

        public Sprite Icon => _icon;
        public string ItemID => _itemID;
        public string Name => _displayName;
        public string Description => _description;

        public static InventoryItem GetFromID(string itemID)
        {
            if (_itemLookupCache == null)
            {
                _itemLookupCache = new Dictionary<string, InventoryItem>();
                var itemList = Resources.LoadAll<InventoryItem>("");
                foreach (var item in itemList)
                {
                    if (_itemLookupCache.ContainsKey(item._itemID))
                    {
                        Debug.LogError(string.Format("Duplicate Inventory ID for objects: {0} and {1}", _itemLookupCache[item._itemID], item));
                        continue;
                    }
                    _itemLookupCache[item._itemID] = item;
                }
            }

            if (itemID == null || !_itemLookupCache.ContainsKey(itemID)) return null;
            return _itemLookupCache[itemID];
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (string.IsNullOrWhiteSpace(_itemID))
                _itemID = Guid.NewGuid().ToString();
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() { }
    }
}
