using System;
using System.Collections;
using System.Collections.Generic;
using _CasualBusJam.Scripts._Data;
using _CasualBusJam.Scripts._Enum;
using DG.Tweening;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("------- Player Movement --------")]
    public static readonly int Walk = Animator.StringToHash("Walk");
    private static readonly int Sit = Animator.StringToHash("Sit");
    
    [Header("------- Player Elements -------")]
    public MaterialHolder materialHolder;
    public MeshRenderer playerMesh;
    public Animator playerAnimator;
    public GameObject animGo;
    public ColorEnum color;

    private void Awake()
    {
        playerAnimator = animGo.GetComponent<Animator>();
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
    
    
}
