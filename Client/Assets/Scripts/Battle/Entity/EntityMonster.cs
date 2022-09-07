/****************************************************
	文件：EntityMonster.cs	
	功能：怪物逻辑实体
*****************************************************/

using UnityEngine;

public class EntityMonster : EntityBase {

    public EntityMonster() {
        entityType = EntityType.Monster;
    }

    public MonsterData md;

    private float checkTime = 2;
    private float checkCountTime = 0;

    private float atkTime = 2;
    private float atkCountTime = 0;

    public override void SetBattleProps(BattleProps props) {
        int level = md.mLevel;

        BattleProps p = new BattleProps {
            hp = props.hp * level,
            ad = props.ad * level,
            ap = props.ap * level,
            addef = props.addef * level,
            apdef = props.apdef * level,
            dodge = props.dodge * level,
            pierce = props.pierce * level,
            critical = props.critical * level
        };

        Props = p;
        HP = p.hp;
    }

    bool runAI = true;
    public override void TickAILogic() {
        if (!runAI) {
            return;
        }

        if (currentAniState == AniState.Idle || currentAniState == AniState.Move) {
            if (battleMgr.isPauseGame) {
                Idle();
                return;
            }

            float delta = Time.deltaTime;
            checkCountTime += delta;
            if (checkCountTime < checkTime) {
                return;
            }
            else {
                Vector2 dir = CalcTargetDir();
                if (!InAtkRange()) {
                    SetDir(dir);
                    Move();
                }
                else {
                    SetDir(Vector2.zero);
                    atkCountTime += checkCountTime;
                    if (atkCountTime > atkTime) {
                        SetAtkRotation(dir);
                        Attack(md.mCfg.skillID);
                        atkCountTime = 0;
                    }
                    else {
                        Idle();
                    }
                }
                checkCountTime = 0;
                checkTime = PETools.RDInt(1, 5) * 1.0f / 10;
            }
        }
    }

    public override Vector2 CalcTargetDir() {
        EntityPlayer entityPlayer = battleMgr.entitySelfPlayer;
        if (entityPlayer == null || entityPlayer.currentAniState == AniState.Die) {
            runAI = false;
            return Vector2.zero;
        }
        else {
            Vector3 target = entityPlayer.GetPos();
            Vector3 self = GetPos();
            return new Vector2(target.x - self.x, target.z - self.z).normalized;
        }
    }

    private bool InAtkRange() {
        EntityPlayer entityPlayer = battleMgr.entitySelfPlayer;
        if (entityPlayer == null || entityPlayer.currentAniState == AniState.Die) {
            runAI = false;
            return false;
        }
        else {
            Vector3 target = entityPlayer.GetPos();
            Vector3 self = GetPos();
            target.y = 0;
            self.y = 0;
            float dis = Vector3.Distance(target, self);
            if (dis <= md.mCfg.atkDis) {
                return true;
            }
            else {
                return false;
            }
        }
    }

    public override bool GetBreakState() {
        if (md.mCfg.isStop) {
            if (curtSkillCfg != null) {
                return curtSkillCfg.isBreak;
            }
            else {
                return true;
            }
        }
        else {
            return false;
        }
    }

    public override void SetHPVal(int oldval, int newval) {
        if (md.mCfg.mType == MonsterType.Boss) {
            BattleSys.Instance.playerCtrlWnd.SetBossHPBarVal(oldval, newval, Props.hp);
        }
        else {
            base.SetHPVal(oldval, newval);
        }
    }
}
