using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

// ESPに対してUDPでFloat32の配列を送信、受信する
public class UDPReadWrite
{
    private string ip; // 送信先IPアドレス
    private int sendPort; // 送信用ポート番号
    private int receivePort; // 受信用ポート番号
    private const int data_num = 5; // 送信するデータの数
    public float[] send_data = new float[data_num]; // 送信するデータ
    public float[] receive_data = new float[data_num]; // 受信するデータ
    private UdpClient udpSend;
    private UdpClient udpReceive;
    private byte[] sendBytes = new byte[4 * data_num + 1]; // 送信するデータのバイト列 float32は4byte
    public byte mode = 0; // 送信するデータのモード 0: 接続テスト，1: 通常動作, 2: サスペンド
    public byte receive_mode = 0; // 受信するデータのモード 0: 接続テスト，1: 通常動作, 2: サスペンド
    private bool isRunning = false; // スレッドの状態管理
    private Task sendTask; // 非同期タスク
    private Task receiveTask; // 非同期タスク

    // コンストラクタ
    public UDPReadWrite(string ip, int sendPort, int receivePort)
    {
        this.ip = ip;
        this.sendPort = sendPort;
        this.receivePort = receivePort;
    }

    ~UDPReadWrite()
    {
        Stop(); // デストラクタで送信停止
        udpSend?.Close();
        udpReceive?.Close();
    }

    public void Connect()
    {
        if (udpSend != null)
        {
            udpSend.Close();
        }
        udpSend = new UdpClient();
        udpSend.Connect(ip, sendPort);

        if (udpReceive != null)
        {
            udpReceive.Close();
        }
        udpReceive = new UdpClient(receivePort);
        Debug.Log("Connected to " + ip + ":" + sendPort);
    }

    // UDP送信を非同期で開始
    public void Start()
    {
        if (isRunning)
        {
            return;
        }
        isRunning = true;
        sendTask = Task.Run(SendLoop); // 別スレッドで実行
        receiveTask = Task.Run(ReceiveLoop); // 別スレッドで実行
    }

    // UDP送信を停止
    public void Stop()
    {
        isRunning = false;
        sendTask?.Wait(); // タスクが終了するのを待機
        receiveTask?.Wait(); // タスクが終了するのを待機
    }

    // 非同期で送信・受信ループを実行
    private async Task SendLoop()
    {
        while (isRunning)
        {
            Send(); // 毎回送信処理を実行
            await Task.Delay(1); // 適切な間隔で待機
        }
    }

    private async Task ReceiveLoop()
    {
        while (isRunning)
        {
            await Receive(); // 毎回受信処理を実行
        }
    }

    // データを更新して送信
    public void Send()
    {
        if (udpSend == null)
        {
            return;
        }
        sendBytes[0] = mode;
        // データをバイト列に変換
        for (int i = 0; i < data_num; i++)
        {
            byte[] temp = BitConverter.GetBytes(send_data[i]);
            for (int j = 0; j < 4; j++)
            {
                sendBytes[i * 4 + j + 1] = temp[j];
            }
        }
        // データを送信（非同期）
        udpSend.SendAsync(sendBytes, sendBytes.Length);
    }

    // データを受信して配列に格納
    public async Task Receive()
    {
        if (udpReceive == null)
        {
            return;
        }
        UdpReceiveResult result;
        try
        {
            result = await udpReceive.ReceiveAsync();
        }
        catch (ObjectDisposedException)
        {
            return; // クライアントが閉じている場合はリターン
        }
        byte[] receiveBytes = result.Buffer;
        if (receiveBytes.Length >= 4 * data_num + 1) // +1 because of the mode byte
        {
            receive_mode = receiveBytes[0];
            for (int i = 0; i < data_num; i++)
            {
                receive_data[i] = BitConverter.ToSingle(receiveBytes, 1 + i * 4);
            }
        }
    }
}