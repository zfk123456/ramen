using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PEProtocol;
using System.Text;

public class FriendWnd : WindowRoot
{
    #region ui组件
    public InputField iptFriend;//好友申请请求
    public Text friendtxt;
    public Text frientxtacc;
    public GameObject frdacc;
    public Transform scrollTrans;//动态ui父亲节点 好友信息UI
    public Transform scrollTrans1;//好友详细信息ui
    #endregion
    //private List<string> friendLst = new List<string>();//好友信息列表  最大限制设置好友为12个
    //private List<string> frdnameLst = new List<string>();//存储好友姓名列表
    private Dictionary<string, string> frdDic = new Dictionary<string, string>();

    private List<TaskRewardData> trdLst = new List<TaskRewardData>();

    private PlayerData pd;



    protected override void InitWnd()
    {
        base.InitWnd();
        SetActive(frdacc, false);
        pd = GameRoot.Instance.PlayerData;

        RefreshUI();
    }
    //重构逻辑 好友的显示用一个单独的预制体动态加载（请求通过时则加载一条然后设置对应预制体信息）显示其信息 以及删除时只需要发送网络消息然后删除对应数据库数据  
    //public void RefreshUI()
    //{
    //    string friendMsg = "";//好友信息显示
    //    for (int i = 0; i < friendLst.Count; i++)
    //    {
    //        friendMsg += friendLst[i] +"\n";//好友信息加N换行
    //    }
    //    SetText(friendtxt, friendMsg);
    //    SetActive(frdacc, false);

