using UnityEngine;
using System;

namespace Liberator.Combat
{
    public class DeathManager : MonoBehaviour
    {
        public event Action OnDeath;

        public void Death()
        {
            OnDeath?.Invoke();
        }
    }
}
