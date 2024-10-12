using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuestManager : MonoBehaviour
{
    public static GuestManager Instance { get; private set; }

    public event Action<int, GuestController> OnOrderFood;

    private ChefManager chefManager;
    private ServingManager servingManager;

    private void Awake()
    {
        chefManager = FindObjectOfType<ChefManager>();
        servingManager = FindObjectOfType<ServingManager>();

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OrderFood(int foodIndex, GuestController guest)
    {
        Debug.Log("GuestManager에서 주문한 음식 인덱스 확인 : " + foodIndex);
        OnOrderFood?.Invoke(foodIndex, guest);
        chefManager.EnqueueOrder(foodIndex, guest);
    }
}
