using UnityEngine;

public class SpaceShip : MonoBehaviour
{
    [SerializeField] private PuzzlePiece[] _puzzlePieces;
    [SerializeField] private Animator _animator;

    private int _piecesCollected = 0;
    private static readonly int FlyAway = Animator.StringToHash("FlyAway");

    private void Start()
    {
        SubscribePuzzlePiecePickups();
    }

    private void SubscribePuzzlePiecePickups()
    {
        foreach (var puzzlePiece in _puzzlePieces)
        {
            puzzlePiece.SubscribePickedAction(OnPuzzlePieceCollected);
        }
    }

    private void OnPuzzlePieceCollected()
    {
        if (++_piecesCollected == _puzzlePieces.Length)
        {
            PlayRepairedAnimation();
        }
    }

    private void PlayRepairedAnimation()
    {
        _animator.SetTrigger(FlyAway);
    }
}
