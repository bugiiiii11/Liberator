using UnityEngine;
using Liberator.Combat.Controllers;

namespace Liberator.Combat.Skills
{
    public class Stun : MonoBehaviour
    {
        EnemyController _enemyController;

        private void Awake()
        {
            _enemyController = GetComponent<EnemyController>();
        }

        private void OnEnable()
        {
            _enemyController.Stunned();
        }

        private void OnDisable()
        {
            _enemyController.Cleanse();
        }
    }
}
