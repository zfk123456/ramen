/****************************************************
	文件：Class1.cs	
	功能：网络通信协议（客户端服务端共用）
*****************************************************/

using System;
using PENet;

namespace PEProtocol
{
    [Serializable]
    public class GameMsg : PEMsg
    {
        public ReqLogin reqLogin;
        public RspLogin rspLogin;

        public ReqRename reqRename;
        public RspRename rspRename;

        public ReqGuide reqGuide;
        public RspGuide rspGuide;

        public ReqStrong reqStrong;
        public RspStrong rspStrong;

        public SndChat sndChat;
        public PshChat pshChat;

        public ReqFriend reqFriend;
        public RspFriend rspFriend;
        public PshFriend pshFriend;
        public SndFriend sndFriend;
        public SndRmvFriend sndRmvFriend;
        public ReqLookFriend reqLookFriend;
        public RspLookFriend rspLookFriend;


        public ReqBuy reqBuy;
        public RspBuy rspBuy;

        public PshPower pshPower;

        public ReqTakeTaskReward reqTakeTaskReward;
        public RspTakeTaskReward rspTakeTaskReward;

        public PshTaskPrgs pshTaskPrgs;

        public ReqFBFight reqFBFight;
        public RspFBFight rspFBFight;

        public ReqFBFightEnd reqFBFightEnd;
        public RspFBFightEnd rspFBFightEnd;


    }

    #region 登录相关
    [Serializable]
    public class ReqLogin
    {
        public string acct;
        public string pass;
    }

    [Serializable]
    public class RspLogin
    {
        public PlayerData playerData;
    }

    [Serializable]
    public class PlayerData
    {
        public int id;
        public string name;
        public int lv;
        public int exp;
        public int power;
        public int coin;
        public int diamond;
        public int crystal;

        public int hp;
        public int ad;
        public int ap;
        public int addef;
        public int apdef;
        public int dodge;//闪避概率
        public int pierce;//穿透比率
        public int critical;//暴击概率

        public int guideid;
        public int[] strongArr;

        public long time;
        public string[] taskArr;
        public int fuben;
        public string[] friendArr;
        //TOADD
    }

    [Serializable]
    public class ReqRename
    {
        public string name;
    }
    [Serializable]
    public class RspRename
    {
        public string name;
    }
    #endregion

    #region 引导相关
    [Serializable]
    public class ReqGuide
    {
        public int guideid;
    }

    [Serializable]
    public class RspGuide
    {
        public int guideid;
        public int coin;
        public int lv;
        public int exp;
    }
    #endregion

    #region 强化相关
    [Serializable]
    public class ReqStrong
    {
        public int pos;
    }
    [Serializable]
    public class RspStrong
    {
        public int coin;
        public int crystal;
        public int hp;
        public int ad;
        public int ap;
        public int addef;
        public int apdef;
        public int[] strongArr;
    }
    #endregion 

    #region 聊天相关
    [Serializable]
    public class SndChat
    {
        public string chat;
    }

    [Serializable]
    public class PshChat
    {
        public string name;
        public string chat;
    }
    #endregion
    #region 好友相关
    [Serializable]
    public class ReqFriend//第一次请求
    {
        public string myname;
        public string frdname;
    }
    [Serializable]
    public class PshFriend//第一次回应  回应给被申请人 被申请人发起第二次请求来进行是否回应
    {
        public string myname;//发送申请人信息
    }
    [Serializable]
    public class SndFriend//第二次请求
    {
        //被申请者发送给服务器的判断依据
        public bool isAccept;
        public string frdname;
    }
    [Serializable]
    public class RspFriend//第二次回应 回应给两个客户端
    {
        public string frdname;//返回的角色名称
        public int lv;//返回的等级
        public int power;//返回的战斗力   
    }
    [Serializable]
    public class SndRmvFriend//删除好友请求
    {
        
        public string frdname;
    }
    [Serializable]
    public class ReqLookFriend//删除好友请求
    {
        //被查看的用户姓名
        public string frdname;
    }
    [Serializable]
    public class RspLookFriend//删除好友请求
    {
        //返回被查看的人的pd
        public PlayerData pd;
    }




    #endregion
    #region 资源交易相关
    [Serializable]
    public class ReqBuy
    {
        public int type;
        public int cost;
    }

    [Serializable]
    public class RspBuy
    {
        public int type;
        public int dimond;
        public int coin;
        public int power;
    }

    [Serializable]
    public class PshPower
    {
        public int power;
    }

    #endregion

    #region 副本战斗相关
    [Serializable]
    public class ReqFBFight
    {//请求副本战斗
        public int fbid;
    }
    [Serializable]
    public class RspFBFight
    {//回应副本战斗
        public int fbid;
        public int power;
    }
    [Serializable]
    public class ReqFBFightEnd
    {//请求求副本战斗结束
        public bool win;//输赢
        public int fbid;//关卡id
        public int resthp;//剩余血量（验证数据）
        public int costtime;//花费时间（验证数据）
    }
    [Serializable]
    public class RspFBFightEnd
    {//回应副本战斗结束
        public bool win;
        public int fbid;//不同关卡奖励不同
        public int resthp;//剩余血量（结算面板显示）
        public int costtime;//花费时间（结算面板显示）
        //副本奖励(最终数据经过服务器校验)
        public int coin;
        public int lv;
        public int exp;
        public int crystal;
        public int fuben;//副本对应进度id


    }
    #endregion

    #region 任务奖励相关
    [Serializable]
    public class ReqTakeTaskReward
    {
        public int rid;
    }

    [Serializable]
    public class RspTakeTaskReward
    {
        public int coin;
        public int lv;
        public int exp;
        public string[] taskArr;
    }
    //[Serializable]
    //public class RspActivityTaskReward
    //{
    //    public int coin;
    //    public int lv;
    //    public int exp;
    //    public string[] taskArr;
    //}

    [Serializable]
    public class PshTaskPrgs
    {
        public string[] taskArr;
    }
    #endregion

    public enum ErrorCode
    {
        None = 0,//没有错误
        ServerDataError,//服务器数据异常
        UpdateDBError,//更新数据库错误
        ClientDataError,//客户端数据异常

        AcctIsOnline,//账号已经上线
        WrongPass,//密码错误
        NameIsExist,//名字已经存在
        NameIsNoExist,//当前用户不在线或不存在
        FriendRefuse,//当前玩家拒绝了你的申请

        LackLevel,
        LackCoin,
        LackCrystal,
        LackDiamond,
        LackPower,//体力不足

    }
    //通信CMD
    public enum CMD
    {
        None = 0,
        //登录相关 100
        ReqLogin = 101,
        RspLogin = 102,

        ReqRename = 103,
        RspRename = 104,

        //主城相关 200
        ReqGuide = 201,
        RspGuide = 202,

        ReqStrong = 203,
        RspStrong = 204,

        SndChat = 205,
        PshChat = 206,

        ReqBuy = 207,
        RspBuy = 208,

        PshPower = 209,

        ReqTakeTaskReward = 210,
        RspTakeTaskReward = 211,

        PshTaskPrgs = 212,

        ReqFriend = 213,
        RspFriend = 214,
        PshFriend=215,
        SndFriend=216,
        SndRmvFriend=217,
        ReqLookFriend=218,
        RspLookFriend=219,

        //副本为全新模块从3开始
        ReqFBFight = 301,
        RspFBFight = 302,
        ReqFBFightEnd = 303,
        RspFBFightEnd = 304,
    }

    public class SrvCfg
    {
        public const string srvIP = "127.0.0.1";
        public const int srvPort = 17666;
    }
}