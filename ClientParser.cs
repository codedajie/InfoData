using ChivaVR.Net.Core;
using ChivaVR.Net.Packet;
using System;
/// <summary>
/// 张志杰 20170117
/// 客户端socket解释器
/// </summary>
namespace ChivaVR.Net.IC
{
    internal class InfocenterParser : Parser
    {
        public InfocenterParser(Infocenter c) : base(c)
        {
        }

        public override void Do(byte[] body)
        {
            try
            {
                if (body != null)
                {
                    int len = body.Length;
                    int id = ProtobufWrapper.GetPacketCmd(body);

                    if (id == (int)ProtoBuffType.S2CLogin)
                    {
                        LoginPt lp = ProtobufWrapper.ByteDeserialize<LoginPt>(body, len - 2);
                        if (lp.service == 1)
                        {
                            //add
                            Notify(CVResult.UserEdited, lp);
                            Log.AddEvent("UserAdded", lp.msg);
                        }
                        else if (lp.service == 2)
                        {
                            //edit
                            Notify(CVResult.UserEdited, lp);
                            Log.AddEvent("UserEdited", lp.msg);
                        }
                        else if (lp.service == 3)
                        {
                            //delete
                            Notify(CVResult.UserEdited, lp);
                            Log.AddEvent("UserDeleted", lp.msg);
                        }
                        else if (lp.service == 4)
                        {
                            //get
                            if (lp.success == 0)
                            {
                                _client.Id = lp.id;
                                Notify(CVResult.LogonSucceeded, lp);
                                Log.AddEvent("LogonSucceeded", lp.msg);
                            }
                            else
                            {
                                Notify(CVResult.LogonFailed, lp);
                                Log.AddEvent("LogonFailed", lp.msg);
                                _client.Shutdown();
                            }
                        }
                    }
                    else if (id == (int)ProtoBuffType.S2CResourceEditResult)
                    {
                        ResourcePt rpb = ProtobufWrapper.ByteDeserialize<ResourcePt>(body, len - 2);
                        Notify(CVResult.ResourceOperationResponded, rpb);

                        if (rpb.service == 1)
                        {
                            //add
                            Log.AddEvent("ResourceAdded", rpb.pathfile);
                        }
                        else if (rpb.service == 2)
                        {
                            //edit
                            Log.AddEvent("ResourceEdited", rpb.pathfile);
                        }
                        else if (rpb.service == 3)
                        {
                            //delete
                            Log.AddEvent("ResourceDeleted", rpb.pathfile);
                        }
                        else if (rpb.service == 4)
                        {
                            //get
                            Log.AddEvent("ResourceGot", rpb.objectid);
                        }
                    }
                    else if (id == (int)ProtoBuffType.S2CResourceQueryResult)
                    {
                        ResourceResultPt rpt = ProtobufWrapper.ByteDeserialize<ResourceResultPt>(body, len - 2);
                        Notify(CVResult.ResourceOperationResponded, rpt);

                        //Log.AddEvent("ResourceQueried", rpt.count.ToString());
                    }
                    else if (id == (int)ProtoBuffType.S2CLogonUser)
                    {
                        LogoutPt lp = ProtobufWrapper.ByteDeserialize<LogoutPt>(body, len - 2);
                        Notify(CVResult.OnlineUserChanged, lp);

                        Log.AddEvent("OnlineUserChanged", lp.id.ToString());
                    }
                    else if (id == (int)ProtoBuffType.S2CGetOnlineUserResult)
                    {
                        LogonUserListPt lulp = ProtobufWrapper.ByteDeserialize<LogonUserListPt>(body, len - 2);
                        Notify(CVResult.OnlineUsersNotified, lulp);

                        Log.AddEvent("OnlineUsersNotified", lulp.logonUsers.Count.ToString());
                    }
                    else if (id == (int)ProtoBuffType.S2CChatReceiveTextMsg)
                    {
                        ChatTextPt chatpt = ProtobufWrapper.ByteDeserialize<ChatTextPt>(body, len - 2);
                        Notify(CVResult.MessageReceived, chatpt);

                        Log.AddEvent("MessageReceived", chatpt.msg);
                    }
                    else if (id == (int)ProtoBuffType.S2CMessage)
                    {
                        MessagePt mp = ProtobufWrapper.ByteDeserialize<MessagePt>(body, len - 2);
                        if (mp.msgtype == "101001")
                        {
                            Notify(CVResult.ServerSessionTimeouted, mp);
                        }
                        else
                        {
                            Notify(CVResult.MessageNotified, mp);
                        }

                        Log.AddEvent("MessageNotified", mp.msg);
                    }
                    else if (id == (int)ProtoBuffType.S2CChatStatusReceived)
                    {
                        ChatTextStatePt o = ProtobufWrapper.ByteDeserialize<ChatTextStatePt>(body, len - 2);
                        Notify(CVResult.MessageStatusReceived, o);

                        Log.AddEvent("MessageStatusReceived", o.state);
                    }
                    else if (id == (int)ProtoBuffType.S2CTableQueryResult)
                    {
                        QueryResultPt o = ProtobufWrapper.ByteDeserialize<QueryResultPt>(body, len - 2);
                        Notify(CVResult.TableQueryReceived, o);

                        Log.AddEvent("TableQueryReceived", o.msg);
                    }
                    else if (id == (int)ProtoBuffType.S2CTableUpdateResult)
                    {
                        ExecuteResultPt o = ProtobufWrapper.ByteDeserialize<ExecuteResultPt>(body, len - 2);
                        if (o.token == "add_evt" || o.token == "add_log")
                        {
                            //日志和事件
                            if (o.success == 1)
                            {
                                //失败
                                if (o.token == "add_evt")
                                {
                                    //系统日志
                                    Notify(CVResult.AddEventFailed, o.msg);
                                }
                                else
                                {
                                    //用户日志
                                    Notify(CVResult.AddLogFailed, o.msg);
                                }
                            }
                        }
                        else
                        {
                            Notify(CVResult.TableUpdateReceived, o);
                            if (o.success == 0)
                            {
                                Log.AddEvent("TableUpdateReceived", o.token);
                            }
                            else
                            {
                                Log.AddEvent("TableUpdateReceived", o.msg);
                            }
                        }
                    }
                    else
                    {
                        //object o = ProtobufWrapper.ByteDeserialize<object>(body, len - 2);
                        //Notify(CVResult.CustomNotificationReceived, o);
                        Notify(CVResult.Unknown, id);
                    }
                }
            }
            catch (Exception ex)
            {
                _client.Notify(CVResult.ExceptionHappened, ex);
            }
        }
    }
}
