using UnityEngine;
using TMPro;

namespace Liberator.Combat.UI
{
    public class Description : MonoBehaviour
    {
        [SerializeField] private GameObject _playerDescription;
        private SkillHandler _enemyManager;
        public bool _isActive;
        private string _myText;
        private GameObject _currentGameObject;

        private string _name;
        private string _maxHealth;
        private string _currentHealth;
        private string _defense;
        private string _damage;
        private string _critDamage;
        private string _critChance;
        private string _dodgeChance;
        private string _actionPoint;
        private string _cooldown;
        private string _descriptionText;

        public string Name { get => _name; set => _name = value; }
        public string MaxHealth { get => _maxHealth; set => _maxHealth = value; }
        public string CurrentHealth { get => _currentHealth; set => _currentHealth = value; }
        public string Defense { get => _defense; set => _defense = value; }
        public string Damage { get => _damage; set => _damage = value; }
        public string CritChance { get => _critChance; set => _critChance = value; }
        public string CritDamage { get => _critDamage; set => _critDamage = value; }
        public string DodgeChance { get => _dodgeChance; set => _dodgeChance = value; }
        public string DescriptionText { get => _descriptionText; set => _descriptionText = value; }
        public string ActionPoint { get => _actionPoint; set => _actionPoint = value; }
        public string Cooldown { get => _cooldown; set => _cooldown = value; }
        public string MyText { get => _myText; set => _myText = value; }

        void Start()
        {
            _playerDescription.SetActive(false);
            _isActive = false;
        }

        public void OpenPlayerDescription(GameObject gameObjectEnemy)
        {
            if (_currentGameObject == gameObjectEnemy) { Close(); }
            else
            {
                _isActive = true;
                _playerDescription.SetActive(true);
                _myText = "Name: " + _name;
                _myText += "\nHealth: " + _maxHealth + "/" + _currentHealth;
                _myText += "\nDefense: " + _defense;
                _myText += "\nDamage: " + _damage;
                _myText += "\nCritical chance: " + _critChance + " %";
                _myText += "\nCritical damage: " + _critDamage + " %";
                _myText += "\nDodge chance: " + _dodgeChance + " %";
                _playerDescription.GetComponentInChildren<TextMeshProUGUI>().text = _myText;
                _currentGameObject = gameObjectEnemy;
            }
        }

        public void OpenEnemyDescription(GameObject gameObjectEnemy)
        {
            if (_currentGameObject == gameObjectEnemy) { Close(); }
            else
            {
                _isActive = true;
                _playerDescription.SetActive(true);
                _myText = "Name: " + _name;
                _myText += "\nHealth: " + _maxHealth + "/" + _currentHealth;
                _myText += "\nDefense: " + _defense;
                _myText += "\nDamage: " + _damage;
                _myText += "\nDodge chance: " + _dodgeChance + " %";
                _myText += "\nInfo:\n" + DescriptionText;
                _playerDescription.GetComponentInChildren<TextMeshProUGUI>().text = _myText;
                _currentGameObject = gameObjectEnemy;
            }
        }

        public void OpenSkillDescription(GameObject gameObjectEnemy)
        {
            if (_currentGameObject == gameObjectEnemy) { Close(); }
            else
            {
                _isActive = true;
                _playerDescription.SetActive(true);
                _myText = "Name: " + _name;
                _myText += "\nAction points: " + _actionPoint;
                _myText += "\nCooldown: " + _cooldown;
                _myText += "\nDamage: " + _damage;
                _myText += "\nInfo:\n" + _descriptionText;
                _playerDescription.GetComponentInChildren<TextMeshProUGUI>().text = _myText;
                _currentGameObject = gameObjectEnemy;
            }
        }

        public void Close()
        {
            _isActive = false;
            _playerDescription.SetActive(false);
            _currentGameObject = null;
        }
    }
}
