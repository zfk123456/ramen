using System.Collections.Generic;
using System.Runtime.InteropServices;
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
    Dictionary<string, MsgPack> msgDic = new Dictionary<string, MsgPack>();//创建一个通过玩家名字存储对应发送的网络消息pack
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
        //此处还应拥有一个判定条件才能发送给对应玩家即如果找不到在线的frdsession或者不存在则发送一个网络回应消息RspFriend给发送者即this.sesion告诉其不存在或者不在线todo
        PlayerData pd = cacheSvc.GetPlayerDataBySession(pack.session);
        msgDic.Add(pd.name, pack);//发送请求时将当前发送消息的玩家名字数据以及对应发送的msgpack数据存入字典 


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
            //用于回复给发送者错误信息 
            GameMsg msg2 = new GameMsg
            {
                cmd = (int)CMD.RspFriend,
            };
            msg2.err = ((int)ErrorCode.NameIsNoExist);
            pack.session.SendMsg(msg2);
        }




    }

    //第二次请求信息 传入的pack信息为被申请者
    public void ReqFriendTarget(MsgPack pack)//此处pack为被请求者的pack  
    {
        SndFriend data = pack.msg.sndFriend;//转接被申请好友是否接受数据以及回应的申请者名字数据
        bool isAccept = data.isAccept;//转接其判断标准
                                      // ReqFriend data = pack.msg.reqFriend;//转接申请好友请求数据
        ServerSession frdsession = AnalysisFrdname(data.frdname);//通过在线函数获取当前申请者session


        GameMsg msg = new GameMsg//返回给发送玩家的回应消息
        {
            cmd = (int)CMD.RspFriend,

        };
        //安全判断
        if (isAccept)
        {
            //此处由于是客户端传入的pack 所以获取到的是最新的的PlayerData（）
            PlayerData frd = cacheSvc.GetPlayerDataBySession(pack.session);//获取被申请玩家数据
            PlayerData frd1 = cacheSvc.GetPlayerDataBySession(frdsession);//获取申请玩家数据

            //由于已经通过 此时匹配字典通过当前缓存层中获取的名字得到其发送消息时对应的valuse值 即对应msg 
            if (msgDic.TryGetValue(frd1.name,out MsgPack val)) {
                //获取其对应的pd数据  然后存入数据库中
               PlayerData frd1_ = cacheSvc.GetPlayerDataBySession(val.session);
               for(int y = 0; y < frd1_.friendArr.Length; y++)
                {
                    if (frd1_.friendArr[y] != null)
                    {
                        continue;
                    }
                    else
                    {
                        frd1_.friendArr[y] = frd.name;
                        cacheSvc.UpdatePlayerData(frd1_.id, frd1_);
                        msgDic.Remove(frd1.name);
                        break;
                    }
                }
            }


            //将返回的数据存入客户端传来的网络pd消息对应的玩家数据中
            for (int i = 0; i < frd.friendArr.Length; i++)
            {
                if (frd.friendArr[i] != null)
                {
                    continue;
                }
                else
                {
                    frd.friendArr[i] = frd1.name;
                    break;
                }
            }
            //排查完毕   通过服务器写的获取在线目标玩家的sesion无法正确的给PlayerData赋值  只有客户端的网络pack信息的session才可以正确为pd赋值
            //将上面存入缓存层的数据更新入数据库
            if (!cacheSvc.UpdatePlayerData(frd.id, frd))
            {
                //更新错误发送网络消息
                msg.err = (int)ErrorCode.UpdateDBError;
            }
            else
            {
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
                TaskSys.Instance.CalcTaskPrgs(frd1, 7);//更新添加好友任务进度
                GameMsg msg1 = new GameMsg//返回给发送玩家的回应消息
                {
                    cmd = (int)CMD.RspFriend,
                    rspFriend = new RspFriend
                    {
                        frdname = frd1.name,
                        lv = frd1.lv,
                        power = PECommon.GetFightByProps(frd1),
                    }
                };
                pack.session.SendMsg(msg1);
            }
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

    //删除请求信息
    public void SndRmvFriend(MsgPack pack)
    {
        SndRmvFriend data = pack.msg.sndRmvFriend;//转接被删除好友的姓名数据
        PlayerData frd = cacheSvc.GetPlayerDataBySession(pack.session);//获取申请删除玩家数据
        for (int i = 0; i < frd.friendArr.Length; i++)
        {
            //已经可以成功删除  接下来处理不同客户端上线调取当前的好友显示以及添加时两边都要存储数据进入数据库  目前只有一个被申请的人存储进了申请者的数据进入数据库  还有删除时两边也需要同时删除数据库 todo 
            if (frd.friendArr[i] == data.frdname)
            {
                //执行删除逻辑先在缓存层中删除数据 再更新至数据库中
                frd.friendArr[i] = "";
                //将上面存入缓存层的数据更新入数据库
                cacheSvc.UpdatePlayerData(frd.id, frd);

            }
            else
            {
                //继续下一层循环
                continue;
            }
        }

    }
    //查看好友信息
    public void ReqLookFriend(MsgPack pack)
    {
        ReqLookFriend data = pack.msg.reqLookFriend;//转接当前需要查看好友的姓名的玩家数据
        ServerSession frdsession = AnalysisFrdname(data.frdname);//通过在线函数获取当前被查看信息session
        GameMsg msg = new GameMsg
        {
            cmd = (int)CMD.RspLookFriend
        };
        if (frdsession != null)
        {
            PlayerData friendpd = cacheSvc.GetPlayerDataBySession(frdsession);
            RspLookFriend rspLookFriend = new RspLookFriend
            {
                pd = friendpd,
            };
            msg.rspLookFriend = rspLookFriend;
            pack.session.SendMsg(msg);
        }
        else
        {
            msg.err=((int)ErrorCode.NameIsNoExist);
            pack.session.SendMsg(msg);
        }



    }
    public ServerSession AnalysisFrdname(string name)
    {
        //获得所有在线玩家数据
        Dictionary<ServerSession, PlayerData> onlineDic = cacheSvc.GetOnlineCache();
        foreach (var Item in onlineDic)
        {
            //将键值数据都取出
            //PlayerData pd = Item.Value;
            //ServerSession session = Item.Key;
            if (name != Item.Value.name)
            {
                continue;
            }
            else if (name == Item.Value.name)
            {
                return Item.Key;
            }
        }
        return null;

    }
}

