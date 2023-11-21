using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GercStudio.USK.Scripts
{
    [CreateAssetMenu(fileName = "Weapons Pool", menuName = "Universal Shooter Kit/Weapon Pool")]
    public class WeaponsPool : ScriptableObject
    {
        public List<WeaponController> weapons = new List<WeaponController>();
    }
}
