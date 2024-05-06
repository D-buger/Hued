using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class MovingCrystal : MonoBehaviour
{
    [SerializeField]
    private Sprite crystalSprite;
    [SerializeField]
    private float moveSpeed = 0.7f;
    [SerializeField]
    private eTwoDirection moveDirection;
    [SerializeField]
    private float eachCrystalDistance = 5;
    [SerializeField]
    private float maxMoveDistance = 5;

    private void Awake()
    {
        GameObject anotherCrystal = new GameObject("AnotherCrystal");
        anotherCrystal.transform.parent = transform;


    }

    private void Start()
    {
        
    }
}
