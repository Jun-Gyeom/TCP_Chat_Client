using UnityEngine;

public class ChatMessage
{
    public string sender;  // 발신자 이름
    public string message; // 메시지 내용
    public float timestamp; // 메시지 전송 시간

    public ChatMessage(string sender, string message)
    {
        this.sender = sender;
        this.message = message;
        this.timestamp = Time.time;  // 메시지 전송 시간 (현재 시간 기준)
    }
}
