using System;
using UnityEngine;

namespace Liberator.TurnManager
{
    public class DebuffModule : MonoBehaviour
    {
        public event EventHandler<EnemyTurnArgs> OnDebuffed;

        public void Debuffed(GameObject iniciator)
        {
            EnemyTurnArgs e = new EnemyTurnArgs() { Actor = iniciator };
            OnDebuffed?.Invoke(this, e);
        }
    }
}
