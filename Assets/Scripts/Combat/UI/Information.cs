using UnityEngine;

namespace Liberator.Combat.UI
{
    [CreateAssetMenu(fileName = "Enemy_", menuName = "Liberator/Enemy Information", order = 3)]
    public class Information : ScriptableObject
    {
        public string detailInformation;
    }
}
