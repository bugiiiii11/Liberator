using System;
using UnityEngine;

namespace Liberator.TurnManager
{
    public class EnemyTurnArgs : EventArgs
    {
        public GameObject Actor { get; set; }
    }

    public class TurnModule : MonoBehaviour
    {
        public event Action OnStartTurn;
        public event Action OnEndTurn;

        public event Action OnPlayerStartTurn;
        public event Action OnPlayerEndTurn;

        public event EventHandler<EnemyTurnArgs> OnEnemyStartTurn;
        public event EventHandler<EnemyTurnArgs> OnEnemyEndTurn;

        public void StartTurn()
        {
            OnStartTurn?.Invoke();
        }

        public void EndTurn()
        {
            OnEndTurn?.Invoke();
        }

        public void StartPlayerTurn()
        {
            OnPlayerStartTurn?.Invoke();
        }

        public void EndPlayerTurn()
        {
            OnPlayerEndTurn?.Invoke();
        }

        public void StartEnemyTurn(GameObject iniciator)
        {
            EnemyTurnArgs e = new EnemyTurnArgs() { Actor = iniciator };
            OnEnemyStartTurn?.Invoke(this, e);
        }

        public void EndEnemyTurn(GameObject iniciator)
        {
            EnemyTurnArgs e = new EnemyTurnArgs() { Actor = iniciator };
            OnEnemyEndTurn?.Invoke(this, e);
        }
    }
}
