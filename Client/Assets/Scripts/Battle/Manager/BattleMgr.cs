/****************************************************
	文件：BattleMgr.cs  	
	功能：战场管理器
*****************************************************/

using System;
using System.Collections.Generic;
using PEProtocol;
using UnityEngine;

public class BattleMgr : MonoBehaviour {
    private ResSvc resSvc;
    private AudioSvc audioSvc;

    private StateMgr stateMgr;
    private SkillMgr skillMgr;
    private MapMgr mapMgr;

    public EntityPlayer entitySelfPlayer;
    private MapCfg mapCfg;

    private Dictionary<string, EntityMonster> monsterDic = new Dictionary<string, EntityMonster>();

    public void Init(int mapid, Action cb = null) {
        resSvc = ResSvc.Instance;
        audioSvc = AudioSvc.Instance;

        //初始化各管理器
        stateMgr = gameObject.AddComponent<StateMgr>();
        stateMgr.Init();
        skillMgr = gameObject.AddComponent<SkillMgr>();
        skillMgr.Init();

        //加载战场地图
        mapCfg = resSvc.GetMapCfg(mapid);
        resSvc.AsyncLoadScene(mapCfg.sceneName, () => {
            //初始化地图数据
            GameObject map = GameObject.FindGameObjectWithTag("MapRoot");
            mapMgr = map.GetComponent<MapMgr>();
            mapMgr.Init(this);

            map.transform.localPosition = Vector3.zero;
            map.transform.localScale = Vector3.one;

            Camera.main.transform.position = mapCfg.mainCamPos;
            Camera.main.transform.localEulerAngles = mapCfg.mainCamRote;

            LoadPlayer(mapCfg);
            entitySelfPlayer.Idle();

            //激活第一批次怪物
            ActiveCurrentBatchMonsters();

            audioSvc.PlayBGMusic(Constants.BGHuangYe);
            if (cb != null) {
                cb();
            }
        });
    }

    public bool triggerCheck = true;
    public bool isPauseGame = false;
    public void Update() {
        foreach (var item in monsterDic) {
            EntityMonster em = item.Value;
            em.TickAILogic();
        }

        //检测当前批次的怪物是否全部死亡
        if (mapMgr != null) {
            if (triggerCheck && monsterDic.Count == 0) {
                bool isExist = mapMgr.SetNextTriggerOn();
                triggerCheck = false;
                if (!isExist) {
                    //关卡结束，战斗胜利
                    EndBattle(true, entitySelfPlayer.HP);
                }
            }
        }
    }

    public void EndBattle(bool isWin, int restHP) {
        isPauseGame = true;
        AudioSvc.Instance.StopBGMusic();
        BattleSys.Instance.EndBattle(isWin, restHP);
    }

    private void LoadPlayer(MapCfg mapData) {
        GameObject player = resSvc.LoadPrefab(PathDefine.AssissnBattlePlayerPrefab);

        player.transform.position = mapData.playerBornPos;
        player.transform.localEulerAngles = mapData.playerBornRote;
        player.transform.localScale = Vector3.one;

        PlayerData pd = GameRoot.Instance.PlayerData;
        BattleProps props = new BattleProps {
            hp = pd.hp,
            ad = pd.ad,
            ap = pd.ap,
            addef = pd.addef,
            apdef = pd.apdef,
            dodge = pd.dodge,
            pierce = pd.pierce,
            critical = pd.critical
        };

        entitySelfPlayer = new EntityPlayer {
            battleMgr = this,
            stateMgr = stateMgr,
            skillMgr = skillMgr
        };
        entitySelfPlayer.Name = "AssassinBattle";
        entitySelfPlayer.SetBattleProps(props);

        PlayerController playerCtrl = player.GetComponent<PlayerController>();
        playerCtrl.Init();
        entitySelfPlayer.SetCtrl(playerCtrl);
    }

