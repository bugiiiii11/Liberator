using Liberator.Attributes;
using UnityEngine;

namespace Liberator.Combat.Controllers
{
    public enum EntityType
    {
        Player,
        Enemy
    }

    public enum PreferLine
    {
        First,
        Second,
        Third,
        Boss
    }

    public class EntityController : MonoBehaviour
    {
        [Header("Entity Attributes")]
        [SerializeField] protected BaseAttributes _entityAttributes;
        [SerializeField] protected CombatManager _entityCombatModule;
        [SerializeField] protected ManaManager _entityManaManager;
        [SerializeField] protected HealthManager _entityHealthManager;
        [SerializeField] protected DeathManager _entityDeathManager;
        [SerializeField] protected EntityType _entityType;
        [SerializeField] protected GameObject _highlight;
        [SerializeField] protected PreferLine _preferLine;

        protected int _spownPosition;
        protected bool _myTurn;
        protected bool _isInvisible;
        protected int _rowIndex;
        protected int _columnIndex;
        [SerializeField] protected Material _myMaterial;
        [SerializeField] protected GameObject _poison;

        public BaseAttributes Attributes => _entityAttributes;
        public CombatManager Combat => _entityCombatModule;
        public GameObject Poison => _poison;
        public ManaManager Mana => _entityManaManager;
        public HealthManager Health => _entityHealthManager;
        public DeathManager DeathManager => _entityDeathManager;
        public GameObject Highlight => _highlight;
        public Material MyMaterial => _myMaterial;
        public EntityType EntityType => _entityType;
        public PreferLine PreferLine => _preferLine;

        public int SpawnPosition
        {
            get => _spownPosition;
            set => _spownPosition = value;
        }

        public int RowIndex
        {
            get => _rowIndex;
            set => _rowIndex = value;
        }

        public int ColumnIndex
        {
            get => _columnIndex;
            set => _columnIndex = value;
        }

        public bool MyTurn
        {
            get => _myTurn;
            set => _myTurn = value;
        }

        public bool IsInvisible
        {
            get => _isInvisible;
            set => _isInvisible = value;
        }
    }
}
