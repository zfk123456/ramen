using System.Collections.Generic;
using PEProtocol;

public class FriendSys
{
    private static FriendSys instance = null;
    public static FriendSys Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new FriendSys();
            }
            return instance;
        }
    }
    private CacheSvc cacheSvc = null;
    public void Init()
    {
        cacheSvc = CacheSvc.Instance;
        PECommon.Log("FriendSys Init Done.");
    }
    //第一次请求传入的pack信息为申请者 发送至被申请人   被申请人在客户端进行逻辑上的处理按钮来发送第二次请求信息  
    public void ReqFriend(MsgPack pack)
    {

        ReqFriend data = pack.msg.reqFriend;//转接申请好友请求数据
        ServerSession frdsession = AnalysisFrdname(data.frdname);//通过在线函数获取当前目标好友的session
        //此处还应拥有一个判定条件才能发送给对应玩家即如果找不到在线的frdsession或者不存在则发送一个网络回应消息RspFriend给发送者即this.sesion告诉其不存在或者不在线
                                                                 //todo
        if (frdsession != null)
        {
            GameMsg msg1 = new GameMsg//发送给对应玩家的请求消息
            {
                cmd = (int)CMD.PshFriend,
                pshFriend = new PshFriend
                {
                    myname = data.myname,
                }
            };
            frdsession.SendMsg(msg1);//发送请求1给被申请玩家 //需要加定在线条件 否则报空
        }
        else
        {
            GameMsg msg2 = new GameMsg
            {
                cmd = (int)CMD.RspFriend,
            };
            msg2.err =((int)ErrorCode.NameIsNoExist);
            pack.session.SendMsg(msg2);
        }





    }

    //第二次请求信息 传入的pack信息为被申请者
    public void ReqFriendTarget(MsgPack pack)//此处pack为被请求者的pack  
    {
        SndFriend data = pack.msg.sndFriend;//转接被申请好友是否接受数据以及回应的申请者名字数据
        bool isAccept = data.isAccept;//转接其判断标准
       // ReqFriend data = pack.msg.reqFriend;//转接申请好友请求数据
        ServerSession frdsession =AnalysisFrdname(data.frdname);//通过在线函数获取当前申请者session

        GameMsg msg = new GameMsg//返回给发送玩家的回应消息
        {
            cmd = (int)CMD.RspFriend,

        };
        //安全判断
        if (isAccept)
        {
            PlayerData frd = cacheSvc.GetPlayerDataBySession(pack.session);//获取被申请玩家数据
            TaskSys.Instance.CalcTaskPrgs(frd, 7);//更新添加好友任务进度

            RspFriend rspFriend = new RspFriend
            {
                frdname = frd.name,
                lv = frd.lv,
                power = PECommon.GetFightByProps(frd),

            };
            msg.rspFriend = rspFriend;
            frdsession.SendMsg(msg);
            //如果分发数据到被申请者还要额外处理一步
            PlayerData frd1 = cacheSvc.GetPlayerDataBySession(frdsession);//获取申请玩家数据
            TaskSys.Instance.CalcTaskPrgs(frd1, 7);//更新添加好友任务进度
            GameMsg msg1 = new GameMsg//返回给发送玩家的回应消息
            {
                cmd = (int)CMD.RspFriend,
                rspFriend = new RspFriend
                {
                    frdname = frd1.name,
                    lv=frd1.lv,
                    power = PECommon.GetFightByProps(frd1),
                }
            };
            pack.session.SendMsg(msg1);
        }
        else if (!isAccept)
        {
            msg.err = ((int)ErrorCode.FriendRefuse);
            frdsession.SendMsg(msg);
        }
        else
        {
            msg.err = ((int)ErrorCode.NameIsNoExist);
            frdsession.SendMsg(msg);
        }
    }


    public ServerSession AnalysisFrdname(string name)
    {
        //获得所有在线玩家数据
        Dictionary<ServerSession, PlayerData> onlineDic = cacheSvc.GetOnlineCache();
        foreach (var Item in onlineDic)
        {
            //将键值数据都取出
            PlayerData pd = Item.Value;
            ServerSession session = Item.Key;
            if (name != pd.name)
            {
                continue;
            }
            else if (name == pd.name)
            {
                return session;
            }
        }
        return null;

    }
}

