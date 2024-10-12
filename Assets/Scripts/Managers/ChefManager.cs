using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChefManager : MonoBehaviour
{
    [SerializeField]
    private ChefController[] chefs; // 요리사 배열

    private Queue<(int, GuestController)> foodOrders = new Queue<(int, GuestController)>(); // 주문 큐

    private const int MaxOrders = 50; // 최대 주문 수 제한

    private void Start()
    {
        // 모든 요리사에게 참조를 전달
        foreach (var chef in chefs)
        {
            chef.SetManager(this);
        }

        // 처음 시작할 때 활성화된 쉐프가 몇 명인지 확인
        AssignOrderToAvailableChef();
    }

    // 모든 요리사를 반환하는 함수
    public ChefController[] GetChefs()
    {
        return chefs;
    }

    // 주문을 큐에 추가하고 가능한 쉐프에게 할당 시도
    public void EnqueueOrder(int foodIndex, GuestController guest)
    {
        if (foodOrders.Count >= MaxOrders)
        {
            Debug.Log("주문이 가득 찼습니다. 더 이상 주문을 받을 수 없습니다.");
            return; // 주문이 가득 찼을 때는 추가하지 않음
        }

        foodOrders.Enqueue((foodIndex, guest));
        AssignOrderToAvailableChef();
    }

    // 쉐프가 쉬고 있으면 주문을 바로 할당하고, 전부 바쁘면 예약 주문
    public void AssignOrderToAvailableChef()
    {
        // 일하지 않는 쉐프가 있으면 바로 주문을 할당
        foreach (var chef in chefs)
        {
            if (chef.gameObject.activeSelf && !chef.IsCooking() && foodOrders.Count > 0)
            {
                var (order, guest) = foodOrders.Dequeue();
                chef.StartCooking(order, guest);
                return; // 한 명에게 주문을 할당했으면 종료
            }
        }

        // 모든 쉐프가 바쁠 때 예약된 주문이 있을 경우 여러 예약 주문 처리
        foreach (var chef in chefs)
        {
            if (chef.gameObject.activeSelf && chef.IsCooking() && foodOrders.Count > 0)
            {
                // 쉐프에게 예약 주문을 추가 (여러 개 가능)
                while (foodOrders.Count > 0)
                {
                    var (order, guest) = foodOrders.Dequeue();
                    chef.SetPendingOrder(order, guest); // 쉐프의 예약 주문 큐에 추가
                }
                return; // 한 명에게 예약 주문을 할당했으면 종료
            }
        }
    }

    // 쉐프가 비활성화인지 확인하고 활성화된 쉐프 수 반환
    public int GetActiveChefCount()
    {
        int activeChefs = 0;
        foreach (var chef in chefs)
        {
            if (chef.gameObject.activeSelf)
            {
                activeChefs++;
            }
        }
        return activeChefs;
    }

    // 새로 추가된 쉐프를 활성화하고 주문 할당
    public void AddChef(ChefController newChef)
    {
        // 새 쉐프에게 매니저 전달
        newChef.SetManager(this);
        // 새 쉐프가 활성화되면 바로 주문 할당 시도
        AssignOrderToAvailableChef();
    }
}