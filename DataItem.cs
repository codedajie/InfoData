using System;
using System.Collections.Generic;
/// <summary>
/// 张志杰 20170124
/// ftp传输数据
/// </summary>
namespace ChivaVR.Net.DC
{
    /// <summary>
    /// 资源操作类型
    /// </summary>
    public enum DataOpType:ushort
    {
        Upload,
        Download,
        DownloadMemory,
        Exist,
        Delete,
        GetSize,
        Rename,
        MakeDirectory,
        RemoveDirectory,
        GetTimeStamp,
        GetFiles,
        GetDirectories
    }

    /// <summary>
    /// 资源操作接口
    /// </summary>
    public interface IDataOpItem
    {
        /// <summary>
        /// 操作类型
        /// </summary>
        DataOpType Type { get; }
        /// <summary>
        /// 操作源
        /// </summary>
        string Source { get; set; }
        /// <summary>
        /// 操作目标
        /// </summary>
        string Target { get; set; }
        /// <summary>
        /// 操作结果
        /// </summary>
        bool Success { get; set; }
        /// <summary>
        /// 是否回复
        /// </summary>
        bool Replied { get; set; }
        /// <summary>
        /// 结果信息
        /// </summary>
        string Message { get; set; }
        /// <summary>
        /// 标签
        /// </summary>
        object Tag { get; set; }
        /// <summary>
        /// 执行操作
        /// </summary>
        /// <param name="dc"></param>
        void Action(Datacenter dc);
    }

    /// <summary>
    /// 上传资源项
    /// </summary>
    public class UploadDataItem : IDataOpItem
    {
        public DataOpType Type { get { return DataOpType.Upload; } }

        public UploadDataItem()
        {
            Replied = true;
            CreateDirectory = true;
        }
        /// <summary>
        /// 本地源全路径文件名
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// 远程ftp全路径文件名
        /// </summary>
        public string Target { get; set; }
        /// <summary>
        /// 是否操作成功
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 是否回执
        /// </summary>
        public bool Replied { get; set; }
        /// <summary>
        /// 结果信息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 版本名称
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// 分组
        /// </summary>
        public int Group { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 全文校验值
        /// </summary>
        public string Crc { get; set; }
        /// <summary>
        /// 标签
        /// </summary>
        public object Tag { get; set; }
        /// <summary>
        /// 是否创建不存在目录
        /// </summary>
        public bool CreateDirectory { set; internal get; }

        public void Action(Datacenter dc)
        {
            dc.UploadFile(this);
        }
    }

    /// <summary>
    /// 下载远程文件项
    /// </summary>
    public class DownloadDataItem : IDataOpItem
    {
        public DataOpType Type { get { return DataOpType.Download; } }

        public DownloadDataItem()
        {
            CreateDirectory = true;
            Replied = true;
        }

        /// <summary>
        /// 远程ftp全路径文件名
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// 本地源全路径文件名
        /// </summary>
        public string Target { get; set; }
        /// <summary>
        /// 是否操作成功
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 是否回执
        /// </summary>
        public bool Replied { get; set; }
        /// <summary>
        /// 结果信息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 标签
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// 资源ID
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 校验值
        /// </summary>
        public string Crc { get; set; }
        /// <summary>
        /// 是否创建不存在目录
        /// </summary>
        public bool CreateDirectory { set; internal get; }

        public void Action(Datacenter dc)
        {
            dc.DownloadFile(this);
        }
    }

    /// <summary>
    /// 下载远程文件到内存
    /// </summary>
    public class DownloadMemoryItem : IDataOpItem
    {
        public DataOpType Type { get { return DataOpType.DownloadMemory; } }

        /// <summary>
        /// 远程ftp全路径文件名
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// 下载内存字节数组
        /// </summary>
        public byte[] Buffer { get; set; }
        /// <summary>
        /// 本地源全路径文件名
        /// </summary>
        public string Target { get; set; }//unused
        /// <summary>
        /// 是否操作成功
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 是否回执
        /// </summary>
        public bool Replied { get; set; }
        /// <summary>
        /// 结果信息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 标签
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// 资源ID
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 校验值
        /// </summary>
        public string Crc { get; set; }
        /// <summary>
        /// 是否创建不存在目录
        /// </summary>
        public bool CreateDirectory { set; internal get; }

        public void Action(Datacenter dc)
        {
            dc.DownloadMemory(this);
        }
    }


    /// <summary>
    /// 是否存在远程文件项
    /// </summary>
    public class ExistDataItem : IDataOpItem
    {
        public DataOpType Type { get { return DataOpType.Exist; } }

        public string Source { get; set; }//unused
        /// <summary>
        /// 远程全路径文件名
        /// </summary>
        public string Target { get; set; }
        /// <summary>
        /// 是否操作成功
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 是否回执
        /// </summary>
        public bool Replied { get; set; }
        /// <summary>
        /// 是否存在
        /// </summary>
        public bool Exist { get; set; }
        /// <summary>
        /// 结果信息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 标签
        /// </summary>
        public object Tag { get; set; }

        public void Action(Datacenter dc)
        {
            dc.ExistFile(this);
        }
    }

    /// <summary>
    /// 删除远程文件项
    /// </summary>
    public class DeleteDataItem : IDataOpItem
    {
        public DataOpType Type { get { return DataOpType.Delete; } }

        public string Source { get; set; }//unused
        /// <summary>
        /// 被删除远程全路径文件名
        /// </summary>
        public string Target { get; set; }
        /// <summary>
        /// 是否操作成功
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 是否回执
        /// </summary>
        public bool Replied { get; set; }
        /// <summary>
        /// 结果信息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 标签
        /// </summary>
        public object Tag { get; set; }

