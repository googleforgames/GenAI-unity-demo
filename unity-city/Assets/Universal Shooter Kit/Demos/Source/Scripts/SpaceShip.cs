using UnityEngine;

public class SpaceShip : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    private static readonly int FlyAway = Animator.StringToHash("FlyAway");

    private void PlayRepairedAnimation()
    {
        _animator.SetTrigger(FlyAway);
    }
}
