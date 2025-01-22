using System;
using System.Collections.Generic;
using UnityEngine;

namespace _CasualBusJam.Scripts._Player
{
    public class PlayerManager : MonoBehaviour
    {
        public List<PlayerController> playersInScene;
        public List<PlayerController> totalPlayerList;
        public List<PlayerController> activePlayerList;
        
        public GameObject playerPrefab;
        public Transform spawnPoint;
        public Transform pickPoint;
        public Vector3 midPoint;
        
        public static PlayerManager Instance { get; private set; }
        private void Awake()
        {
            Instance = this;
        }
        
        
    }
}