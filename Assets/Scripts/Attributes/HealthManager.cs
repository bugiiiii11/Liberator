using System;
using UnityEngine;
using UnityEngine.UI;
using Liberator.Combat;
using Liberator.Combat.Controllers;
using TMPro;

namespace Liberator.Attributes
{
    public class HealthManager : MonoBehaviour
    {
        private float _currentHealth;
        private float _maxHealth;
        private float _fixedDamage;
        [SerializeField] private Slider _slider;
        private Spawner _spawner;

        [SerializeField] private GameObject _bleed;
        [SerializeField] private GameObject _heal;
        [SerializeField] private Vector3 _bleedPosition;

        [SerializeField] private GameObject _hurtTextGameObject;
        [SerializeField] private GameObject _healTextGameObject;

        private EntityController _entity;

        public Vector3 BleedPosition => _bleedPosition;
        public GameObject HurtText => _hurtTextGameObject;
        public GameObject HealText => _healTextGameObject;

        public float MaxHealth => _maxHealth;

        public float CurrentHealth
        {
            get => _currentHealth;
            set => _currentHealth = value;
        }

        private void Awake()
        {
            _entity = GetComponent<EntityController>();
        }

        private void Start()
        {
            _maxHealth = _entity.Attributes.Health;
            _spawner = GameObject.FindGameObjectWithTag("Spawner").GetComponent<Spawner>();
            _currentHealth = _maxHealth;
            _slider.maxValue = _maxHealth;
            _slider.value = _currentHealth;
        }

        public void Hurt(float damage, bool dodgeable = true)
        {
            int dodge = UnityEngine.Random.Range(0, 100);
            if (dodge < _entity.Attributes.DodgeChance && dodgeable)
            {
                GameObject newHurtText = Instantiate(_hurtTextGameObject,
                    gameObject.transform.position - _bleedPosition, gameObject.transform.rotation);
                newHurtText.GetComponentInChildren<TextMeshPro>().text = "Dodge";
            }
            else
            {
                _fixedDamage = Math.Max(0, damage);
                _currentHealth = Math.Max(0, _currentHealth - _fixedDamage);
                _slider.value = _currentHealth;
                GameObject newParticle = Instantiate(_bleed, gameObject.transform.position - _bleedPosition,
                    gameObject.transform.rotation);
                GameObject newHurtText = Instantiate(_hurtTextGameObject,
                    gameObject.transform.position - _bleedPosition, gameObject.transform.rotation);
                newHurtText.GetComponentInChildren<TextMeshPro>().text = _fixedDamage.ToString();
                if (_currentHealth <= 0)
                {
                    _entity.DeathManager.Death();
                    Destroy(gameObject);
                }
            }
        }

        public void Heal(float healValue)
        {
            _currentHealth = Math.Min(_maxHealth, _currentHealth + healValue);
            _slider.value = _currentHealth;
            GameObject newParticle = Instantiate(_heal, gameObject.transform.position - _bleedPosition,
                gameObject.transform.rotation);
            GameObject newHealText = Instantiate(_healTextGameObject, gameObject.transform.position - _bleedPosition,
                gameObject.transform.rotation);
            newHealText.GetComponentInChildren<TextMeshPro>().text = healValue.ToString();
        }
    }
}
