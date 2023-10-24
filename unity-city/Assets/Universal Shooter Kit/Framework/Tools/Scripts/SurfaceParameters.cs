// GercStudio
// © 2018-2020

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GercStudio.USK.Scripts
{
    [CreateAssetMenu(fileName = "Surface Material", menuName = "Universal Shooter Kit/Create New Material")]
    public class SurfaceParameters : ScriptableObject
    {
        public Transform Sparks;
        public Transform Hit;
        public AudioClip HitAudio;
        
        public List <AudioClip> ShellDropSounds;

        [Serializable]
        public class FootstepsSounds
        {
            public List<AudioClip> FootstepsAudios = new List<AudioClip>();
        }

        public FootstepsSounds[] CharacterFootstepsSounds = new FootstepsSounds[0];
        public FootstepsSounds[] EnemyFootstepsSounds = new FootstepsSounds[0];
        
        public int currentCharacterTag;
        public int currentEnemyTag;
        public int tagsCount;
        public int inspectorTab;
        public int stepsTab;
        
        [Range(0,100)]public int ricochetChance = 50;
        
        public float penetrationWidth = 1;
        public bool stepsForAllEnemies;
        
        public ProjectSettings projectSettings;
    }

}


