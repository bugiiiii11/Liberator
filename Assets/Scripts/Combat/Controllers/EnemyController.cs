using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Liberator.Attributes;
using Liberator.Combat.Skills;
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
            if (gameObject != e.Actor) return;
            if (_actionType == ActionType.Stunned) { Cleanse(); return; }
            if (_target == null || !_playerController.IsAlive) return;

            ChooseAction();

            // Execute chosen skill if we have enough mana
            SkillHolder skill = _entityCombatModule.GetSkillOnIndex(_skillIndex);
            if (skill != null && _entityManaManager.CurrentMana >= skill.ManaCost)
            {
                // Heal targets self, other skills target the player
                EntityController attackTarget = (skill.SkillType == SkillType.Heal)
                    ? this as EntityController
                    : _target;
                _entityCombatModule.Attack(attackTarget, _skillIndex);
            }
            else
            {
                // Fall back to skill 1 (basic attack, cheapest)
                _skillIndex = 1;
                skill = _entityCombatModule.GetSkillOnIndex(1);
                if (skill != null && _entityManaManager.CurrentMana >= skill.ManaCost)
                    _entityCombatModule.Attack(_target, _skillIndex);
            }
        }

        public virtual void ChooseAction()
        {
            // AI behavior based on CharacterClass
            switch (_entityAttributes.CharacterClass)
            {
                case CharacterClass.Grunt:
                    AI_Grunt();
                    break;
                case CharacterClass.Archer:
                    AI_Archer();
                    break;
                case CharacterClass.Knight:
                    AI_Knight();
                    break;
                case CharacterClass.Healer:
                    AI_Healer();
                    break;
                case CharacterClass.Hunter:
                    AI_Hunter();
                    break;
                case CharacterClass.Assassin:
                    AI_Assassin();
                    break;
                case CharacterClass.SwordMaster:
                    AI_SwordMaster();
                    break;
                case CharacterClass.Necromancer:
                    AI_Necromancer();
                    break;
                case CharacterClass.Boss:
                    AI_Boss();
                    break;
                default:
                    _skillIndex = 1;
                    break;
            }
        }

        // --- GRUNT: Always attacks with basic attack ---
        private void AI_Grunt()
        {
            _skillIndex = 1;
            _actionType = ActionType.Attack;
        }

        // --- ARCHER: Prefers ranged attacks, uses DoubleShot when mana allows ---
        private void AI_Archer()
        {
            // Skill 1 = ArrowShot (1 mana), Skill 2 = DoubleShot (2 mana)
            if (_entityManaManager.CurrentMana >= 2)
            {
                _skillIndex = 2; // DoubleShot
                _actionType = ActionType.SpecialAttack;
            }
            else
            {
                _skillIndex = 1; // ArrowShot
                _actionType = ActionType.Attack;
            }
        }

        // --- KNIGHT: Uses PowerStrike when at full mana, basic otherwise ---
        private void AI_Knight()
        {
            // Skill 1 = PowerStrike (3 mana)
            if (_entityManaManager.CurrentMana >= 3)
            {
                _skillIndex = 1;
                _actionType = ActionType.SpecialAttack;
            }
            else
            {
                _skillIndex = 1;
                _actionType = ActionType.Deffense; // conserving, but still attacks
            }
        }

        // --- HEALER: Heals self when below 50% HP, otherwise attacks ---
        private void AI_Healer()
        {
            // Skill 1 = Heal (3 mana), Skill 2 = BasicAttack (1 mana)
            float healthPercent = _entityHealthManager.CurrentHealth / _entityHealthManager.MaxHealth;

            if (healthPercent < 0.5f && _entityManaManager.CurrentMana >= 3)
            {
                _skillIndex = 1; // Heal self
                _actionType = ActionType.Heal;
            }
            else
            {
                _skillIndex = 2; // BasicAttack
                _actionType = ActionType.Attack;
            }
        }

        // --- HUNTER: Fast, always double-attacks. Uses Volley when mana is high ---
        private void AI_Hunter()
        {
            // Skill 1 = ArrowShot (1 mana), Skill 2 = Volley (5 mana)
            if (_entityManaManager.CurrentMana >= 5)
            {
                _skillIndex = 2; // Volley (AoE — hits player hard)
                _actionType = ActionType.SpecialAttack;
            }
            else
            {
                _skillIndex = 1; // ArrowShot
                _actionType = ActionType.Attack;
            }
        }

        // --- ASSASSIN: Always uses PoisonStrike to stack poison ---
        private void AI_Assassin()
        {
            // Skill 1 = PoisonStrike (2 mana)
            if (_entityManaManager.CurrentMana >= 2)
            {
                _skillIndex = 1;
                _actionType = ActionType.SpecialAttack;
            }
            else
            {
                _skillIndex = 1;
                _actionType = ActionType.Attack;
            }
        }

        // --- SWORDMASTER: Alternates Cleave and PowerStrike for max damage ---
        private void AI_SwordMaster()
        {
            // Skill 1 = Cleave (4 mana, AoE), Skill 2 = PowerStrike (3 mana)
            if (_entityManaManager.CurrentMana >= 4)
            {
                _skillIndex = 1; // Cleave
                _actionType = ActionType.SpecialAttack;
            }
            else if (_entityManaManager.CurrentMana >= 3)
            {
                _skillIndex = 2; // PowerStrike
                _actionType = ActionType.Attack;
            }
            else
            {
                _skillIndex = 1;
                _actionType = ActionType.Attack;
            }
        }

        // --- NECROMANCER: Uses ColumnStrike for area damage, PoisonStrike to debuff ---
        private void AI_Necromancer()
        {
            // Skill 1 = ColumnStrike (3 mana), Skill 2 = PoisonStrike (2 mana)
            if (_entityManaManager.CurrentMana >= 3)
            {
                _skillIndex = 1; // ColumnStrike
                _actionType = ActionType.SpecialAttack;
            }
            else if (_entityManaManager.CurrentMana >= 2)
            {
                _skillIndex = 2; // PoisonStrike
                _actionType = ActionType.Attack;
            }
            else
            {
                _skillIndex = 1;
                _actionType = ActionType.Attack;
            }
        }

        // --- BOSS: Cycles through all skills for unpredictable attacks ---
        private int _bossPhase = 0;
        private void AI_Boss()
        {
            // Skill 1 = PowerStrike (3 mana), Skill 2 = Cleave (4 mana), Skill 3 = StunBash (3 mana)
            _bossPhase = (_bossPhase + 1) % 3;

            switch (_bossPhase)
            {
                case 0: // PowerStrike
                    if (_entityManaManager.CurrentMana >= 3)
                    {
                        _skillIndex = 1;
                        _actionType = ActionType.Attack;
                    }
                    else goto default;
                    break;
                case 1: // Cleave
                    if (_entityManaManager.CurrentMana >= 4)
                    {
                        _skillIndex = 2;
                        _actionType = ActionType.SpecialAttack;
                    }
                    else goto default;
                    break;
                case 2: // StunBash
                    if (_entityManaManager.CurrentMana >= 3)
                    {
                        _skillIndex = 3;
                        _actionType = ActionType.SpecialAttack;
                    }
                    else goto default;
                    break;
                default:
                    _skillIndex = 1;
                    _actionType = ActionType.Attack;
                    break;
            }
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
