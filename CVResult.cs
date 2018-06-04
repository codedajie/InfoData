namespace ChivaVR.Net.Core
{
    public delegate void CVRNotifyEventHandler(CVResult r, object o);

    public enum CVResult : ushort
    {
        #region 公共

        /// <summary>
        /// 未知
        /// int
        /// </summary>
        Unknown,
        /// <summary>
        /// 消息
        /// string
        /// </summary>
        Message,
        /// <summary>
        /// 异常问题
        /// Exception
        /// </summary>
        ExceptionHappened,

        #endregion

        #region 信息中心

        /// <summary>
        /// 服务器就绪
        /// Infocenter
        /// </summary>
        InfocenterReady,
        /// <summary>
        /// 服务器未连接
        /// Infocenter
        /// </summary>
        InfocenterNotConnected,
        /// <summary>
        /// 服务器已经连接，重复连接
        /// Infocenter
        /// </summary>
        InfocenterAlreadyConnected,
        /// <summary>
        /// 已断开连接
        /// Infocenter
        /// </summary>
        InfocenterDisconnected,
        /// <summary>
        /// 服务器已经启动，重复启动
        /// Infocenter
        /// </summary>
        InfocenterAlreadyStarted,
        /// <summary>
        /// 服务器参数配置问题
        /// string
        /// </summary>
        InfocenterParameterHappened,
        /// <summary>
        /// 资源操作命令响应
        /// 对应ProtoBuff结构
        /// </summary>
        ResourceOperationResponded,
        /// <summary>
        /// 登录成功
        /// LoginPt
        /// </summary>
        LogonSucceeded,
        /// <summary>
        /// 登录失败
        /// LoginPt
        /// </summary>
        LogonFailed,
        /// <summary>
        /// 用户管理：增加、删除、修改
        /// LoginPt
        /// </summary>
        UserEdited,
        /// <summary>
        /// 获取在线或下线的用户信息
        /// LogoutPt
        /// </summary>
        OnlineUserChanged,
        /// <summary>
        /// 获取所有在线用户信息
        /// LogonUserListPt
        /// </summary>
        OnlineUsersNotified,
        /// <summary>
        /// 接收聊天内容
        /// ChatTextPt
        /// </summary>
        MessageReceived,
        /// <summary>
        /// 通用消息(当相同账号再次登陆时系统会把之前登陆的账号踢出系统)
        /// MessagePt
        /// </summary>
        MessageNotified,
        /// <summary>
        /// 接收聊天状态
        /// ChatTextStatePt
        /// </summary>
        MessageStatusReceived,
        /// <summary>
        /// 网络问题
        /// SocketException
        /// </summary>
        SocketExceptionHappened,
        /// <summary>
        /// 连接失败
        /// SocketException
        /// </summary>
        ConnectFailed,
        /// <summary>
        /// 服务器连接超时
        /// SocketException
        /// </summary>
        ServerConnectTimeouted,
        /// <summary>
        /// 与服务器会话超时
        /// MessagePt
        /// </summary>
        ServerSessionTimeouted,
        /// <summary>
        /// 接收问题
        /// SocketException
        /// </summary>
        ReceiveCallbackFailed,
        /// <summary>
        /// 发送问题
        /// SocketException
        /// </summary>
        SendFailed,
        /// <summary>
        /// 发送问题
        /// Infocenter
        /// </summary>
        SendCallbackFailed,
        /// <summary>
        /// 服务器接收问题
        /// SocketException
        /// </summary>
        ReceiveFailed,
        /// <summary>
        /// 查询表
        /// QueryResultPt
        /// </summary>
        TableQueryReceived,
        /// <summary>
        /// 编辑表
        /// ExecuteResultPt
        /// </summary>
        TableUpdateReceived,
        /// <summary>
        /// 用户自定义ProtoBuffer
        /// 自定义ProtoBuffer
        /// </summary>
        CustomNotificationReceived,
        /// <summary>
        /// 超级用户登录失败
        /// string
        /// </summary>
        AccessDenied,

        #endregion

        #region 数据中心

        /// <summary>
        /// 服务器就绪
        /// Datacenter
        /// </summary>
        DatacenterReady,
        /// <summary>
        /// 服务器未连接
        /// string
        /// </summary>
        DatacenterNotConnected,
        /// <summary>
        /// 服务器已经连接，重复连接
        /// Datacenter
        /// </summary>
        DatacenterAlreadyConnected,
        /// <summary>
        /// FTP服务器类型未知
        /// string
        /// </summary>
        DatacenterUnknownServer,
        /// <summary>
        /// 上传问题
        /// UploadDataItem
        /// </summary>
        UploadExceptionHappened,
        /// <summary>
        /// 文件未发现
        /// FileNotFoundException
        /// </summary>
        FileNotFoundExceptionHanppened,
        /// <summary>
        /// 目录未发现
        /// DirectoryNotFoundException
        /// </summary>
        DirectoryNotFoundExceptionHappened,
        /// <summary>
        /// 下载问题
        /// DownloadDataItem
        /// </summary>
        DownloadExceptionHappened,
        /// <summary>
        /// 是否存在文件问题
        /// ExistDataItem
        /// </summary>
        ExistFileExceptionHappened,
        /// <summary>
        /// io问题
        /// IOException
        /// </summary>
        IOExceptionHappened,
        /// <summary>
        /// 删除文件问题
        /// DeleteDataItem
        /// </summary>
        DeleteFileExceptionHappened,
        /// <summary>
        /// 获取文件大小问题
        /// DataSizeItem
        /// </summary>
        GetFileSizeExceptionHappened,
        /// <summary>
        /// 重命名问题
        /// RenameItem
        /// </summary>
        RenameExceptionHappened,
        /// <summary>
        /// 创建远程目录问题
        /// MakeDirectoryItem
        /// </summary>
        MakeDirectoryExceptionHappened,
        /// <summary>
        /// 获取远程文件时间问题
        /// TimeStampItem
        /// /// </summary>
        GetTimeStampExceptionHappened,
        /// <summary>
        ///  获取远程目录下文件
        ///  GetFilesItem
        /// </summary>
        GetFilesExceptionHappened,
        /// <summary>
        /// 获取远程目录下子目录问题
        /// GetDirectoriesItem
        /// </summary>
        GetDirectoriesExceptionHappened,
        /// <summary>
        /// 远程目录、用户名、密码、地址问题
        /// string
        /// </summary>
        RemoteException,
        /// <summary>
        /// 本地目录设置问题
        /// string
        /// </summary>
        LocalDirectoryException,

        #endregion

        #region 日志中心

        /// <summary>
        /// 日志中心已就绪
        /// Log
        /// </summary>
        LogReady,
        /// <summary>
        /// 日志系统失败
        /// string
        /// </summary>
        LogFailed,
        /// <summary>
        /// 增加日志失败
        /// string
        /// </summary>
        AddLogFailed,
        /// <summary>
        /// 增加事件失败
        /// string
        /// </summary>
        AddEventFailed,

        #endregion
    }
}
