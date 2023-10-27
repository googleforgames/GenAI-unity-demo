using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

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
        // TODO implement the flying away animation
        _animator.SetTrigger(FlyAway);
        
        var transform = gameObject.transform;
        transform.rotation = Quaternion.identity;
        var position = transform.position;
        transform.Translate(new Vector3(position.x + 2, position.y, position.z));
    }
}
