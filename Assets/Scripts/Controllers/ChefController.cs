

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChefController : MonoBehaviour
{

    // 필드 추가
    private Queue<(int foodIndex, GuestController guest)> pendingOrders = new Queue<(int foodIndex, GuestController guest)>(); // 대기 중인 주문 큐

    public float moveSpeed = 3f;  // 이동 속도

    private ChefManager chefManager; // 요리사 매니저 참조

    [SerializeField]
    private bool isCooking; // 요리 중인지 여부

    private ServingTableManager servingTableManager; // ServingTableManager 스크립트
    private ServingController servingController; // ServingController 스크립트

    [SerializeField]
    private TargetManager targetManager; // TargetManager 스크립트
    private AStarManager aStarManager; // A* 알고리즘 매니저

    [SerializeField]
    private GameObject oneCookingPen; // 조리전 볶음밥 오브젝트
    [SerializeField]
    private GameObject twoCookingPen;// 조리전 볶음밥 오브젝트

    [SerializeField]
    private GameObject oneSteakObject; // 스테이크 오브젝트
    [SerializeField]
    private GameObject twoSteakObject; // 스테이크 오브젝트

    [SerializeField]
    private GameObject oneRamenObject; // 라면 오브젝트
    [SerializeField]
    private GameObject twoRamenObject; // 라면 오브젝트

    [SerializeField]
    private GameObject oneFriedRiceObject; // 볶음밥 오브젝트
    [SerializeField]
    private GameObject twoFriedRiceObject; // 볶음밥 오브젝트

    private Animator animator; // Animator 컴포넌트

    [SerializeField] private GameObject cooldownUIPrefab; // 쿨타임 UI 프리팹
    private GameObject currentCooldownUI; // 현재 생성된 쿨타임 UI
    private RectTransform cooldownUIRect; // UI RectTransform
    private Image cooldownUIImage; // UI 이미지
    private float height = 3f; // 쉐프 머리 위로 UI를 띄울 높이

    private GameManager gameManager;
    private float cookingTimeReduction = 0f; // 총 요리 시간 감소율 (0% ~ 50%)

    // 이동 방향을 나타내는 enum 추가
    private Vector3 lastPosition; // 이전 프레임의 위치 저장
    private bool isMoving = false; // 이동 중인지 확인
    private bool isCookingAnim = false; // 이동 중인지 확인

    private int currentFoodIndex; // 현재 요리 중인 음식의 인덱스

    private bool isRumi; // 루미 캐릭터 여부를 저장하는 변수

    private bool isUmika; // 우미카 캐릭터 여부를 저장하는 변수

    private bool isSiba; // 시바 캐릭터 여부를 저장하는 변수

    // 시작 위치와 돌아갈 위치 추가
    [SerializeField] private Transform startingPosition; // 시작 위치
    [SerializeField] private Transform returnPosition; // 돌아갈 위치


    private void Start()
    {
        servingTableManager = FindObjectOfType<ServingTableManager>();
        servingController = FindObjectOfType<ServingController>();
        chefManager = GetComponent<ChefManager>();

        aStarManager = FindObjectOfType<AStarManager>(); // A* 알고리즘 매니저 가져오기
        targetManager = FindObjectOfType<TargetManager>(); // TargetManager 스크립트 가져오기

        animator = GetComponentInChildren<Animator>(); // 자식 오브젝트에서 Animator 컴포넌트 가져오기

        lastPosition = transform.position; // 초기 위치 설정


        // GameManager 참조 가져오기
        gameManager = FindObjectOfType<GameManager>();

        if (servingTableManager == null)
        {
            Debug.LogError("ServingTableManager를 찾을 수 없습니다!");
        }

        if (servingController == null)
        {
            Debug.LogError("ServingController를 찾을 수 없습니다!");
        }


        // 쉐프 이름 검색
        if (gameObject.name == "Rumi")
        {
            isRumi = true; // Rumi일 경우 true로 설정
            startingPosition = GameObject.Find("Rumi").transform; // Rumi의 시작 위치
            returnPosition = GameObject.Find("Rumi").transform;

        }
        else if (gameObject.name == "Umika")
        {
            isUmika = true; // Umika일 경우 true로 설정
            startingPosition = GameObject.Find("Umika").transform; // Umika의 시작 위치
            returnPosition = GameObject.Find("Umika").transform; // Umika의 돌아갈 위치
        }
        else if (gameObject.name == "Siba")
        {
            isRumi = true;
            startingPosition = GameObject.Find("Siba").transform; // Siba의 시작 위치
            returnPosition = GameObject.Find("Siba").transform; // Siba의 돌아갈 위치
        }

        // 캐릭터의 시작 위치로 이동
        transform.position = startingPosition.position;

        isCooking = false;
    }


    private void Update()
    {
        // isCookingAnim이 true일 경우
        if (isCookingAnim)
        {
            isMoving = false; // 이동 중지
            isCooking = true; // 요리 상태로 전환

            // 애니메이터 파라미터 설정
            animator.SetBool("isMoving", false); // 이동 애니메이션 종료
            animator.SetBool("isCooking", true); // 요리 애니메이션 시작

            // 여기서 요리에 따른 애니메이션 재생
            switch ((GuestController.Order)currentFoodIndex)
            {
                case GuestController.Order.Steak:
                    if (isRumi)
                        animator.Play("Rumi Front Cooking"); // Rumi 스테이크 요리 애니메이션
                    else if (isUmika)
                        animator.Play("Umika Front Cooking"); // Umika 스테이크 요리 애니메이션
                    else if (isSiba)
                        animator.Play("Rumi Front Cooking"); // Siba 스테이크 요리 애니메이션
                    break;
                case GuestController.Order.FriedRice:
                    if (isRumi)
                        animator.Play("Rumi Left Cooking"); // Rumi 볶음밥 요리 애니메이션
                    else if (isUmika)
                        animator.Play("Umika Left Cooking"); // Umika 볶음밥 요리 애니메이션
                    else if (isSiba)
                        animator.Play("Rumi Left Cooking"); // Siba 볶음밥 요리 애니메이션
                    break;
                case GuestController.Order.Ramen:
                    if (isRumi)
                        animator.Play("Rumi Right Cooking"); // Rumi 라멘 요리 애니메이션
                    else if (isUmika)
                        animator.Play("Umika Right Cooking"); // Umika 라멘 요리 애니메이션
                    else if (isSiba)
                        animator.Play("Rumi Right Cooking"); // Siba 라멘 요리 애니메이션
                    break;
            }
        }
        else if (isMoving && moveSpeed != 0) // 이동 중일 때 애니메이션을 재생
        {
            animator.SetBool("isMoving", true);
            animator.SetBool("isCooking", false); // 요리 중이 아님

            // 현재 위치와 이전 위치의 차이를 통해 이동 방향 계산
            Vector2 movementDirection = (transform.position - lastPosition).normalized;

            // 각 방향에 따라 애니메이션 설정
            if (movementDirection == Vector2.left)
            {
                if (isRumi)
                    animator.Play("Rumi Default Left Move");
                else if (isUmika)
                    animator.Play("Umika Default Left Move");
                else if (isSiba)
                    animator.Play("Rumi Default Left Move");
            }
            else if (movementDirection == Vector2.right)
            {
                if (isRumi)
                    animator.Play("Rumi Default Right Move");
                else if (isUmika)
                    animator.Play("Umika Default Right Move");
                else if (isSiba)
                    animator.Play("Rumi Default Right Move");
            }
            else if (movementDirection == Vector2.up)
            {
                if (isRumi)
                    animator.Play("Rumi Defualt Up Move");
                else if (isUmika)
                    animator.Play("Umika Defualt Up Move");
                else if (isSiba)
                    animator.Play("Rumi Defualt Up Move");
            }
            else if (movementDirection == Vector2.down)
            {
                if (isRumi)
                    animator.Play("Rumi Default Down Move");
                else if (isUmika)
                    animator.Play("Umika Default Down Move");
                else if (isSiba)
                    animator.Play("Rumi Default Down Move");
            }

            // 마지막 위치 업데이트
            lastPosition = transform.position;
        }
        else
        {
            // 정지 상태일 때
            animator.SetBool("isMoving", false);
            animator.SetBool("isCooking", false);
            if (isRumi)
                animator.Play("Rumi Idle");
            else if (isUmika)
                animator.Play("Umika Idle");
            else if (isSiba)
                animator.Play("Rumi Idle");
        }
    }





    public bool HasPendingOrder()
    {
        return pendingOrders.Count > 0;
    }

    public void SetPendingOrder(int foodIndex, GuestController guest)
    {
        pendingOrders.Enqueue((foodIndex, guest));
    }

    public void StartCooking(int foodIndex, GuestController guest)
    {
        StartCoroutine(Cook(foodIndex, guest));
    }


    public void SetManager(ChefManager manager)
    {
        chefManager = manager;
    }
    public bool IsCooking()
    {
        return isCooking;
    }





    // 요리 시간 감소율 업데이트 함수
    public void UpdateCookingTimeReduction(float reduction)
    {
        cookingTimeReduction = reduction;
    }

    // 요리 시간을 반환하는 함수 수정
    private float GetCookingTime(int foodIndex)
    {

        float baseTime;
        switch ((GuestController.Order)foodIndex)
        {
            case GuestController.Order.Steak:
                baseTime = 15f;
                break;
            case GuestController.Order.Ramen:
                baseTime = 10f;
                break;
            case GuestController.Order.FriedRice:
                baseTime = 5f;
                break;
            default:
                baseTime = 5f;
                break;
        }
        // 요리 시간 감소 적용
        float reducedTime = baseTime * (1f - cookingTimeReduction);
        return reducedTime;
    }

    // 이동 속도 증가율 적용 함수
    public void UpdateMoveSpeed(float increasePercentage)
    {
        moveSpeed = 3f * (1f + increasePercentage); // 기본 속도에 증가율 적용
    }

    private void CreateCooldownUI(float cookingTime)
    {
        // 쿨타임 UI 생성 및 설정
        currentCooldownUI = Instantiate(cooldownUIPrefab);
        currentCooldownUI.transform.SetParent(FindObjectOfType<Canvas>().transform, false);
        cooldownUIRect = currentCooldownUI.GetComponent<RectTransform>();
        cooldownUIRect.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        currentCooldownUI.transform.SetAsFirstSibling(); // 캔버스의 첫 번째 자식으로 설정

        // 쉐프 머리 위에 UI 배치
        Vector3 uiPos = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0, height, 0));
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


    // 요리 완료
    private IEnumerator Cook(int foodIndex, GuestController guest)
    {
        currentFoodIndex = foodIndex; // 현재 요리 중인 음식 인덱스 저장

        isCooking = true;  // 요리 상태로 전환
        isMoving = true;   // 이동 중 상태로 설정

        // 요리할 위치로 이동
        Transform cookingPosition = GetCookingPosition(foodIndex);
        if (cookingPosition == null)
        {
            Debug.LogError("모든 요리 위치가 사용 중입니다. 대기 중인 주문 처리.");
            isCooking = false;
            isMoving = false; // 이동 상태 해제
            yield break;
        }

        // A* 알고리즘을 사용하여 요리 위치로 이동
        aStarManager.startPos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        aStarManager.SetTargetPos(cookingPosition.position);

        Debug.Log($"Starting pathfinding from {aStarManager.startPos} to {cookingPosition.position}");

        List<Vector3> path = aStarManager.PathFinding();

        if (path == null || path.Count == 0)
        {
            Debug.LogError($"Path not found or is invalid! StartPos: {aStarManager.startPos}, TargetPos: {cookingPosition.position}");
            isCooking = false;
            isMoving = false; // 이동 상태 해제
            yield break;
        }

        yield return StartCoroutine(FollowPath(path));

        // 요리 완료 후 자리 비활성화
        int cookingTargetIndex = targetManager.chefCookingTargets.IndexOf(cookingPosition);
        if (cookingTargetIndex != -1)
        {
            targetManager.cookingTargetStates[cookingTargetIndex] = false; // 요리 자리 비활성화
        }

        // 음식 오브젝트 활성화
        GameObject foodObject = GetFoodObject(foodIndex, targetManager.chefCookingTargets.IndexOf(cookingPosition));
        if (foodObject != null)
        {
            PlayCookingSound(foodIndex);  // 요리 소리 재생
            foodObject.SetActive(true);

            float cookingTime = GetCookingTime(foodIndex);  // 요리 시간 가져오기

            // 쿨타임 UI 시작
            CreateCooldownUI(cookingTime);

            // 애니메이션 재생을 위한 플래그 활성화
            isCookingAnim = true;
            isMoving = false; // 요리 중이므로 이동 멈춤


            // 요리 시간만큼 대기
            yield return new WaitForSeconds(cookingTime);

            // 요리 종료 후 애니메이션 비활성화
            isCookingAnim = false;
            isMoving = true;
            animator.SetBool("isCooking", false); // 요리 애니메이션 시작
            animator.SetBool("isMoving", true); // 요리 애니메이션 시작
            foodObject.SetActive(false);  // 요리 오브젝트 비활성화
            AudioManager.Instance.StopSound();  // 요리 소리 종료
        }

        // 요리 완료 후 자리 비활성화
        if (cookingTargetIndex != -1)
        {
            targetManager.cookingTargetStates[cookingTargetIndex] = false; // 요리 자리 비활성화
        }

        // 서빙 테이블로 이동
        Transform servingPosition = GetRandomServingPosition();
        if (servingPosition != null)
        {
            aStarManager.startPos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
            aStarManager.SetTargetPos(servingPosition.position);

            path = aStarManager.PathFinding();

            if (path != null && path.Count > 0)
            {
                yield return StartCoroutine(FollowPath(path));
                ServeFoodAtTable(foodIndex, guest, servingPosition); // 서빙
            }
        }

        // 초기 위치로 돌아가기
        aStarManager.startPos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        // 여기를 수정하여 각 캐릭터의 돌아갈 위치를 사용하도록 변경
        aStarManager.SetTargetPos(returnPosition.position); // Umika와 Rumi의 돌아갈 위치 사용

        path = aStarManager.PathFinding();

        if (path == null || path.Count == 0)
        {
            Debug.LogError($"Path not found or is invalid! StartPos: {aStarManager.startPos}, TargetPos: {returnPosition.position}");
            isCooking = false;
            isMoving = false; // 이동 상태 해제
            yield break;
        }

        yield return StartCoroutine(FollowPath(path));

        isCooking = false; // 요리 상태 해제
        isMoving = false; // 이동 상태 해제

        // 대기 중인 주문이 있는 경우 처리
        if (pendingOrders.Count > 0)
        {
            var (nextFoodIndex, nextGuest) = pendingOrders.Dequeue();
            StartCooking(nextFoodIndex, nextGuest);  // 다음 주문 처리
        }
    }


    // 특정 음식 위치 반환
    private Transform GetCookingPosition(int foodIndex)
    {
        // 음식에 따른 가능한 요리 위치 인덱스 설정
        List<int> availableIndices;

        switch ((GuestController.Order)foodIndex)
        {
            case GuestController.Order.Steak:
                availableIndices = new List<int> { 2, 3 }; // 2, 3 인덱스는 Steak 위치
                break;

            case GuestController.Order.Ramen:
                availableIndices = new List<int> { 4, 5 }; // 4, 5 인덱스는 Ramen 위치
                break;

            case GuestController.Order.FriedRice:
                availableIndices = new List<int> { 0, 1 }; // 0, 1 인덱스는 FriedRice 위치
                break;

            default:
                return null;
        }

        // 가능한 위치 중 비활성화된 위치만 추출
        List<int> inactiveIndices = availableIndices.FindAll(index => !targetManager.cookingTargetStates[index]);

        // 비활성화된 위치가 없으면 null 반환하고 대기 큐에 넣음
        if (inactiveIndices.Count == 0)
        {
            Debug.Log("모든 요리 위치가 사용 중입니다! 주문을 대기 큐에 추가합니다.");
            SetPendingOrder(foodIndex, null);  // 대기 큐에 추가
            return null;
        }

        // 비활성화된 인덱스 중 랜덤으로 선택
        int randomIndex = UnityEngine.Random.Range(0, inactiveIndices.Count);
        int selectedIndex = inactiveIndices[randomIndex];

        // 선택한 위치 반환
        return targetManager.chefCookingTargets[selectedIndex];
    }


    // 랜덤 서빙 테이블 위치 반환
    private Transform GetRandomServingPosition()
    {
        if (targetManager.chefServingTargets.Count == 0)
        {
            Debug.LogError("서빙 테이블 위치가 설정되어 있지 않습니다.");
            return null;
        }

        int randomIndex = UnityEngine.Random.Range(0, targetManager.chefServingTargets.Count);
        return targetManager.chefServingTargets[randomIndex];
    }

    // A* 알고리즘을 사용하여 경로를 따르는 코루틴
    private IEnumerator FollowPath(List<Vector3> path)
    {
        foreach (Vector3 point in path)
        {
            //while (transform.position != point)
            while (!(transform.position.x == point.x && transform.position.y == point.y))
            {
                //transform.position = Vector3.MoveTowards(transform.position, point, moveSpeed * Time.deltaTime);
                Vector3 pos = Vector3.MoveTowards(transform.position, point, moveSpeed * Time.deltaTime);
                pos.z = (pos.y / 1000f) + ((float)transform.GetSiblingIndex() / 100f);

                transform.position = pos;
                yield return null;
            }
        }

    }


    // 특정 음식 오브젝트 반환
    private GameObject GetFoodObject(int foodIndex, int cookingTargetIndex)
    {
        switch ((GuestController.Order)foodIndex)
        {
            case GuestController.Order.FriedRice:
                return (cookingTargetIndex == 0) ? oneFriedRiceObject :
                       (cookingTargetIndex == 1) ? twoFriedRiceObject : null;

            case GuestController.Order.Steak:
                return (cookingTargetIndex == 2) ? oneSteakObject :
                       (cookingTargetIndex == 3) ? twoSteakObject : null;

            case GuestController.Order.Ramen:
                return (cookingTargetIndex == 4) ? oneRamenObject :
                       (cookingTargetIndex == 5) ? twoRamenObject : null;

            default:
                return null;
        }
    }

    // 사운드 재생
    private void PlayCookingSound(int foodIndex)
    {
        AudioClip clip = null;
        switch ((GuestController.Order)foodIndex)
        {
            case GuestController.Order.Steak:
                clip = AudioManager.Instance.steakClip;
                break;
            case GuestController.Order.Ramen:
                clip = AudioManager.Instance.ramenClip;
                break;
            case GuestController.Order.FriedRice:
                clip = AudioManager.Instance.friedRiceClip;
                break;
        }
        if (clip != null)
        {
            AudioManager.Instance.PlaySound(clip);
        }
    }

    // 서빙 테이블에서 음식 서빙
    private void ServeFoodAtTable(int foodIndex, GuestController guest, Transform servingPosition)
    {
        int servingPositionIndex = targetManager.chefServingTargets.IndexOf(servingPosition);
        if (servingPositionIndex >= 0 && servingPositionIndex < targetManager.foodRespawn.Count)
        {
            Transform foodRespawnTransform = targetManager.foodRespawn[servingPositionIndex];
            GameObject foodRespawnObject = foodRespawnTransform.gameObject;
            servingTableManager.ToggleFoodOnServingTable(foodRespawnObject, GetFoodName(foodIndex));

            ServingManager servingManager = FindObjectOfType<ServingManager>();
            if (servingManager != null) servingManager.EnqueueServingOrder(foodIndex, GetFoodName(foodIndex), foodRespawnObject, guest, servingPositionIndex);
        }
    }

    // 음식 인덱스로 음식 이름 반환
    private string GetFoodName(int foodIndex)
    {
        switch ((GuestController.Order)foodIndex)
        {
            case GuestController.Order.Steak:
                return "Steak";
            case GuestController.Order.Ramen:
                return "Ramen";
            case GuestController.Order.FriedRice:
                return "Fried Rice";
            default:
                return "Unknown Food";
        }
    }
}