using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBaseController : MonoBehaviour
{

    protected float moveSpeed;

    public virtual void UpdateMoveSpeed(float increasePercentage)
    {
        moveSpeed = moveSpeed * (1f + increasePercentage);
        Debug.Log($"이동 속도: {moveSpeed}");
    }
}
