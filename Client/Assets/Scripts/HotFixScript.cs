using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;//引入xlua命名空间
using System.IO;//引入io命名空间
public class HotFixScript : MonoBehaviour
{
    private LuaEnv luaEnv;//lua虚拟机对象
    // Start is called before the first frame update
    private void Awake()
    {

        //打补丁步骤一定游戏逻辑、ui控制以及加载资源之前就打上（生命周期）
        luaEnv = new LuaEnv();//实例化lua虚拟机对象
        //通过AddLoader可以注册个回调，该回调参数是字符串，lua代码里头调用require时，参数将会透传给回调，回调中就可以根据这个参数去加载指定文件，如果需要支持调试，需要把filepath修改为真实路径传出。该回调返回值是一个byte数组，如果为空表示该loader找不到，否则则为lua文件的内容。
        luaEnv.AddLoader(MyLoder);//参数为委托类型 执行lua语言时 自动执行传入的函数
        luaEnv.DoString("require'DarkGod'");//具体执行的lua文件 括号内写的为lua命令
        //Lua提供了一个名为require的函数用来加载模块
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    //普通加载 
    //自定义Loder  传入值为ref关键字指示按引用传递值  filePath表示需要执行的lua文件路径
    private byte[] MyLoder(ref string filePath)
    {
        //绝对路径
        //filePath表示需要加载的lua文件 .lua.txt表示后缀
        //absPath将文件路径定义完整
        string absPath = @"/Users/apple/Downloads/ResDarkGod/DarkGod/Client/AssetBdunles/" + filePath + ".lua.txt";
        return System.Text.Encoding.UTF8.GetBytes(File.ReadAllText(absPath));//File.ReadAllText(absPath)读取lua下所有文本
        //System.Text.Encoding.UTF8.GetBytes为c#下读取文本的静态方法并转为byte数组

    }
    //ab包加载
    private byte[] MyABLoader(ref string filepath)
    {
        // Debug.Log("用ab包加载lua文件");
        //找到ab包的路径
        //string path = Application.streamingAssetsPath + "/lua";
        string path = @"/Users/apple/Downloads/ResDarkGod/DarkGod/Client/AssetBdunles/" + filepath +".lua.txt";
        //加载含有lua文件的ab包
        AssetBundle luaab = AssetBundle.LoadFromFile(path);
        //加载ab包里的lua文件，读成txt，这里注意，因为打包的时候后缀是.lua.txt所以文件名还要加上个.lua
        TextAsset txt = luaab.LoadAsset<TextAsset>(filepath);
        return txt.bytes;
    }

    //反注册清空当前lua脚本
    private void OnDisable()
    {
        luaEnv.DoString("require'Dispose'");
    }
    //销毁lua虚拟机环境
    private void OnDestroy()
    {
        //当前游戏物体被销毁时自动调用该方法 mono生命周期
        luaEnv.Dispose();//销毁lua虚拟环境
    }
}
