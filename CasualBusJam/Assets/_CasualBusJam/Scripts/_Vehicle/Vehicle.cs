using System;
using System.Collections.Generic;
using System.Linq;
using _CasualBusJam.Scripts._Enum;
using _CasualBusJam.Scripts._Events;
using _CasualBusJam.Scripts._Player;
using _CasualBusJam.Scripts.SFX;
using _CasualBusJam.Scripts.VFX;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _CasualBusJam.Scripts._Vehicle
{
    public class Vehicle : MonoBehaviour
    {
        [Header("------- Vehicle Elements -------")]
        public ColorEnum vehicleColor;
        public List<Transform> seats;
        public List<MeshRenderer> vehMesh;
        public List<GameObject> removableParts;


        [Header("----- Vehicle Settings ------")]
        public Tween movingZdir;
        public bool isFull = false;
        public Vector3 originalPosition;
        public Vector3 originalScale;
        public Vector3 newScale;
        public int playersInSeat = 0;
        public static bool IsMovingStraight = false;
        public static ColorEnum LastTouchedCarColor;
        public bool isMovingForward = false;
        public int SeatCount => seats.Count;
        
        private float _distance = 30f;
        private bool _isCollided = false;
        private ParkingSlots _slot;
        private bool _canCollideWithOtherVehicle = true;
        private bool _toggle;
        private static int _counter = 0;
        
        private void Start()
        {
            SetInitialPosition();
            originalScale = transform.localScale;
            IsMovingStraight = false;
            Vector3 currentRotation = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(0, currentRotation.y, 0);
        }

        private void OnValidate()
        {
            Vector3 currentRotation = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(0, currentRotation.y, 0);
        }

        private void OnMouseDown()
        {
            if (IsMovingStraight || EventSystem.current.IsPointerOverGameObject())
                return;
            
            LastTouchedCarColor = vehicleColor;
            
            if (CheckForVehicleInFront(out RaycastHit hitInfo))
            {
                IsMovingStraight = true;
                isMovingForward = true;
                
                Vector3 targetPosition =
                    transform.position + transform.forward * (hitInfo.distance + 1);
                movingZdir = transform.DOMove(targetPosition, 0.2f).SetEase(Ease.InQuad);
                
                return;
            }

            _slot = ParkingManager.Instance.CheckForFreeSlot();
            if (!_slot)
            {
                Debug.Log("No Free Slot");
                return;
            }

            MoveCarStraight();
        }

        private void SetInitialPosition()
        {
            originalPosition = transform.position;
        }

        private bool CheckForVehicleInFront(out RaycastHit hitInfo)
        {
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            float rayDistance = Mathf.Infinity;

            if (Physics.Raycast(transform.position, forward, out hitInfo, rayDistance))
            {
                if (hitInfo.collider.TryGetComponent(out Vehicle vehicle) && vehicle._canCollideWithOtherVehicle && !vehicle.isMovingForward)
                {
                    Debug.Log("Vehicle is in front");
                    return true;
                }
            }

            return false;
        }

        private void MoveCarStraight()
        {
            SoundController.Instance.PlayOneShot(SoundController.Instance.tapSound, .5f);
            _slot.isOccupied = true;
            IsMovingStraight = true;
            isMovingForward = true;
            
            Vector3 localPosition = transform.localPosition;
            Vector3 localForwardDirection = transform.localRotation * Vector3.forward;
            Vector3 pointAtDistance = localPosition + localForwardDirection * _distance;
            Vector3 worldPoint = transform.parent.TransformPoint(pointAtDistance);
            
            Debug.DrawLine(transform.position, worldPoint, Color.green);
            movingZdir = transform.DOMove(worldPoint, 12f).SetSpeedBased();
            GetComponent<AudioSource>().enabled = true;
        }

        private void ChangeScale(bool shift)
        {
            newScale = new Vector3(1f, 1f, 1f);
            if (shift)
                transform.localScale = newScale;
            else
                transform.localScale = originalScale;
        }
        
        public void ChangeColor(ColorEnum colorEnum)
        {
            vehicleColor = colorEnum;
            Material material = VehicleController.Instance.vehicleMaterialHolder.FindMaterialByName(colorEnum);
            if (material)
            {
                for (int i = 0; i < vehMesh.Count; i++)
                {
                    vehMesh[i].material = material;
                }
            }
        }

        private void ShakeVehicle() => transform.DOShakeRotation(0.2f, transform.forward * 2, vibrato: 10, randomness: 90).SetEase(Ease.InBounce);

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Down"))
            {
                _canCollideWithOtherVehicle = false;
                movingZdir.Pause();
                Debug.Log("Vehicle entered!");
                _toggle = !_toggle;
                MoveToSideBorder(VehicleController.Instance.leftCollider,-20f);
                
                return;
            }

            if (_canCollideWithOtherVehicle && other.TryGetComponent(out Vehicle vehicle) &&
                vehicle._canCollideWithOtherVehicle)
            {
                if (_slot && _canCollideWithOtherVehicle)
                    _slot.isOccupied = false;
                
                movingZdir.Pause();
                if (!_isCollided)
                {
                    if (IsMovingStraight && _counter == 0 && isMovingForward)
                    {
                        _counter++;
                        GetComponent<AudioSource>().enabled = false;
                        SoundController.Instance.PlayOneShot(SoundController.Instance.hitSound);
                        EffectsManager.Instance.PlayEffect(EffectsManager.Instance.hitEffect,
                            other.ClosestPoint(transform.position + new Vector3(0, 0.25f, 0)), Quaternion.identity);
                    }
                    
                    vehicle.ShakeVehicle();
                    transform.DOMove(originalPosition, 0.3f).SetEase(Ease.OutBack).OnComplete(() =>
                    {
                        _counter = 0;
                        isMovingForward = false;
                        IsMovingStraight = false;
                    });
                }
            }

            if (other.gameObject.CompareTag("Border") && !_isCollided)
            {
                _isCollided = true;
                IsMovingStraight = false;
                _canCollideWithOtherVehicle = false;
                MoveToTargetFromBorder();
                // REMOVE VEHICLE
                movingZdir.Pause();
            }

            if (other.gameObject.CompareTag("Upborder") && !_isCollided)
            {
                _isCollided = true;
                IsMovingStraight = false;
                _canCollideWithOtherVehicle = false;
                MoveToTargetFromUpBorder();
                //
                movingZdir.Pause();
            }
        }

        private void MoveToSideBorder(Transform collider, float distance)
        {
            IsMovingStraight = false;
            _canCollideWithOtherVehicle = false;
            
            Transform cube = collider.transform;
            Vector3 cubePos = cube.position;

            Vector3 directionToCube = new Vector3(cubePos.x - transform.position.x, 0, 0);
            Quaternion targetRotation = Quaternion.LookRotation(directionToCube, Vector3.up);
            
            transform.DORotateQuaternion(targetRotation, 0.1f);
            transform.DOLocalMoveX(distance, 0.8f);
            //VehicleController.Instance.RemoveVehicle(this);
        }

        private void MoveToTargetFromBorder()
        {
            Transform road = VehicleController.Instance.road;
            Vector3 roadPos = road.position;

            Vector3[] path = new Vector3[]
            {
                transform.position,
                new Vector3(transform.position.x, transform.position.y, roadPos.z),
            };
            
            transform.DORotate(Vector3.zero, 0.1f);
            transform.DOPath(path, 0.3f, PathType.Linear).SetEase(Ease.Linear).OnComplete(() =>
            {
                transform.DOLookAt(roadPos, 0.2f);
                MoveToSlot();
                foreach (var parts in removableParts)
                {
                    parts.SetActive(false);
                }
            });
        }

        private void MoveToTargetFromUpBorder()
        {
            transform.DOLookAt(_slot.transform.position, 0.2f);
            MoveToSlot();
            foreach (var parts in removableParts)
            {
                parts.SetActive(false);
            }
        }

        private void MoveToSlot()
        {
            Vector3[] waypoints = new Vector3[]
            {
                new(_slot.enterPoint.position.x, transform.position.y, _slot.enterPoint.position.z),
                new(_slot.stopPoint.position.x, transform.position.y + .5f, _slot.stopPoint.position.z),
            };

            ChangeScale(true);

            transform.DOPath(waypoints, .5f, PathType.CatmullRom).OnWaypointChange(waypointindex =>
            {
                if (waypointindex == 1)
                {
                    transform.DORotateQuaternion(_slot.stopPoint.rotation, 0.2f);
                }
            }).OnComplete(() =>
            {
                IsMovingStraight = false;
                ParkingManager.Instance.parkedVehicles.Add(this);
                transform.parent = _slot.transform;
                GetComponent<BoxCollider>().enabled = false;
                Debug.Log("Moved to Slot");
                if (!PlayerController.Instance.isColorMatched)
                    EventManager.OnNewVehArrived?.Invoke();
                GetComponent<AudioSource>().enabled = false;
            });

        }

        public Transform GetFreeSeat()
        {
            for (int i = 0; i < seats.Count; i++)
            {
                if (seats[i].childCount == 0)
                {
                    playersInSeat++;
                    IsVehicleFull();
                    return seats[i];
                }
            }

            return null;
        }

        private void IsVehicleFull()
        {
            if (playersInSeat == seats.Count)
            {
                isFull = true;
                DOVirtual.DelayedCall(1f, () =>
                {
                    VehicleGoingToExit();
                    //GameManager.instance.CheckGameWin();
                });
            }
        }

        private void VehicleGoingToExit()
        {
            VehicleController.Instance.vehicles = VehicleController.Instance.vehicles
                .Where(v => v != this)
                .ToArray();
            
            transform.DORotateQuaternion(ParkingManager.Instance.exitPoint.rotation, 0.2f);
            transform.DOMove(
                new Vector3(_slot.enterPoint.transform.position.x, transform.position.y,
                    _slot.enterPoint.transform.position.z), 30f).SetSpeedBased().OnComplete(() =>
            {
                _slot.isOccupied = false;
                _canCollideWithOtherVehicle = false;
                ParkingManager.Instance.parkedVehicles.Remove(this);
                transform.parent = null;
                transform.DOMove(ParkingManager.Instance.exitPoint.transform.position, 35f).SetSpeedBased().SetEase(Ease.InBack)
                    .OnComplete(() => { transform.gameObject.SetActive(false); });
            });
            
            SoundController.Instance.PlayFullSound();
            SoundController.Instance.PlayOneShot(SoundController.Instance.moving);
        }
    }
}