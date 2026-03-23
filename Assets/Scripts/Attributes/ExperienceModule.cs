using System;
using Liberator.Utils;
using UnityEngine;

namespace Liberator.Attributes
{
    public class ExperienceModule : MonoBehaviour
    {
        [SerializeField] private GameObject _levelUpEffect;

        [Header("Support modules")]
        [SerializeField] private BaseAttributes _attributes;

        public event Action OnExperienceGained;
        public event Action OnLevelUp;

        private float _currentExperiencePoints;
        private float _usedExperienceToLevelUp;
        private LazyValue<float> _experienceToLevelUp;
        private int _currentLevel = 1;

        public float PercentageToLevelUp => CheckMaxLevel() ? 1 : _currentExperiencePoints / _experienceToLevelUp.value;
        public int Level => _currentLevel;

        private void Awake()
        {
            _experienceToLevelUp = new LazyValue<float>(LazyValueInitialize);
        }

        private float LazyValueInitialize()
        {
            return _attributes.GetExperienceToLevelUp(_currentLevel);
        }

        private void Start()
        {
            _experienceToLevelUp.ForceInit();
        }

        public void GainExperience(float exp)
        {
            _currentExperiencePoints += exp;

            if (CheckMaxLevel()) return;

            if (CheckLevelUp())
                LevelUp();
            else
                OnExperienceGained?.Invoke();
        }

        public void LevelUp()
        {
            _usedExperienceToLevelUp += _experienceToLevelUp.value;
            _currentExperiencePoints -= _experienceToLevelUp.value;

            _currentLevel++;

            _experienceToLevelUp.value = _attributes.GetExperienceToLevelUp(_currentLevel);

            if (_experienceToLevelUp.value <= 0)
                _experienceToLevelUp.value = 0;

            LevelUpEffect();
            OnLevelUp?.Invoke();

            if (CheckMaxLevel()) return;

            if (CheckLevelUp())
                LevelUp();
        }

        private bool CheckMaxLevel()
        {
            return _currentLevel == _attributes.MaxLevel;
        }

        private bool CheckLevelUp()
        {
            return _currentExperiencePoints >= _experienceToLevelUp.value;
        }

        private void LevelUpEffect()
        {
            Instantiate(_levelUpEffect, transform);
        }
    }
}
