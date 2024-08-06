/***************************************************************
* Author: HuangXiaoDong
* Data  : 2024/08/06 10:44:40
* Note  : 因为使用 web和download 组件时可能无论成功失败都需要返回 
*         所以封装了 [WebResult](./WebResult.cs)
*         1. WebRequest(AwaitExtension) 和	 
*         [DownLoadResult](./DownLoadResult.cs) 
*         用于查看请求是否成功 或返回数据。
           ```csharp
           WebResult result = await GameEntry.WebRequest.AddWebRequestAsync("url");
           Debug.Log(result.IsError);
           if (!result.IsError)
           {
               Debug.Log(result.Bytes);
           }
           ```
***************************************************************/
using GameFramework;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// web 访问结果
    /// </summary>
    public class WebResult : IReference
    {
        /// <summary>
        /// web请求 返回数据
        /// </summary>
        public byte[] Bytes { get; private set; }

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
        /// <param name="bytes"></param>
        /// <param name="isError"></param>
        /// <param name="errorMessage"></param>
        /// <param name="userData"></param>
        /// <returns></returns>
        public static WebResult Create(byte[] bytes, bool isError, string errorMessage, object userData)
        {
            WebResult webResult = ReferencePool.Acquire<WebResult>();
            webResult.Bytes = bytes;
            webResult.IsError = isError;
            webResult.ErrorMessage = errorMessage;
            webResult.UserData = userData;
            return webResult;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="isError"></param>
        /// <param name="errorMessage"></param>
        /// <param name="userData"></param>
        /// <returns></returns>
        public WebResult Init(byte[] bytes, bool isError, string errorMessage, object userData)
        {
            this.Bytes = bytes;
            this.IsError = isError;
            this.ErrorMessage = errorMessage;
            this.UserData = userData;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            Bytes = null;
            IsError = false;
            ErrorMessage = string.Empty;
            UserData = null;
        }
    }
}
