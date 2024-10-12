using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServingTableManager : MonoBehaviour
{
    public void ToggleFoodOnServingTable(GameObject servingTable, string foodName)
    {
        if (servingTable != null)
        {
            Debug.Log("서빙 테이블에서 이름이 " + foodName + "인 음식 오브젝트를 찾는 중: " + servingTable.name);

            bool foodFound = false;

            // 모든 자식 오브젝트 순회
            foreach (Transform child in servingTable.transform)
            {
                if (child.name == foodName)
                {
                    foodFound = true;
                    // 음식 오브젝트의 활성화 상태를 토글
                    bool isActive = child.gameObject.activeSelf;
                    child.gameObject.SetActive(!isActive);
                    Debug.Log("이름이 " + foodName + "인 음식 오브젝트 " + (isActive ? "비활성화됨" : "활성화됨"));
                    break;
                }
            }

            if (!foodFound)
            {
                Debug.LogError("서빙 테이블 " + servingTable.name + "에서 이름이 " + foodName + "인 음식 오브젝트를 찾을 수 없음");
                foreach (Transform child in servingTable.transform)
                {
                    Debug.Log("자식 오브젝트: " + child.name);
                }
            }
        }
        else
        {
            Debug.LogError("서빙 테이블이 null입니다.");
        }
    }
}