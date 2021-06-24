using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MathScript : MonoBehaviour
{
    // 玩家
    private GameObject player;
    // 玩家的半径
    private float radius;

    private Vector3 leftTangent;
    private Vector3 rightTangent;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        var playerCollider = player.GetComponent<CapsuleCollider>();
        radius = playerCollider.radius;
    }

    // Update is called once per frame
    void Update()
    {
        // vectorLine();
        // quaternionLine();
        calculateTangent();
    }

    /**
     * 炸弹爆炸的时候，检查玩家的切点位置
     */
    private void calculateTangent()
    {
        var playerPosition = player.transform.position;
        var bombPosition = transform.position;
        var playerToBomb = bombPosition - playerPosition;
        var playerToBombDirection = playerToBomb.normalized * radius;
        var angle = Mathf.Acos(radius / playerToBomb.magnitude) * Mathf.Rad2Deg;
        // 两者等价
        // Quaternion.Euler(0, angle, 0)
        // Quaternion.AngleAxis(angle, Vector3.up);
        leftTangent = playerPosition + Quaternion.Euler(0, -angle, 0) * playerToBombDirection;
        rightTangent = playerPosition + Quaternion.Euler(0, angle, 0) * playerToBombDirection;
        Debug.DrawLine(leftTangent, bombPosition);
        Debug.DrawLine(rightTangent, bombPosition);
    }

    /**
     * 计算物体右前方30度，10m远的坐标
     */
    private void vectorLine()
    {
        var playerTransform = player.transform;
        var x = Mathf.Sin(30 * Mathf.Deg2Rad) * 10;
        var z = Mathf.Cos(30 * Mathf.Deg2Rad) * 10;
        var targetVector = new Vector3(x, 0, z);
        var wordPoint = playerTransform.TransformPoint(targetVector);
        Debug.DrawLine(playerTransform.position, wordPoint);
    }

    private void quaternionLine()
    {
        var playerTransform = player.transform;
        var playerPosition = playerTransform.position;
        var vector = new Vector3(0, 0, 10);
        // var wordPoint = playerTransform.TransformPoint(Quaternion.Euler(0, 30, 0) * vector);
        var wordPoint = playerTransform.position + Quaternion.Euler(0, 30, 0) * playerTransform.rotation * vector;
        Debug.DrawLine(playerPosition, wordPoint);
    }
}