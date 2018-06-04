//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from: LoginPt.proto
namespace ChivaVR.Net.Packet
{
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"LoginPt")]
  public partial class LoginPt : global::ProtoBuf.IExtensible
  {
    public LoginPt() {}
    
    private int _service;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"service", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int service
    {
      get { return _service; }
      set { _service = value; }
    }
    private int _id;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"id", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int id
    {
      get { return _id; }
      set { _id = value; }
    }
    private string _username;
    [global::ProtoBuf.ProtoMember(3, IsRequired = true, Name=@"username", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string username
    {
      get { return _username; }
      set { _username = value; }
    }
    private string _password;
    [global::ProtoBuf.ProtoMember(4, IsRequired = true, Name=@"password", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string password
    {
      get { return _password; }
      set { _password = value; }
    }
    private string _newpassword = "";
    [global::ProtoBuf.ProtoMember(5, IsRequired = false, Name=@"newpassword", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string newpassword
    {
      get { return _newpassword; }
      set { _newpassword = value; }
    }
    private int _state = default(int);
    [global::ProtoBuf.ProtoMember(6, IsRequired = false, Name=@"state", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(int))]
    public int state
    {
      get { return _state; }
      set { _state = value; }
    }
    private int _role = default(int);
    [global::ProtoBuf.ProtoMember(7, IsRequired = false, Name=@"role", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(int))]
    public int role
    {
      get { return _role; }
      set { _role = value; }
    }
    private int _success = default(int);
    [global::ProtoBuf.ProtoMember(8, IsRequired = false, Name=@"success", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(int))]
    public int success
    {
      get { return _success; }
      set { _success = value; }
    }
    private string _msg = "";
    [global::ProtoBuf.ProtoMember(9, IsRequired = false, Name=@"msg", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string msg
    {
      get { return _msg; }
      set { _msg = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"LogoutPt")]
  public partial class LogoutPt : global::ProtoBuf.IExtensible
  {
    public LogoutPt() {}
    
    private int _id;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"id", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int id
    {
      get { return _id; }
      set { _id = value; }
    }
    private string _username;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"username", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string username
    {
      get { return _username; }
      set { _username = value; }
    }
    private int _role;
    [global::ProtoBuf.ProtoMember(3, IsRequired = true, Name=@"role", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int role
    {
      get { return _role; }
      set { _role = value; }
    }
    private int _state;
    [global::ProtoBuf.ProtoMember(4, IsRequired = true, Name=@"state", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int state
    {
      get { return _state; }
      set { _state = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"LogonUserListPt")]
  public partial class LogonUserListPt : global::ProtoBuf.IExtensible
  {
    public LogonUserListPt() {}
    
    private readonly global::System.Collections.Generic.List<ChivaVR.Net.Packet.LoginPt> _logonUsers = new global::System.Collections.Generic.List<ChivaVR.Net.Packet.LoginPt>();
    [global::ProtoBuf.ProtoMember(3, Name=@"logonUsers", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public global::System.Collections.Generic.List<ChivaVR.Net.Packet.LoginPt> logonUsers
    {
      get { return _logonUsers; }
    }
  
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
}