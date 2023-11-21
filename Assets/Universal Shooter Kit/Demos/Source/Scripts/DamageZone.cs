using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GercStudio.USK.Scripts
{
    [RequireComponent(typeof(BoxCollider))]
    public class DamageZone : MonoBehaviour
    {
        public int DamagePerSecond = 5;
        
        public List<int> knownEnemies = new List<int>();

        private void Start()
        {
            GetComponent<BoxCollider>().isTrigger = true;
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                var controller = other.gameObject.GetComponent<Controller>();
                controller.Damage(DamagePerSecond * Time.deltaTime, "fire");
            }

            if (other.gameObject.GetComponentInParent<AIController>())
            {
                var controller = other.gameObject.GetComponentInParent<AIController>();
                controller.Damage(DamagePerSecond / 5f * Time.deltaTime, "fire");
            }
        }
    }
}
