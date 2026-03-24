using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Liberator.Attributes;
using Liberator.Combat.UI;
using Liberator.TurnManager;

namespace Liberator.Combat.Controllers
{
    public enum ActionType { Attack, Deffense, Heal, SpecialAttack, Stunned }

    public class EnemyController : EntityController, IEnemyAction
    {
        protected ActionType _actionType = ActionType.Attack;

        [Header("Enemy Specific")]
        public Information _info;

        [SerializeField] protected List<Sprite> _actionImages;
        [SerializeField] protected GameObject _actionImageHolder;
        [SerializeField] private int _enemyValue;

        [Header("Enemy UI")]
        [HideInInspector] public int _skillIndex;

        [SerializeField] private Description _playerDescription;

        private EntityController _target;
        protected Image _image;
        private BattleLog _battleLog;
        protected Spawner _spawner;
        private DebuffModule _debuffModule;
        private TurnModule _turnModule;
        private PlayerController _playerController;

        private float _endTurnTime;

        public int EnemyValue => _enemyValue;
        public EntityController Target => _target;

        private void Awake()
        {
            _entityType = EntityType.Enemy;

            _entityManaManager = GetComponent<ManaManager>();
            _entityHealthManager = GetComponent<HealthManager>();
            _entityCombatModule = GetComponent<CombatManager>();
            _entityAttributes = GetComponent<BaseAttributes>();
            _entityDeathManager = GetComponent<DeathManager>();
            _image = _actionImageHolder.GetComponent<Image>();
            _target = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>() as EntityController;
            _battleLog = GameObject.FindGameObjectWithTag("BattleLog").GetComponent<BattleLog>();
            _turnModule = GameObject.FindGameObjectWithTag("Finish").GetComponent<TurnModule>();
            _playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
            _myMaterial = GetComponentInChildren<Renderer>().material;
            _spawner = GameObject.FindGameObjectWithTag("Spawner").GetComponent<Spawner>();
            _debuffModule = GameObject.FindGameObjectWithTag("Debuff").GetComponent<DebuffModule>();
            _playerDescription = GameObject.FindGameObjectWithTag("PlayerInfo").GetComponent<Description>();
        }

        private void Update()
        {
            if (MyTurn)
            {
                _endTurnTime += Time.deltaTime;

                if (_endTurnTime > 1 && _playerController.IsAlive)
                {
                    _endTurnTime = 0;
                    MyTurn = false;
                    _highlight.SetActive(false);
                    _turnModule.EndEnemyTurn(gameObject);
                }
            }
        }

        private void OnEnable()
        {
            _turnModule.OnEnemyEndTurn += DoDebuff;
            _turnModule.OnEnemyStartTurn += OnMyTurn;
            _turnModule.OnEnemyStartTurn += DoAction;
            _turnModule.OnEnemyStartTurn += ManaGain;
            _entityDeathManager.OnDeath += OnDeathEnemy;
        }

        private void OnDisable()
        {
            _turnModule.OnEnemyStartTurn -= OnMyTurn;
            _turnModule.OnEnemyStartTurn -= DoAction;
            _turnModule.OnEnemyStartTurn -= ManaGain;
            _turnModule.OnEnemyEndTurn -= DoDebuff;
            _entityDeathManager.OnDeath -= OnDeathEnemy;
        }

        public void OnMyTurn(object sender, EnemyTurnArgs e)
        {
            if (gameObject != e.Actor) return;
            _highlight.SetActive(true);
        }

        public void EndTurn(object sender, EnemyTurnArgs e)
        {
            if (gameObject != e.Actor) return;
            _turnModule.EndEnemyTurn(gameObject);
        }

        public virtual void DoAction(object sender, EnemyTurnArgs e)
        {
        }

        public virtual void ChooseAction()
        {
        }

        public void DoDebuff(object sender, EnemyTurnArgs e)
        {
            if (gameObject != e.Actor) return;
            _debuffModule.Debuffed(gameObject);
        }

        void Start()
        {
            _skillIndex = 0;
            if (_actionImages != null && _actionImages.Count > 0)
                _actionImageHolder.GetComponent<Image>().sprite = _actionImages[0];
        }

        public void Stunned()
        {
            if (_actionImages != null && _actionImages.Count > 2)
                _actionImageHolder.GetComponent<Image>().sprite = _actionImages[2];
            _actionType = ActionType.Stunned;
        }

        public void Cleanse()
        {
            _actionType = ActionType.Attack;
        }

        public void ManaGain(object sender, EnemyTurnArgs e)
        {
            if (gameObject != e.Actor) return;
            _entityManaManager.RefreshMana();
            MyTurn = true;
        }

        public void OnDeathEnemy()
        {
            _spawner.NumberOfEnemies--;
            _spawner.FreePositions[_spownPosition] = false;
            _battleLog.ShowBattleLogWin();
        }

        private void OnMouseDown()
        {
            _playerDescription.MaxHealth = Health.MaxHealth.ToString();
            _playerDescription.CurrentHealth = Health.CurrentHealth.ToString();
            _playerDescription.Defense = Attributes.Defense.ToString();
            _playerDescription.Damage = Attributes.Damage.ToString();
            _playerDescription.DodgeChance = Attributes.DodgeChance.ToString();
            _playerDescription.DescriptionText = _info.detailInformation.ToString();
            _playerDescription.Name = Attributes.name.ToString();

            _playerDescription.OpenEnemyDescription(gameObject);
        }
    }
}
