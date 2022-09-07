using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PEProtocol;
using System.Text;

public class FriendWnd : WindowRoot
{
    public InputField iptFriend;//好友申请请求
    public Text friendtxt;
    public Text frientxtacc;
    public GameObject frdacc;

    private List<string> friendLst = new List<string>();//好友信息列表  最大限制设置好友为12个
    private List<string> frdnameLst = new List<string>();//存储好友姓名列表

    protected override void InitWnd()
    {
        base.InitWnd();
        SetActive(frdacc, false);

        RefreshUI();
    }
    //重构逻辑 好友的显示用一个单独的预制体动态加载（请求通过时则加载一条然后设置对应预制体信息）显示其信息 以及删除时只需要发送网络消息然后删除对应数据库数据  
    public void RefreshUI()
    {
        string friendMsg = "";//好友信息显示
        for (int i = 0; i < friendLst.Count; i++)
        {
            friendMsg += friendLst[i] + "\n";//好友信息加N换行
        }
        SetText(friendtxt, friendMsg);
        SetActive(frdacc, false);

    }
    //增加好友成功处理函数
    public void AddFriendMsg(string frdname, int lv, int power)
    {
        friendLst.Add(Constants.Color(frdname, TxtColor.Blue) + "        等级:" + lv + "        战力:" + power);
        frdnameLst.Add(frdname);
        if (friendLst.Count >= 12)
        {
            GameRoot.AddTips("好友已达上限");

        }
        //处理完成后调用自身窗口刷新ui显示
        if (GetWndState())
        {
            RefreshUI();
        }
    }

    private string name2;
    #region 第二次发送消息（回应）

    //推送给被申请的人
    public void AddFriendTips(string myname)
    {
        SetActive(frdacc, true);
        SetText(frientxtacc, myname + "请求添加你为好友");
        name2 = myname;
    }
    public void ClickAcceptBtn()//接受好友请求按钮
    {
        GameMsg msg = new GameMsg
        {
            cmd = (int)CMD.SndFriend,
            sndFriend = new SndFriend
            {
                isAccept = true,
                frdname = name2
            }
        };
        netSvc.SendMsg(msg);//发送网络消息
        RefreshUI();
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
        RefreshUI();
    }
    #endregion


    #region 第一次发送申请（发送好友请求）
    private bool canSend = true;//好友申请时间限制标志变量
    //这里逻辑没问题  问题出在第二段发送取得请求 
    public void ClickSendBtn()//发送好友请求按钮 第一次
    {
        if (canSend == false)
        {
            GameRoot.AddTips("好友申请每5秒钟才能发送一条");
            return;
        }
        PlayerData pd = GameRoot.Instance.PlayerData;//引入玩家数据
        if (iptFriend != null && iptFriend.text != "" && iptFriend.text != " ")
        {
            if (iptFriend.text.Length > 4)
            {
                GameRoot.AddTips("输入的用户名称不合法");

            }
            else if (iptFriend.text == pd.name)
            {
                GameRoot.AddTips("您无法添加自己为好友");
            }
            else if (frdnameLst.Contains(iptFriend.text))
            {
                GameRoot.AddTips("您已经添加过该好友");
            }
            else
            {
                //发送服务器请求
                GameMsg msg = new GameMsg
                {
                    cmd = (int)CMD.ReqFriend,
                    reqFriend = new ReqFriend
                    {
                        myname = pd.name,
                        frdname = iptFriend.text

                    }
                };
                iptFriend.text = "";//将好友名字清空            
                netSvc.SendMsg(msg);//发送网络消息
                //将标志变量设置为false
                canSend = false;

                //使用定时服务设置标志变量
                timerSvc.AddTimeTask((int tid) =>
                {
                    canSend = true;
                }, 5, PETimeUnit.Second);
            }
        }
        else
        {

            GameRoot.AddTips("尚未输入好友名称");
        }
    }

    #endregion


    public void ClickCloseBtn()
    {
        audioSvc.PlayUIAudio(Constants.UIClickBtn);
        RefreshUI();//添加成功时刷新ui 刷新好友显示
        SetWndState(false);
    }
}

