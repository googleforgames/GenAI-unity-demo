using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GercStudio.USK.Scripts
{
    public class Door : MonoBehaviour
    {
        public Transform door;
        
        public Vector3 openedPosition;
        public Vector3 closedPosition;

        public bool saveStatus;
        public bool automaticallyClose;
        public float speed;

        private bool openDoor;
        private float timeout;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                openDoor = true;
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                openDoor = false;
                timeout = 0;
            }
        }

        public void Start()
        {
            if(PlayerPrefs.HasKey(gameObject.name))
            {
                door.localPosition = openedPosition;
            }
        }

        public void Update()
        {
            if (openDoor)
            {
                door.localPosition = Vector3.MoveTowards(door.localPosition, openedPosition, speed * Time.deltaTime);
                
                if(saveStatus)
                    PlayerPrefs.SetInt(gameObject.name, 1);
            }
            else
            {
                if (automaticallyClose)
                {
                    timeout += Time.deltaTime;

                    if (timeout > 3)
                    {
                        door.localPosition = Vector3.MoveTowards(door.localPosition, closedPosition, speed * Time.deltaTime);

                        if (Helper.ReachedPositionAndRotation(door.localPosition, closedPosition, 0.1f))
                            timeout = 0;
                    }
                }
            }
        }
    }
}
