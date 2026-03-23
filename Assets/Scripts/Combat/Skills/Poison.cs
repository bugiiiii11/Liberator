using Liberator.Attributes;
using Liberator.Combat.Controllers;
using Liberator.TurnManager;
using TMPro;
using UnityEngine;

namespace Liberator.Combat.Skills
{
    public class Poison : MonoBehaviour
    {
        private DebuffModule _debuffModule;
        private HealthManager _healthManager;
        private EntityController _entityController;
        public TextMeshProUGUI _text;
        private int _turnCount;

        public int TurnCount
        {
            get => _turnCount;
            set => _turnCount = value;
        }

        public float _damage;

        private void Awake()
        {
            _debuffModule = GameObject.FindGameObjectWithTag("Debuff").GetComponent<DebuffModule>();
            _healthManager = gameObject.GetComponent<HealthManager>();
            _entityController = gameObject.GetComponent<EntityController>();

            _text = _entityController.Poison.GetComponentInChildren<TextMeshProUGUI>();
        }

        void Start()
        {
            _turnCount = 3;
            _text.text = _turnCount.ToString();
        }

        private void OnEnable()
        {
            _debuffModule.OnDebuffed += DamageOverTime;
            _entityController.Poison.SetActive(true);
        }

        private void OnDisable()
        {
            _debuffModule.OnDebuffed -= DamageOverTime;
        }

        public void DamageOverTime(object sender, EnemyTurnArgs e)
        {
            if (gameObject != e.Actor) return;
            if (_turnCount > 0)
            {
                _turnCount--;
                _healthManager.Hurt(_damage, false);
                _entityController.Poison.SetActive(true);
                _text.text = _turnCount.ToString();
                if (_turnCount == 0)
                {
                    _entityController.Poison.SetActive(false);
                    Destroy(this);
                }
            }
            else
            {
                _entityController.Poison.SetActive(false);
                Destroy(this);
            }
        }
    }
}
