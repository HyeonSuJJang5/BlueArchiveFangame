using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServingManager : MonoBehaviour
{
    [SerializeField]
    private ServingController[] servers; // 서빙 담당자 배열

    private Queue<(int, string, GameObject, GuestController, int)> servingOrders = new Queue<(int, string, GameObject, GuestController, int)>(); // 서빙 주문 큐

    private void Start()
    {
        // 모든 서빙 담당자에게 참조를 전달
        foreach (var server in servers)
        {
            server.SetManager(this);
        }
    }

    public void EnqueueServingOrder(int foodIndex, string foodName, GameObject servingTable, GuestController guest, int foodRespawnIndex)
    {
        servingOrders.Enqueue((foodIndex, foodName, servingTable, guest, foodRespawnIndex));
        Debug.Log("손님의 음식이 완성됐습니다: " + foodName);

        AssignOrderToAvailableServer();
    }

    public void AssignOrderToAvailableServer()
    {
        foreach (var server in servers)
        {
            if (!server.IsServing() && servingOrders.Count > 0)
            {
                var (foodIndex, foodName, servingTable, guest, foodRespawnIndex) = servingOrders.Dequeue();
                server.StartServing(foodIndex, foodName, servingTable, guest, foodRespawnIndex);
                Debug.Log("Order assigned to server: " + server.name);
            }

            else if (server.IsServing() && !server.HasPendingOrder() && servingOrders.Count > 0)
            {
                // 서빙 중인 서버에게 새로운 주문을 예약
                var (foodIndex, foodName, servingTable, guest, servingPositionIndex) = servingOrders.Dequeue();
                server.SetPendingOrder(foodIndex, foodName, servingTable, guest, servingPositionIndex);
            }
        }
    }

    public void OnServerFinishedServing()
    {
        AssignOrderToAvailableServer();
    }
}