using System;
using System.Collections.Generic;
using UnityEngine;

namespace GercStudio.USK.Scripts
{
    public class Surface : MonoBehaviour
    {
        public SurfaceParameters Material; 
        // public bool Cover;
        // public bool grass;
        
        [HideInInspector] public Transform Sparks;
        [HideInInspector] public Transform Hit;
        [HideInInspector] public AudioClip HitAudio;
        [HideInInspector] public List<AudioClip> ShellDropSounds;
        [HideInInspector] public SurfaceParameters.FootstepsSounds[] CharacterFootstepsSounds;
        [HideInInspector] public SurfaceParameters.FootstepsSounds[] EnemyFootstepsSounds;
        [HideInInspector] public GameObject Shadow;

        void Awake()
        {
            if (Material)
            {
                Array.Resize(ref CharacterFootstepsSounds, Material.CharacterFootstepsSounds.Length);
                Array.Resize(ref EnemyFootstepsSounds, Material.EnemyFootstepsSounds.Length);

                
                Sparks = Material.Sparks;
                Hit = Material.Hit;
                
                CharacterFootstepsSounds = Material.CharacterFootstepsSounds;
                EnemyFootstepsSounds = Material.EnemyFootstepsSounds;
                
                ShellDropSounds = Material.ShellDropSounds;
                HitAudio = Material.HitAudio;
            }

            // if (grass)
            //     gameObject.layer = 10;
        }

       
    }
}


