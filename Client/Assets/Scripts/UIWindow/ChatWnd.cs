/****************************************************
    文件：ChatWnd.cs
	功能：聊天界面
*****************************************************/

using PEProtocol;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class ChatWnd : WindowRoot {
    public InputField iptChat;
    public Text txtChat;
    public Image imgWorld;
    public Image imgGuild;
    public Image imgFriend;

    private int chatType;
    private List<string> chatLst = new List<string>();

    protected override void InitWnd() {
        base.InitWnd();

        chatType = 0;

        RefreshUI();
    }

    public void AddChatMsg(string name, string chat) {
        chatLst.Add(Constants.Color(name + "：", TxtColor.Blue) + chat);
        if (chatLst.Count > 12) {
            chatLst.RemoveAt(0);
        }
        if (GetWndState()) {
            RefreshUI();
        }
    }

    private void RefreshUI() {
        if (chatType == 0) {
            string chatMsg = "";
            for (int i = 0; i < chatLst.Count; i++) {
                chatMsg += chatLst[i] + "\n";
            }
            SetText(txtChat, chatMsg);

            SetSprite(imgWorld, "ResImages/btntype1");
            SetSprite(imgGuild, "ResImages/btntype2");
            SetSprite(imgFriend, "ResImages/btntype2");
        }
        else if (chatType == 1) {
            SetText(txtChat, "尚未加入公会");
            SetSprite(imgWorld, "ResImages/btntype2");
            SetSprite(imgGuild, "ResImages/btntype1");
            SetSprite(imgFriend, "ResImages/btntype2");
        }
        else if (chatType == 2) {
            SetText(txtChat, "暂无好友信息");
            SetSprite(imgWorld, "ResImages/btntype2");
            SetSprite(imgGuild, "ResImages/btntype2");
            SetSprite(imgFriend, "ResImages/btntype1");
        }
    }

    private bool canSend = true;
    public void ClickSendBtn() {
        if (!canSend) {
            GameRoot.AddTips("聊天消息每5秒钟才能发送一条");
            return;
        }

        if (iptChat.text != null && iptChat.text != "" && iptChat.text != " ") {
            if (iptChat.text.Length > 12) {
                GameRoot.AddTips("输入信息不能超过12个字");
            }
            else {
                //发送网络消息到服务器
                GameMsg msg = new GameMsg {
                    cmd = (int)CMD.SndChat,
                    sndChat = new SndChat {
                        chat = iptChat.text
                    }
                };
                iptChat.text = "";
                netSvc.SendMsg(msg);
                canSend = false;

                timerSvc.AddTimeTask((int tid) => {
                    canSend = true;
                }, 5, PETimeUnit.Second);
            }
        }
        else {
            GameRoot.AddTips("尚未输入聊天信息");
        }
    }
    public void ClickWorldBtn() {
        audioSvc.PlayUIAudio(Constants.UIClickBtn);
        chatType = 0;
        RefreshUI();
    }
    public void ClickGuildBtn() {
        audioSvc.PlayUIAudio(Constants.UIClickBtn);
        chatType = 1;
        RefreshUI();
    }
    public void ClickFriendBtn() {
        audioSvc.PlayUIAudio(Constants.UIClickBtn);
        chatType = 2;
        RefreshUI();
    }
    public void ClickCloseBtn() {
        audioSvc.PlayUIAudio(Constants.UIClickBtn);
        chatType = 0;
        SetWndState(false);
    }
}