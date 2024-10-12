using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetManager : MonoBehaviour
{
    // 손님, 서빙 공통 사용 
    public List<Transform> guestTargetTables; // 6 target for serving

    // 요리사, 서빙 공통 사용
    public List<Transform> foodRespawn; // 5 target for foodIndex

    //요리사 이동 경로
    public List<Transform> chefCookingTargets; // 3 targets for cooking
    public List<Transform> chefServingTargets; // 5 targets for serving

    //서빙 이동 경로
    public List<Transform> servingTables; // 5 targets for serving 
    public List<Transform> serverServingTargets; // 12 targets for serving

    //손님 이동 경로
    public List<Transform> guestSeatingTargets; // 12 targets for seating
    public Transform guestExitTarget; // 1 exit target

    public bool[] cookingTargetStates; // 요리 자리 상태 배열

    private void Awake()
    {
        InitializeCookingTargetStates();
    }

    // cookingTargetStates 배열을 chefCookingTargets 리스트 크기에 맞춰 초기화
    public void InitializeCookingTargetStates()
    {
        // chefCookingTargets 크기에 맞게 cookingTargetStates 크기 설정
        cookingTargetStates = new bool[chefCookingTargets.Count];

        // 모든 자리를 비활성화 상태로 초기화
        for (int i = 0; i < cookingTargetStates.Length; i++)
        {
            cookingTargetStates[i] = false;
        }
    }

    public void AddChefCookingTarget(Transform newTarget)
    {
        chefCookingTargets.Add(newTarget);

        // cookingTargetStates 배열 확장
        bool[] newStates = new bool[chefCookingTargets.Count];
        for (int i = 0; i < cookingTargetStates.Length; i++)
        {
            newStates[i] = cookingTargetStates[i]; // 기존 상태 복사
        }
        newStates[chefCookingTargets.Count - 1] = false; // 새로 추가된 자리는 비활성화

        cookingTargetStates = newStates; // 배열 교체
    }

    public void RemoveChefCookingTarget(int index)
    {
        if (index >= 0 && index < chefCookingTargets.Count)
        {
            chefCookingTargets.RemoveAt(index);

            // cookingTargetStates 배열도 해당 인덱스를 제거
            bool[] newStates = new bool[chefCookingTargets.Count];
            for (int i = 0, j = 0; i < cookingTargetStates.Length; i++)
            {
                if (i == index) continue; // 삭제된 인덱스는 건너뜀
                newStates[j++] = cookingTargetStates[i];
            }

            cookingTargetStates = newStates; // 배열 교체
        }
    }
    public void EnsureCookingTargetConsistency()
    {
        if (cookingTargetStates.Length != chefCookingTargets.Count)
        {
            Debug.LogError("cookingTargetStates 배열과 chefCookingTargets 리스트의 크기가 일치하지 않습니다.");
            InitializeCookingTargetStates(); // 다시 초기화하여 동기화
        }
    }

}
