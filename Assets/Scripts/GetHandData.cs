using UnityEngine;

public class GetHandData
{
    private OVRSkeleton right_hand;
    // 角度のデータを格納する16自由度
    public float[] hand_data;
    public GetHandData(OVRSkeleton right_hand)
    {
        this.right_hand = right_hand;
        hand_data = new float[16];
    }

    public void Update()
    {
        hand_data[0] = GetAngle((int)OVRSkeleton.BoneId.Hand_Thumb3, 2);
        hand_data[1] = GetAngle((int)OVRSkeleton.BoneId.Hand_Thumb2, 2);
        hand_data[2] = GetAngle((int)OVRSkeleton.BoneId.Hand_Thumb1, 2);
        hand_data[3] = GetAngle((int)OVRSkeleton.BoneId.Hand_Thumb1, 1);
        hand_data[4] = GetAngle((int)OVRSkeleton.BoneId.Hand_Index2, 2);
        hand_data[5] = GetAngle((int)OVRSkeleton.BoneId.Hand_Index1, 2);
        hand_data[6] = GetAngle((int)OVRSkeleton.BoneId.Hand_Index1, 1);
        hand_data[7] = GetAngle((int)OVRSkeleton.BoneId.Hand_Middle2, 2);
        hand_data[8] = GetAngle((int)OVRSkeleton.BoneId.Hand_Middle1, 2);
        hand_data[9] = GetAngle((int)OVRSkeleton.BoneId.Hand_Middle1, 1);
        hand_data[10] = GetAngle((int)OVRSkeleton.BoneId.Hand_Ring2, 2);
        hand_data[11] = GetAngle((int)OVRSkeleton.BoneId.Hand_Ring1, 2);
        hand_data[12] = GetAngle((int)OVRSkeleton.BoneId.Hand_Ring1, 1);
        hand_data[13] = GetAngle((int)OVRSkeleton.BoneId.Hand_Pinky2, 2);
        hand_data[14] = GetAngle((int)OVRSkeleton.BoneId.Hand_Pinky1, 2);
        hand_data[15] = GetAngle((int)OVRSkeleton.BoneId.Hand_Pinky1, 1);
    }

    float GetAngle(int bone_id, int axis = 2)
    {
        float deg = right_hand.Bones[bone_id].Transform.localRotation.eulerAngles[axis];
        if (deg > 180)
        {
            deg -= 360;
        }
        return deg;
    }
}

/*
親指	5	付け根側から 0, 1, 2, 3, tip
人差し指	4	付け根側から 1, 2, 3, tip
中指	4	付け根側から 1, 2, 3, tip
薬指	4	付け根側から 1, 2, 3, tip
小指	5	付け根側から 0, 1, 2, 3, tip
*/