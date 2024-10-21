using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    // 생성 가능한 게스트 목록
    [SerializeField] private List<GuestController> guests;
    private List<GuestController> _spawnableList;

    private GuestController _uniqueGuest = null;

    private void Start()
    {
        _spawnableList = guests.Where(x => x.IsSpawnable(GameManager.CurrentStage) && !x.IsUniqueGuest).ToList();
        _uniqueGuest = guests.FirstOrDefault(x => x.IsUniqueGuest);

        if (_uniqueGuest != null) _spawnableList.Add(_uniqueGuest);
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
        // 소환 가능한 모든 게스트의 가중치의 합
        int weightSum = _spawnableList.Sum(x => x.SpawnWeight);

        // 랜덤값
        int random = UnityEngine.Random.Range(0, weightSum);

        int debugValue = random;
        // 소환할 게스트 타겟을 찾기
        GuestController guest = _spawnableList.First();
        for (int i = 0; i < _spawnableList.Count; i++)
        {
            if (random <= _spawnableList[i].SpawnWeight)
            {
                guest = _spawnableList[i];
                break;
            }

            random -= _spawnableList[i].SpawnWeight;
        }

        Debug.Log($"spawn {guest.name} ## {debugValue} / {weightSum}");
        // 생서ㅗㅇ
        GuestController instance = Instantiate(guest, spawnPoint.position, Quaternion.identity);

        // 특수 게스트인 경우 생성되지 않도록 목록에서 제거
        if (guest.IsUniqueGuest)
        {
            _spawnableList.Remove(guest);

            // 나가는 경우 다시 목록에 추가
            instance.OnExitGuest += () => _spawnableList.Add(_uniqueGuest);
        }

        // 이하 미사용
        return;

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

