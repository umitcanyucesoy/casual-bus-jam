using System;
using _CasualBusJam.Scripts._Data;
using UnityEngine;

namespace _CasualBusJam.Scripts._Vehicle
{
    public class VehicleController : MonoBehaviour
    {
        public Vehicle[] vehicles;
        public MaterialHolder vehicleMaterialHolder;
        public Transform road;
        public Transform leftCollider;
        public Transform rightCollider;
        public static VehicleController Instance;
        private void Awake()
        {
            Instance = this;
        }
        
        
    }
}