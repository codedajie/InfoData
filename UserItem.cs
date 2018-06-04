using ChivaVR.Net.Packet;

namespace ChivaVR.Net.Core
{
    public enum UserRole : ushort
    {
        SystemAdministrator = 0,//系统管理员
        Administrator = 1,//管理员
        Developer = 2,//开发者
        Client = 3//普通客户
    }

    public enum UserStatus : ushort
    {
        Unknown = 255,
        Online = 0,
        Offline = 1,
        Busying = 2
    }

    public class UserItem
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 用户名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public string Pwd { get; set; }
        /// <summary>
        /// 用户角色
        /// </summary>
        public UserRole Role { get; set; }
        /// <summary>
        /// 用户状态
        /// </summary>
        public UserStatus Status { get; set; }

        public static UserRole FromRoleInt(int r)
        {
            switch(r)
            {
                case 0:return UserRole.SystemAdministrator;
                case 1:return UserRole.Administrator;
                case 2:return UserRole.Developer;
                case 3:return UserRole.Client;
                default: return UserRole.Client;
            }
        }

        public static UserItem From(LoginPt lp)
        {
            return new UserItem
            {
                Id = lp.id,
                Name = lp.username,
                Role = FromRoleInt(lp.role),
                Status = UserStatus.Online
            };
        }
        public static UserItem From(int id, string name, int role, int status)
        {
            return new UserItem
            {
                Id = id,
                Name = name,
                Role = FromRoleInt(role),
                Status = UserStatus.Online//todo
            };
        }
        public static UserItem From(int id, string name, UserRole role, int status)
        {
            return new UserItem
            {
                Id = id,
                Name = name,
                Role = role,
                Status = UserStatus.Online//todo
            };
        }
        public static UserItem From(string name, string pwd, UserRole role= UserRole.Client)
        {
            return new UserItem
            {
                Id = 0,
                Name = name,
                Pwd = pwd,
                Role = role,
                Status = UserStatus.Unknown
            };
        }
    }
}
