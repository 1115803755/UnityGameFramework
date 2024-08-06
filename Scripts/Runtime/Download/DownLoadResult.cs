/***************************************************************
* Author: HuangXiaoDong
* Data  : 2024/08/06 11:16:36
* Note  : 因为使用 web和download 组件时可能无论成功失败都需要返回 
*         所以封装了 [WebResult](./WebResult.cs) 和	 
*         [DownLoadResult](./DownLoadResult.cs) 
*         用于查看请求是否成功 或返回数据。
*         DownLoad
           ```csharp
           DownLoadResult result = await GameEntry.Download.AddDownloadAsync("", "");
           Debug.Log(result.IsError);
           if(result.IsError){
               DoSomething();
           }
           ```
***************************************************************/
using GameFramework;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// DownLoad 结果
    /// </summary>
    public class DownLoadResult : IReference
    {
        /// <summary>
        /// 是否有错误
        /// </summary>
        public bool IsError { get; private set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; private set; }
        /// <summary>
        /// 自定义数据
        /// </summary>
        public object UserData { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isError"></param>
        /// <param name="errorMessage"></param>
        /// <param name="userData"></param>
        /// <returns></returns>
        public static DownLoadResult Create(bool isError, string errorMessage, object userData)
        {
            DownLoadResult downLoadResult = ReferencePool.Acquire<DownLoadResult>();
            downLoadResult.IsError = isError;
            downLoadResult.ErrorMessage = errorMessage;
            downLoadResult.UserData = userData;
            return downLoadResult;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            IsError = false;
            ErrorMessage = string.Empty;
            UserData = null;
        }
    }
}