    public void LoadMonsterByWaveID(int wave) {
        for (int i = 0; i < mapCfg.monsterLst.Count; i++) {
            MonsterData md = mapCfg.monsterLst[i];
            if (md.mWave == wave) {
                GameObject m = resSvc.LoadPrefab(md.mCfg.resPath, true);
                m.transform.localPosition = md.mBornPos;
                m.transform.localEulerAngles = md.mBornRote;
                m.transform.localScale = Vector3.one;

                m.name = "m" + md.mWave + "_" + md.mIndex;

                EntityMonster em = new EntityMonster {
                    battleMgr = this,
                    stateMgr = stateMgr,
                    skillMgr = skillMgr
                };
                //设置初始属性
                em.md = md;
                em.SetBattleProps(md.mCfg.bps);
                em.Name = m.name;

                MonsterController mc = m.GetComponent<MonsterController>();
                mc.Init();
                em.SetCtrl(mc);

                m.SetActive(false);
                monsterDic.Add(m.name, em);
                if (md.mCfg.mType == MonsterType.Normal) {
                    GameRoot.Instance.dynamicWnd.AddHpItemInfo(m.name, mc.hpRoot, em.HP);
                }
                else if (md.mCfg.mType == MonsterType.Boss) {
                    BattleSys.Instance.playerCtrlWnd.SetBossHPBarState(true);
                }
            }
        }
    }

    public void ActiveCurrentBatchMonsters() {
        TimerSvc.Instance.AddTimeTask((int tid) => {
            foreach (var item in monsterDic) {
                item.Value.SetActive(true);
                item.Value.Born();
                TimerSvc.Instance.AddTimeTask((int id) => {
                    //出生1秒钟后进入Idle
                    item.Value.Idle();
                }, 1000);
            }
        }, 500);
    }

    public List<EntityMonster> GetEntityMonsters() {
        List<EntityMonster> monsterLst = new List<EntityMonster>();
        foreach (var item in monsterDic) {
            monsterLst.Add(item.Value);
        }
        return monsterLst;
    }

    public void RmvMonster(string key) {
        EntityMonster entityMonster;
        if (monsterDic.TryGetValue(key, out entityMonster)) {
            monsterDic.Remove(key);
            GameRoot.Instance.dynamicWnd.RmvHpItemInfo(key);
        }
    }

    #region 技能施放与角色控制
    public void SetSelfPlayerMoveDir(Vector2 dir) {
        //设置玩家移动
        //PECommon.Log(dir.ToString());

        if (entitySelfPlayer.canControl == false) {
            return;
        }

        if (entitySelfPlayer.currentAniState == AniState.Idle || entitySelfPlayer.currentAniState == AniState.Move) {
            if (dir == Vector2.zero) {
                entitySelfPlayer.Idle();
            }
            else {
                entitySelfPlayer.Move();
                entitySelfPlayer.SetDir(dir);
            }
        }
    }
    public void ReqReleaseSkill(int index) {
        switch (index) {
            case 0:
                ReleaseNormalAtk();
                break;
            case 1:
                ReleaseSkill1();
                break;
            case 2:
                ReleaseSkill2();
                break;
            case 3:
                ReleaseSkill3();
                break;
        }
    }


    public double lastAtkTime = 0;
    private int[] comboArr = new int[] { 111, 112, 113, 114, 115 };
    public int comboIndex = 0;
    private void ReleaseNormalAtk() {
        //PECommon.Log("Click Normal Atk");
        if (entitySelfPlayer.currentAniState == AniState.Attack) {
            //在500ms以内进行第二次点击，存数据
            double nowAtkTime = TimerSvc.Instance.GetNowTime();
            if (nowAtkTime - lastAtkTime < Constants.ComboSpace && lastAtkTime != 0) {
                if (comboArr[comboIndex] != comboArr[comboArr.Length - 1]) {
                    comboIndex += 1;
                    entitySelfPlayer.comboQue.Enqueue(comboArr[comboIndex]);
                    lastAtkTime = nowAtkTime;
                }
                else {
                    lastAtkTime = 0;
                    comboIndex = 0;
                }
            }
        }
        else if (entitySelfPlayer.currentAniState == AniState.Idle || entitySelfPlayer.currentAniState == AniState.Move) {
            comboIndex = 0;
            lastAtkTime = TimerSvc.Instance.GetNowTime();
            entitySelfPlayer.Attack(comboArr[comboIndex]);
        }
    }
    private void ReleaseSkill1() {
        //PECommon.Log("Click Skill1");
        entitySelfPlayer.Attack(101);
    }
    private void ReleaseSkill2() {
        //PECommon.Log("Click Skill2");
        entitySelfPlayer.Attack(102);
    }
    private void ReleaseSkill3() {
        //PECommon.Log("Click Skill3");
        entitySelfPlayer.Attack(103);
    }
    public Vector2 GetDirInput() {
        return BattleSys.Instance.GetDirInput();
    }

    public bool CanRlsSkill() {
        return entitySelfPlayer.canRlsSkill;
    }

    #endregion
}
