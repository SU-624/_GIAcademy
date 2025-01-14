using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Security.Cryptography;
using System.Text;
//using PacketBase;

/// <summary>
/// Mang 10. 19
/// 
/// 플레이어가 계정생성을 계속 할 시 그 정보를 저장하기 위한 struct
/// </summary>
[System.Serializable]
public class LogData
{
    // 플레이어 회원 가입 시 필요한 정보 / 'm_IDName' 은 계속 가지고 있어야 할 데이터
    private string m_IDName;
    // 플레이어 회원 가입 시 필요한 패스워드
    private string m_PWName;

    #region LogData_Property

    // 유니티 property
    public string MyID
    {
        get { return m_IDName; }
        set { m_IDName = value; }
    }

    public string MyPW
    {
        get { return m_PWName; }
        set { m_PWName = value; }
    }
    #endregion
}

/// <summary>
/// Mang 11. 3
/// 
/// 플레이어의 회원가입에 대한 클래스
/// </summary>
public class SignInInfo : MonoBehaviour
{
    LogData m_NowLogInfo;                                       // 현재 입력하는 데이터를 잠시 저장할 변수

    public TMP_InputField m_InputID;                            // 플레이어가 입력한 ID
    public TMP_InputField m_InputPW;                            // 플레이어가 입력한 PW
    public TMP_InputField m_InputCheckPW;                       // 비밀번호 확인

    public TextMeshProUGUI m_PopupNotice;                       // 안내문구 띄워줄 변수

    // 함수가 실행되었을지 판별할 bool 변수
    // bool m_isSaved = false;

    private void Awake()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        m_NowLogInfo = new LogData();       // 모든 로그인 데이터가 저장될 딕셔너리 초기화?
    }

    // Update is called once per frame
    void Update()
    {
        //if (Network.Instance.ClientNet.networkMessage.Count != 0)
        //{
        //    if (Network.Instance.ClientNet.networkMessage.Peek() == "아이디 사용 가능" ||
        //        Network.Instance.ClientNet.networkMessage.Peek() == "아이디 중복")
        //    {
        //        IdCheckResult();
        //    }
        //    else if (Network.Instance.ClientNet.networkMessage.Peek() == "회원 가입 성공" ||
        //             Network.Instance.ClientNet.networkMessage.Peek() == "회원 가입 실패")
        //    {
        //        CreateIdResult();
        //    }
        //}
    }

    public void CheckID()
    {
        //m_NowLogInfo.MyID = m_InputID.text;

        //IdCheckPacket idCheckPacket = new IdCheckPacket();
        //idCheckPacket.packetId = (short)PacketId.ReqIdCheck;
        //idCheckPacket.packetError = (short)ErrorCode.None;
        //idCheckPacket.UserId = m_NowLogInfo.MyID;

        //Network.Instance.ClientNet.PacketSend(idCheckPacket);
    }

    private void IdCheckResult()
    {
        //if (Network.Instance.ClientNet.networkMessage.Peek() == "아이디 사용 가능")
        //{
        //    m_PopupNotice.text = Network.Instance.ClientNet.networkMessage.Dequeue();

        //    m_PopupNotice.gameObject.SetActive(true);
        //    m_PopupNotice.gameObject.GetComponent<PopOffUI>().DelayTurnOffUI();    // 팝업창 띄우기 
        //}
        //else if (Network.Instance.ClientNet.networkMessage.Peek() == "아이디 중복")
        //{
        //    m_PopupNotice.text = Network.Instance.ClientNet.networkMessage.Dequeue();

        //    m_PopupNotice.gameObject.SetActive(true);
        //    m_PopupNotice.gameObject.GetComponent<PopOffUI>().DelayTurnOffUI();    // 팝업창 띄우기 
        //}
    }

    public void CheckPW()
    {
        //// 비밀번호 & 비밀번호 확인 
        //if (m_InputPW.text != m_InputCheckPW.text)
        //{
        //    m_PopupNotice.text = "비밀번호가 같지 않습니다";

        //    // '비밀번호가 같지 않습니다' 팝업창 띄우기
        //    m_PopupNotice.gameObject.SetActive(true);

        //    Debug.Log("비밀번호 다름");

        //    m_PopupNotice.gameObject.GetComponent<PopOffUI>().DelayTurnOffUI();    // 팝업창 띄우기 
        //}
        //else                // 여기서 데이터 저장해주기
        //{
        //    m_NowLogInfo.MyID = m_InputID.text;
        //    m_NowLogInfo.MyPW = m_InputPW.text;

        //    SHA256Managed sha256Managed = new SHA256Managed();
        //    byte[] encryptBytes = sha256Managed.ComputeHash(Encoding.UTF8.GetBytes(m_NowLogInfo.MyPW));

        //    string encryptPw = Convert.ToBase64String(encryptBytes);

        //    SignUpPacket signUpPacket = new SignUpPacket();
        //    signUpPacket.packetId = (short)PacketId.ReqSignUp;
        //    signUpPacket.packetError = (short)ErrorCode.None;
        //    signUpPacket.UserId = m_NowLogInfo.MyID;
        //    signUpPacket.UserPw = encryptPw;

        //    Network.Instance.ClientNet.PacketSend(signUpPacket);

        //    Debug.Log("ID : " + m_NowLogInfo.MyID);
        //    Debug.Log("PW : " + m_NowLogInfo.MyPW);
        //}
    }

    private void CreateIdResult()
    {
        //if (Network.Instance.ClientNet.networkMessage.Peek() == "회원 가입 성공")
        //{
        //    m_PopupNotice.text = Network.Instance.ClientNet.networkMessage.Dequeue();

        //    m_PopupNotice.gameObject.SetActive(true);
        //    m_PopupNotice.gameObject.GetComponent<PopOffUI>().DelayTurnOffUI();    // 팝업창 띄우기 
        //}
        //else if (Network.Instance.ClientNet.networkMessage.Peek() == "회원 가입 실패")
        //{
        //    m_PopupNotice.text = Network.Instance.ClientNet.networkMessage.Dequeue();

        //    m_PopupNotice.gameObject.SetActive(true);
        //    m_PopupNotice.gameObject.GetComponent<PopOffUI>().DelayTurnOffUI();    // 팝업창 띄우기 
        //}
    }
}