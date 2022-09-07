/****************************************************
    文件：GameRoot.cs
	功能：游戏启动入口
*****************************************************/

using PEProtocol;
using UnityEngine;

public class GameRoot : MonoBehaviour {
    public static GameRoot Instance = null;

    public LoadingWnd loadingWnd;
    public DynamicWnd dynamicWnd;

    private void Start() {
        Instance = this;
        DontDestroyOnLoad(this);
        PECommon.Log("Game Start...");

        ClearUIRoot();

        Init();
    }

    private void ClearUIRoot() {
        Transform canvas = transform.Find("Canvas");
        for (int i = 0; i < canvas.childCount; i++) {
            canvas.GetChild(i).gameObject.SetActive(false);
        }
    }

    private void Init() {
        //服务模块初始化
        NetSvc net = GetComponent<NetSvc>();
        net.InitSvc();
        ResSvc res = GetComponent<ResSvc>();
        res.InitSvc();
        AudioSvc audio = GetComponent<AudioSvc>();
        audio.InitSvc();
        TimerSvc timer = GetComponent<TimerSvc>();
        timer.InitSvc();


        //业务系统初始化
        LoginSys login = GetComponent<LoginSys>();
        login.InitSys();
        MainCitySys maincity = GetComponent<MainCitySys>();
        maincity.InitSys();
        FubenSys fuben = GetComponent<FubenSys>();
        fuben.InitSys();
        BattleSys battle = GetComponent<BattleSys>();
        battle.InitSys();

        dynamicWnd.SetWndState();
        //进入登录场景并加载相应UI
        login.EnterLogin();
    }

    public static void AddTips(string tips) {
        Instance.dynamicWnd.AddTips(tips);
    }

    private PlayerData playerData = null;
    public PlayerData PlayerData {
        get {
            return playerData;
        }
    }
    public void SetPlayerData(RspLogin data) {
        playerData = data.playerData;
    }

    public void SetPlayerName(string name) {
        PlayerData.name = name;
    }

    public void SetPlayerDataByGuide(RspGuide data) {
        PlayerData.coin = data.coin;
        PlayerData.lv = data.lv;
        PlayerData.exp = data.exp;
        PlayerData.guideid = data.guideid;
    }

    public void SetPlayerDataByStrong(RspStrong data) {
        PlayerData.coin = data.coin;
        PlayerData.crystal = data.crystal;
        PlayerData.hp = data.hp;
        PlayerData.ad = data.ad;
        PlayerData.ap = data.ap;
        PlayerData.addef = data.addef;
        PlayerData.apdef = data.apdef;

        PlayerData.strongArr = data.strongArr;
    }

    public void SetPlayerDataByBuy(RspBuy data) {
        PlayerData.diamond = data.dimond;
        PlayerData.coin = data.coin;
        PlayerData.power = data.power;
    }
    public void SetPlayerDataByPower(PshPower data) {
        PlayerData.power = data.power;
    }
    public void SetPlayerDataByTask(RspTakeTaskReward data) {
        PlayerData.coin = data.coin;
        PlayerData.lv = data.lv;
        PlayerData.exp = data.exp;
        PlayerData.taskArr = data.taskArr;
        //PlayerData = data.taskArr;
        

    }
    //服务器拉取缓存层的玩家playerdata数据 然后缓存层拉取的数据库层的数据    发送给客户端进行客户端playerdata数据更新
    public void SetPlayerDataByTaskPsh(PshTaskPrgs data) {
        PlayerData.taskArr = data.taskArr;
    }
    public void SetPlayerDataByFBStart(RspFBFight data) {
        PlayerData.power = data.power;
    }
    public void SetPlayerDataByFBEnd(RspFBFightEnd data) {
        PlayerData.coin = data.coin;
        PlayerData.lv = data.lv;
        PlayerData.exp = data.exp;
        PlayerData.crystal = data.crystal;
        PlayerData.fuben = data.fuben;
    }
}