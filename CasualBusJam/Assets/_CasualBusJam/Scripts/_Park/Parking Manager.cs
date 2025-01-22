using System;
using System.Collections;
using System.Collections.Generic;
using _CasualBusJam.Scripts._Vehicle;
using UnityEngine;
using UnityEngine.Serialization;

public class ParkingManager : MonoBehaviour
{
    [Header("------ Parking Elements -----")]
    public List<ParkingSlots> slots;
    public List<Vehicle> parkedVehicles;
    public ParkingSlots parkingSlotsAdd; 
    public Transform exitPoint;
       
    public static ParkingManager Instance;
    private void Awake()
    { 
        Instance = this;
    }
   
    public ParkingSlots CheckForFreeSlot()
    {
        foreach (var slot in slots)
        {
            if (!slot.isOccupied)
                return slot;
        }
   
        return null;
    } 
}
