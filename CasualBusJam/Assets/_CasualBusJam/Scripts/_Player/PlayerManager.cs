using System;
using System.Collections;
using System.Collections.Generic;
using _CasualBusJam.Scripts._Vehicle;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace _CasualBusJam.Scripts._Player
{
    public class PlayerManager : MonoBehaviour
    {
        [Header("----- PLAYER ELEMENTS ------")]
        [SerializeField] private GameObject passengerPrefab;
        public List<PlayerController> playersInScene = new();
        public List<PlayerController> totalPlayerList = new();
        public List<PlayerController> activePlayerList = new();
         
        
        [Header("----- PLAYER MOVEMENT POINTS -----")]
        [SerializeField] private Transform passengerSpawnPoint;
        [SerializeField] private Transform passengerPickPoint;
        public Vector3 passengerMidPoint;
        public List<Vector3> pointsBetweenMidAndPick;
        public List<Vector3> pointsBetweenMidAndSpawn;
        public List<Vector3> allPoints;
        
        public static PlayerManager Instance { get; private set; }
        private void Awake()
        {
            Instance = this;
            GeneratePoints();
        }

        public void InstantiatePlayer(Vehicle[] vehicles)
        {
            foreach (var vehicle in vehicles)
            {
                for (int i = 0; i < vehicle.SeatCount; i++)
                {
                    GameObject obj = Instantiate(passengerPrefab, passengerSpawnPoint);
                    PlayerController passenger = obj.GetComponent<PlayerController>();
                    //passenger.ChangeColor(vehicle.vehicleColor);
                    playersInScene.Add(passenger);
                }
            }

            StartCoroutine(PlayerMovement());
        }

        private void GeneratePoints()
        {
            passengerMidPoint = new Vector3(passengerSpawnPoint.position.x, 
                passengerPickPoint.position.y, passengerPickPoint.position.z);

            pointsBetweenMidAndPick = GeneratePointsBetween(passengerPickPoint.position, passengerMidPoint, 12);
            pointsBetweenMidAndSpawn = GeneratePointsBetween(passengerMidPoint, passengerSpawnPoint.position, 9);

            allPoints = new List<Vector3>();
            allPoints.Add(passengerPickPoint.position);
            allPoints.AddRange(pointsBetweenMidAndPick);
            allPoints.Add(passengerMidPoint);
            allPoints.AddRange(pointsBetweenMidAndSpawn);
            allPoints.Add(passengerSpawnPoint.position);
        }

        private List<Vector3> GeneratePointsBetween(Vector3 start, Vector3 end, int numberOfPoints)
        {
            List<Vector3> points = new List<Vector3>();
            for (int i = 1; i <= numberOfPoints; i++)
            {
                float t = i / (float)(numberOfPoints + 1);
                Vector3 point = Vector3.Lerp(start, end, t);
                points.Add(point);
            }
            
            return points;
        }

        private IEnumerator PlayerMovement()
        {
            yield return new WaitForSeconds(0f);

            totalPlayerList = new List<PlayerController>(playersInScene);
            foreach (var t in totalPlayerList)
                t.transform.gameObject.SetActive(false);

            for (int i = 0; i < 24; i++)
            {
                if (totalPlayerList.Count <= 0 || !totalPlayerList[0]) continue;
                
                activePlayerList.Add(totalPlayerList[0]);
                totalPlayerList.RemoveAt(0);
            }

            var points = allPoints;
            for (int i = 0; i < activePlayerList.Count; i++)
            {
                PlayerController currentPassenger = activePlayerList[i];
                currentPassenger.transform.gameObject.SetActive(true);
                currentPassenger.playerAnimator.SetBool(PlayerController.Walk, true);
                if (i < 14)
                {
                    StartCoroutine(currentPassenger.MoveToSlot1(passengerMidPoint, passengerPickPoint, points[i], i * .15f));
                }
                else
                {
                    StartCoroutine(currentPassenger.MoveToSlot2(points[i], i * .15f));
                }
            }
        }
    }
}