using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SlidingFenceBehavior : MonoBehaviour
{

    private Vector3 startingPosition;
    private Vector3 endingPosition;

    // Start is called before the first frame update
    void Start()
    {
        startingPosition = GetComponentInParent<Transform>().position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