        public void Action(Datacenter dc)
        {
            dc.DeleteFile(this);
        }
    }

    /// <summary>
    /// 获取远程文件大小项
    /// </summary>
    public class DataSizeItem : IDataOpItem
    {
        public DataOpType Type { get { return DataOpType.GetSize; } }

        public string Source { get; set; }//unused
        /// <summary>
        /// 远程全路径文件名
        /// </summary>
        public string Target { get; set; }
        /// <summary>
        /// 是否操作成功
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 是否回执
        /// </summary>
        public bool Replied { get; set; }
        /// <summary>
        /// 结果信息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 标签
        /// </summary>
        public object Tag { get; set; }
        /// <summary>
        /// 返回文件大小
        /// </summary>
        public long Size { get; set; }

        public void Action(Datacenter dc)
        {
            dc.GetFileSize(this);
        }
    }

    /// <summary>
    /// 重新命名远程文件项
    /// </summary>
    public class RenameItem : IDataOpItem
    {
        public DataOpType Type { get { return DataOpType.Rename; } }
        /// <summary>
        /// 远程全路径文件名
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// 仅文件名
        /// </summary>
        public string Target { get; set; }
        /// <summary>
        /// 是否操作成功
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 是否回执
        /// </summary>
        public bool Replied { get; set; }
        /// <summary>
        /// 结果信息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 标签
        /// </summary>
        public object Tag { get; set; }

        public void Action(Datacenter dc)
        {
            dc.Rename(this);
        }
    }

    /// <summary>
    /// 新建远程目录项
    /// </summary>
    public class MakeDirectoryItem : IDataOpItem
    {
        public DataOpType Type { get { return DataOpType.MakeDirectory; } }

        public string Source { get; set; }//unused
        /// <summary>
        /// 远程全路径名
        /// </summary>
        public string Target { get; set; }
        /// <summary>
        /// 是否操作成功
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 是否回执
        /// </summary>
        public bool Replied { get; set; }
        /// <summary>
        /// 结果信息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 标签
        /// </summary>
        public object Tag { get; set; }

        public void Action(Datacenter dc)
        {
            dc.MakeDirectory(this);
        }
    }

    /// <summary>
    /// 删除远程目录项
    /// </summary>
    public class RemoveDirectoryItem : IDataOpItem
    {
        public DataOpType Type { get { return DataOpType.RemoveDirectory; } }

        public string Source { get; set; }//unused
        /// <summary>
        /// 远程全路径名
        /// </summary>
        public string Target { get; set; }
        /// <summary>
        /// 是否操作成功
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 是否回执
        /// </summary>
        public bool Replied { get; set; }
        /// <summary>
        /// 结果信息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 标签
        /// </summary>
        public object Tag { get; set; }

        public void Action(Datacenter dc)
        {
            dc.RemoveDirectory(this);
        }
    }

    /// <summary>
    /// 获取远程文件时间戳项
    /// </summary>
    public class TimeStampItem : IDataOpItem
    {
        public DataOpType Type { get { return DataOpType.GetTimeStamp; } }

        public string Source { get; set; }//unused
        /// <summary>
        /// 远程全路径文件名
        /// </summary>
        public string Target { get; set; }
        /// <summary>
        /// 返回时间
        /// </summary>
        public DateTime TimeStamp { get; set; }
        /// <summary>
        /// 是否操作成功
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 是否回执
        /// </summary>
        public bool Replied { get; set; }
        /// <summary>
        /// 结果信息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 标签
        /// </summary>
        public object Tag { get; set; }

        public void Action(Datacenter dc)
        {
            dc.GetTimeStamp(this);
        }
    }

    /// <summary>
    /// 获取指定远程目录文件集项
    /// </summary>
    public class GetFilesItem : IDataOpItem
    {
        public GetFilesItem() { Filter = string.Empty; }

        public DataOpType Type { get { return DataOpType.GetFiles; } }

        public string Source { get; set; }//unused
        /// <summary>
        /// 远程全路径名
        /// </summary>
        public string Target { get; set; }
        /// <summary>
        /// 通配符
        /// </summary>
        public string Filter { get; set; }
        /// <summary>
        /// 返回文件集
        /// </summary>
        public List<FileInfoItem> Files { get; set; }
        /// <summary>
        /// 是否操作成功
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 是否回执
        /// </summary>
        public bool Replied { get; set; }
        /// <summary>
        /// 结果信息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 标签
        /// </summary>
        public object Tag { get; set; }

        public void Action(Datacenter dc)
        {
            dc.GetFiles(this);
        }
    }

    /// <summary>
    /// 获取远程目录的子目录集项
    /// </summary>
    public class GetDirectoriesItem : IDataOpItem
    {
        public DataOpType Type { get { return DataOpType.GetDirectories; } }

        public string Source { get; set; }//unused
        /// <summary>
        /// 远程全路径
        /// </summary>
        public string Target { get; set; }
        /// <summary>
        /// 返回目录集
        /// </summary>
        public List<FileInfoItem> Directories { get; set; }
        /// <summary>
        /// 是否操作成功
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 是否回执
        /// </summary>
        public bool Replied { get; set; }
        /// <summary>
        /// 结果信息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 标签
        /// </summary>
        public object Tag { get; set; }

        public void Action(Datacenter dc)
        {
            dc.GetDirectories(this);
        }
    }
}
