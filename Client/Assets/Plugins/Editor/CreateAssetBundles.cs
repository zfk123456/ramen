//ab包打包
using UnityEditor;
using System.IO;
public class CreateAssetBundles
{
    [MenuItem("Assets/Build AssetsBundles")]//unity特性设置为菜单栏
    //打包所有ab包  //静态方法 
    static void BulidAllAssetBundles()
    {
        string dir = "AssetBdunles";//文件ab包输出路径
        //Directory目录类的静态方法Exists判定当前文件夹是否存在
        if (Directory.Exists(dir) == false)
        {
            //判断当前文件夹是否存在不存在则去创建
            Directory.CreateDirectory(dir);

        }
        //传入值为路径目录、资源包构建选项、平台目标
        BuildPipeline.BuildAssetBundles(dir, BuildAssetBundleOptions.None, BuildTarget.StandaloneOSX);
        //ChunkBasedCompression使用lz4算法压缩  速度较快 且可以自由选择解压资源  但是占内存比较大
        //none为默认lzma算法压缩  压缩体积最小 但是压缩时会全部解压  且解压后会使用lz4重新解压
        //UncompressedAssetBundle为不压缩 速度最快 体积最大
    }
}
