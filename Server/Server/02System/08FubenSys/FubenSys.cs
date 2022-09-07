/****************************************************
	文件：FubenSys.cs 	
	功能：副本战斗业务
*****************************************************/

using PEProtocol;

public class FubenSys
{
    private static FubenSys instance = null;
    public static FubenSys Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new FubenSys();
            }
            return instance;
        }
    }
    private CacheSvc cacheSvc = null;
    private CfgSvc cfgSvc = null;

    public void Init()
    {
        cacheSvc = CacheSvc.Instance;
        cfgSvc = CfgSvc.Instance;
        PECommon.Log("FubenSys Init Done.");
    }

    //处理战斗请求函数
    public void ReqFBFight(MsgPack pack)
    {
        ReqFBFight data = pack.msg.reqFBFight;//转接副本战斗数据
                                              //回应副本战斗消息
        GameMsg msg = new GameMsg
        {
            cmd = (int)CMD.RspFBFight
        };
        //检测体力数据是否符合需求 符合将消息发送至客户端开始对应战斗
        PlayerData pd = cacheSvc.GetPlayerDataBySession(pack.session);//获取玩家数据
        int power = cfgSvc.GetMapCfg(data.fbid).power;//从配置文件中拿取副本对应体力数据
        //判断当前副本id是否有效即关卡是否符合需求 pd.fuben为当前玩家可以打的任务关卡  data.fbid为当前副本地图的ID
        if (pd.fuben < data.fbid)
        {
            msg.err = (int)ErrorCode.ClientDataError;//客户端数据异常
        }
        //判断当前体力是否满足需求
        else if (pd.power < power)
        {
            msg.err = (int)ErrorCode.LackPower;//体力不足数据异常
        }
        //满足所有条件
        else
        {
            //角色当前体力减去所需副本所需
            pd.power -= power;
            //更新数据至服务器数据库中
            if (cacheSvc.UpdatePlayerData(pd.id, pd))
            {
                //更新成功写入回应消息中回应给客户端
                RspFBFight rspFBFight = new RspFBFight
                {
                    fbid = data.fbid,
                    power = pd.power
                };
                msg.rspFBFight = rspFBFight;
            }
            else
            {
                //更新失败 传错误码
                msg.err = (int)ErrorCode.UpdateDBError;
            }
        }
        //发送消息数据至客户端
        pack.session.SendMsg(msg);
    }
    //处理战斗结束请求函数  传入值为网络服务消息的pack包
    public void ReqFBFightEnd(MsgPack pack)
    {
        ReqFBFightEnd data = pack.msg.reqFBFightEnd;//转接副本战斗结束数据
        //回应副本战斗结束消息 通过gamemsg网络通信协议
        GameMsg msg = new GameMsg
        {
            cmd = (int)CMD.RspFBFightEnd,
        };
        //校验战斗是否合法
        if (data.win)
        {
            //赢得战斗合法操作
            if (data.costtime > 0 && data.resthp > 0)//验证时间和剩余血量合法度(简单判定合法)
            {
                //根据副本id获取相应奖励(配置文件)
                MapCfg rd = cfgSvc.GetMapCfg(data.fbid);//从配置文件中拿取副本id获取地图相应配置
                PlayerData pd = cacheSvc.GetPlayerDataBySession(pack.session);//获取玩家体力数据(从数据库中调取)

                //任务进度更新
                TaskSys.Instance.CalcTaskPrgs(pd, 2);//副本战斗在表格内id为2
                //奖励赋值
                pd.coin += rd.coin;
                pd.crystal += rd.crystal;
                //经验由于不同等级不一样封装了一个计算经验值的函数直接进行调用
                PECommon.CalcExp(pd, rd.exp);//传入值为玩家对应数据以及增加值
                //更新副本id 通过后（可以打前面打过的关卡）
                if (pd.fuben == data.fbid)//判断当前副本进度和记录的副本进度是否一样
                {
                    //相同表示打过最新的关卡 开启下一关(进度+1)
                    pd.fuben += 1;

                }
                //更新数据至数据库
                if (!cacheSvc.UpdatePlayerData(pd.id, pd))
                {
                    msg.err = (int)ErrorCode.UpdateDBError;//更新失败返回错误码
                }
                else
                {
                    //更新成功写入回应消息中回应给客户端
                    RspFBFightEnd rspFBFightEnd = new RspFBFightEnd
                    {
                        win = data.win,
                        fbid = data.fbid,
                        resthp = data.resthp,
                        costtime = data.costtime,
                        //奖励为更新完成后的数据
                        coin = pd.coin,
                        lv = pd.lv,
                        exp = pd.exp,
                        crystal = pd.crystal,

                        fuben = pd.fuben//也为更新后的数据

                    };
                    msg.rspFBFightEnd = rspFBFightEnd;//将经过处理后的服务端数据赋值给网络消息协议的回应函数 
                }
            }
        }
        else
        {
            //输了战斗不会发送网络消息到服务端若发送代表非法
            msg.err = (int)ErrorCode.ClientDataError;//发送错误码
            //根据具体业务场景来定义网络消息的发送 有的游戏输了也会发送消息 本游戏至验证胜利发送消息
        }
        pack.session.SendMsg(msg);//通过网络服务发送处理完成后的数据至客户端  客户端再进行相应处理
    }


}
