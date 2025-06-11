using System;
using UnityEngine;

[Serializable]
public class MoveHandlerTest {
    [SerializeField] private float rotateSpeed =  110.0f;
    [SerializeField] private float moveSpeed = 4.0f;
    private Transform _thisTransform;
    
    public void Initialize(Transform thisTransform) {
        _thisTransform = thisTransform;
    }

    public void Move(Vector2 moveVector) {
        _thisTransform.Rotate(0, moveVector.x * Time.deltaTime * rotateSpeed, 0);
        _thisTransform.Translate(0, 0, moveVector.y * Time.deltaTime * moveSpeed);
    }
}