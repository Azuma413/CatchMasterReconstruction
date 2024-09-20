using UnityEngine;

public class IKControl
{
    private Transform target; // 追従する物体
    private Transform endEffector; // 手先の物体
    private Transform root; // ロボットアームの根元
    private Transform[] Joints;
    public double[] theta = new double[6];    //angle of the joints
    private float L1, L2, L3, L4, L5, L6;    //arm length in order from base
    private float C3;
    public const float ik_range = 0.5f; // 手先の位置がこの範囲内に収まるとIKを計算する。
    public IKControl(Transform target, Transform endEffector, Transform root, Transform[] joints)
    {
        this.target = target;
        this.endEffector = endEffector;
        this.root = root;
        this.Joints = joints;
        theta[0] = theta[1] = theta[2] = theta[3] = theta[4] = theta[5] = 0.0;
        L1 = 0.1735f;
        L2 = 0.5f;
        L3 = 0.0671f;
        L4 = 0.5f - L3;
        L5 = 0.0451f;
        L6 = 0.075f - L5;
        C3 = 0.0f;
    }
    public bool CheckDistance(float range)
    { // 手先の位置が範囲内に収まっているか確認
        Vector3 target_pos = target.position - root.position;
        if (Vector3.Distance(target_pos, endEffector.position - root.position) > range)
        {
            return false;
        }
        return true;
    }
    public void Update()
    {
        Vector3 target_pos = target.position - root.position;
        if (Vector3.Distance(target_pos, endEffector.position - root.position) > ik_range)
        {
            return;
        }
        float ax, ay, az;
        float asx, asy, asz;
        float p5x, p5y, p5z;
        float C1, C23, S1, S23;
        float px, py, pz, ry, rz;
        px = -target_pos.z;
        py = target_pos.x;
        pz = target_pos.y;
        Quaternion q = target.rotation * Quaternion.AngleAxis(-90, Vector3.up);
        ry = -q.eulerAngles.x;
        rz = -q.eulerAngles.y;
        ax = Mathf.Cos(rz * Mathf.PI / 180.0f) * Mathf.Cos(ry * Mathf.PI / 180.0f);
        ay = Mathf.Sin(rz * Mathf.PI / 180.0f) * Mathf.Cos(ry * Mathf.PI / 180.0f);
        az = -Mathf.Sin(ry * Mathf.PI / 180.0f);
        p5x = px - (L5 + L6) * ax;
        p5y = py - (L5 + L6) * ay;
        p5z = pz - (L5 + L6) * az;
        theta[0] = Mathf.Atan2(p5y, p5x);
        C3 = (Mathf.Pow(p5x, 2) + Mathf.Pow(p5y, 2) + Mathf.Pow(p5z - L1, 2) - Mathf.Pow(L2, 2) - Mathf.Pow(L3 + L4, 2)) / (2 * L2 * (L3 + L4));
        theta[2] = Mathf.Atan2(Mathf.Pow(1 - Mathf.Pow(C3, 2), 0.5f), C3);
        float M = L2 + (L3 + L4) * C3;
        float N = (L3 + L4) * Mathf.Sin((float)theta[2]);
        float A = Mathf.Pow(p5x * p5x + p5y * p5y, 0.5f);
        float B = p5z - L1;
        theta[1] = Mathf.Atan2(M * A - N * B, N * A + M * B);
        C1 = Mathf.Cos((float)theta[0]);
        C23 = Mathf.Cos((float)theta[1] + (float)theta[2]);
        S1 = Mathf.Sin((float)theta[0]);
        S23 = Mathf.Sin((float)theta[1] + (float)theta[2]);
        asx = C23 * (C1 * ax + S1 * ay) - S23 * az;
        asy = -S1 * ax + C1 * ay;
        asz = S23 * (C1 * ax + S1 * ay) + C23 * az;
        theta[3] = Mathf.Atan2(asy, asx);
        theta[4] = Mathf.Atan2(Mathf.Cos((float)theta[3]) * asx + Mathf.Sin((float)theta[3]) * asy, asz);
        if (!double.IsNaN(theta[0]))
            Joints[0].transform.localEulerAngles = new Vector3(0, -(float)theta[0] * Mathf.Rad2Deg, 0);
        if (!double.IsNaN(theta[1]))
            Joints[1].transform.localEulerAngles = new Vector3(0, 0, (float)theta[1] * Mathf.Rad2Deg - 90);
        if (!double.IsNaN(theta[2]))
            Joints[2].transform.localEulerAngles = new Vector3(0, 0, (float)theta[2] * Mathf.Rad2Deg);
        if (!double.IsNaN(theta[3]))
            Joints[3].transform.localEulerAngles = new Vector3((float)theta[3] * Mathf.Rad2Deg, 0, 0);
        if (!double.IsNaN(theta[4]))
            Joints[4].transform.localEulerAngles = new Vector3(0, 0, (float)theta[4] * Mathf.Rad2Deg);
        // ターゲットのローカル回転を計算
        Quaternion q_joint4 = Joints[4].transform.rotation;
        Quaternion q_local = Quaternion.Inverse(q_joint4) * q;
        theta[5] = -q_local.eulerAngles.z * Mathf.Deg2Rad;
        if (!double.IsNaN(theta[5])) Joints[5].transform.localEulerAngles = new Vector3((float)theta[5] * Mathf.Rad2Deg, 0, 0);
    }
}

