using Unity.Netcode;
using UnityEngine;

namespace Demo
{
    public class EscapeShip : NetworkBehaviour
    {
        [SerializeField] private Animator _animator;
        
        private static readonly int FlyAwayTriggerHash = Animator.StringToHash("FlyAway");

        public void FlyAway()
        {
            _animator.SetTrigger(FlyAwayTriggerHash);
        }
    }
}