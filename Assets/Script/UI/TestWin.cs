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
        Button btnTest1 = transform.Find("Test1").GetComponent<Button>();
        Button btnCreate = transform.Find("BtnCreate").GetComponent<Button>();

        UIEventTriggerListener.Get(btnLogin.gameObject).onClick = OnClickLogin;
        UIEventTriggerListener.Get(btnTest1.gameObject).onClick = OnClickTest1;
       
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

    /**
     * 发送登录协议
     **/
    private void OnClickLogin(GameObject go, PointerEventData ed)
    {
        string host = "192.168.199.195";
        int port = 9777;
        NetCore.Connect(host, port, () => NetCore.Receive());
        _txtTips.text = "login ..";
        NetSender.Send<ProtoProtocol.travelerLogin>(null, (info) =>
        {
            var res = info as ProtoSprotoType.travelerLogin.response;
            NetCore.Connect(res.ip, (int)res.port, ()=>NetCore.Receive());

            var sendInfo = new ProtoSprotoType.login.request();
            sendInfo.session = res.session;
            sendInfo.token = res.token;
            NetSender.Send<ProtoProtocol.login>(sendInfo);
        });
       
    }

    private void OnClickTest1(GameObject go, PointerEventData ed)
    {
        var msg = new ProtoSprotoType.GetMoudleInfo.request();
        msg.moudleBase = new ProtoSprotoType.moudle_base
        {
            grade = 3,
            term = 1,
            unit = 1,
            moudleId = 1.2
        };
        NetSender.Send<ProtoProtocol.GetMoudleInfo>(msg, (info) =>
        {
            var rsp = info as ProtoSprotoType.GetMoudleInfo.response;
            Debug.Log(rsp.status);
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
