using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuestRespawns : MonoBehaviour
{
    [SerializeField]
    private GameObject millenniumPrefab; // Millennium 프리팹

    [SerializeField]
    private GameObject trinityPrefab; // Trinity 프리팹

    [SerializeField]
    Transform spawnPoint; // 손님 생성 위치

    [SerializeField]
    public float respawnTime = 10f; // 손님 리스폰 간격


    private Coroutine respawnCoroutine; // 코루틴을 추적하기 위한 변수

    private void Start()
    {
    }


    // 손님 리스폰 코루틴 시작
    private void StartRespawn()
    {
        if (respawnCoroutine == null)
        {
            respawnCoroutine = StartCoroutine(RespawnCustomers());
        }
    }

    // 손님 생성 코루틴
    private IEnumerator RespawnCustomers()
    {
        while (true) // 무한 루프를 통해 계속 생성
        {
            RespawnCustomer(); // 손님 생성
            yield return new WaitForSeconds(respawnTime); // respawnTime 대기
        }
    }
    // 손님 생성 메소드
    public void RespawnCustomer()
    {
        // 50% 확률로 Millennium 또는 Trinity 리스폰
        GameObject newCustomer;

        if (Random.value < 0.5f) // 50% 확률
        {
            newCustomer = Instantiate(millenniumPrefab, spawnPoint.position, Quaternion.identity);
        }
        else
        {
            newCustomer = Instantiate(trinityPrefab, spawnPoint.position, Quaternion.identity);
        }

        // (배치된 손님에 대한 추가 초기화나 설정을 할 수 있습니다.)
    }

    // 손님 생성 메소드 호출
    public void CreateCustomer()
    {

        // 손님 리스폰 시작
        StartRespawn();
    }
}

