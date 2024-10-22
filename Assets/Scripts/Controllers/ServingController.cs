using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServingController : MonoBehaviour
{
    private bool isServing = false;
    private bool hasPendingOrder = false;

    private (int, string, GameObject, GuestController, int) pendingOrder; // 예약된 주문

    [SerializeField]
    private float moveSpeed = 1f;

    [SerializeField]
    private float foodDisplayDuration = 0.5f;

    private Vector2 initialPosition;
    private AStarManager aStarManager;

    [SerializeField]
    private TargetManager targetManager;
    private ServingManager servingManager;

    private Vector3 lastPosition; // 이전 프레임의 위치 저장
    private Animator animator;
    private bool isMoving;

    private bool isSerika; // 세리카 캐릭터 여부를 저장하는 변수

    private bool isShizuko; // 시즈코 캐릭터 여부를 저장하는 변수

    private bool isFuuka;

    // 시작 위치와 돌아갈 위치 추가
    [SerializeField] private Transform startingPosition; // 시작 위치

    private void Start()
    {
        aStarManager = FindObjectOfType<AStarManager>();
        targetManager = FindObjectOfType<TargetManager>();
        servingManager = FindObjectOfType<ServingManager>();
        // Reference Animator component from child object
        animator = GetComponentInChildren<Animator>();

        // 캐릭터 이름 검색
        if (gameObject.name == "Serika")
        {
            isSerika = true;
            startingPosition = GameObject.Find("Serika").transform; // Serika의 시작 위치
           
        }
        else if (gameObject.name == "Shizuko")
        {
            isShizuko = true;
            startingPosition = GameObject.Find("Shizuko").transform; // Shizuko의 시작 위치

        }
        else if (gameObject.name == "Fuuka")
        {
            isShizuko = true;
            startingPosition = GameObject.Find("Fuuka").transform; // Shizuko의 시작 위치

        }

        // 캐릭터의 시작 위치로 이동
        transform.position = startingPosition.position;

        // 초기 위치 저장
        initialPosition = transform.position;

        lastPosition = transform.position;
        isMoving = false;
    }

    private void Update()
    {
        Vector3 currentPosition = transform.position;
        Vector2 movementDirection = (currentPosition - lastPosition).normalized;

        // 이동 여부 판단
        if (Vector3.Distance(currentPosition, lastPosition) > 0.01f)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }

        // 애니메이션 재생 처리
        if (isMoving)
        {
            animator.SetBool("isMoving", true);

            // 이동 방향에 따른 애니메이션 설정
            if (Mathf.Abs(movementDirection.x) > Mathf.Abs(movementDirection.y))
            {
                if (movementDirection.x > 0)
                {
                    // 오른쪽 이동
                    if (isSerika)
                        animator.Play("Serika Default Right Move");
                    else if (isShizuko)
                        animator.Play("Shizuko Default Right Move");
                    else if (isFuuka)
                        animator.Play("Shizuko Default Right Move");
                }
                else
                {
                    // 왼쪽 이동
                    if (isSerika)
                        animator.Play("Serika Default Left Move");
                    else if (isShizuko)
                        animator.Play("Shizuko Default Left Move");
                    else if (isFuuka)
                        animator.Play("Shizuko Default Left Move");
                }
            }
            else
            {
                if (movementDirection.y > 0)
                {
                    // 위로 이동
                    if (isSerika)
                        animator.Play("Serika Defualt Up Move");
                    else if (isShizuko)
                        animator.Play("Shizuko Defualt Up Move");
                    else if (isFuuka)
                        animator.Play("Shizuko Defualt Up Move");
                }
                else
                {
                    // 아래로 이동
                    if (isSerika)
                        animator.Play("Serika Default Down Move");
                    else if (isShizuko)
                        animator.Play("Shizuko Default Down Move");
                    else if (isFuuka)
                        animator.Play("Shizuko Default Down Move");
                }
            }
        }
        else
        {
            // 이동하지 않을 때 정지 애니메이션 재생
            animator.SetBool("isMoving", false);
            if (isSerika)
                animator.Play("Serika Idle");
            else if (isShizuko)
                animator.Play("Shizuko Idle");
            else if (isFuuka)
                animator.Play("Shizuko Idle");
        }

        // 마지막 위치 업데이트
        lastPosition = currentPosition;
    }

    public void SetManager(ServingManager manager)
    {
        servingManager = manager;
    }

    public bool IsServing()
    {
        return isServing;
    }

    public bool HasPendingOrder()
    {
        return hasPendingOrder;
    }

    public void StartServing(int foodIndex, string foodName, GameObject servingTable, GuestController guest, int servingPositionIndex)
    {
        StartCoroutine(ServeOrder(foodIndex, foodName, servingTable, guest, servingPositionIndex));
    }

    public void SetPendingOrder(int foodIndex, string foodName, GameObject servingTable, GuestController guest, int servingPositionIndex)
    {
        pendingOrder = (foodIndex, foodName, servingTable, guest, servingPositionIndex);
        hasPendingOrder = true;
    }

    private IEnumerator ServeOrder(int foodIndex, string foodName, GameObject servingTable, GuestController guest, int servingPositionIndex)
    {

        isServing = true;
        isMoving = true; // 이동 시작

        // foodRespawn와 동일한 인덱스의 servingTables 위치 찾기
        Transform servingPosition = targetManager.servingTables[servingPositionIndex];

        if (servingPosition != null)
        {
            // 경로 찾기 실행
            aStarManager.startPos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
            aStarManager.SetTargetPos(servingPosition.position);

            List<Vector3> path = aStarManager.PathFinding();

            if (path != null && path.Count > 0)
            {
                // 경로를 따라 서빙 테이블로 이동
                yield return StartCoroutine(FollowPath(path));

                // 음식 활성화/비활성화 토글
                ServingTableManager servingTableManager = servingTable.GetComponent<ServingTableManager>();
                if (servingTableManager != null)
                {
                    servingTableManager.ToggleFoodOnServingTable(servingTable, foodName);
                }

                // 손님 위치로 이동
                Transform guestPosition = guest.transform;
                Vector3 closestTarget = FindClosestServerServingTarget(guestPosition.position);

                aStarManager.startPos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
                aStarManager.SetTargetPos(closestTarget);

                path = aStarManager.PathFinding();
                if (path != null && path.Count > 0)
                {
                    yield return StartCoroutine(FollowPath(path));

                    // 손님에게 음식 제공
                    guest.StartCoroutine(guest.EatFood());
                    AudioManager.Instance.PlaySound(AudioManager.Instance.serveFoodClip);

                    yield return new WaitForSeconds(foodDisplayDuration);
                }
                else
                {
                    Debug.LogError("손님 위치로 경로를 찾을 수 없습니다!");
                }

                // 본래 위치로 돌아가기
                yield return StartCoroutine(ReturnToInitialPosition());
            }
            else
            {
                Debug.LogError("유효하지 않은 foodIndex 또는 servingTables/foodRespawn 설정 오류");
            }

            isServing = false;
            isMoving = false; // 이동 종료

            // 예약된 주문이 있다면 처리
            if (hasPendingOrder)
            {
                var (pendingFoodIndex, pendingFoodName, pendingServingTable, pendingGuest, pendingServingPositionIndex) = pendingOrder;
                hasPendingOrder = false;
                StartServing(pendingFoodIndex, pendingFoodName, pendingServingTable, pendingGuest, pendingServingPositionIndex);
            }
            else
            {
                servingManager.OnServerFinishedServing();
            }
        }
    }

    private Vector3 FindClosestServerServingTarget(Vector3 guestPosition)
    {
        Vector3 closestTarget = Vector3.zero;
        float closestDistance = float.MaxValue;

        foreach (var target in targetManager.serverServingTargets)
        {
            float distance = Vector3.Distance(guestPosition, target.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = target.position;
            }
        }

        return closestTarget;
    }

    private IEnumerator ReturnToInitialPosition()
    {
        aStarManager.startPos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        aStarManager.SetTargetPos(initialPosition); // 초기 위치로 설정

        List<Vector3> path = aStarManager.PathFinding();

        // 경로를 따라 초기 위치로 이동
        if (path != null && path.Count > 0)
        {
            yield return StartCoroutine(FollowPath(path));
        }
        else
        {
            Debug.LogError("본래 위치로 경로를 찾을 수 없습니다!");
        }
    }

    private IEnumerator FollowPath(List<Vector3> path)
    {
        foreach (var waypoint in path)
        {
            while (Vector3.Distance(transform.position, waypoint) > 0.1f)
            {
                //transform.position = Vector3.MoveTowards(transform.position, waypoint, moveSpeed * Time.deltaTime);
                //yield return null;

                Vector3 pos = Vector3.MoveTowards(transform.position, waypoint, moveSpeed * Time.deltaTime);
                pos.z = (pos.y / 1000f) + ((float)transform.GetSiblingIndex() / 100f);

                transform.position = pos;
                yield return null;
            }
        }
    }

   
}