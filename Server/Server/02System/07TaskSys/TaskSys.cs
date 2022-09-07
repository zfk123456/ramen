/****************************************************
	文件：TaskSys.cs 	
	功能：任务奖励系统
*****************************************************/

using PEProtocol;

public class TaskSys
{
    private static TaskSys instance = null;
    public static TaskSys Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new TaskSys();
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
        PECommon.Log("TaskSys Init Done.");
    }
    //处理网络消息函数
    public void ReqTakeTaskReward(MsgPack pack)
    {
        //转接数据
        ReqTakeTaskReward data = pack.msg.reqTakeTaskReward;
        //回应网络消息
        GameMsg msg = new GameMsg
        {
            cmd = (int)CMD.RspTakeTaskReward
        };
        //接收缓存层中的玩家session数据
        PlayerData pd = cacheSvc.GetPlayerDataBySession(pack.session);
        //获取解析后的配置文件
        TaskRewardCfg trc = cfgSvc.GetTaskRewardCfg(data.rid);//配置数据
        TaskRewardData trd = CalcTaskRewardData(pd, data.rid);//进度数据
        //数据安全判断 即进度数据是否等于配置数据（需要完成次数数量）并且还未被领取
        if (trd.prgs == trc.count && !trd.taked)
        {
            //满足条件奖励发放
            pd.coin += trc.coin;
            //工具类计算经验值
            PECommon.CalcExp(pd, trc.exp);
            trd.taked = true;//代表被领取
            //更新任务进度数据
            CalcTaskArr(pd, trd);
            //更新至数据库
            if (!cacheSvc.UpdatePlayerData(pd.id, pd))
            {
                //更新错误发送网络消息
                msg.err = (int)ErrorCode.UpdateDBError;
            }
            else
            {
                //更新成功发送网络消息
                RspTakeTaskReward rspTakeTaskReward = new RspTakeTaskReward
                {
                    coin = pd.coin,
                    lv = pd.lv,
                    exp = pd.exp,
                    
                    taskArr = pd.taskArr
                };
                //赋值回应消息
                msg.rspTakeTaskReward = rspTakeTaskReward;
            }
        }
        else
        {
            //若不等于则返回错误码
            msg.err = (int)ErrorCode.ClientDataError;
        }
        //发送消息至客户端
        pack.session.SendMsg(msg);
    }
    //根据传入的id获取字符数组中的数据处理函数（数据解析）//数据处理
    public TaskRewardData CalcTaskRewardData(PlayerData pd, int rid)
    {
        TaskRewardData trd = null;
        //遍历字符数组来填充数据
        for (int i = 0; i < pd.taskArr.Length; i++)
        {
            //遍历填充并分割
            string[] taskinfo = pd.taskArr[i].Split('|');
            //1|0|0 第一个代表ID 2代表进度 3代表领取状态
            if (int.Parse(taskinfo[0]) == rid)
            {
                trd = new TaskRewardData
                {
                    ID = int.Parse(taskinfo[0]),//id
                    prgs = int.Parse(taskinfo[1]),//进度
                    taked = taskinfo[2].Equals("1")//是否被完成
                };
                break;
            }
        }
        //返回找到的trd
        return trd;
    }

    public void CalcTaskArr(PlayerData pd, TaskRewardData trd)
    {
        string result = trd.ID + "|" + trd.prgs + '|' + (trd.taked ? 1 : 0);
        int index = -1;
        for (int i = 0; i < pd.taskArr.Length; i++)
        {
            string[] taskinfo = pd.taskArr[i].Split('|');
            if (int.Parse(taskinfo[0]) == trd.ID)
            {
                index = i;
                break;
            }
        }
        pd.taskArr[index] = result;
    }
    //根据传入的trd(进度数据)转化成字符数组中的数据处理函数(数据更新)
    public void CalcTaskPrgs(PlayerData pd, int tid)
    {
        TaskRewardData trd = CalcTaskRewardData(pd, tid);
        TaskRewardCfg trc = cfgSvc.GetTaskRewardCfg(tid);

        if (trd.prgs < trc.count)
        {
            trd.prgs += 1;
            //更新任务进度
            CalcTaskArr(pd, trd);
            //获取当前在线客户端连接
            ServerSession session = cacheSvc.GetOnlineServersession(pd.id);
            if (session != null)
            {
                //发送数据库中的进度数据给客户端
                session.SendMsg(new GameMsg
                {
                    cmd = (int)CMD.PshTaskPrgs,
                    pshTaskPrgs = new PshTaskPrgs
                    {
                        taskArr = pd.taskArr
                    }
                });
            }
        }
    }
    //计算任务进度函数  tid即为taskid  任务id
    //网络并包优化 用于节省网络消息头 即网络性能优化
    public PshTaskPrgs GetTaskPrgs(PlayerData pd, int tid)
    {
        TaskRewardData trd = CalcTaskRewardData(pd, tid);//先进行任务解析
        TaskRewardCfg trc = cfgSvc.GetTaskRewardCfg(tid);//获取tid配置
        //判断当前任务进度是否小于配置数据中的任务最大进度
        if (trd.prgs < trc.count)
        {
            trd.prgs += 1;
            //更新任务进度
            CalcTaskArr(pd, trd);//更新数据至playerdata中 
            //计算完成后
            return new PshTaskPrgs
            {
                taskArr = pd.taskArr
            };
        }
        else
        {
            //不满足并包条件
            return null;
        }
    }
}