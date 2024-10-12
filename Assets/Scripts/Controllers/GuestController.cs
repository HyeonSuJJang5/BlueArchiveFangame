using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Security.Cryptography;

public class GuestController : MonoBehaviour
{
    public enum Order
    {
        Steak = 0, Ramen = 1, FriedRice = 2
    }

    private bool isSitting = false; // 의자에 앉았는지 여부
    private bool hasOrdered = false; // 주문이 발생했는지 여부

    private AStarManager aStarManager; // A* 관리자
    private TargetManager targetManager; // Target 관리자
    private ChairManager chairManager; // Chair 관리자

    [SerializeField]
    private float moveSpeed = 2f; // 이동 속도 설정

    private Order currentOrder; // 현재 손님의 주문


    private List<Vector3> path; // A* 경로
    private int currentTargetIndex; // 현재 경로 인덱스
    private int chairIndex = -1; // 앉을 의자 인덱스
    private bool isLeftChair = false; // 왼쪽 의자에 앉는지 여부
    private bool isRightChair = false; // 왼쪽 의자에 앉는지 여부

    [SerializeField] private GameObject foodUIPrefab;
    private GameObject currentFoodUI;
    private RectTransform foodUIRect;
    public float height = 2f; // UI가 손님 머리 위에 나타날 높이 조정
    public float coolTime_height = 2f; // UI가 손님 머리 위에 나타날 높이 조정

    [SerializeField] private GameObject cooldownUIPrefab; // Cooldown UI prefab
    private GameObject currentCooldownUI; // Current cooldown UI instance
    private RectTransform cooldownUIRect; // UI RectTransform for positioning
    private Image cooldownUIImage; // Image component for filling cooldown UI


    private Animator animator; // Animator 컴포넌트
    private bool isMoving = true; // 이동 중인지 확인
    private Vector3 lastPosition; // 이전 프레임의 위치 저장

    // 음식 섭취 시간을 저장할 변수 추가
    private float foodEatDuration;

    private bool isOrder = false;

    private bool isTrinity = false; // 루미 캐릭터 여부를 저장하는 변수

    private bool isMillennium = false; // Millennium 캐릭터 여부를 저장하는 변수



    private void Start()
    {
        aStarManager = FindObjectOfType<AStarManager>();
        targetManager = FindObjectOfType<TargetManager>();
        chairManager = FindObjectOfType<ChairManager>();
        animator = GetComponentInChildren<Animator>(); // 자식 오브젝트에서 Animator 컴포넌트 가져오기

        StartCoroutine(FindAndMoveToAvailableChair());
        lastPosition = transform.position;


        isMoving = true;

        if (gameObject.CompareTag("Trinity"))
        {
            isTrinity = true;
        }

        // Millennium 태그 확인
        if (gameObject.CompareTag("Millennium"))
        {
            isMillennium = true; // Millennium 캐릭터로 설정
        }

    }

    private void Update()
    {

        Vector3 currentPosition = transform.position;
        Vector2 movementDirection = (currentPosition - lastPosition).normalized;

        // 의자에 앉지 않았으면 의자로 이동
        if (!isSitting)
        {

            MoveAlongPath();
        }

        // 이동 여부 판단
        if (Vector3.Distance(currentPosition, lastPosition) > 0.01f)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }

        if (isMoving) // 이동 중일 때 애니메이션을 재생
        {
            animator.SetBool("isMoving", true);

            // 이동 방향에 따른 애니메이션 설정
            if (Mathf.Abs(movementDirection.x) > Mathf.Abs(movementDirection.y))
            {
                if (movementDirection.x > 0)
                {
                    // 오른쪽 이동
                    if (isTrinity)
                        animator.Play("Trinity Default Right Move");
                    else if (isMillennium)
                        animator.Play("Millennium Default Right Move"); // Millennium 애니메이션

                }
                else
                {
                    // 왼쪽 이동
                    if (isTrinity)
                        animator.Play("Trinity Default Left Move");
                    else if (isMillennium)
                        animator.Play("Millennium Default Left Move"); // Millennium 애니메이션

                }
            }
            else
            {
                if (movementDirection.y > 0)
                {
                    // 위로 이동
                    if (isTrinity)
                        animator.Play("Trinity Defualt Up Move");
                    else if (isMillennium)
                        animator.Play("Millennium Defualt Up Move"); // Millennium 애니메이션
                }
                else
                {
                    // 아래로 이동
                    if (isTrinity)
                        animator.Play("Trinity Default Down Move");
                    else if (isMillennium)
                        animator.Play("Millennium Default Down Move"); // Millennium 애니메이션

                }
            }
            lastPosition = currentPosition;
        }

