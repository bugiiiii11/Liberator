using UnityEngine;
using Liberator.Combat.Controllers;

namespace Liberator.Combat
{
    public class EnemyHolder : MonoBehaviour
    {
        private EntityController _enemyControllerHolder;

        private int _rowIndex;
        private int _columnIndex;

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

        public void AddEnemy(EntityController enemy)
        {
            if (enemy != null)
            {
                _enemyControllerHolder = enemy;
                _columnIndex = enemy.ColumnIndex;
                _rowIndex = enemy.RowIndex;
            }
        }

        public EntityController GetEnemyController()
        {
            if (_enemyControllerHolder == null)
                return gameObject.GetComponent<PlayerController>();

            return _enemyControllerHolder;
        }
    }
}
