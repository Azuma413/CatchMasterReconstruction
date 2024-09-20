using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetTargetPose : MonoBehaviour
{
    [SerializeField] private GameObject hand;
    [SerializeField] private GameObject target;
    // Start is called before the first frame update
    void Start()
    {
        if (hand == null)
        {
            Debug.LogError("Hand object is not set.");
            return;
        }
        if (target == null)
        {
            Debug.LogError("Target object is not set.");
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        target.transform.position = hand.transform.position + hand.transform.forward * 0.1f;
        target.transform.rotation = hand.transform.rotation;
    }
}
