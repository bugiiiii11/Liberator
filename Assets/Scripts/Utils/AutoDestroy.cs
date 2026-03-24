using UnityEngine;

namespace Liberator.Utils
{
    /// <summary>
    /// Auto-destroys the GameObject after a set lifetime.
    /// Used by VFX prefabs (particles, floating text).
    /// </summary>
    public class AutoDestroy : MonoBehaviour
    {
        [SerializeField] private float _lifetime = 1f;

        private void Start()
        {
            Destroy(gameObject, _lifetime);
        }
    }
}
