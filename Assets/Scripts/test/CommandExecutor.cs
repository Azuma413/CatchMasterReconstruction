using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Renci.SshNet;
using TMPro;
using UnityEngine.UI;
using System;

public class CommandExecutor : MonoBehaviour
{
    [SerializeField] private string command;
    private string kill_command;
    private TextMeshProUGUI text;
    private Button button;
    bool execute_flag = false;
    void Start()
    {
        if (string.IsNullOrEmpty(command))
        {
            Debug.LogError("Please set command in the inspector.");
            return;
        }
        kill_command = "pkill -f \"" + command + "\"";
        text = GetComponentInChildren<TextMeshProUGUI>();
        text.text = "Execute";
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
        try{
            if (execute_flag){
                SshCommand cmd = SSHConnector.ssh.RunCommand(kill_command);
                cmd.Execute();
                text.text = "Execute";
                execute_flag = false;
            }
            else{
                if(ExecuteCommand(command)){
                    text.text = "Stop";
                    execute_flag = true;
                }else{
                    text.text = "Failed";
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error: " + e.Message);
            text.text = "Failed";
        }
    }

    bool ExecuteCommand(string command)
    {
        if (SSHConnector.shellStream != null)
        {
            SSHConnector.shellStream.WriteLine(command);
            Debug.LogFormat("[CMD] {0}", command);
            return true;
        }
        else
        {
            Debug.LogError("Shell stream is not available.");
            return false;
        }
    }
}
