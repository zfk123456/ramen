using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PEProtocol;
using System.Text;
public class FriendAccWnd:WindowRoot
{

    public Button acceptBtn;
    public Button refuseBtn;
    public Text friendtxt;
    protected override void InitWnd()
    {
        base.InitWnd();
        
    }

    public void AddFriendTips(string myname)
    {
        InitWnd();
        SetText(friendtxt,myname+"请求添加你为好友");
    }
    public void ClickAcceptBtn()//接受好友请求按钮
    {
        GameMsg msg = new GameMsg
        {
            cmd = (int)CMD.SndFriend,
            sndFriend = new SndFriend
            {
                isAccept = true,
            }
        };
        netSvc.SendMsg(msg);//发送网络消息
        Close();
    }

public void ClickRefuseBtn()//拒绝好友请求按钮
{
    GameMsg msg = new GameMsg
    {
        cmd = (int)CMD.SndFriend,
        sndFriend = new SndFriend
        {
            isAccept = false,
        }
    };
        netSvc.SendMsg(msg);//发送网络消息
        Close();
}

public void Close()
    {
        audioSvc.PlayUIAudio(Constants.UIExtenBtn);
        SetWndState(false);
    }
}
