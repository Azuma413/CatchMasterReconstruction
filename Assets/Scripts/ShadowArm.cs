using UnityEngine;

public class ShadowArm
{
    private Transform[] Joints;
    public double[] theta = new double[6];    //angle of the joints
    public ShadowArm(Transform[] joints)
    {
        this.Joints = joints;
        theta[0] = theta[1] = theta[2] = theta[3] = theta[4] = theta[5] = 0.0;
    }
    public void Update()
    {
        if (!double.IsNaN(theta[0])) Joints[0].transform.localEulerAngles = new Vector3(0, -(float)theta[0] * Mathf.Rad2Deg, 0);
        if (!double.IsNaN(theta[1])) Joints[1].transform.localEulerAngles = new Vector3(0, 0, (float)theta[1] * Mathf.Rad2Deg - 90);
        if (!double.IsNaN(theta[2])) Joints[2].transform.localEulerAngles = new Vector3(0, 0, (float)theta[2] * Mathf.Rad2Deg);
        if (!double.IsNaN(theta[3])) Joints[3].transform.localEulerAngles = new Vector3((float)theta[3] * Mathf.Rad2Deg, 0, 0);
        if (!double.IsNaN(theta[4])) Joints[4].transform.localEulerAngles = new Vector3(0, 0, (float)theta[4] * Mathf.Rad2Deg);
        if (!double.IsNaN(theta[5])) Joints[5].transform.localEulerAngles = new Vector3((float)theta[5] * Mathf.Rad2Deg, 0, 0);
    }
}

