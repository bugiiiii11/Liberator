using System;
using System.Collections.Generic;
using Liberator.Attributes;
using UnityEngine;
using Attribute = Liberator.Attributes.Attribute;

namespace Liberator.Inventory
{
    [CreateAssetMenu(menuName = ("Liberator/Inventory/Stats Equipable Item"))]
    public class StatsEquipableItem : EquipableItem, IModifierProvider
    {
        [Header("Attributes Providers")]
        [SerializeField] private List<Modifier> additiveModifiers;
        [SerializeField] private List<Modifier> percentageModifiers;

        [Serializable]
        struct Modifier
        {
            public Attribute Attribute;
            public float Value;
        }

        public IEnumerable<float> GetAdditiveModifiers(Attribute attribute)
        {
            foreach (var modifier in additiveModifiers)
            {
                if (modifier.Attribute == attribute)
                    yield return modifier.Value;
            }
        }

        public IEnumerable<float> GetPercentageModifiers(Attribute attribute)
        {
            foreach (var modifier in percentageModifiers)
            {
                if (modifier.Attribute == attribute)
                    yield return modifier.Value;
            }
        }
    }
}
