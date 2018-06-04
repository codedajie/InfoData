using ChivaVR.Net.Core;
using ChivaVR.Net.IC;
using ChivaVR.Net.Packet;
using System;
using System.Collections.Generic;
/// <summary>
/// 信息交换任务
/// 张志杰 20170302
/// </summary>
namespace ChivaVR.Net.Task
{
    #region 消息
    public enum CVRInfoExResult : ushort
    {
        /// <summary>
        /// 未知
        /// object
        /// </summary>
        Unknown,
        /// <summary>
        /// 登录成功
        /// UserItem
        /// </summary>
        LogonSucceeded,
        /// <summary>
        /// 收到的消息
        /// MessageItem
        /// </summary>
        MessageReceived,
        /// <summary>
        /// 在线用户改变
        /// Dictionary<int，UserItem>
        /// </summary>
        UserChanged,
        /// <summary>
        /// 登录失败
        /// string
        /// </summary>
        LogonFailed,
        /// <summary>
        /// 来自服务器的通知todo
        /// string
        /// </summary>
        MessageNotified,
        /// <summary>
        /// 收到协同消息
        /// TeamworkItem
        /// </summary>
        TeamworkReceived,
        /// <summary>
        /// 协同消息解析问题
        /// string
        /// </summary>
        TeamworkHappened,
        /// <summary>
        /// 用户权限问题，UserRole.SystemAdministrator和UserRole.Administrator角色无法使用本组件
        /// string
        /// </summary>
        PrivilegeHappened,
        /// <summary>
        /// 没有获取用户信息
        /// string
        /// </summary>
        CurrentUserHappened,
        /// <summary>
        /// 没有获取其它用户信息
        /// string
        /// </summary>
        NoneUsersHappened,
    }
    #endregion

    public enum CVRInfoExStatus:ushort
    {
        Unknown,
        Ready,
        Timeout,
        Offline,
        Refused,
    }

    public delegate void CVRInfoExTaskEventHandler(CVRInfoExResult r, object o);

    public class CVRInfoExTask : IChivaVRNet, IDisposable
    {
        /// <summary>
        /// 事件
        /// </summary>
        public event CVRInfoExTaskEventHandler Notified = null;

        /// <summary>
        /// 组件版本
        /// </summary>
        public string Version { get { return "1.3.0203.1106"; } }

        public CVRInfoExStatus Status { get; private set; }

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="ic"></param>
        public CVRInfoExTask(Infocenter ic)
        {
            _ic = ic;
            _ic.Notified += _ic_Notified;
            _ieo = OpFactory<InfoExOp>.Create(_ic);
            Status = CVRInfoExStatus.Unknown;
        }

        public void Dispose()
        {
            _ic.Notified -= _ic_Notified;
        }

        #region 聊天发送
        /// <summary>
        /// 发送信息给所有
        /// </summary>
        /// <param name="message"></param>
        public void Send(string message)
        {
            if (_user != null)
            {
                string sentId = string.Empty;
                foreach (KeyValuePair<int, UserItem> kvp in _users)
                {
                    sentId += kvp.Key + ",";
                }
                if (sentId != string.Empty)
                {
                    _ieo.Send(_user.Id, _user.Name, sentId, message);
                }
                else
                {
                    Notified?.Invoke(CVRInfoExResult.NoneUsersHappened, "没有其他用户信息");
                }
            }
            else
            {
                Notified?.Invoke(CVRInfoExResult.CurrentUserHappened, "没有用户信息");
            }
        }

        /// <summary>
        /// 发送信息给id
        /// </summary>
        /// <param name="message"></param>
        /// <param name="i"></param>
        public void Send(string message, int i)
        {
            if (_user != null)
            {
                _ieo.Send(_user.Id, _user.Name, i.ToString(), message);
            }
            else
            {
                Notified?.Invoke(CVRInfoExResult.CurrentUserHappened, "没有用户信息");
            }
        }

        /// <summary>
        /// 发送信息给多个id
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ids"></param>
        public void Send(string message, params int[] ids)
        {
            if (_user != null)
            {
                string sentId = string.Empty;
                foreach (int i in ids)
                {
                    sentId += i + ",";
                }
                _ieo.Send(_user.Id, _user.Name, sentId, message);
            }
            else
            {
                Notified?.Invoke(CVRInfoExResult.CurrentUserHappened, "没有用户信息");
            }
        }
        #endregion

        #region 协同更新
        /// <summary>
        /// 刷新协同信息给所有在线
        /// </summary>
        /// <param name="ti"></param>
        public void Update(TeamworkItem ti)
        {
            if (ti.Value == null)
            {
                throw new ArgumentNullException("未对属性进行赋值");
            }

            Send(ti.Boby);
        }

        /// <summary>
        /// 刷新协同信息给指定id
        /// </summary>
        /// <param name="ti"></param>
        /// <param name="i"></param>
        public void Update(TeamworkItem ti, int i)
        {
            if (ti.Value == null)
            {
                throw new ArgumentNullException("未对属性进行赋值");
            }
            Send(ti.Boby, i);
        }

