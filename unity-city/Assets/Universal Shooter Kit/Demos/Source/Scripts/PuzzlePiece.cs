using System;
using UnityEngine;
using UnityEngine.UI;

public class PuzzlePiece : MonoBehaviour
{
    [SerializeField] private Text _uiHint;
    private Action _piecePicked;
    private bool _isShown = false;

    private void Update()
    {
        if (_isShown && Input.GetKey(KeyCode.E))
        {
            ActivatePiece();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !_isShown)
        {
            ShowHint(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && _isShown)
        {
            ShowHint(false);
        }
    }

    public void SubscribePickedAction(Action callback)
    {
        _piecePicked = callback;
    }

    private void ShowHint(bool showHint)
    {
        _isShown = showHint;
        _uiHint.gameObject.SetActive(showHint);
    }

    private void ActivatePiece()
    {
        Debug.Log($"An object {this} is activated");
        ShowHint(false);
        _piecePicked.Invoke();
        gameObject.SetActive(false);
    }
}
