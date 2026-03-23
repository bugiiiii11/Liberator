using System;
using System.Collections.Generic;
using UnityEngine;

namespace Liberator.Attributes
{
    [CreateAssetMenu(fileName = "Progression", menuName = "Liberator/Create Progression", order = 1)]
    public class Progression : ScriptableObject
    {
        [SerializeField] private List<ProgressionCharacter> _charaters = null;

        private Dictionary<CharacterClass, Dictionary<Attribute, List<float>>> _lookupTable = null;

        public float GetStatFromCharacter(Attribute stat, CharacterClass character, int level)
        {
            BuildLookupTable();
            List<float> valuesByLevel = _lookupTable[character][stat];
            if (valuesByLevel.Count < level) return 0;

            return valuesByLevel[level - 1];
        }

        public int MaxLevel(CharacterClass character)
        {
            BuildLookupTable();
            List<float> valuesByLevel = _lookupTable[character][Attribute.ExperienceToLevelUp];
            return valuesByLevel.Count + 1;
        }

        private void BuildLookupTable()
        {
            if (_lookupTable != null) return;

            _lookupTable = new Dictionary<CharacterClass, Dictionary<Attribute, List<float>>>();

            foreach (ProgressionCharacter pChar in _charaters)
            {
                Dictionary<Attribute, List<float>> statLookupTable = new Dictionary<Attribute, List<float>>();

                foreach (ProgressionStat pStat in pChar.Stats)
                {
                    statLookupTable[pStat.Stat] = pStat.Values;
                }

                _lookupTable[pChar.Class] = statLookupTable;
            }
        }
    }

    [Serializable]
    public class ProgressionCharacter
    {
        [SerializeField] private CharacterClass _character;
        [SerializeField] private List<ProgressionStat> _stats;

        public CharacterClass Class => _character;
        public List<ProgressionStat> Stats => _stats;
    }

    [Serializable]
    public class ProgressionStat
    {
        [SerializeField] private Attribute _stat;
        [SerializeField] private List<float> _valuesByLevel;

        public Attribute Stat => _stat;
        public List<float> Values => _valuesByLevel;
    }
}
