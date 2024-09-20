using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TMPro;

public class PlotHandPose : MonoBehaviour
{
    [SerializeField] private GameObject hand;
    [SerializeField] private GameObject plot_object;
    [SerializeField] private TextMeshProUGUI text;
    // Start is called before the first frame update
    void Start()
    {
        if (hand == null)
        {
            Debug.LogError("Hand object is not set.");
            return;
        }
        if (plot_object == null)
        {
            Debug.LogError("Plot object is not set.");
            return;
        }
        if (text == null)
        {
            Debug.LogError("Text object is not set.");
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // hand��Transform���擾
        Transform hand_transform = hand.transform;
        // plot_object�ɑ΂���hand�̈ʒu���v���b�g
        plot_object.transform.position = hand_transform.position;
        Quaternion rot = Quaternion.AngleAxis(90, Vector3.up);
        plot_object.transform.rotation = hand_transform.rotation*rot;
        // text�ɑ΂���hand�̈ʒu��\��
        text.text = "Position: " + hand_transform.position.ToString() + "\n" + "Rotation: " + hand_transform.rotation.eulerAngles.ToString();
    }
}
