using System;
using _CasualBusJam.Scripts._Data;
using _CasualBusJam.Scripts._Player;
using UnityEngine;

namespace _CasualBusJam.Scripts._Vehicle
{
    public class VehicleController : MonoBehaviour
    {
        public Vehicle[] vehicles;
        public MaterialHolder vehicleMaterialHolder;
        public MaterialHolder stickmanMaterialHolder;
        public Transform road;
        public Transform leftCollider;
        public Transform rightCollider;
        public static VehicleController Instance;
        public int totalPlayersCount;
        private void Awake()
        {
            Instance = this;
            CalculatePlayersCount();
        }

        private void CalculatePlayersCount()
        {
            foreach (var vehicle in vehicles)
            {
                totalPlayersCount += vehicle.SeatCount;
            }
            
            PlayerManager.Instance.InstantiatePlayer(vehicles);
        }
    }
}