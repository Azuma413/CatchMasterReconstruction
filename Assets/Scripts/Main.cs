using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System.Collections.Generic;
// メインスレッド上の処理を管理するクラス

public class Main : MonoBehaviour
{
    // Canvas上のボタンを取得する
    [SerializeField] private Button launch_button;
    [SerializeField] private Button interrupt_button;
    [SerializeField] private Button stop_button;
    // ログを表示するためのTMP
    [SerializeField] private TextMeshProUGUI log;
    [SerializeField] private TextMeshProUGUI ssh_log;
    // 指の角度を取得するためのOVRSkeleton
    [SerializeField] private OVRSkeleton right_hand;
    // IK関連
    [SerializeField] private Transform trace_target; // 追従する対象
    [SerializeField] private Transform endEffector; // 手先の物体
    [SerializeField] private Transform root; // ロボットアームの根元
    [SerializeField] private GameObject pointCloudTarget; // 点群を表示する基点
    [SerializeField] private Transform[] joints; // ロボットアームの各関節
    [SerializeField] private Transform[] shadow_joints;
    [SerializeField] private AudioClip[] audioClips; // 音源の登録 最大12個
    private string[] log_text = new string[12] {
        "SSH接続を開始します",
        "SSH接続を終了します",
        "ROSを実行します",
        "ROSを終了します",
        "UDP通信を開始します",
        "成功しました",
        "失敗しました",
        "Systemを起動します",
        "System停止します",
        "トレースを開始します",
        "トレースを中断します",
        "こんにちは"
    };
    private AudioSource audioSource;
    // private GameObject sphere; // 手先の位置を示す球
    private int launch_state = 0; // 0: 未実行, 1: 実行中, 2: 完了
    private bool trace_flag = false;
    private bool after_trace = false;
    private ROSControl ros_control;
    private SSHControl ssh_control;
    public string host_name = "192.168.0.50";
    public string user_name = "kikaiken2";
    public string password = "kikaiken";
    public string command;
    public string kill_command = "pkill -f ros2";
    private GetHandData get_hand_data;
    // private SendForESP send_for_esp;
    private UDPReadWrite udp_control;
    public string ip = "192.168.0.255";
    // public string ip = "192.168.0.77";
    public int sendPort = 12345;
    public int receivePort = 8888;
    private ShadowArm shadow_arm;
    private IKControl ik_control;
    public int max_lines = 11;
    private List<string> output;
    public float transparent = 0.3f;
    public const float detect_range = 0.15f; // 手先の位置がこの範囲内に収まるとIKを開始する。
    // Start is called before the first frame update
    void Start()
    {
        // ボタンの色を変更
        launch_button.GetComponent<Image>().color = Color.white;
        interrupt_button.GetComponent<Image>().color = Color.gray;
        stop_button.GetComponent<Image>().color = Color.gray;
        // ボタンが押されたときの処理を登録
        launch_button.onClick.AddListener(LaunchOnClick);
        interrupt_button.onClick.AddListener(InterruptOnClick);
        stop_button.onClick.AddListener(StopOnClick);
        // ROSControlクラスを取得
        ros_control = new ROSControl(pointCloudTarget);
        // SSHControlクラスを取得
        ssh_control = new SSHControl(host_name, user_name, password, ssh_log, command, kill_command);
        // GetHandDataクラスを取得
        get_hand_data = new GetHandData(right_hand);
        // SendForESPクラスを取得
        udp_control = new UDPReadWrite(ip, sendPort, receivePort);
        // send_for_esp = new SendForESP(ip, port);
        // IKControlクラスを取得
        ik_control = new IKControl(trace_target, endEffector, root, joints);
        // ShadowArmクラスを取得
        shadow_arm = new ShadowArm(shadow_joints);
        // 球の色を変更
        // sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        // Material material = sphere.GetComponent<Renderer>().material;
        // material.shader = Shader.Find("Standard"); // Standard Shader を使用
        // material.SetFloat("_Mode", 3); // Transparent モード
        // material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        // material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        // material.SetInt("_ZWrite", 0);
        // material.DisableKeyword("_ALPHATEST_ON");
        // material.EnableKeyword("_ALPHABLEND_ON");
        // material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        // material.renderQueue = 3000; // Transparentのレンダリングキュー
        // sphere.GetComponent<Renderer>().material.color = new Color(1.0f, 0.0f, 0.0f, transparent); // 赤
        // sphere.transform.localScale = new Vector3(detect_range*2, detect_range*2, detect_range*2);
        // 音源を作成
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialize = false; // 空間化を無効にする
        // delayを入れて音を再生する
        output = new List<string>();
        UpdateLog(log_text[11]);
        audioSource.clip = audioClips[11];
        audioSource.PlayDelayed(2.0f);
    }

