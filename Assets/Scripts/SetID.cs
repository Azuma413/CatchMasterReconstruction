using System;
using UnityEngine;

public class SetID : MonoBehaviour
{
    [SerializeField] private int ROS_DOMAIN_ID = 30;
    void Start()
    {
        // ROS_DOMAIN_IDに123を設定する
        Environment.SetEnvironmentVariable("ROS_DOMAIN_ID", ROS_DOMAIN_ID.ToString());
        // 上手く設定できているか確認する
        string value = Environment.GetEnvironmentVariable("ROS_DOMAIN_ID");
        Debug.Log("ROS_DOMAIN_ID:" + value + "\n");
    }
}
