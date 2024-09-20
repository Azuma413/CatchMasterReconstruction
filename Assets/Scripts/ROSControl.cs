using System.Linq;
using ROS2;
using UnityEngine;

public class ROSControl
{
    private ROS2UnityComponent ros2Unity;
    private ROS2Node ros2Node;
    private IPublisher<std_msgs.msg.Float32> wrist_deg_pub;
    private IPublisher<std_msgs.msg.Float32MultiArray> xl430_deg_pub;
    private IPublisher<std_msgs.msg.Float32MultiArray> xl320_deg_pub;
    private Mesh mesh;
    private MeshFilter mf;
    private Vector3[] positions;
    private Color[] colors;
    private int[] indices;
    // ROS2のトピックに送信するデータ
    public float wrist_deg = 0;
    public float[] xl430_deg = new float[8];
    public float[] xl320_deg = new float[8];
    public int state_message = 0; // 0: 初期状態，1: ROS実行成功，2: UDP送信成功，3: トレース開始，4: サスペンド
    public ROSControl(GameObject target)
    { // コンストラクタ
        mf = target.AddComponent<MeshFilter>();
        target.TryGetComponent(out ros2Unity);
        mesh = new Mesh() { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
    }

    public void Update()
    {
        if (ros2Unity.Ok())
        {
            if (ros2Node == null)
            {
                ros2Node = ros2Unity.CreateNode("unity_node");
                wrist_deg_pub = ros2Node.CreatePublisher<std_msgs.msg.Float32>("wrist_deg");
                xl430_deg_pub = ros2Node.CreatePublisher<std_msgs.msg.Float32MultiArray>("xl430_deg");
                xl320_deg_pub = ros2Node.CreatePublisher<std_msgs.msg.Float32MultiArray>("xl320_deg");
                ros2Node.CreateSubscription<sensor_msgs.msg.PointCloud2>("/camera/camera/depth/color/points", point_render_cb);
                ros2Node.CreateSubscription<std_msgs.msg.Int8>("state_message", state_message_cb);
                Debug.Log("Connected to ROS2");
            }
            std_msgs.msg.Float32 wrist_deg_msg = new std_msgs.msg.Float32();
            wrist_deg_msg.Data = wrist_deg;
            wrist_deg_pub.Publish(wrist_deg_msg);
            // std_msgs.msg.Float32MultiArray xl430_deg_msg = new std_msgs.msg.Float32MultiArray();
            // xl430_deg_msg.Data = xl430_deg.ToArray();
            // xl430_deg_pub.Publish(xl430_deg_msg);
            // std_msgs.msg.Float32MultiArray xl320_deg_msg = new std_msgs.msg.Float32MultiArray();
            // xl320_deg_msg.Data = xl320_deg.ToArray();
            // xl320_deg_pub.Publish(xl320_deg_msg);
        }
    }

    private void state_message_cb(std_msgs.msg.Int8 msg)
    {
        Debug.Log("Received Int8 message: " + msg.Data);
        state_message = msg.Data;
    }

    private void point_render_cb(sensor_msgs.msg.PointCloud2 msg)
    {
        try{
            Debug.Log("Received PointCloud2 message: " + msg.Width + "x" + msg.Height + " points.");
            if (msg.Data.Length == 0)
            {
                Debug.Log("Received empty PointCloud2 message.");
                return;
            }
            uint point_size = msg.Width * msg.Height;
            positions = new Vector3[point_size];
            colors = new Color[point_size];
            indices = new int[point_size];
            byte[] array_data = msg.Data.ToArray(); // byte配列に変換
            for (uint i = 0; i < point_size; i++)
            {
                uint offset = i * msg.Point_step;
                positions[i].Set(
                    -System.BitConverter.ToSingle(array_data, (int)(offset + msg.Fields[0].Offset)),
                    System.BitConverter.ToSingle(array_data, (int)(offset + msg.Fields[1].Offset)),
                    System.BitConverter.ToSingle(array_data, (int)(offset + msg.Fields[2].Offset))
                );
                colors[i].r = array_data[(int)(offset + msg.Fields[3].Offset + 2)]/255f;
                colors[i].g = array_data[(int)(offset + msg.Fields[3].Offset + 1)]/255f;
                colors[i].b = array_data[(int)(offset + msg.Fields[3].Offset)]/255f;
                colors[i].a = 1f;
                indices[i] = (int)i;
            }
            mesh.Clear();
            mesh.vertices = positions;
            mesh.colors = colors;
            mesh.SetIndices(indices, MeshTopology.Points, 0);
            mf.mesh = mesh;
        }catch(System.Exception e){
            Debug.Log("Error: " + e.Message);
        }
    }
}