    // void Update()
    // {
    //     log.text =  get_hand_data.hand_data[0].ToString("F2") + "," +
    //                 get_hand_data.hand_data[1].ToString("F2") + "," +
    //                 get_hand_data.hand_data[2].ToString("F2") + "," +
    //                 get_hand_data.hand_data[3].ToString("F2") + "\n" +
    //                 get_hand_data.hand_data[4].ToString("F2") + "," +
    //                 get_hand_data.hand_data[5].ToString("F2") + "," +
    //                 get_hand_data.hand_data[6].ToString("F2") + "\n" +
    //                 get_hand_data.hand_data[7].ToString("F2") + "," +
    //                 get_hand_data.hand_data[8].ToString("F2") + "," +
    //                 get_hand_data.hand_data[9].ToString("F2") + "\n" +
    //                 get_hand_data.hand_data[10].ToString("F2") + "," +
    //                 get_hand_data.hand_data[11].ToString("F2") + "," +
    //                 get_hand_data.hand_data[12].ToString("F2") + "\n" +
    //                 get_hand_data.hand_data[13].ToString("F2") + "," +
    //                 get_hand_data.hand_data[14].ToString("F2") + "," +
    //                 get_hand_data.hand_data[15].ToString("F2");
    //     get_hand_data.Update();
    // }
    void Update()
    {
        // Sphereの位置を更新
        // sphere.transform.position = endEffector.position;

        ros_control.Update();
        ssh_control.Update();
        if (trace_flag)
        { // トレース中の場合
            ik_control.Update();
            get_hand_data.Update();
            // send_for_esp.mode = 1; // 通常動作
            udp_control.mode = 1; // 通常動作
        }else{ // トレース中でない場合
            // send_for_esp.mode = 2; // サスペンド
            udp_control.mode = 2; // サスペンド
            if (launch_state != 2){ // Systemが起動していない場合は何もしない
                return;
            }else{ // Systemが起動している場合
                if (after_trace && !ik_control.CheckDistance(detect_range*1.5f)){
                    after_trace = false;
                }
                if (!after_trace && ik_control.CheckDistance(detect_range))
                { // 手先が目標位置に近づいたらトレースを開始
                    Debug.Log("Start Trace");
                    PlaySound(9); // トレースを開始します
                    trace_flag = true;
                    interrupt_button.GetComponent<Image>().color = Color.yellow;
                    // sphere.GetComponent<Renderer>().material.color = new Color(0.0f, 1.0f, 0.0f, transparent); // 緑
                }
            }
        }

        // ik_control.Update();
        // send_for_esp.mode = 1;

        ros_control.wrist_deg = (float)ik_control.theta[4]*180.0f/Mathf.PI; // degree
        ros_control.xl430_deg = new float[8] { // 0,1,4,5,7,8,10,13
            get_hand_data.hand_data[0],
            get_hand_data.hand_data[1],
            get_hand_data.hand_data[4],
            get_hand_data.hand_data[5],
            get_hand_data.hand_data[7],
            get_hand_data.hand_data[8],
            get_hand_data.hand_data[10],
            get_hand_data.hand_data[13]
        };
        ros_control.xl320_deg = new float[8] { // 2,3,6,9,11,12,14,15
            get_hand_data.hand_data[2],
            get_hand_data.hand_data[3],
            get_hand_data.hand_data[6],
            get_hand_data.hand_data[9],
            get_hand_data.hand_data[11],
            get_hand_data.hand_data[12],
            get_hand_data.hand_data[14],
            get_hand_data.hand_data[15]
        };
        // send_for_esp.send_data = new float[5] {
        udp_control.send_data = new float[5] {
            (float)ik_control.theta[2]*180.0f/Mathf.PI, // CyberGear 1
            (float)ik_control.theta[1]*180.0f/Mathf.PI, // CyberGear 2
            (float)ik_control.theta[3]*180.0f/Mathf.PI, // GM6020 1
            (float)ik_control.theta[5]*180.0f/Mathf.PI, // GM6020 2
            (float)ik_control.theta[0]*180.0f/Mathf.PI // m3508
        };
        shadow_arm.theta[0] = udp_control.receive_data[4]*Mathf.PI/180.0; // m3508
        shadow_arm.theta[1] = udp_control.receive_data[1]*Mathf.PI/180.0; // CyberGear 2
        shadow_arm.theta[2] = udp_control.receive_data[0]*Mathf.PI/180.0; // CyberGear 1
        shadow_arm.theta[3] = udp_control.receive_data[2]*Mathf.PI/180.0; // GM6020 1
        shadow_arm.theta[4] = ik_control.theta[4]; // dynamixel
        shadow_arm.theta[5] = udp_control.receive_data[3]*Mathf.PI/180.0; // GM6020 2
        shadow_arm.Update();
    }

