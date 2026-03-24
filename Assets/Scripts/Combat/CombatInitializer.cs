using UnityEngine;

namespace Liberator.Combat
{
    /// <summary>
    /// Sets initial combat parameters before Spawner runs.
    /// Uses DefaultExecutionOrder(-100) to guarantee it runs before Spawner.
    /// In production, these values come from the Liberation Map via WebGLBridge.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class CombatInitializer : MonoBehaviour
    {
        [Header("Combat Settings")]
        [SerializeField] private int _difficulty = 0;  // 0=Easy, 1=Normal, 2=Hard
        [SerializeField] private int _enemyCount = 6;  // Total spawn budget (sum of enemy values)

        private void Awake()
        {
            PlayerPrefs.SetInt("Difficulty", _difficulty);
            PlayerPrefs.SetInt("EnemyCount", _enemyCount);
            PlayerPrefs.Save();
            Debug.Log($"[CombatInitializer] Difficulty={_difficulty}, EnemyCount={_enemyCount}");
        }
    }
}
