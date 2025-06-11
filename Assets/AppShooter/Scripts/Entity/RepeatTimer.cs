using System;
using UnityEngine;

public class RepeatTimer {
    public event Action AlarmEvent;

    private bool _isActive;
    private float _timer, _interval;

    public void Start(float interval){
        _isActive = true;
        _interval = interval;
        Restart();
    }

    public void Stop(){
        _isActive = false;
    }

    public void Tick(){
        if (!_isActive) return;
        if (Time.time < _timer) return;

        AlarmEvent?.Invoke();
        Restart();
    }

    private void Restart(){
        _timer = Time.time + _interval;
    }
}