    void LaunchOnClick()
    { // 通常時にボタンが押されると，起動を開始する
        if (launch_state == 0)
        {
            PlaySound(7); // Systemを起動します
            launch_button.GetComponent<Image>().color = Color.yellow;
            launch_state = 1;
            StartCoroutine(LaunchTask()); // ここでフリーズする
        }
    }

    void InterruptOnClick()
    { // トレース中にボタンが押されると，トレースを中断する
        if (trace_flag)
        {
            PlaySound(10); // トレースを中断します
            trace_flag = false;
            after_trace = true;
            interrupt_button.GetComponent<Image>().color = Color.gray;
            // sphere.GetComponent<Renderer>().material.color = new Color(1.0f, 1.0f, 0.0f, transparent); // 黄
        }
    }

    void StopOnClick()
    { // Launch後にボタンが押されると，プログラムを停止する
        if (launch_state == 2)
        {
            PlaySound(8); // System停止します
            trace_flag = false;
            interrupt_button.GetComponent<Image>().color = Color.gray;
            launch_button.GetComponent<Image>().color = Color.white;
            stop_button.GetComponent<Image>().color = Color.gray;
            // sphere.GetComponent<Renderer>().material.color = new Color(1.0f, 0.0f, 0.0f, transparent); // 赤
            launch_state = 0;
            StartCoroutine(StopTask());
        }
    }

    IEnumerator LaunchTask()
    { // プログラムを起動する処理
        int count = 0;
        yield return new WaitForSeconds(2.0f);
        PlaySound(0); // SSH接続を開始します
        yield return new WaitForSeconds(2.0f);
        // 1. SSH接続
        if (!ssh_control.Connect())
        {
            PlaySound(6); // 失敗しました
            launch_button.GetComponent<Image>().color = Color.white;
            launch_state = 0;
            yield break;
        }
        PlaySound(2); // ROSを実行します
        yield return new WaitForSeconds(2.0f);
        // 2. PC上でROS2を起動
        ssh_control.ExecuteCommand();
        while (ros_control.state_message == 0) // 0以外ならOK
        {
            count++;
            if (count > 10)
            {
                PlaySound(6); // 失敗しました
                ssh_control.KillCommand();
                ssh_control.Disconnect();
                launch_button.GetComponent<Image>().color = Color.white;
                launch_state = 0;
                yield break;
            }
            yield return new WaitForSeconds(0.5f); // 0.5秒待つ
        }
        PlaySound(4); // UDP通信を開始します
        yield return new WaitForSeconds(2.0f);
        // 3. ESP32との通信を開始
        // send_for_esp.Connect();
        udp_control.Connect();
        // send_for_esp.StartSending();
        udp_control.Start();
        count = 0;
        while (ros_control.state_message < 2) // 2以降ならOK
        {
            count++;
            if (count > 10)
            {
                PlaySound(6); // 失敗しました
                // send_for_esp.StopSending();
                udp_control.Stop();
                ssh_control.KillCommand();
                ssh_control.Disconnect();
                launch_button.GetComponent<Image>().color = Color.white;
                launch_state = 0;
                // send_for_esp.mode = 0;
                udp_control.mode = 0;
                yield break;
            }
            yield return new WaitForSeconds(0.5f); // 0.5秒待つ
        }
        PlaySound(5); // 成功しました
        // 全ての処理が完了したら，ボタンの色を変更
        launch_button.GetComponent<Image>().color = Color.green;
        stop_button.GetComponent<Image>().color = Color.red;
        // sphere.GetComponent<Renderer>().material.color = new Color(1.0f, 1.0f, 0.0f, transparent); // 黄
        launch_state = 2;
        yield return null;
    }

    IEnumerator StopTask()
    { // プログラムを停止する処理
        // send_for_esp.mode = 0; // 接続テスト
        udp_control.mode = 0; // 接続テスト
        yield return new WaitForSeconds(2.0f);
        // 1. UDPのモードをサスペンドに変更
        // send_for_esp.StopSending();
        udp_control.Stop();
        // 2. PC上でROS2を停止
        PlaySound(3); // ROSを終了します
        ssh_control.KillCommand();
        yield return new WaitForSeconds(2.0f);
        // 3. SSH接続を切断
        PlaySound(1); // SSH接続を終了します
        ssh_control.Disconnect();
        yield return null;
    }

    void PlaySound(int index)
    {
        UpdateLog(log_text[index]);
        audioSource.PlayOneShot(audioClips[index]);
    }

    void UpdateLog(string text)
    {
        output.Add(text);
        if (output.Count > max_lines)
        {
            output.RemoveAt(0);
        }
        StringBuilder sb = new StringBuilder();
        foreach (string line in output)
        {
            sb.AppendLine(line);
        }
        log.text = sb.ToString();
    }

    // 終了時にSSHを切断
    void OnApplicationQuit()
    {
        ssh_control.KillCommand();
        ssh_control.Disconnect();
    }
}
