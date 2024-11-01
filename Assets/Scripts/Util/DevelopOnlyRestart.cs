using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevelopOnlyRestart : MonoBehaviour
{
    private RectTransform _rectTransform = null;

    private float _minX, _minY;
    private float _maxX, _maxY;

    [SerializeField, Range(1f, 10f)] private float restartTime = 2f; 

    private void Start()
    {
        _rectTransform = GetComponent<RectTransform>();

        _minX = _rectTransform.position.x - (_rectTransform.sizeDelta.x / 2f);
        _maxX = _rectTransform.position.x + (_rectTransform.sizeDelta.x / 2f);

        _minY = _rectTransform.position.y - (_rectTransform.sizeDelta.y / 2f);
        _maxY = _rectTransform.position.y + (_rectTransform.sizeDelta.y / 2f);
    }

    private float _pressTime = -1f;
    
    private void Update()
    {
        if (Input.touchCount <= 0) return;

        Touch t = Input.GetTouch(0);

        // 터치 상태 확인
        if (t.phase == TouchPhase.Ended)
        {
            _pressTime = -1f;
        }

        else if (_pressTime < 0 && t.phase == TouchPhase.Began && CheckClick(t.position))
        {
            _pressTime = Time.time;
            //Debug.Log("press");
        }

        

        // 일정시간동안 누르는 경우
        if (_pressTime > 0f && _pressTime + restartTime <= Time.time)
        {
            _pressTime = -1f;

            Destroy(this); // 스크립트만 제거
            UnityEngine.SceneManagement.SceneManager.LoadScene("Intro");
        }
    }

    private bool CheckClick(Vector2 touch)
    {
        if (touch.x < _minX || touch.x > _maxX) return false;
        if (touch.y < _minY || touch.y > _maxY) return false;

        return true;
    }
}
