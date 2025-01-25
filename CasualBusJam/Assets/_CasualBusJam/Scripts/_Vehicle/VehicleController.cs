using System;
using System.Collections.Generic;
using System.Linq;
using _CasualBusJam.Scripts._Data;
using _CasualBusJam.Scripts._Enum;
using _CasualBusJam.Scripts._Player;
using Unity.VisualScripting;
using UnityEngine;

namespace _CasualBusJam.Scripts._Vehicle
{
    public class VehicleController : MonoBehaviour
    {
        [Header("----- Vehicle Elements ------")]
        public Vehicle[] vehicles;
        public MaterialHolder vehicleMaterialHolder;
        public Transform road;
        public Transform leftCollider;
        public Transform rightCollider;
        
        [Header("----- Vehicle Settings ------")]
        public int totalPlayersCount;
        
        
        public static VehicleController Instance;
        private void Awake()
        {
            Instance = this;
            vehicleMaterialHolder.InitializeMaterialDictionary();
            RandomVehColor();
            
        }

        private void Start()
        {
            CalculatePlayersCount();
        }

        private void CalculatePlayersCount()
        {
            foreach (var vehicle in vehicles)
            {
                totalPlayersCount += vehicle.SeatCount;
            }
            
            PlayerController.Instance.InstantiatePlayer(vehicles);
        }

        private void RandomVehColor()
        {
            System.Random random = new System.Random();
            ColorEnum[] values = Enum.GetValues(typeof(ColorEnum)) as ColorEnum[];
            if (values != null)
            {
                List<ColorEnum> colors = new List<ColorEnum>(values);
                colors = colors.OrderBy(x => random.Next()).ToList();

                int colorIndex = 0;
                for (int i = 0; i < vehicles.Length; i++)
                {
                    if (colorIndex >= colors.Count)
                        colorIndex = 0;
                    
                    vehicles[i].ChangeColor(colors[colorIndex]);
                    colorIndex++;
                }
            }
        }
        
    }
}