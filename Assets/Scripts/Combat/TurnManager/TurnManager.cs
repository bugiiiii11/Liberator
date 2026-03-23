using System.Collections;
using TMPro;
using System.Linq;
using System;
using Liberator.Attributes;
using Liberator.Combat.Controllers;
using UnityEngine;
using UnityEngine.UI;

namespace Liberator.TurnManager
{
    public class TurnManager : MonoBehaviour
    {
        private TurnModule _turnModule;
        [SerializeField] private GameObject[] _initiativeList;
        private GameObject[] _enemies;
        private GameObject[] _players;
        private int _turnCount = 1;
        [SerializeField] private TextMeshProUGUI _turnText;
        public PlayerController _playerController;

        private Button _button;

        private void Awake()
        {
            _turnModule = GetComponent<TurnModule>();
            _button = GetComponent<Button>();
            OnSpawn();
        }

        private void Start()
        {
            StartCoroutine(StartGame());
        }

        private IEnumerator StartGame()
        {
            yield return new WaitForSeconds(1f);
            DoCharacterList();
            WhosTurn();
            _turnText.text = "Turn: 1";
        }

        private void Update()
        {
            if (!_playerController.MyTurn)
                _button.interactable = false;
            else
                _button.interactable = true;
        }

        public void OnSpawn()
        {
            _turnModule.OnStartTurn += _playerController.ManaGain;
            _turnModule.OnPlayerEndTurn += _playerController.OnEndTurn;
            _turnModule.OnPlayerEndTurn += WhosTurn;
            _turnModule.OnPlayerStartTurn += _playerController.OnMyTurn;
            _turnModule.OnEnemyEndTurn += WhosTurn;
        }

        private void WhosTurn(object sender, EnemyTurnArgs e)
        {
            RemoveElement(ref _initiativeList, 0);
            WhosTurn();
        }

        public void OnDeath()
        {
            _turnModule.OnStartTurn -= _playerController.ManaGain;
            _turnModule.OnPlayerEndTurn -= WhosTurn;
            _turnModule.OnPlayerStartTurn -= _playerController.ManaGain;
            _turnModule.OnEnemyEndTurn -= WhosTurn;
            _turnModule.OnPlayerStartTurn -= _playerController.OnMyTurn;
            _turnModule.OnPlayerEndTurn -= _playerController.OnEndTurn;
        }

        public void DoCharacterList()
        {
            _enemies = GameObject.FindGameObjectsWithTag("Enemy");
            _players = GameObject.FindGameObjectsWithTag("Player");
            _initiativeList = _enemies.Concat(_players).ToArray();
            SortList();
        }

        public void SortList()
        {
            _initiativeList = _initiativeList.OrderByDescending(x => x.GetComponent<BaseAttributes>().Initiative).ToArray();
        }

        public void TurnCounter()
        {
            _turnCount++;
            _turnText.text = "Turn: " + _turnCount.ToString();
        }

        public void WhosTurn()
        {
            if (_initiativeList.Length > 0)
            {
                if (_initiativeList[0] != null)
                {
                    if (_initiativeList[0].tag == "Player")
                    {
                        _playerController.MyTurn = true;
                        _turnModule.StartPlayerTurn();
                        RemoveElement(ref _initiativeList, 0);
                    }
                    else if (_initiativeList[0].tag == "Enemy")
                    {
                        _playerController.MyTurn = false;
                        _turnModule.StartEnemyTurn(_initiativeList[0]);
                    }
                }
                else
                {
                    RemoveElement(ref _initiativeList, 0);
                    WhosTurn();
                }
            }
            else
            {
                _turnModule.StartTurn();
                DoCharacterList();
                TurnCounter();
                WhosTurn();
            }
        }

        public void RemoveElement<T>(ref T[] arr, int index)
        {
            for (int i = index; i < arr.Length - 1; i++)
            {
                arr[i] = arr[i + 1];
            }
            Array.Resize(ref arr, arr.Length - 1);
        }
    }
}
