using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public static Dictionary<string, GameObject> prefabDic = new Dictionary<string, GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        LoadResourece("abUpdate", "abupdate.ab");
        GameObject ab = GetGameObject("abUpdate");
        Instantiate(ab);//实例化游戏组件到场景中
    }

    // Update is called once per frame
    void Update()
    {

    }
    public static void LoadResourece(string resName, string filePath)
    {
        AssetBundle ab = AssetBundle.LoadFromFile(@"/Users/apple/Downloads/ResDarkGod/DarkGod_第15章/Client/AssetBdunles/ui/" + filePath);
        GameObject gameObject = ab.LoadAsset<GameObject>(resName);
        //每次调用此方法时将当前ab资源加入字典
        prefabDic.Add(resName, gameObject);
    }
    //静态方法 通过传入的名字从存储ab资源的字典中获取其值 即通过键获取其值
    public static GameObject GetGameObject(string goName)
    {
        return prefabDic[goName];//通过键获取其值并返回
    }

}
