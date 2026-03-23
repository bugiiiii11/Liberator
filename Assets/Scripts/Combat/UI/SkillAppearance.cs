using TMPro;
using UnityEngine;

namespace Liberator.Combat.UI
{
    public class SkillAppearance : MonoBehaviour
    {
        [SerializeField] private SkillHandler _skillManager;
        [SerializeField] private TextMeshProUGUI _manaCostText;
        [SerializeField] private TextMeshProUGUI _nameOfSkillText;

        private void OnEnable() { _skillManager.OnSkilLLoaded += SetupCard; }
        private void OnDisable() { _skillManager.OnSkilLLoaded -= SetupCard; }

        private void SetupCard(object sender, SkillArgs e)
        {
            _manaCostText.text = e.Skill.ManaCost.ToString();
            _nameOfSkillText.text = e.Skill.Name;
        }
    }
}
