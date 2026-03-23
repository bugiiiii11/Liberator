using System;
using Liberator.Combat.Controllers;
using UnityEngine;
using TMPro;

namespace Liberator.Attributes
{
    public class ManaManager : MonoBehaviour
    {
        private float _currentMana;
        private float _maxMana;
        [SerializeField] private TextMeshProUGUI _textMana;

        public float CurrentMana
        {
            get => _currentMana;
            set => _currentMana = value;
        }

        private void Start()
        {
            _maxMana = GetComponent<EntityController>().Attributes.Mana;
            _currentMana = _maxMana;
            _textMana.text = "Action points: " + _currentMana + "/" + _maxMana;
        }

        public void SpendMana(float cost)
        {
            _currentMana = Math.Max(0, _currentMana - cost);
            _textMana.text = "Action points: " + _currentMana + "/" + _maxMana;
        }

        public void RefreshMana()
        {
            if (_maxMana < 8)
                _maxMana++;

            _currentMana = _maxMana;
            _textMana.text = "Action points: " + _currentMana + "/" + _maxMana;
        }
    }
}
