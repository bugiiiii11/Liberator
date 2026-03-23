using UnityEngine;

namespace Liberator.Attributes
{
    public class BaseAttributes : MonoBehaviour
    {
        [Range(1, 3)] [SerializeField] private int _fixedLevel = 1;

        [SerializeField] private CharacterClass _class;
        [SerializeField] private Progression _progression;
        [SerializeField] private bool _activeModifiers = false;

        private ExperienceModule _experienceModule;

        private void Awake()
        {
            _experienceModule = GetComponent<ExperienceModule>();
        }

        public CharacterClass CharacterClass => _class;
        public int Level => _experienceModule == null ? _fixedLevel : _experienceModule.Level;

        public float Health =>
            (_progression.GetStatFromCharacter(Attribute.Health, _class, Level) + AdditiveModifier(Attribute.Health)) *
            (1 + GetPercentageModifier(Attribute.Health) / 100);

        public float Damage =>
            (_progression.GetStatFromCharacter(Attribute.Damage, _class, Level) + AdditiveModifier(Attribute.Damage)) *
            (1 + GetPercentageModifier(Attribute.Damage) / 100);

        public float Defense =>
            (_progression.GetStatFromCharacter(Attribute.Defense, _class, Level) + AdditiveModifier(Attribute.Defense)) *
            (1 + GetPercentageModifier(Attribute.Defense) / 100);

        public float AttackSpeed =>
            (_progression.GetStatFromCharacter(Attribute.AttackSpeed, _class, Level) + AdditiveModifier(Attribute.AttackSpeed)) *
            (1 + GetPercentageModifier(Attribute.AttackSpeed) / 100);

        public float AttackRange =>
            (_progression.GetStatFromCharacter(Attribute.AttackRange, _class, Level) + AdditiveModifier(Attribute.AttackRange)) *
            (1 + GetPercentageModifier(Attribute.AttackRange) / 100);

        public float CritChance => _progression.GetStatFromCharacter(Attribute.CriticalChance, _class, Level) +
                                   GetPercentageModifier(Attribute.CriticalChance);

        public float CritDamage => _progression.GetStatFromCharacter(Attribute.CriticalDamage, _class, Level) +
                                   GetPercentageModifier(Attribute.CriticalDamage);

        public float DodgeChance => _progression.GetStatFromCharacter(Attribute.DodgeChance, _class, Level) +
                                    GetPercentageModifier(Attribute.DodgeChance);

        public float Initiative =>
            (_progression.GetStatFromCharacter(Attribute.Initiative, _class, Level) + AdditiveModifier(Attribute.Initiative)) *
            (1 + GetPercentageModifier(Attribute.Initiative) / 100);

        public float Mana =>
            (_progression.GetStatFromCharacter(Attribute.Mana, _class, Level) + AdditiveModifier(Attribute.Mana)) *
            (1 + GetPercentageModifier(Attribute.Mana) / 100);

        public int MaxLevel => _progression.MaxLevel(_class);

        public float GetExperienceToLevelUp(int level)
        {
            if (level == 1)
                return _progression.GetStatFromCharacter(Attribute.ExperienceToLevelUp, _class, Level);

            float lastLevelExp = _progression.GetStatFromCharacter(Attribute.ExperienceToLevelUp, _class, Level - 1);
            float actualLevelExp = _progression.GetStatFromCharacter(Attribute.ExperienceToLevelUp, _class, Level);

            return actualLevelExp - lastLevelExp;
        }

        private float AdditiveModifier(Attribute attribute)
        {
            if (!_activeModifiers) return 0;

            float additiveSum = 0;
            foreach (IModifierProvider provider in GetComponents<IModifierProvider>())
            {
                foreach (float modifier in provider.GetAdditiveModifiers(attribute))
                {
                    additiveSum += modifier;
                }
            }
            return additiveSum;
        }

        private float GetPercentageModifier(Attribute attribute)
        {
            if (!_activeModifiers) return 0;

            float percentageSum = 0;
            foreach (IModifierProvider provider in GetComponents<IModifierProvider>())
            {
                foreach (float modifier in provider.GetPercentageModifiers(attribute))
                {
                    percentageSum += modifier;
                }
            }
            return percentageSum;
        }
    }
}
