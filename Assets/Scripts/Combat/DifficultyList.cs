using Liberator.Combat.Controllers;
using UnityEngine;

namespace Liberator.Combat
{
    [CreateAssetMenu(fileName = "DifficultyList", menuName = "Liberator/Create Difficulty List", order = 2)]
    public class DifficultyList : ScriptableObject
    {
        [SerializeField] private EnemyController[] _enemyList;

        public EnemyController[] EnemyList => _enemyList;
    }
}
