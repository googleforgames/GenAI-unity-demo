using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GercStudio.USK.Scripts
{
    [RequireComponent(typeof(BoxCollider))]
    public class Portal : MonoBehaviour
    {
        
        public enum PortalType
        {
            Point, 
            Scene
        }
        
        public PortalType portalType;
        
        public Transform destination;
        
        public string sceneName;
        
        private float portalTimeout;
        
        private void OnEnable()
        {
            portalTimeout = 3;
        }

        void Start()
        {
            GetComponent<BoxCollider>().isTrigger = true;
        }

        void Update()
        {
            portalTimeout += Time.deltaTime;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (portalTimeout > 3)
                {
                    portalTimeout = 0;
                    
                    if(portalType == PortalType.Point)
                        other.transform.position = destination.position;
                    else
                    {
                        if (SaveManager.Instance)
                            SaveManager.Instance.SaveData();

                        SceneManager.LoadScene(sceneName);
                    }
                }
            }
        }
    }
}