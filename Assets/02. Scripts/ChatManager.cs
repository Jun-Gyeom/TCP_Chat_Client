using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatManager : MonoBehaviour
{
    public TCPChatClient tcpChatClient;
    public GameObject chatMessagePrefab; // 채팅 메시지 UI 오브젝트 (Text 또는 TextMeshPro 텍스트 프리팹)
    public Transform chatContent; // 채팅 메시지가 추가될 ScrollView의 Content 오브젝트
    public TMP_InputField chatInput; // 사용자가 메시지를 입력하는 InputField
    public ScrollRect scrollRect; 
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            EnterSendButton();
        }
    }

    public void EnterSendButton()
    {
        if (!string.IsNullOrEmpty(chatInput.text))
        {
            string newMessage = $"You: {chatInput.text}";

            // 서버에 전송 
            tcpChatClient.SendMessageToServer($"{tcpChatClient.clientName}: {chatInput.text}");
        
            DisplayMessage(newMessage);
            
            chatInput.text = ""; // 입력 필드 비우기
            chatInput.ActivateInputField();
        }
    }
    
    // 채팅 UI에 메시지 추가하는 함수
    public void DisplayMessage(string message)
    {
        GameObject messageObj = Instantiate(chatMessagePrefab, chatContent); // 메시지 프리팹 생성
        TMP_Text messageText = messageObj.GetComponent<TMP_Text>(); // 또는 TextMeshProUGUI

        messageText.text = message; // 메시지 내용 설정

        // 스크롤 뷰가 항상 최신 메시지로 스크롤되도록 설정
        Canvas.ForceUpdateCanvases(); // Canvas 업데이트 강제
        scrollRect.verticalNormalizedPosition = 0f; // 스크롤을 가장 아래로 내리기
    }
}
