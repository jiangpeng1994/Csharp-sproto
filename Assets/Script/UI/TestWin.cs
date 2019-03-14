using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Sproto;

public class TestWin : MonoBehaviour 
{
    private Text _txtTips;

	void Start ()
    {
        _txtTips = transform.Find("TxtTips").GetComponent<Text>();
        Button btnLogin = transform.Find("BtnLogin").GetComponent<Button>();
        Button btnCreate = transform.Find("BtnCreate").GetComponent<Button>();

        UIEventTriggerListener.Get(btnLogin.gameObject).onClick = OnClickLogin;
       
        //监听服务端主推的协议
        NetReceiver.AddHandler<ProtoProtocol.client_user_info>(UserInfo);
        NetReceiver.AddHandler<ProtoProtocol.sync_role_offline>(RoleOffline);

        _txtTips.text = "login ..";
       
    }
	
	void Update () 
    {
	
	}

    void Destory()
    {
        //ProtoMsgListener.GetInstance().Remove<Protocol.sc_map_enter>();
    }
    
    private void SocketConnected(SprotoTypeBase rsp)
    {
        var res = rsp as ProtoSprotoType.logintest.response;
        var info = new ProtoSprotoType.login.request();
        info.session = res.session;
        info.token = res.token;
        NetCore.Connect(res.ip, (int)res.port, null);
        NetSender.Send<ProtoProtocol.login>(info);
    }
    /**
     * 发送登录协议
     **/
    private void OnClickLogin(GameObject go, PointerEventData ed)
    {
        string host = "192.168.186.146";
        int port = 9777;
        NetCore.Connect(host, port, null);
        _txtTips.text = "login ..";
        var msg = new ProtoSprotoType.logintest.request();
        msg.account = "18668067789";
        msg.password = "666666";
        NetSender.Send<ProtoProtocol.logintest>(msg, (info) =>
        {
            var rsp = info as ProtoSprotoType.logintest.response;
            SocketConnected(rsp);
        });
       
    }


    private SprotoTypeBase RoleOffline(SprotoTypeBase msg)
    {
        NetCore.Disconnect();
        return null;
    }

    private SprotoTypeBase UserInfo(SprotoTypeBase msg)
    {
        return null;
    }
}
