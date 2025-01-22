using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkingSlots : MonoBehaviour
{
    [Header("----- Parking Slots Elements ------")]
    public Transform enterPoint;
    public Transform stopPoint;
    public bool isOccupied;

    private void Start()
    {
        enterPoint = transform.GetChild(0).transform;
        stopPoint = transform.GetChild(1).transform;
    }
}
