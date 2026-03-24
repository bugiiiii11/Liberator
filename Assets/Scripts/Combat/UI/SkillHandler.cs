using System;
using System.Collections.Generic;
using Liberator.Combat.Controllers;
using Liberator.Combat.Skills;
using Liberator.TurnManager;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Liberator.Combat.UI
{
    public class SkillArgs : EventArgs
    {
        public SkillHolder Skill { get; set; }
    }

    public class SkillHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public EventHandler<SkillArgs> OnSkilLLoaded;

        private RectTransform _rectTransform;
        Ray _ray;
        RaycastHit _hit;
        private Canvas _canvas;

        [SerializeField] private Material _material;
        private ColorButton _colorButton;
        private TurnModule _turnModule;
        private EntityController _entityController;

        private int _skillIndex;
        private InfoText _infoText;
        private SkillHolder _skillHolder;
        private Description _description;

        private GameObject[] _allEnemies;
        private GameObject[] _placements;
        private List<GameObject> _attackableEnemies = new List<GameObject>();

        private bool _isAttackble;
        private bool _isbeginDrag = false;
        private float _currentCooldown;
        private int _rangeIndex;

        public int Index
        {
            get => _skillIndex;
            set => _skillIndex = value;
        }

        private void Awake()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            _entityController = player.GetComponent<EntityController>();
            _infoText = FindObjectOfType<InfoText>();
            _description = GameObject.FindGameObjectWithTag("PlayerInfo").GetComponent<Description>();
            _turnModule = FindObjectOfType<TurnModule>();
            _canvas = GameObject.FindGameObjectWithTag("Canvas").GetComponent<Canvas>();
            _rectTransform = GetComponent<RectTransform>();
            _colorButton = GetComponent<ColorButton>();
        }

        private void OnEnable()
        {
            _turnModule.OnPlayerStartTurn += CooldownUp;
        }

        private void OnDisable()
        {
            _turnModule.OnPlayerStartTurn -= CooldownUp;
        }

        void Start()
        {
            LoadSkill();
            _allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
            _placements = GameObject.FindGameObjectsWithTag("Placement");
            _currentCooldown = 0;
            _colorButton.CldSlider.maxValue = _skillHolder.Cooldown;
            _colorButton.CldSlider.value = _currentCooldown;
        }

        public void OpenDescription()
        {
            _description.Name = _skillHolder.Name;
            _description.ActionPoint = _skillHolder.ManaCost.ToString();
            _description.Cooldown = _skillHolder.Cooldown.ToString();
            if (_skillHolder.SkillType == SkillType.Heal)
                _description.Damage = (_entityController.Attributes.Damage + _skillHolder.SourceWeaponDamage).ToString();
            else
                _description.Damage = ((_entityController.Attributes.Damage + _skillHolder.SourceWeaponDamage) * (_skillHolder.DamagePercent / 100)).ToString();

            _description.DescriptionText = _skillHolder.Description;
            _description.OpenSkillDescription(gameObject);
        }

        private void LoadSkill()
        {
            _skillHolder = _entityController.Combat.GetSkillOnIndex(_skillIndex);
            OnSkilLLoaded?.Invoke(this, new SkillArgs { Skill = _skillHolder });
        }

        void Update()
        {
            if (_skillHolder.ManaCost > _entityController.Mana.CurrentMana)
                _colorButton.ChangeColorToLowMana();
            else
                _colorButton.ResetColor();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_entityController.MyTurn) return;
            _isbeginDrag = false;
            _description.Close();
            _allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject item in _allEnemies)
            {
                item.GetComponent<EnemyController>().Highlight.SetActive(false);
                foreach (SpriteRenderer renderer in item.GetComponent<EnemyController>().GetComponentsInChildren<SpriteRenderer>())
                    renderer.color = Color.white;
            }

            gameObject.SetActive(false);
            gameObject.SetActive(true);
            _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(_ray, out _hit, 100f))
            {
                if (_hit.transform != null)
                {
                    foreach (GameObject item in _attackableEnemies)
                    {
                        if (item == _hit.transform.gameObject)
                        {
                            if (_isAttackble && !item.GetComponent<EnemyHolder>().GetEnemyController().IsInvisible)
                            {
                                _entityController.Combat.Attack(item.GetComponent<EnemyHolder>().GetEnemyController(), _skillIndex, item);
                                _currentCooldown = _skillHolder.Cooldown;
                                _colorButton.CldSlider.value = _currentCooldown;
                            }
                        }
                    }
                }
            }
        }

        Transform hittedHit;

        public void OnDrag(PointerEventData eventData)
        {
            if (!_entityController.MyTurn) return;
            if (!_isAttackble) return;
            if (!_isbeginDrag) return;

            _rectTransform.anchoredPosition += eventData.delta / _canvas.scaleFactor;

            _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(_ray, out _hit, 100f))
            {
                if (_hit.transform != null)
                {
                    if (_hit.transform != hittedHit.transform)
                    {
                        foreach (GameObject item in _placements)
                        {
                            EnemyHolder enemyHolder = item.GetComponent<EnemyHolder>();
                            if (enemyHolder.GetEnemyController() != null)
                                enemyHolder.GetEnemyController().Highlight.SetActive(false);
                        }

                        hittedHit = _hit.transform;
                        foreach (GameObject hittedItem in _attackableEnemies)
                        {
                            if (hittedItem == _hit.transform.gameObject)
                            {
                                if (_isAttackble)
                                {
                                    EnemyHolder _enemyHolder = hittedItem.GetComponent<EnemyHolder>();
                                    if (_enemyHolder.GetEnemyController().IsInvisible) return;

                                    switch (_skillHolder.AreaOfEffect)
                                    {
                                        case AreaOfEffect.Single:
                                            _enemyHolder.GetEnemyController().Highlight.SetActive(true);
                                            break;
                                        case AreaOfEffect.Line:
                                            foreach (GameObject highlightItem in _placements)
                                            {
                                                EntityController itemEntity = highlightItem.GetComponent<EnemyHolder>().GetEnemyController();
                                                if (itemEntity != null && itemEntity.RowIndex == _enemyHolder.GetEnemyController().RowIndex)
                                                    itemEntity.Highlight.SetActive(true);
                                            }
                                            break;
                                        case AreaOfEffect.All:
                                            foreach (GameObject highlightItem in _placements)
                                            {
                                                EntityController itemEntity = highlightItem.GetComponent<EnemyHolder>().GetEnemyController();
                                                if (itemEntity != null)
                                                    itemEntity.Highlight.SetActive(true);
                                            }
                                            break;
                                        case AreaOfEffect.Cross:
                                            foreach (GameObject highlightItem in _placements)
                                            {
                                                EnemyHolder _itemEntity = highlightItem.GetComponent<EnemyHolder>();
                                                if (_itemEntity.GetEnemyController() != null)
                                                    if (_enemyHolder.ColumnIndex == _itemEntity.ColumnIndex || _enemyHolder.RowIndex == _itemEntity.RowIndex)
                                                        _itemEntity.GetEnemyController().Highlight.SetActive(true);
                                            }
                                            break;
                                        case AreaOfEffect.Column:
                                            foreach (GameObject highlightItem in _placements)
                                            {
                                                EnemyHolder _itemEntity = highlightItem.GetComponent<EnemyHolder>();
                                                if (_itemEntity.GetEnemyController() != null)
                                                    if (_enemyHolder.ColumnIndex == _itemEntity.ColumnIndex)
                                                        _itemEntity.GetEnemyController().Highlight.SetActive(true);
                                            }
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!_entityController.MyTurn) return;
            _isbeginDrag = true;
            hittedHit = gameObject.transform;
            CheckSkill();
            _rangeIndex = 0;
            CheckTarget();
            foreach (GameObject item in _placements)
            {
                EnemyHolder enemyHolder = item.GetComponent<EnemyHolder>();
                if (enemyHolder.GetEnemyController() != null)
                    foreach (SpriteRenderer renderer in enemyHolder.GetEnemyController().GetComponentsInChildren<SpriteRenderer>())
                        renderer.color = Color.black;
            }

            foreach (GameObject item in _attackableEnemies)
            {
                EnemyHolder enemyHolder = item.GetComponent<EnemyHolder>();
                if (!enemyHolder.GetEnemyController().IsInvisible)
                    foreach (SpriteRenderer renderer in enemyHolder.GetEnemyController().GetComponentsInChildren<SpriteRenderer>())
                        renderer.color = Color.white;
            }
        }

        public void CheckTarget()
        {
            _attackableEnemies.Clear();
            _placements = GameObject.FindGameObjectsWithTag("Placement");
            switch (_skillHolder.SkillType)
            {
                case SkillType.Melee:
                    foreach (GameObject item in _placements)
                    {
                        EnemyHolder itemObject = item.GetComponent<EnemyHolder>();
                        var itemEntity = itemObject.GetEnemyController();
                        if (itemEntity != null && itemObject.ColumnIndex == _rangeIndex)
                            _attackableEnemies.Add(item);
                    }
                    break;
                case SkillType.Ranged:
                    foreach (GameObject item in _placements)
                    {
                        EnemyHolder itemObject = item.GetComponent<EnemyHolder>();
                        var itemEntity = itemObject.GetEnemyController();
                        if (itemEntity != null)
                            _attackableEnemies.Add(item);
                    }
                    break;
                case SkillType.Heal:
                    _attackableEnemies.Add(_entityController.gameObject);
                    break;
            }

            if (_attackableEnemies.Count == 0)
            {
                _rangeIndex++;
                if (_rangeIndex < 4) CheckTarget();
            }
        }

        public void CheckSkill()
        {
            if (_skillHolder.ManaCost <= _entityController.Mana.CurrentMana)
            {
                if (_currentCooldown <= 0) _isAttackble = true;
                else { _isAttackble = false; _infoText.NoCooldown(); }
            }
            else { _isAttackble = false; _infoText.NoMana(); }
        }

        public void CooldownUp()
        {
            _currentCooldown--;
            _colorButton.CldSlider.value = _currentCooldown;
        }
    }
}
