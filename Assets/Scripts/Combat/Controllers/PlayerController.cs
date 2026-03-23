using UnityEngine;
using Liberator.Attributes;
using Liberator.Combat.UI;
using Liberator.TurnManager;

namespace Liberator.Combat.Controllers
{
    public class PlayerController : EntityController
    {
        private DebuffModule _debuffModule;
        TurnModule _turnModule;

        [Header("Player UI")]
        [SerializeField] private Description _playerDescription;
        private EnemyHolder _enemyHolder;

        [SerializeField] private InfoText _infoText;

        private BattleLog _battleLog;

        private string _myText;
        private bool _isAlive;
        public bool IsAlive => _isAlive;
        public string MyText => _myText;

        private void Awake()
        {
            _playerDescription = GameObject.FindGameObjectWithTag("PlayerInfo").GetComponent<Description>();
            _debuffModule = GameObject.FindGameObjectWithTag("Debuff").GetComponent<DebuffModule>();
            _turnModule = GameObject.FindGameObjectWithTag("Finish").GetComponent<TurnModule>();
            _battleLog = GameObject.FindGameObjectWithTag("BattleLog").GetComponent<BattleLog>();
            _myMaterial = GetComponentInChildren<Renderer>().material;

            _entityType = EntityType.Player;
            _entityManaManager = GetComponent<ManaManager>();
            _entityHealthManager = GetComponent<HealthManager>();
            _entityCombatModule = GetComponent<CombatManager>();
            _entityAttributes = GameObject.FindGameObjectWithTag("Player").GetComponent<BaseAttributes>();
            _entityDeathManager = GetComponent<DeathManager>();
            _enemyHolder = GetComponent<EnemyHolder>();
        }

        private void Start()
        {
            _isAlive = true;
            _enemyHolder.AddEnemy(this as EntityController);
        }

        private void OnEnable()
        {
            _entityDeathManager.OnDeath += OnDeathPlayer;
            _turnModule.OnPlayerEndTurn += DoDebuff;
        }

        private void OnDisable()
        {
            _entityDeathManager.OnDeath -= OnDeathPlayer;
            _turnModule.OnPlayerEndTurn -= DoDebuff;
        }

        public void OnMyTurn()
        {
            _myTurn = true;
            _infoText.Myturn();
            _highlight.SetActive(true);
        }

        public void ManaGain()
        {
            _entityManaManager.RefreshMana();
        }

        public void OnDeathPlayer()
        {
            _isAlive = false;
            _infoText.Lose();
            _battleLog.ShowBattleLogLose();
        }

        private void OnMouseDown()
        {
            _playerDescription.MaxHealth = Health.MaxHealth.ToString();
            _playerDescription.CurrentHealth = Health.CurrentHealth.ToString();
            _playerDescription.Defense = Attributes.Defense.ToString();
            _playerDescription.Damage = Attributes.Damage.ToString();
            _playerDescription.CritChance = Attributes.CritChance.ToString();
            _playerDescription.CritDamage = Attributes.CritDamage.ToString();
            _playerDescription.DodgeChance = Attributes.DodgeChance.ToString();
            _playerDescription.Name = Attributes.name.ToString();

            _playerDescription.OpenPlayerDescription(gameObject);
        }

        public void DoDebuff()
        {
            _debuffModule.Debuffed(gameObject);
        }

        public void OnEndTurn()
        {
            _highlight.SetActive(false);
            _myTurn = false;
        }
    }
}
