using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

// ESPに対してUDPでFloat32の配列を送信する
public class SendForESP
{
    private string ip; // 送信先IPアドレス
    private int sendPort; // 送信用ポート番号
    private int receivePort; // 受信用ポート番号
    private const int data_num = 5; // 送信するデータの数
    public float[] send_data = new float[data_num]; // 送信するデータ
    private UdpClient udp;
    private byte[] sendBytes = new byte[4 * data_num + 1]; // 送信するデータのバイト列 float32は4byte
    public byte mode = 0; // 送信するデータのモード 0: 接続テスト，1: 通常動作, 2: サスペンド
    private bool isRunning = false; // スレッドの状態管理
    private Task udpTask; // 非同期タスク

    // コンストラクタ
    public SendForESP(string ip, int sendPort, int receivePort)
    {
        this.ip = ip;
        this.sendPort = sendPort;
        this.receivePort = receivePort;
    }

    ~SendForESP()
    {
        StopSending(); // デストラクタで送信停止
        udp?.Close();
    }

    public void Connect()
    {
        if (udp != null)
        {
            udp.Close();
        }
        udp = new UdpClient();
        udp.Connect(ip, sendPort);
        Debug.Log("Connected to " + ip + ":" + sendPort);
    }

    // UDP送信を非同期で開始
    public void StartSending()
    {
        if (isRunning)
        {
            return;
        }
        isRunning = true;
        udpTask = Task.Run(SendLoop); // 別スレッドで実行
    }

    // UDP送信を停止
    public void StopSending()
    {
        isRunning = false;
        udpTask?.Wait(); // タスクが終了するのを待機
    }

    // 非同期で送信ループを実行
    private async Task SendLoop()
    {
        while (isRunning)
        {
            Update(); // 毎回送信処理を実行
            await Task.Delay(1); // 適切な間隔で待機
        }
    }

    // データを更新して送信
    public void Update()
    {
        if (udp == null)
        {
            return;
        }

        sendBytes[0] = mode;
        // データをバイト列に変換
        for (int i = 0; i < data_num; i++)
        {
            byte[] temp = System.BitConverter.GetBytes(send_data[i]);
            for (int j = 0; j < 4; j++)
            {
                sendBytes[i * 4 + j + 1] = temp[j];
            }
        }

        // データを送信（非同期）
        udp.SendAsync(sendBytes, sendBytes.Length);
    }
}