        else
        {
            // 정지 상태일 때
            animator.SetBool("isMoving", false);

            if (isTrinity)
            {
                animator.Play("Trinity Idle");
            }

            else if (isMillennium)
            {
                animator.Play("Millennium Idle"); // Millennium Idle 애니메이션
            }

        }

        // Update lastPosition at the end of the frame
        lastPosition = currentPosition;

        // 음식 섭취 시, 의자 상태에 따른 애니메이션 처리
        if (isLeftChair && isOrder)
        {
            animator.SetBool("isLeftChair", false);
            animator.SetBool("isLeftEat", true);
            animator.Play(isMillennium ? "Millennium Left Eat" : "Trinity Left Eat");

        }
        else if (isRightChair && isOrder)
        {
            animator.SetBool("isRightChair", false);
            animator.SetBool("isRightEat", true);
            animator.Play(isMillennium ? "Millennium Right Eat" : "Trinity Right Eat");
        }
    }

    // 남아 있는 의자가 생길 때까지 기다렸다가, 의자가 생기면 해당 의자로 이동
    private IEnumerator FindAndMoveToAvailableChair()
    {
        while (true)
        {
            // 왼쪽 의자 중 빈 의자 확인
            List<int> availableLeftChairs = GetAvailableChairs(chairManager.leftChairOccupied);

            // 오른쪽 의자 중 빈 의자 확인
            List<int> availableRightChairs = GetAvailableChairs(chairManager.rightChairOccupied);

            // 만약 왼쪽과 오른쪽 의자에 각각 하나의 빈 의자만 남았다면
            if (availableLeftChairs.Count == 1 && availableRightChairs.Count == 1)
            {
                // 우선 왼쪽 의자로 이동
                chairManager.OccupyLeftChair(availableLeftChairs[0]);
                chairIndex = availableLeftChairs[0];
                isLeftChair = true;
                MoveToChair(targetManager.guestSeatingTargets[chairIndex * 2]); // 왼쪽 의자 타겟으로 이동
                break;
            }
            else if (availableLeftChairs.Count > 0) // 왼쪽 의자에 여러 개 빈 의자가 있으면 랜덤 선택
            {
                int randomChair = availableLeftChairs[UnityEngine.Random.Range(0, availableLeftChairs.Count)];
                chairManager.OccupyLeftChair(randomChair);
                chairIndex = randomChair;
                isLeftChair = true;
                MoveToChair(targetManager.guestSeatingTargets[randomChair * 2]); // 왼쪽 의자 타겟으로 이동
                break;
            }

            // 오른쪽 의자에 하나의 빈 의자만 있으면 바로 이동
            if (availableRightChairs.Count == 1)
            {
                chairManager.OccupyRightChair(availableRightChairs[0]);
                chairIndex = availableRightChairs[0];
                isRightChair = true;
                MoveToChair(targetManager.guestSeatingTargets[chairIndex * 2 + 1]); // 오른쪽 의자 타겟으로 이동
                break;
            }
            else if (availableRightChairs.Count > 0) // 오른쪽 의자에 여러 개 빈 의자가 있으면 랜덤 선택
            {
                int randomChair = availableRightChairs[UnityEngine.Random.Range(0, availableRightChairs.Count)];
                chairManager.OccupyRightChair(randomChair);
                chairIndex = randomChair;
                isRightChair = true;
                MoveToChair(targetManager.guestSeatingTargets[randomChair * 2 + 1]); // 오른쪽 의자 타겟으로 이동
                break;
            }

            // 빈 의자가 없을 경우, 0.5초마다 다시 확인
            yield return new WaitForSeconds(0.5f);
        }
    }

   

    // 이동 속도 증가율 적용 함수
    public void UpdateMoveSpeed(float increasePercentage)
    {
        moveSpeed = 2f * (1f + increasePercentage); // 기본 속도에 증가율 적용
    }

    // 빈 의자 리스트 반환
    private List<int> GetAvailableChairs(bool[] chairs)
    {
        List<int> availableChairs = new List<int>();
        for (int i = 0; i < chairs.Length; i++)
        {
            if (!chairs[i])
            {
                availableChairs.Add(i);
            }
        }
        return availableChairs;
    }


    // 랜덤한 의자 선택하기 
    private void MoveToChair(Transform targetChair)
    {
        // 현재 위치를 aStarManager의 시작 위치로 설정합니다.
        aStarManager.startPos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));

        // 선택한 의자의 위치를 aStarManager의 목표 위치로 설정합니다.
        aStarManager.SetTargetPos(targetChair.position);

        // A* 알고리즘을 사용하여 경로를 계산하고, 계산된 경로를 path 변수에 저장합니다.
        path = aStarManager.PathFinding();

        // 경로의 현재 타겟 인덱스를 0으로 초기화합니다.
        currentTargetIndex = 0;

    }

    // A* 알고리즘을 이용해서, 선택한 의자로 이동하기 
    private void MoveAlongPath()
    {
        if (path != null && currentTargetIndex < path.Count)
        {
            transform.position = Vector3.MoveTowards(transform.position, path[currentTargetIndex], Time.deltaTime * moveSpeed);
            if (Vector3.Distance(transform.position, path[currentTargetIndex]) < 0.1f)
            {
                currentTargetIndex++;
                if (currentTargetIndex >= path.Count)
                {
                    SitOnChair(); // 의자에 앉기
                    path = null;
                }
            }
        }
    }

    // 의자에 잘 앉았는지 기록 저장
    private void SitOnChair()
    {
        Debug.Log(name + "이(가) 의자에 앉습니다.");
        isSitting = true;
        isMoving = false;  // 의자에 앉으면 이동 상태를 false로 설정

        // 의자에 앉았다고 설정
        if (isLeftChair && !isOrder)
        {

            chairManager.OccupyLeftChair(chairIndex);
            animator.SetBool("isLeftChair", true);
            animator.SetBool("isRightChair", false);

            if (isTrinity)
            {
                animator.Play("Trinity Left Sit");
            }

            else if (isMillennium)
            {
                animator.Play("Millennium Left Sit");
            }

        }
        else if (isRightChair && !isOrder)
        {
            
            chairManager.OccupyRightChair(chairIndex);
            animator.SetBool("isRightChair", true);
            animator.SetBool("isLeftChair", false);
            if (isTrinity)
            {
                animator.Play("Trinity Right Sit");
            }

            else if (isMillennium)
            {
                animator.Play("Millennium Right Sit");
            }
        }

        if (!hasOrdered)
        {
            OrderFood();
        }
    }

    // 음식 주문
    public void OrderFood()
    {
        Debug.Log(name + "이(가) 음식을 주문합니다.");

        // GameManager의 잠금 해제 상태를 확인
        bool isRamenUnlocked = GameManager.Instance.ramenOrderUnlook;
        bool isSteakUnlocked = GameManager.Instance.steakOrderUnlook;

        // 모든 음식이 잠금 해제되었는지 확인
        if (isRamenUnlocked && isSteakUnlocked)
        {
            Debug.Log("모든 음식이 잠금 해제되었습니다.");
        }

        // 잠금 상태에 따라 선택할 수 있는 음식을 필터링
        List<Order> availableOrders = new List<Order> { Order.FriedRice }; // 기본적으로 볶음밥은 항상 주문 가능

        if (isRamenUnlocked)
        {
            availableOrders.Add(Order.Ramen);
        }

        if (isSteakUnlocked)
        {
            availableOrders.Add(Order.Steak);
        }

        // 모든 음식이 해금되었으면 더 이상 검사하지 않고 랜덤 주문 가능
        if (availableOrders.Count == System.Enum.GetValues(typeof(Order)).Length)
        {
            availableOrders = new List<Order> { Order.Steak, Order.Ramen, Order.FriedRice };
        }

        // 랜덤으로 주문할 음식 선택
        int random = GetSecureRandomNumber(0, availableOrders.Count);
        currentOrder = availableOrders[random];

        Debug.Log("주문한 음식: " + currentOrder);

        // 선택한 음식 UI 생성
        CreateFoodUI(currentOrder);

        // 버튼을 눌렀을 때 주문을 완료하는 함수를 호출
        AssignButtonClickAction((int)currentOrder);
    }
    // 선택한 음식 UI를 생성하고 손님 위에 배치하는 함수
    // 선택한 음식 UI를 생성하고 손님 위에 배치하는 함수
    private void CreateFoodUI(Order order)
    {
        // 음식 UI 프리팹을 생성하고, 캔버스의 첫 번째 자식으로 설정
        currentFoodUI = Instantiate(foodUIPrefab);
        currentFoodUI.transform.SetParent(FindObjectOfType<Canvas>().transform, false);
        currentFoodUI.transform.SetAsFirstSibling(); // 캔버스의 첫 번째 자식으로 설정

        // RectTransform 컴포넌트 캐시
        foodUIRect = currentFoodUI.GetComponent<RectTransform>();

        // 크기 조정 (원래 크기의 0.7배로 조정)
        foodUIRect.localScale = new Vector3(0.2f, 0.2f, 0.2f);

        // 손님 위치를 기준으로 UI 위치를 설정
        Vector3 uiPos = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0, height, 0));
        foodUIRect.position = uiPos;

        // Speech Buble Button의 Image 컴포넌트 변경
        if (currentFoodUI != null)
        {
            Image foodUIImage = currentFoodUI.GetComponent<Image>();
            string spritePath = "";
            switch (order)
            {
                case Order.Steak:
                    spritePath = "UI/Speech_Bubles/Steak Speech Buble";
                    break;
                case Order.Ramen:
                    spritePath = "UI/Speech_Bubles/Ramen Speech Buble";
                    break;
                case Order.FriedRice:
                    spritePath = "UI/Speech_Bubles/FriedRice Speech Buble";
                    break;
            }

            Sprite speechBubbleSprite = Resources.Load<Sprite>(spritePath); // 스프라이트 로드
            if (speechBubbleSprite != null)
            {
                foodUIImage.sprite = speechBubbleSprite; // 스프라이트 변경
            }
            else
            {
                Debug.LogWarning("해당 스프라이트를 찾을 수 없습니다: " + spritePath);
            }

            // currentFoodUI에 Image가 제대로 반영되었는지 확인
            Debug.Log("currentFoodUI와 변경한 Image가 동일하도록 설정되었습니다.");
        }
        else
        {
            Debug.LogWarning("Speech Buble Button 오브젝트를 찾을 수 없습니다.");
        }
    }
    // 버튼 클릭 시 주문 이벤트를 발생시키는 함수
    private void AssignButtonClickAction(int foodIndexToSave)
    {
        Button orderButton = currentFoodUI.GetComponentInChildren<Button>();
        orderButton.onClick.AddListener(() =>
        {
            ExecuteOrder(foodIndexToSave);
        });
    }

    // 주문을 실행하는 함수
    private void ExecuteOrder(int foodIndexToSave)
    {
        GuestManager.Instance.OrderFood(foodIndexToSave, this);

        hasOrdered = true;

        // UI 비활성화
        Destroy(currentFoodUI);
    }




    private int GetSecureRandomNumber(int minValue, int maxValue)
    {
        using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
        {
            byte[] randomNumber = new byte[4]; // 4바이트 배열 생성
            rng.GetBytes(randomNumber); // 배열에 랜덤 값을 채운다
            int value = BitConverter.ToInt32(randomNumber, 0) & int.MaxValue; // 부호를 제거하여 양수 값만 사용
            return (value % (maxValue - minValue)) + minValue;
        }
    }


    public IEnumerator EatFood()
    {
        isOrder = true;
        // 음식 섭취 시간을 랜덤하게 설정 (4초 ~ 7초)
        foodEatDuration = UnityEngine.Random.Range(4f, 7f);
        float elapsedTime = 0f; // 경과 시간

        // 쿨타임 UI 생성 및 설정
        CreateCooldownUI(foodEatDuration);

        // 음식 섭취 시간 동안 쿨타임 UI 업데이트
        while (elapsedTime < foodEatDuration)
        {
            // 경과 시간에 따라 쿨타임 UI의 fillAmount를 업데이트
            if (cooldownUIImage != null)
            {
                cooldownUIImage.fillAmount = elapsedTime / foodEatDuration;
            }

            // 경과 시간 업데이트
            elapsedTime += Time.deltaTime;
            yield return null;
        }


        // 음식 섭취 완료 후
        Debug.Log(name + "이(가) 음식을 먹습니다.");

        // 점수 추가
        int salePrice = GetSalePriceForFood();
        GameManager.Instance.AddScore(salePrice);

        // 쿨타임 UI 제거
        Destroy(currentCooldownUI);


        isSitting = false; // 더 이상 앉아있지 않음


        // 사운드 재생
        AudioManager.Instance.PlaySound(AudioManager.Instance.customerEatClip);

        // 음식을 먹고 가게를 나감
        LeaveRestaurant();
    }

    private void CreateCooldownUI(float cookingTime)
    {
        // 쿨타임 UI 생성 및 설정
        currentCooldownUI = Instantiate(cooldownUIPrefab);
        currentCooldownUI.transform.SetParent(FindObjectOfType<Canvas>().transform, false);
        currentCooldownUI.transform.SetAsFirstSibling(); // 캔버스의 첫 번째 자식으로 설정
        cooldownUIRect = currentCooldownUI.GetComponent<RectTransform>();
        cooldownUIRect.localScale = new Vector3(0.3f, 0.3f, 0.3f);


        // 쉐프 머리 위에 UI 배치
        Vector3 uiPos = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0, coolTime_height, 0));
        cooldownUIRect.position = uiPos;

        // Filled Image 가져오기
        cooldownUIImage = currentCooldownUI.GetComponent<Image>();

        // Fill Amount를 0으로 초기화 (0에서 1로 채우기 시작)
        cooldownUIImage.fillAmount = 0;

        // 코루틴으로 쿨타임 진행
        StartCoroutine(UpdateCooldownUI(cookingTime));
    }

    private IEnumerator UpdateCooldownUI(float cookingTime)
    {
        float elapsedTime = 0f;
        while (elapsedTime < cookingTime)
        {
            // fillAmount는 0에서 1로 증가
            cooldownUIImage.fillAmount = elapsedTime / cookingTime;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 쿨타임이 끝나면 fillAmount를 1로 설정하고 UI 제거
        cooldownUIImage.fillAmount = 1;
        Destroy(currentCooldownUI);
    }

    // 음식 가격을 지불하는 코드
    private int GetSalePriceForFood()
    {
        switch (currentOrder)
        {
            case Order.Steak:
                return GameManager.Instance.GetSteakSalePrice();
            case Order.Ramen:
                return GameManager.Instance.GetRamenSalePrice();
            case Order.FriedRice:
                return GameManager.Instance.GetFriedRiceSalePrice();
            default:
                return 0;
        }
    }

    // 가게를 나감
    private void LeaveRestaurant()
    {
        isSitting = false;
        isMoving = true;   // 다시 이동 상태로 변경

        // 의자 비우기
        if (isLeftChair == true)
        {
            isLeftChair = false;
            chairManager.ReleaseLeftChair(chairIndex);
            isSitting = false;
        }


        if (isRightChair == true)
        {
            isRightChair = false;
            chairManager.ReleaseRightChair(chairIndex);
            isSitting = false;
        }

        // 음식 섭취 후 애니메이션 초기화
        if (isLeftChair == false)
        {
            animator.SetBool("isLeftEat", false);
        }
        else if (isRightChair == false)
        {
            animator.SetBool("isRightEat", false);
        }


        Debug.Log(name + "이(가) 가게를 나갑니다.");

        aStarManager.startPos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        aStarManager.SetTargetPos(targetManager.guestExitTarget.position);

        path = aStarManager.PathFinding();
        currentTargetIndex = 0;


        if (path == null || path.Count == 0)
        {
            Debug.LogError("경로를 찾을 수 없습니다.");
            return;
        }

        StartCoroutine(MoveToEndZone());
    }

    // EndZone으로 이동
    private IEnumerator MoveToEndZone()
    {
        while (path != null && currentTargetIndex < path.Count)
        {
            transform.position = Vector3.MoveTowards(transform.position, path[currentTargetIndex], Time.deltaTime * moveSpeed);
            if (Vector3.Distance(transform.position, path[currentTargetIndex]) < 0.1f)
            {
                currentTargetIndex++;
            }
            yield return null;
        }

        Debug.Log(name + "이(가) EndZone에 도착하여 가게를 나갑니다.");
        Destroy(gameObject);
    }
}