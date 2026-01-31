using UnityEngine;
using System.Collections.Generic;
using System;
public class PlayerDesitionTimer : MonoBehaviour
{
    public float decisionTimeLimit = 5f;
    private float timer;
    private bool isTimerRunning = false;
    public event System.Action OnTimeOut;

    public void StartTimer()
    {
        timer = decisionTimeLimit;
        isTimerRunning = true;
    }
    public void StopTimer()
    {
        isTimerRunning = false;
    }
    private void Update()
    {
        if (!isTimerRunning) return;
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            isTimerRunning = false;
            OnTimeOut?.Invoke();
        }
    }
}
