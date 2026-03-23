using UnityEngine;
using TMPro;

namespace Liberator.Combat.UI
{
    public class InfoText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _infoText;
        private float _popUp = 1;
        private float _timePopUp;

        void Start() { _infoText.text = ""; }

        void Update() { _timePopUp += Time.deltaTime; ResetText(); }

        public void NoCooldown() { _timePopUp = 0; _infoText.text = "Skill is on cooldown."; }
        public void NoSkillSelected() { _timePopUp = 0; _infoText.text = "You must select skill first."; }
        public void NoMana() { _timePopUp = 0; _infoText.text = "Mana is too low."; }
        public void OutOfRange() { _timePopUp = 0; _infoText.text = "Out of range."; }
        public void Lose() { _timePopUp = 0; _infoText.text = "You have lost."; }
        public void Win() { _timePopUp = 0; _infoText.text = "You have won."; }
        public void Myturn() { _timePopUp = 0; _infoText.text = "Your turn."; }

        public void ResetText()
        {
            if (_timePopUp > _popUp) _infoText.text = "";
        }
    }
}