        /// <summary>
        /// 刷新协同信息给某些id
        /// </summary>
        /// <param name="ti"></param>
        /// <param name="ids"></param>
        public void Update(TeamworkItem ti, params int[] ids)
        {
            if (ti.Value == null)
            {
                throw new ArgumentNullException("未对属性进行赋值");
            }
            Send(ti.Boby, ids);
        }
        #endregion

        #region 用户

        /// <summary>
        /// 当前在线用户
        /// </summary>
        public Dictionary<int, UserItem> Users
        {
            get { return _users; }
            set { _users = value; }
        }

        /// <summary>
        /// 本机
        /// </summary>
        public UserItem User { get { return _user; } set { _user = value; } }

        public string GetUserName(int id)
        {
            if (_users.ContainsKey(id))
            {
                return _users[id].Name;
            }
            return string.Empty;
        }

        public UserItem GetUser(int id)
        {
            if (_users.ContainsKey(id))
            {
                return _users[id];
            }
            return null;
        }

        public int GetUserId(string name)
        {
            foreach (UserItem ui in _users.Values)
            {
                if (ui.Name == name)
                {
                    return ui.Id;
                }
            }
            return -1;
        }

        public UserItem GetUser(string name)
        {
            foreach (UserItem ui in _users.Values)
            {
                if (ui.Name == name)
                {
                    return ui;
                }
            }
            return null;
        }
        #endregion

        #region 字段
        protected Infocenter _ic = null;
        protected UserItem _user = null;
        protected InfoExOp _ieo = null;
        protected Dictionary<int, UserItem> _users = new Dictionary<int, UserItem>();
        #endregion

        #region 操作

        private UserItem Add(int id, string name, int role)
        {
            if (!_users.ContainsKey(id))
            {
                UserItem ui = new UserItem
                {
                    Id = id,
                    Name = name,
                    Role =UserItem.FromRoleInt(role),
                    Status = UserStatus.Unknown
                };
                _users.Add(id, ui);
                return ui;
            }
            return null;
        }

        private void _ic_Notified(CVResult result, object obj)
        {
            if (result == CVResult.LogonSucceeded)
            {
                LoginPt lp = obj as LoginPt;
                _user = UserItem.From(lp);
                if (_user.Role == UserRole.Administrator || _user.Role == UserRole.SystemAdministrator)
                {
                    Status = CVRInfoExStatus.Refused;
                    Notified?.Invoke(CVRInfoExResult.PrivilegeHappened, _user.Role.ToString() + " 权限访问使用");
                    _ic.Shutdown();
                }
                else
                {
                    Status = CVRInfoExStatus.Ready;
                    Notified?.Invoke(CVRInfoExResult.LogonSucceeded, _user);
                }
            }
            else if (result == CVResult.LogonFailed)
            {
                Status = CVRInfoExStatus.Offline;
                LoginPt lp = obj as LoginPt;
                Notified?.Invoke(CVRInfoExResult.LogonFailed, lp.msg);
            }
            else if (result == CVResult.OnlineUsersNotified)
            {
                LogonUserListPt lulp = obj as LogonUserListPt;

                foreach (LoginPt lp in lulp.logonUsers)
                {
                    if (!_users.ContainsKey(lp.id))
                    {
                        Add(lp.id, lp.username, lp.role);
                    }
                }
                Notified?.Invoke(CVRInfoExResult.UserChanged, _users);
            }
            else if (result == CVResult.OnlineUserChanged)
            {
                LogoutPt lp = obj as LogoutPt;
                if (lp.state == 0)
                {
                    //用户上线
                    UserItem ui = Add(lp.id, lp.username, lp.role);
                    if (ui != null)
                    {
                        Notified?.Invoke(CVRInfoExResult.UserChanged, _users);
                    }
                }
                else if (lp.state == 1)
                {
                    //用户
                    _users.Remove(lp.id);
                    Notified?.Invoke(CVRInfoExResult.UserChanged, _users);
                }
            }
            else if (result == CVResult.MessageReceived)
            {
                ChatTextPt ctp = obj as ChatTextPt;
                if (ctp.msg.Contains("\r"))
                {
                    TeamworkItem ti = TeamworkItem.FromMsg(ctp.msg);
                    ti.Item = MessageItem.From(ctp);
                    Notified?.Invoke(CVRInfoExResult.TeamworkReceived, ti);
                }
                else
                {
                    MessageItem mi = MessageItem.From(ctp);
                    Notified?.Invoke(CVRInfoExResult.MessageReceived, mi);
                }
            }
            else if (result == CVResult.MessageNotified)
            {
                MessagePt mp = obj as MessagePt;
                Status = mp.msgtype == "101001" ? CVRInfoExStatus.Timeout: CVRInfoExStatus.Unknown;
                Notified?.Invoke(CVRInfoExResult.MessageNotified, mp.msg);
            }
        }

        #endregion
    }
}
