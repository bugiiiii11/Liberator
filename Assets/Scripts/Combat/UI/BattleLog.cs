using Liberator.Combat.Controllers;
using UnityEngine;
using TMPro;

namespace Liberator.Combat.UI
{
    public class BattleLog : MonoBehaviour
    {
        [SerializeField] private GameObject _battleLog;
        private Spawner _spawner;
        [SerializeField] private GameObject _darkScreen;
        [SerializeField] private PlayerController _playerController;

        private void Awake()
        {
            _spawner = GameObject.FindGameObjectWithTag("Spawner").GetComponent<Spawner>();
        }

        private void Start()
        {
            _darkScreen.SetActive(false);
        }

        public void ShowBattleLogWin()
        {
            if (_spawner.NumberOfEnemies <= 0)
            {
                if (_playerController.IsAlive)
                {
                    _darkScreen.GetComponentInChildren<TextMeshProUGUI>().text = "VICTORY";
                    _darkScreen.SetActive(true);
                }
            }
        }

        public void ShowBattleLogLose()
        {
            _darkScreen.GetComponentInChildren<TextMeshProUGUI>().text = "DEFEAT";
            _darkScreen.SetActive(true);
        }
    }
}
