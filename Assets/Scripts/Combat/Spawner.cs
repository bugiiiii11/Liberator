using System.Collections.Generic;
using UnityEngine;
using Liberator.Combat.Controllers;
using Random = UnityEngine.Random;

namespace Liberator.Combat
{
    public class Spawner : MonoBehaviour
    {
        [SerializeField] private DifficultyList[] _difficultyLists;

        private List<EntityController> _allEnemies = new List<EntityController>();

        [SerializeField] GameObject[] _spawners;
        private int _spawnIndex;
        private bool[] _freePositions = new bool[6];
        private bool _isBossSpawned = false;

        int error = 50;
        private int _numberOfEnemies;

        public int NumberOfEnemies
        {
            get => _numberOfEnemies;
            set => _numberOfEnemies = value;
        }

        public bool[] FreePositions
        {
            get => _freePositions;
            set => _freePositions = value;
        }

        private void Awake()
        {
            _numberOfEnemies = 0;
            SpawnerData(PlayerPrefs.GetInt("Difficulty"), PlayerPrefs.GetInt("EnemyCount"));
        }

        public void SpawnerData(int difficulty, int enemyTotalValue)
        {
            while (enemyTotalValue != 0)
            {
                int _spawnEnemyIndex = Random.Range(0, _difficultyLists[difficulty].EnemyList.Length);
                int myValue = _difficultyLists[difficulty].EnemyList[_spawnEnemyIndex].EnemyValue;

                if (enemyTotalValue - myValue > 0)
                {
                    switch (_difficultyLists[difficulty].EnemyList[_spawnEnemyIndex].PreferLine)
                    {
                        case PreferLine.First:
                            if (!_freePositions[0]) { _spawnIndex = 0; _freePositions[0] = true; SpawnEnemy(difficulty, _spawnEnemyIndex, 0, 0); enemyTotalValue -= myValue; }
                            else if (!_freePositions[1]) { _spawnIndex = 1; _freePositions[1] = true; SpawnEnemy(difficulty, _spawnEnemyIndex, 1, 0); enemyTotalValue -= myValue; }
                            break;
                        case PreferLine.Second:
                            if (!_freePositions[2]) { _spawnIndex = 2; _freePositions[2] = true; SpawnEnemy(difficulty, _spawnEnemyIndex, 0, 1); enemyTotalValue -= myValue; }
                            else if (!_freePositions[3]) { _spawnIndex = 3; _freePositions[3] = true; SpawnEnemy(difficulty, _spawnEnemyIndex, 1, 1); enemyTotalValue -= myValue; }
                            break;
                        case PreferLine.Third:
                            if (!_freePositions[4]) { _spawnIndex = 4; _freePositions[4] = true; SpawnEnemy(difficulty, _spawnEnemyIndex, 0, 2); enemyTotalValue -= myValue; }
                            else if (!_freePositions[5]) { _spawnIndex = 5; _freePositions[5] = true; SpawnEnemy(difficulty, _spawnEnemyIndex, 1, 2); enemyTotalValue -= myValue; }
                            break;
                        case PreferLine.Boss:
                            if (!_freePositions[2] && !_freePositions[4] && !_isBossSpawned) { _spawnIndex = 2; _freePositions[2] = true; _freePositions[4] = true; SpawnEnemy(difficulty, _spawnEnemyIndex, 0, 1); enemyTotalValue -= myValue; _isBossSpawned = true; }
                            else if (!_freePositions[3] && !_freePositions[5] && !_isBossSpawned) { _spawnIndex = 3; _freePositions[3] = true; _freePositions[5] = true; SpawnEnemy(difficulty, _spawnEnemyIndex, 1, 1); enemyTotalValue -= myValue; _isBossSpawned = true; }
                            break;
                    }
                }
                error--;
                if (error < 0) { Debug.Log("Error: EnemyValue is not 0"); break; }
            }
        }

        public void SpawnEnemy(int difficulty, int spawnEnemyIndex, int row, int column)
        {
            if (_difficultyLists[difficulty].EnemyList[spawnEnemyIndex].PreferLine != PreferLine.Boss)
            {
                EntityController enemy = Instantiate(_difficultyLists[difficulty].EnemyList[spawnEnemyIndex], _spawners[_spawnIndex].transform.position, Quaternion.identity);
                enemy.RowIndex = row;
                enemy.ColumnIndex = column;
                enemy.SpawnPosition = _spawnIndex;
                _spawners[_spawnIndex].GetComponent<EnemyHolder>().AddEnemy(enemy);
                _allEnemies.Add(enemy);
            }
            else
            {
                EnemyController boss = Instantiate(_difficultyLists[difficulty].EnemyList[spawnEnemyIndex], _spawners[_spawnIndex].transform.position + new Vector3(1, 0, 0), Quaternion.identity);
                boss.RowIndex = row;
                boss.ColumnIndex = column;
                boss.SpawnPosition = _spawnIndex;
                _spawners[_spawnIndex].GetComponent<EnemyHolder>().AddEnemy(boss);
                boss.ColumnIndex = column + 1;
                boss.SpawnPosition = _spawnIndex + 2;
                _spawners[_spawnIndex + 2].GetComponent<EnemyHolder>().AddEnemy(boss);
                _allEnemies.Add(boss);
            }
            _numberOfEnemies++;
        }
    }
}
