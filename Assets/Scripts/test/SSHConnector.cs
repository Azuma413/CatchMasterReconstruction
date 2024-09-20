using UnityEngine;
using System;
using Renci.SshNet;
using UnityEngine.UI;
using System.Text;
using TMPro;
public class SSHConnector : MonoBehaviour
{
    [SerializeField] private string host_name;
    [SerializeField] private string user_name;
    [SerializeField] private string password;
    [SerializeField] private TextMeshProUGUI log;
    private TextMeshProUGUI text;
    private Button button;
    private int port = 22;
    public int max_lines = 5;
    public static SshClient ssh;
    public static ShellStream shellStream;
    StringBuilder output;
    void Start()
    {
        if (String.IsNullOrEmpty(host_name) || String.IsNullOrEmpty(user_name) || String.IsNullOrEmpty(password))
        {
            Debug.LogError("Please set host_name, user_name, password in the inspector.");
            return;
        }
        button = GetComponent<Button>();
        text = GetComponentInChildren<TextMeshProUGUI>();
        text.text = "SSH";
        button.onClick.AddListener(OnClick);
        output = new StringBuilder();
    }

    void OnDestroy()
    {
        if (ssh != null)
        {
            ssh.Disconnect();
            ssh.Dispose();
        }
    }

    public void OnClick()
    {
        text.text = "Connecting...";
        try
        {
            ConnectionInfo info = new ConnectionInfo(host_name, port, user_name, new PasswordAuthenticationMethod(user_name, password));
            ssh = new SshClient(info);
            ssh.Connect();
            if (ssh.IsConnected)
            {
                Debug.Log("SSH connection established.");
                text.text = "Established";
                if (shellStream == null)
                {
                    shellStream = ssh.CreateShellStream("xterm", 80, 24, 800, 600, 1024);
                }
            }
            else
            {
                Debug.LogError("SSH connection failed.");
                text.text = "Failed";
                return;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error: " + e.Message);
            text.text = "Failed";
            return;
        }
    }

    void Update()
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
