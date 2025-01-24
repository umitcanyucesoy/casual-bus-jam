using System;
using UnityEngine;

namespace _CasualBusJam.Scripts.VFX
{
    public class EffectsManager : MonoBehaviour
    {
        public GameObject hitEffect;
        
        public static EffectsManager Instance { get; private set; }
        private void Awake()
        {
            Instance = this;
        }

        public void PlayEffect(GameObject effect, Vector3 position, Quaternion rotation)
        {
            Instantiate(effect, position, rotation);
        }
    }
}