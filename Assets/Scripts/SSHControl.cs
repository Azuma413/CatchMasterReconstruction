using UnityEngine;
using TMPro;
using Renci.SshNet;
using System.Text;
using System.Threading;

public class SSHControl
{
    private int port = 22;
    private int max_lines = 10;
    private static SshClient ssh;
    private static ShellStream shellStream;
    private StringBuilder output;
    private ConnectionInfo info;
    private TextMeshProUGUI log;
    private string command;
    private string kill_command;
    private const int ConnectTimeout = 1000; // 接続タイムアウト時間 ms 500だと誤って切断されることがある

    // Start is called before the first frame update
    public SSHControl(string host_name, string user_name, string password, TextMeshProUGUI log, string command, string kill_command)
    {
        if (string.IsNullOrEmpty(host_name) || string.IsNullOrEmpty(user_name) || string.IsNullOrEmpty(password))
        {
            Debug.LogError("Please set host_name, user_name, password in the inspector.");
            return;
        }
        output = new StringBuilder();
        info = new ConnectionInfo(host_name, port, user_name, new PasswordAuthenticationMethod(user_name, password));
        this.log = log;
        this.log.text = "";
        this.command = command;
        this.kill_command = kill_command;
    }

    // デコンストラクタ
    ~SSHControl()
    {
        if (ssh != null)
        {
            ssh.Disconnect();
            ssh.Dispose();
        }
    }
    public bool Connect()
    {
        Debug.Log("Connecting...");
        var connectionThread = new Thread(() =>
        {
            try
            {
                ssh = new SshClient(info);
                ssh.Connect();
                if (ssh.IsConnected)
                {
                    Debug.Log("SSH connection established.");
                    if (shellStream == null)
                    {
                        shellStream = ssh.CreateShellStream("xterm", 80, 24, 800, 600, 1024);
                    }
                }
                else
                {
                    Debug.LogError("SSH connection failed.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error: " + e.Message);
            }
        });

        connectionThread.Start();
        bool isConnected = connectionThread.Join(ConnectTimeout); // タイムアウトまで待機
        if (!isConnected)
        {
            Debug.LogError("SSH connection timeout.");
            connectionThread.Abort(); // スレッドを中止
            return false;
        }
        return ssh.IsConnected;
    }

    public void Disconnect()
    {
        if (shellStream != null)
        {
            shellStream.Close();
            shellStream.Dispose();
            shellStream = null;
        }
        if (ssh != null)
        {
            ssh.Disconnect();
            ssh.Dispose();
            ssh = null;
        }
    }

    public void SetCommand(string cmd)
    {
        command = cmd;
        kill_command = "pkill -f \"" + command + "\"";
    }

    public void ExecuteCommand()
    {
        if (shellStream != null)
        {
            shellStream.WriteLine(command);
            Debug.LogFormat("[CMD] {0}", command);
        }
    }

    public void KillCommand()
    {
        Debug.LogFormat("[CMD] {0}", kill_command);
        SshCommand cmd = ssh.RunCommand(kill_command);
        cmd.Execute();
    }

    // Update is called once per frame
    public void Update()
    {
        if (shellStream != null)
        {
            if (shellStream.DataAvailable)
            {
                string line = shellStream.Read();
                string[] unwantedChars = { "\u0007", "\u001B", "\u6708", "\u5408", "\u8A08" };
                foreach (string ch in unwantedChars)
                {
                    line = line.Replace(ch, "");
                }
                output.AppendLine(line);
                string[] lines = output.ToString().Split('\n');
                output.Clear();
                foreach (string l in lines)
                {
                    if (!string.IsNullOrWhiteSpace(l))
                    {
                        output.AppendLine(l);
                    }
                }
                lines = output.ToString().Split('\n');
                if (lines.Length > max_lines)
                {
                    output.Clear();
                    for (int i = lines.Length - max_lines; i < lines.Length; i++)
                    {
                        output.AppendLine(lines[i]);
                    }
                }
                log.text = output.ToString();
            }
        }
    }
}
