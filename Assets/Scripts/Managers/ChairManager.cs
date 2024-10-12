using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChairManager : MonoBehaviour
{

    public bool[] leftChairOccupied; // 왼쪽 의자 상태 배열
    public bool[] rightChairOccupied; // 오른쪽 의자 상태 배열

    private void Start()
    {
        int halfCount = transform.childCount / 2;
        // 왼쪽 의자 상태 배열 초기화
        leftChairOccupied = new bool[halfCount];
        // 오른쪽 의자 상태 배열 초기화
        rightChairOccupied = new bool[halfCount];
    }

    // 왼쪽 의자가 사용 중인지 여부 반환
    public bool IsLeftChairOccupied(int chairIndex)
    {
        return leftChairOccupied[chairIndex];
    }

    // 오른쪽 의자가 사용 중인지 여부 반환
    public bool IsRightChairOccupied(int chairIndex)
    {
        return rightChairOccupied[chairIndex];
    }

    // 왼쪽 의자를 사용 중으로 설정
    public void OccupyLeftChair(int chairIndex)
    {
        leftChairOccupied[chairIndex] = true;
    }

    // 오른쪽 의자를 사용 중으로 설정
    public void OccupyRightChair(int chairIndex)
    {
        rightChairOccupied[chairIndex] = true;
    }

    // 왼쪽 의자를 비어 있는 상태로 설정
    public void ReleaseLeftChair(int chairIndex)
    {
        leftChairOccupied[chairIndex] = false;
    }

    // 오른쪽 의자를 비어 있는 상태로 설정
    public void ReleaseRightChair(int chairIndex)
    {
        rightChairOccupied[chairIndex] = false;
    }
}
