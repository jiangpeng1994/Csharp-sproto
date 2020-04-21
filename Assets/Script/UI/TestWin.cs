using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Sproto;
using System;

public class TestWin : MonoBehaviour 
{
    private Text _txtTips;
    private uint lastHeartTime;
    private uint heartTimeFlag;
    private bool loginStatus;
    private DateTime startTime; // 当地时区
    private System.Random rand = new System.Random(2);//实例化后
    

    void Start ()
    {
        _txtTips = transform.Find("TxtTips").GetComponent<Text>();
        Button btnLogin = transform.Find("BtnLogin").GetComponent<Button>();
        Button btnTest1 = transform.Find("Test1").GetComponent<Button>();
        Button btnCreate = transform.Find("BtnCreate").GetComponent<Button>();

        UIEventTriggerListener.Get(btnLogin.gameObject).onClick = OnClickLogin;
        UIEventTriggerListener.Get(btnTest1.gameObject).onClick = OnClickTest1;
       
        //监听服务端主推的协议
        NetReceiver.AddHandler<ProtoProtocol.sync_role_offline>(RoleOffline);

        _txtTips.text = "login ..";
        lastHeartTime = 0;
        heartTimeFlag = 0;
        loginStatus = false;
        startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
    }

    void Update()
    {
        //发送心跳包
        var nowTime = (uint)(DateTime.Now - startTime).TotalSeconds; // 相差秒数
        if (heartTimeFlag == 0 && nowTime >= lastHeartTime + 2)
        {
            Debug.Log(rand.Next(10, 500));
            Debug.Log(rand.Next(10, 500));
            lastHeartTime = nowTime;
            heartTimeFlag = nowTime;
            NetSender.Send<ProtoProtocol.heartbeat>(null, (info) =>
            {
                heartTimeFlag = 0;
                //var rsp = info as ProtoSprotoType.heartbeat.response;
                //Debug.Log(rsp.status);
            });
        }
        //正常情况  没有收到心跳包的回包 开启重连
        if (loginStatus && heartTimeFlag != 0 && nowTime >= heartTimeFlag + 2)
        {
            heartTimeFlag = 0;
            OnClickLogin(null, null);
        }
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
        loginStatus = true;
        string host = "192.168.87.36";
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
            NetSender.Send<ProtoProtocol.login>(sendInfo,(loginInfo)=> {
                var loginResInfo = loginInfo as ProtoSprotoType.login.response;
                Debug.Log(loginResInfo.status);
                if (!loginResInfo.status)
                {
                    //账号密码错误
                    loginStatus = false;
                }
            });
        });
    }

    private void OnClickTest1(GameObject go, PointerEventData ed)
    {

    }

    private SprotoTypeBase RoleOffline(SprotoTypeBase msg)
    {
        //服务器t掉
        NetCore.Disconnect();
        return null;
    }
}
