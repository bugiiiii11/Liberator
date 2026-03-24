using UnityEngine;

namespace Liberator.Utils
{
    /// <summary>
    /// Floats the attached text upward over its lifetime.
    /// Used by HurtText and HealText VFX prefabs.
    /// </summary>
    public class FloatingText : MonoBehaviour
    {
        [SerializeField] private float _floatSpeed = 1.5f;
        [SerializeField] private float _fadeSpeed = 1.5f;

        private TMPro.TextMeshPro _tmp;
        private Color _startColor;

        private void Start()
        {
            _tmp = GetComponent<TMPro.TextMeshPro>();
            if (_tmp != null) _startColor = _tmp.color;
        }

        private void Update()
        {
            transform.position += Vector3.up * _floatSpeed * Time.deltaTime;

            if (_tmp != null)
            {
                var c = _tmp.color;
                c.a -= _fadeSpeed * Time.deltaTime;
                _tmp.color = c;
            }
        }
    }
}