    //}
    public void RefreshUI()
    {

        //防止动态ui无限生成
        for (int i = 0; i < scrollTrans.childCount; i++)
        {
            Destroy(scrollTrans.GetChild(i).gameObject);
        }
        //for (int i = 0; i < frdnameLst.Count; i++)
        //{
        //    GameObject go = resSvc.LoadPrefab(PathDefine.ItemFriend);//动态加载ui预制体
        //    go.transform.SetParent(scrollTrans);//设置父亲节点
        //    go.transform.localPosition = Vector3.zero;
        //    go.transform.localScale = Vector3.one;
        //    go.name = "taskItem_" + i;
        //    SetText(GetTrans(go.transform, "txtName"),frdnameLst[i]);
        //    SetText(GetTrans(go.transform, "txtInfor"),friendLst[i]);
        //    Button btnTake = GetTrans(go.transform, "btnDel").GetComponent<Button>();
        //    //btnTake.onClick.AddListener(ClickTakeBtn);
        //    //使用事件监听传入匿名函数将当前的点击事件委托给ClickTakeBtn处理
        //    btnTake.onClick.AddListener(() => {
        //        ClickDelBtn(go.name,go);
        //    });
        //    SetActive(frdacc, false);
        //}
        //调用客户端返回的pd数据 设置显示
        for(int i = 0; i < pd.friendArr.Length; i++)
        {
            string friend = pd.friendArr[i];
            if (pd.friendArr[i] != null)
            {
                GameObject go = resSvc.LoadPrefab(PathDefine.ItemFriend);//动态加载ui预制体
                go.transform.SetParent(scrollTrans);//设置父亲节点
                go.transform.localPosition = Vector3.zero;
                go.transform.localScale = Vector3.one;
                go.name = friend;
                SetText(GetTrans(go.transform, "txtName"), friend);
                //此处如果需要调用其战力或者等级还需要再维护一个专门的friend表 一对多 一一对应其信息（即每次上线时都更新一次好友最新的数据）
                //SetText(GetTrans(go.transform, "txtInfor"), Item.Value);
                //调用点击头像按钮执行好友详细信息按钮事件监听  使用匿名函数
                Button btnHead = GetTrans(go.transform, "icon").GetComponent<Button>();
                btnHead.onClick.AddListener(()=> {
                    //调用点击头像按钮方法  发送网络消息查询数据库
                    audioSvc.PlayUIAudio(Constants.UIOpenPage);
                    ClickHeadBtn(go.name);
                    //ClickHeadBtn();


                });
                //调用删除按钮执行删除好友按钮事件监听  使用匿名函数
                Button btnDel = GetTrans(go.transform, "btnDel").GetComponent<Button>();
                btnDel.onClick.AddListener(() => {
                    //调用删除按钮方法  发送网络消息 更新数据库
                    audioSvc.PlayUIAudio(Constants.UIExtenBtn);

                    ClickDelBtn(go.name);
                });
                SetActive(frdacc, false);
            }
            else
            {
                break;
            }

        }
       // 这段重构将两个列表换成了字典 更好删除只需删除对应数据即可  此处为调用当前临时存储的即在线申请好友成功返回的数据   下线时当前字典自动销毁 
        foreach (var Item in frdDic)
        {

            GameObject go = resSvc.LoadPrefab(PathDefine.ItemFriend);//动态加载ui预制体
            go.transform.SetParent(scrollTrans);//设置父亲节点
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.name = Item.Key;
            SetText(GetTrans(go.transform, "txtName"), Item.Key);
            //SetText(GetTrans(go.transform, "txtInfor"), Item.Value);
            //调用点击头像按钮执行好友详细信息按钮事件监听  使用匿名函数
            Button btnHead = GetTrans(go.transform, "icon").GetComponent<Button>();
            btnHead.onClick.AddListener(() => {
                //调用点击按钮方法  发送网络消息查询数据库
                audioSvc.PlayUIAudio(Constants.UIOpenPage);
                ClickHeadBtn(go.name);

            });
            //调用删除按钮执行删除好友按钮事件监听  使用匿名函数
            Button btnDel = GetTrans(go.transform, "btnDel").GetComponent<Button>();
            btnDel.onClick.AddListener(() =>
            {
                audioSvc.PlayUIAudio(Constants.UIExtenBtn);
                ClickDelBtn(go.name);
            });
            SetActive(frdacc, false);
        }
    }
    #region lookfriend
    /// <summary>
    /// 点击发送查看好友详细信息网络消息给客户端
    /// 查资料后发现，在写好的方法的上一行打出“///”，系统会自动补全。是一个很好用的技巧呢。
    /// 记住调用主城的所有ui时都需要经过MainCitySys主城业务系统上 因为所有的主城uiwnd脚本都挂载在主城业务系统上！！！
    /// </summary>
    private void ClickHeadBtn(string name)
    {
        //发送查看好友消息发送至客户端  （然后等待服务端回应被查看好友的pd信息再进行函数处理）
        GameMsg lookmsg = new GameMsg
        {
            cmd = (int)CMD.ReqLookFriend,
            reqLookFriend=new ReqLookFriend
            {
                frdname=name,
            }
        };
        netSvc.SendMsg(lookmsg);
        //此处底下写个全新函数接受服务器函数客户端有消息回调回来时直接调用那个方法
        //用动态窗口添加到canvas画布上显示（ab处理）
        //todo
        //audioSvc.PlayUIAudio(Constants.UIOpenPage);
        //MainCitySys.Instance.OpenInfoWnd();
    }
    public void OpenFriendInfo(PlayerData pd)
    {
        GameObject go = resSvc.LoadResourece("FriendInfoWnd", "fiendinfownd.ab");//使用ab包加载资源 ab包会保存对应的脚本代码
        go.transform.SetParent(scrollTrans1); 
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;
        //由于原来默认激活状态为fasle 所以需要激活状态
        SetActive(go);
        //transform.Find用于查找子节点，它并不会递归的查找物体，也就是说它只会查找它的子节点，并不会查找子节点的子节点。
        //GameObject只能查找到active的物体。如果name指定路径，则按路径查找；否则递归查找，直到查找到第一个符合条件的GameObject或者返回null
        //由于没有面板赋值且由于层级过多transform.Find用于查找子节点，它并不会递归的查找物体，也就是说它只会查找它的子节点，并不会查找子节点的子节点。找不到子物体的子物体  无法直接设置值
        //算了  写个简易面板吧。
        ////GetTrans(go.transform, "charbg");
        //SetText(GetTrans(go.transform, "txtInfo"), pd.name + " LV." + pd.lv);
        //SetText(GetTrans(go.transform, "txtExp"), pd.exp + "/" + PECommon.GetExpUpValByLv(pd.lv));

        ////imgExpPrg.fillAmount = pd.exp * 1.0F / PECommon.GetExpUpValByLv(pd.lv);
        //SetText(GetTrans(go.transform, "txtPower"), pd.power + "/" + PECommon.GetPowerLimit(pd.lv));
        //// imgPowerPrg.fillAmount = pd.power * 1.0F / PECommon.GetPowerLimit(pd.lv);

        //SetText(GetTrans(go.transform, "txtJob"), " 职业   暗夜刺客");
        //SetText(GetTrans(go.transform, "txtFight"), " 战力   " + PECommon.GetFightByProps(pd));
        //SetText(GetTrans(go.transform, "txtHP"), " 血量   " + pd.hp);
        //SetText(GetTrans(go.transform, "txtHurt"), " 伤害   " + (pd.ad + pd.ap));
        //SetText(GetTrans(go.transform, "txtDef"), " 防御   " + (pd.addef + pd.apdef));

        ////detail TODO
        //SetText(GetTrans(go.transform, "dtxhp"), pd.hp);
        //SetText(GetTrans(go.transform, "dtxad"), pd.ad);
        //SetText(GetTrans(go.transform, "dtxap"), pd.ap);
        //SetText(GetTrans(go.transform, "dtxaddef"), pd.addef);
        //SetText(GetTrans(go.transform, "dtxapdef"), pd.apdef);
        //SetText(GetTrans(go.transform, "dtxdodge"), pd.dodge + "%");
        //SetText(GetTrans(go.transform, "dtxpierce"), pd.pierce + "%");
        //SetText(GetTrans(go.transform, "dtxcritical"), pd.critical + "%");

    }
    #endregion
    #region delFriend
    //点击发送删除好友网络消息给客户端
    private void ClickDelBtn(string name)
    {
        //一个消息发送到服务器去删除数据库数据无需回应（如果删除错误直接回应一个错误码消息回来）
        GameMsg rmvmsg = new GameMsg
        {
            cmd = (int)CMD.SndRmvFriend,
            sndRmvFriend = new SndRmvFriend
            {
                frdname = name
            }
        };
        netSvc.SendMsg(rmvmsg);//发送删除好友网络消息

        //Destroy(go);
        //删除对应的key值   同时刷新ui显示则可以删除对应的元素？待会需要测试三个客户端 todo
        frdDic.Remove(name);
        RefreshUI();
        //frdnameLst.
       // frdnameLst.
        GameRoot.AddTips(Constants.Color("您已成功删除好友!", TxtColor.Red));
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
            else if (frdDic.ContainsKey(iptFriend.text))
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

    //增加好友成功处理函数(服务器回应消息请求)
    public void AddFriendMsg(string frdname, int lv, int power)
    {
        //friendLst.Add(Constants.Color(frdname, TxtColor.Blue) + "        等级:" + lv + "        战力:" + power);
        //friendLst.Add( "等级:" + lv + "战力:" + power);
        //frdnameLst.Add(frdname);

        frdDic.Add(frdname, "等级:" + lv + "战力:" + power);
        //if (friendLst.Count >= 12)
        //{
        //    GameRoot.AddTips("好友已达上限");

        //}
        //todo好友上限设置 不应该在这处理  想下在哪里处理且不能增加（应该是在添加好友申请处进行判定处理 包起来将整个）
        if (frdDic.Count >= 12)
        {
            GameRoot.AddTips("好友已达上限");

        }
        //处理完成后调用自身窗口刷新ui显示
        if (GetWndState())
        {
            RefreshUI();
        }
    }

    public void ClickCloseBtn()
    {
        audioSvc.PlayUIAudio(Constants.UIClickBtn);
        RefreshUI();//添加成功时刷新ui 刷新好友显示
        SetWndState(false);
    }
}

