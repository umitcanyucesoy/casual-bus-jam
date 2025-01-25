using System;
using System.Collections;
using System.Collections.Generic;
using _CasualBusJam.Scripts._Data;
using _CasualBusJam.Scripts._Enum;
using _CasualBusJam.Scripts._Player;
using _CasualBusJam.Scripts._Vehicle;
using DG.Tweening;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("------- Player Movement --------")]
    public static readonly int Walk = Animator.StringToHash("Walk");
    private static readonly int Sit = Animator.StringToHash("Sit");
    
    [Header("------- Player Elements -------")]
    public MaterialHolder materialHolder;
    public Renderer playerMesh;
    public Animator playerAnimator;
    public GameObject animGo;
    public ColorEnum color;

    private void Awake()
    {
        playerAnimator = animGo.GetComponent<Animator>();
        materialHolder.InitializeMaterialDictionary();
    }

    public void ChangeColor(ColorEnum colorEnum)
    {
        Material material = materialHolder.FindMaterialByName(colorEnum);
        playerMesh.material = material;
        gameObject.name += colorEnum.ToString();
        color = colorEnum;
    }

    public IEnumerator MoveToSlot1(Vector3 midPoint, Transform pickPoint, Vector3 finishPoint, float delay)
    {
        yield return new WaitForSeconds(delay);
        transform.DOMove(midPoint, 0.3f).OnComplete(() =>
        {
            transform.rotation = pickPoint.rotation;
            transform.DOMove(finishPoint, 0.3f).OnComplete(() =>
            {
                playerAnimator.SetBool("Walk",false);
            });
        });
    }

    public IEnumerator MoveToSlot2(Vector3 finishPoint, float delay)
    {
        yield return new WaitForSeconds(delay);
        transform.DOMove(finishPoint, 0.3f).OnComplete(() =>
            {
                playerAnimator.SetBool("Walk",false);
            });
    }

    public IEnumerator MoveToTruck(Vehicle vehicle)
    {
        PlayerController.Instance.playersInScene.Remove(this);
        var seat = vehicle.GetFreeSeat();
        transform.parent = seat.transform;
        playerAnimator.SetBool(Walk,true);

        Vector3[] path = new Vector3[]
        {
            transform.position,
            transform.position - new Vector3(0,0,1),
            vehicle.transform.position,
            seat.transform.position
        };

        transform.DOPath(path, .8f, PathType.CatmullRom).OnComplete(() =>
        {
            playerAnimator.SetBool(Walk,true);
            playerAnimator.SetBool(Sit,true);
            transform.localRotation = Quaternion.identity;
            transform.localPosition += new Vector3(0, -.34f, .2f);
            transform.localScale = new Vector3(0.8f,0.8f,0.8f);
        });
        
        yield return new WaitForSeconds(.1f);
        //Updateplayercount
        PlayerController.Instance.RepositionPlayer();
        //sound
    }
    
    
}
