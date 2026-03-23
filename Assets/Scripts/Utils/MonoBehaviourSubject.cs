using System.Collections.Generic;
using UnityEngine;

namespace Liberator.Utils
{
    public class MonoBehaviourSubject : MonoBehaviour
    {
        protected List<Observer> _observers = new List<Observer>();

        public virtual void Notify()
        {
            for (int i = 0; i < _observers.Count; i++)
            {
                _observers[i].OnNotify();
            }
        }

        public void AddObserver(Observer observer)
        {
            _observers.Add(observer);
        }

        public void RemoveObserver(Observer observer)
        {
            _observers.Remove(observer);
        }
    }
}
