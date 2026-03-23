using UnityEngine;
using Liberator.Combat.Controllers;

namespace Liberator.Combat
{
    [RequireComponent(typeof(EnemyController))]
    public class EnemyCombatManager : CombatManager
    {
        private EnemyController _enemyController;

        protected new void Start()
        {
            base.Start();
            if (GetComponent<EnemyController>())
            {
                _enemyController = GetComponent<EnemyController>();
                AppendBasicSkill();
                _myself = GetComponent<EnemyController>() as EntityController;
            }
        }
    }
}
