using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _CasualBusJam.Scripts._Events;
using _CasualBusJam.Scripts._Vehicle;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace _CasualBusJam.Scripts._Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("----- PLAYER ELEMENTS ------")]
        [SerializeField] private GameObject passengerPrefab;
        public List<Player> playersInScene = new();
        public List<Player> totalPlayerList = new();
        public List<Player> activePlayerList = new();
        public bool isColorMatched = false;
        
        [Header("----- PLAYER COLOR SHUFFLE -----")]
        public bool canShuffle;
        [Range(0, 1)] public float shuffleIntensity = 0.5f;
         
        
        [Header("----- PLAYER MOVEMENT POINTS -----")]
        [SerializeField] private Transform passengerSpawnPoint;
        [SerializeField] private Transform passengerPickPoint;
        public Vector3 passengerMidPoint;
        public List<Vector3> pointsBetweenMidAndPick;
        public List<Vector3> pointsBetweenMidAndSpawn;
        public List<Vector3> allPoints;
        
        public static PlayerController Instance { get; private set; }
        private void Awake()
        {
            Instance = this;
            GeneratePoints();
        }

        private void OnEnable()
        {
            EventManager.OnNewVehArrived += AnyCarColorMatched;
        }

        private void OnDisable()
        {
            EventManager.OnNewVehArrived -= AnyCarColorMatched;
        }

        public void InstantiatePlayer(Vehicle[] vehicles)
        {
            foreach (var vehicle in vehicles)
            {
                for (int i = 0; i < vehicle.SeatCount; i++)
                {
                    GameObject obj = Instantiate(passengerPrefab, passengerSpawnPoint);
                    Player passenger = obj.GetComponent<Player>();
                    passenger.ChangeColor(vehicle.vehicleColor);
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

            if (canShuffle)
                playersInScene = ShufflePlayerListBasedOnColor(playersInScene, shuffleIntensity);
            
            totalPlayerList = new List<Player>(playersInScene);
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
                Player currentPassenger = activePlayerList[i];
                currentPassenger.transform.gameObject.SetActive(true);
                currentPassenger.playerAnimator.SetBool(Player.Walk, true);
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

        private List<Player> ShufflePlayerListBasedOnColor(List<Player> list, float intensity)
        {
            var colorGroups = list.GroupBy(player => player.color).ToList();
            
            var firstHalf = new List<Player>();
            var secondHalf = new List<Player>();

            foreach (var group in colorGroups.Take(4))
                firstHalf.AddRange(group);

            foreach (var group in colorGroups.Skip(4))
                secondHalf.AddRange(group);
            
            firstHalf = ShuffleWithIntensity(firstHalf, intensity);
            secondHalf = ShuffleWithIntensity(secondHalf, intensity);
            
            return firstHalf.Concat(secondHalf).ToList();
        }


        private List<Player> ShuffleWithIntensity(List<Player> list, float intensity)
        {
            int n = list.Count;

            for (int i = 0; i < n - 1; i++)
            {
                int j = Mathf.Min(i + Mathf.FloorToInt(Random.Range(0f, 1f) * intensity * (n - 1)), n - 1);
                (list[i], list[j]) = (list[j], list[i]);
            }

            return list;
        }

        private void AnyCarColorMatched()
        {
            var cars = ParkingManager.Instance.parkedVehicles;
            if (cars.Count <= 0) return;

            foreach (var car in cars)
            {
                if (activePlayerList.Count > 0 && activePlayerList[0].color == car.vehicleColor && !car.isFull)
                {
                    isColorMatched = true;
                    StartCoroutine(activePlayerList[0].MoveToTruck(car));
                    return;
                }   
            }
            
            isColorMatched = false;
            // Checkifgameover
        }

        public void RepositionPlayer()
        {
            activePlayerList.RemoveAt(0);
            if (totalPlayerList.Count > 0)
            {
                activePlayerList.Add(totalPlayerList[0]);
                totalPlayerList[0].gameObject.SetActive(true);
                totalPlayerList.RemoveAt(0);
            }

            for (int i = 0; i < activePlayerList.Count; i++)
            {
                Player currentPlayer = activePlayerList[i];
                currentPlayer.playerAnimator.SetBool(Player.Walk, true);
                Vector3 startPosition = currentPlayer.transform.position;
                Vector3 endPosition = allPoints[i];
                
                currentPlayer.transform.DOMove(endPosition, .1f)
                    .OnComplete(() => currentPlayer.playerAnimator.SetBool(Player.Walk, false));
                
                Vector3 direction = (endPosition - startPosition).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                currentPlayer.transform.DORotate(targetRotation.eulerAngles, .2f);
            }
            
            AnyCarColorMatched();
        }
        
    }
}