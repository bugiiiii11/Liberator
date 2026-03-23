using UnityEngine;
using UnityEngine.UI;

namespace Liberator.Combat.UI
{
    public class ColorButton : MonoBehaviour
    {
        [SerializeField] private Color _activeColor;
        [SerializeField] private Color _cooldownColor;
        [SerializeField] private Color _lowManaColor;
        private Color _basicColor;
        [SerializeField] private Slider _cldSlider;

        public Slider CldSlider
        {
            get => _cldSlider;
            set => _cldSlider = value;
        }

        private void Awake()
        {
            _basicColor = gameObject.GetComponent<Image>().color;
        }

        public void ChangeColorToActive() { gameObject.GetComponent<Image>().color = _activeColor; }
        public void ChangeColorToCooldown() { gameObject.GetComponent<Image>().color = _cooldownColor; }
        public void ChangeColorToLowMana() { gameObject.GetComponent<Image>().color = _lowManaColor; }
        public void ResetColor() { gameObject.GetComponent<Image>().color = _basicColor; }
    }
}
