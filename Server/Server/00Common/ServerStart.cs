/****************************************************
	文件：ServerStart.cs 	
	功能：服务器入口
*****************************************************/

using System.Threading;

class ServerStart
{
    static void Main(string[] args)
    {
        ServerRoot.Instance.Init();

        while (true)
        {
            ServerRoot.Instance.Update();
            Thread.Sleep(20);
        }
    }
}