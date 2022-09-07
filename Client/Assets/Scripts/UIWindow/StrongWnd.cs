/****************************************************
    文件：StrongWnd.cs
	功能：强化升级界面
*****************************************************/

using PEProtocol;
using UnityEngine;
using UnityEngine.UI;

public class StrongWnd : WindowRoot {
    #region UI Define
    public Image imgCurtPos;
    public Text txtStartLv;
    public Transform starTransGrp;
    public Text propHP1;
    public Text propHurt1;
    public Text propDef1;
    public Text propHP2;
    public Text propHurt2;
    public Text propDef2;
    public Image propArr1;
    public Image propArr2;
    public Image propArr3;

    public Text txtNeedLv;
    public Text txtCostCoin;
    public Text txtCostCrystal;

    public Transform costTransRoot;
    public Text txtCoin;
    #endregion

    #region Data Area
    public Transform posBtnTrans;
    private Image[] imgs = new Image[6];
    private int currentIndex;
    private PlayerData pd;
    StrongCfg nextSd;

    #endregion

    protected override void InitWnd() {
        base.InitWnd();
        pd = GameRoot.Instance.PlayerData;
        RegClickEvts();

        ClickPosItem(0);
    }

    private void RegClickEvts() {
        for (int i = 0; i < posBtnTrans.childCount; i++) {
            Image img = posBtnTrans.GetChild(i).GetComponent<Image>();

            OnClick(img.gameObject, (object args) => {
                ClickPosItem((int)args);
                audioSvc.PlayUIAudio(Constants.UIClickBtn);
            }, i);
            imgs[i] = img;
        }
    }

    private void ClickPosItem(int index) {
        PECommon.Log("Click Item:" + index);

        currentIndex = index;
        for (int i = 0; i < imgs.Length; i++) {
            Transform trans = imgs[i].transform;
            if (i == currentIndex) {
                //箭头显示
                SetSprite(imgs[i], PathDefine.ItemArrorBG);
                trans.localPosition = new Vector3(10, trans.localPosition.y, 0);
                trans.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 95);
            }
            else {
                SetSprite(imgs[i], PathDefine.ItemPlatBG);
                trans.localPosition = new Vector3(0, trans.localPosition.y, 0);
                trans.GetComponent<RectTransform>().sizeDelta = new Vector2(220, 85);
            }
        }

        RefreshItem();
    }

    private void RefreshItem() {
        //金币
        SetText(txtCoin, pd.coin);
        switch (currentIndex) {
            case 0:
                SetSprite(imgCurtPos, PathDefine.ItemToukui);
                break;
            case 1:
                SetSprite(imgCurtPos, PathDefine.ItemBody);
                break;
            case 2:
                SetSprite(imgCurtPos, PathDefine.ItemYaobu);
                break;
            case 3:
                SetSprite(imgCurtPos, PathDefine.ItemHand);
                break;
            case 4:
                SetSprite(imgCurtPos, PathDefine.ItemLeg);
                break;
            case 5:
                SetSprite(imgCurtPos, PathDefine.ItemFoot);
                break;
        }
        SetText(txtStartLv, pd.strongArr[currentIndex] + "星级");

        int curtStarLv = pd.strongArr[currentIndex];
        for (int i = 0; i < starTransGrp.childCount; i++) {
            Image img = starTransGrp.GetChild(i).GetComponent<Image>();
            if (i < curtStarLv) {
                SetSprite(img, PathDefine.SpStar2);
            }
            else {
                SetSprite(img, PathDefine.SpStar1);
            }
        }

        int nextStartLv = curtStarLv + 1;
        int sumAddHp = resSvc.GetPropAddValPreLv(currentIndex, nextStartLv, 1);
        int sumAddHurt = resSvc.GetPropAddValPreLv(currentIndex, nextStartLv, 2);
        int sumAddDef = resSvc.GetPropAddValPreLv(currentIndex, nextStartLv, 3);
        SetText(propHP1, "生命  +" + sumAddHp);
        SetText(propHurt1, "伤害  +" + sumAddHurt);
        SetText(propDef1, "防御  +" + sumAddDef);

        nextSd = resSvc.GetStrongCfg(currentIndex, nextStartLv);
        if (nextSd != null) {
            SetActive(propHP2);
            SetActive(propHurt2);
            SetActive(propDef2);

            SetActive(costTransRoot);
            SetActive(propArr1);
            SetActive(propArr2);
            SetActive(propArr3);

            SetText(propHP2, "强化后 +" + nextSd.addhp);
            SetText(propHurt2, "+" + nextSd.addhurt);
            SetText(propDef2, "+" + nextSd.adddef);

            SetText(txtNeedLv, "需要等级：" + nextSd.minlv);
            SetText(txtCostCoin, "需要消耗：      " + nextSd.coin);

            SetText(txtCostCrystal, nextSd.crystal + "/" + pd.crystal);
        }
        else {
            SetActive(propHP2, false);
            SetActive(propHurt2, false);
            SetActive(propDef2, false);

            SetActive(costTransRoot, false);
            SetActive(propArr1, false);
            SetActive(propArr2, false);
            SetActive(propArr3, false);
        }
    }

    public void ClickCloseBtn() {
        audioSvc.PlayUIAudio(Constants.UIClickBtn);
        SetWndState(false);
    }

    public void ClickStrongBtn() {
        audioSvc.PlayUIAudio(Constants.UIClickBtn);

        if (pd.strongArr[currentIndex] < 10) {
            if (pd.lv < nextSd.minlv) {
                GameRoot.AddTips("角色等级不够");
                return;
            }
            if (pd.coin < nextSd.coin) {
                GameRoot.AddTips("金币数量不够");
                return;
            }
            if (pd.crystal < nextSd.crystal) {
                GameRoot.AddTips("水晶不够");
                return;
            }

            netSvc.SendMsg(new GameMsg {
                cmd = (int)CMD.ReqStrong,
                reqStrong = new ReqStrong {
                    pos = currentIndex
                }
            });
        }
        else {
            GameRoot.AddTips("星级已经升满");
        }
    }

    public void UpdateUI() {
        audioSvc.PlayUIAudio(Constants.FBItemEnter);
        ClickPosItem(currentIndex);
    }
}