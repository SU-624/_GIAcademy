using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.IO;
using System.Text;
using Newtonsoft.Json;


/// <summary>
/// 2023. 01. 16 Mang
/// 
/// 여기서 이제 만들어둔 프리팹에 데이터를 넣고, 데이터가 들어간 오브젝트를 옮기고, 다 해줄 것이다
/// 싱글턴으로 만들어서, 가져다 쓰기 필요한 곳에서 쓸 수 있도록
/// </summary>
public class SwitchEventList : MonoBehaviour
{
    private static SwitchEventList _instance = null;

    // Inspector 창에 연결해줄 변수들
    [Tooltip("이벤트 선택 창의 사용가능 이벤트목록을 만들기 위한 프리팹변수들")] [Header("EventList Prefab")]
    public GameObject m_PossibleEventprefab; // 

    public Transform m_PossibleParentScroll; // 
    public GameObject m_ParentCalenderButton;

    [Space(10f)] [Tooltip("달력 창의 선택된 이벤트 리스트를 만들 프리팹변수들")] [Header("SelectdEvent&SetOkEvent Prefab")]
    public GameObject m_FixedPrefab;

    public GameObject m_SelectedPrefab;
    public Transform m_ParentCalenderScroll;

    // 이벤트 창에서 쓰이는 텍스트 들을 다루는 변수들
    public TextMeshProUGUI NoticeMoneyText;

    public TextMeshProUGUI NowMoneyText;
    public TextMeshProUGUI NowSpecialPointText;

    public TextMeshProUGUI PayMoneyText;
    public TextMeshProUGUI PaySpecialPointText;

    public GameObject EventSetPanel;


    // Json 파일을 파싱해서 그 데이터들을 다 담아 줄 리스트 변수
    // 이 변수들도 EventSchedule 의 Instance.변수 들에 넣어주고 쓰도록 하자
    public List<SuddenEventTableData> SuddenEventClassInfo = new List<SuddenEventTableData>(); // 전체 고정 이벤트

    public List<SuddenEventTableData> SelectEventClassInfo = new List<SuddenEventTableData>(); // 전체 선택 이벤트
    //

    public List<SuddenEventTableData>
        PossibleChooseEventClassList = new List<SuddenEventTableData>(); //  사용가능한 선택이벤트 목록

    public List<SuddenEventTableData> ThisMonthMySelectEventList = new List<SuddenEventTableData>(); // 이번달 나의 선택이벤트 목록
    public List<SuddenEventTableData> ThisMonthMySuddenEventList = new List<SuddenEventTableData>(); // 이번달 나의 돌발이벤트 목록

    public List<SuddenEventTableData>
        PrevIChoosedEvent = new List<SuddenEventTableData>(); // 현재 내가 선택한 이벤트(최대 2개) 담아 줄 임시 변수

    public SuddenEventTableData TempIChoosed; // 임시로 내가 방금 누른 것

    int month;

    const int FixedEvenetCount = 3;
    const int SelectedEventCount = 2;
    const int PossibleEventCount = 10;

    public bool IsSetEventList = false;

    GameObject _PrevSelect = null;

    [SerializeField] PopOffUI _PopOfEventCalenderPanel;

    public static SwitchEventList Instance
    {
        get
        {
            if (_instance == null)
            {
                return null;
            }

            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // woodpie9 0615 미사용 하는 콘텐츠 주석 처리
        // for (int i = 0; i < AllOriginalJsonData.Instance.OriginalSuddenEventDataList.Count; i++)
        // {
        //     ThisMonthMySuddenEventList.Add(AllOriginalJsonData.Instance.OriginalSuddenEventDataList[i]);
        // }


        // Debug.Log("선택 이벤트 로드 완료?" + SelectEventClassInfo);
        Debug.Log("고정 이벤트 로드 완료?" + SuddenEventClassInfo);

        #region 제이슨 무대뽀로 데이터 넣기 주석처리한것

        // 1. 제이슨 파일 전체 이벤트리스트 변수에 담기
        //
        // 고정이벤트
        // SaveEventData TempFixedData = new SaveEventData();
        // SaveEventData TempFixedData1 = new SaveEventData();
        // 
        // TempFixedData.EventClassName = "FixedTestEvent0";
        // TempFixedData.EventDate[0] = 1;
        // TempFixedData.EventDate[1] = 3;     // 3월
        // TempFixedData.EventDate[2] = 2;     // 2주차
        // TempFixedData.EventDate[3] = 4;     // 목요일
        // 
        // TempFixedData.EventInformation = "이벤트설명";
        // TempFixedData.IsFixedEvent = true;
        // TempFixedData.IsPossibleUseEvent = true;
        // 
        // TempFixedData.EventRewardStatName[0] = "StatName0";
        // TempFixedData.EventRewardStat[0] = 2 + (1 * 3);
        // 
        // TempFixedData.EventRewardStatName[1] = "StatName1";
        // TempFixedData.EventRewardStat[1] = 8 + (3 * 7);
        // 
        // TempFixedData.EventRewardStatName[2] = "StatName2";
        // TempFixedData.EventRewardStat[2] = 46 + (2 * 2);
        // 
        // TempFixedData.EventRewardMoney = 386 + (1 * 4);
        // 
        // FixedEventClassInfo.Add(TempFixedData);
        // ////
        // TempFixedData1.EventClassName = "FixedTestEvent1";
        // TempFixedData1.EventDate[0] = 1;
        // TempFixedData1.EventDate[1] = 4;     // 4월
        // TempFixedData1.EventDate[2] = 1;      // 1주차
        // TempFixedData1.EventDate[3] = 2;      // 화요일
        // 
        // TempFixedData1.EventInformation = "이벤트설명";
        // TempFixedData1.IsFixedEvent = true;
        // TempFixedData1.IsPossibleUseEvent = true;
        // 
        // TempFixedData1.EventRewardStatName[0] = "StatName0";
        // TempFixedData1.EventRewardStat[0] = 2 + (1 * 4);
        // 
        // TempFixedData1.EventRewardStatName[1] = "StatName1";
        // TempFixedData1.EventRewardStat[1] = 8 + (3 * 3);
        // 
        // TempFixedData1.EventRewardStatName[2] = "StatName2";
        // TempFixedData1.EventRewardStat[2] = 46 + (2 * 5);
        // 
        // TempFixedData1.EventRewardMoney = 386 + (1 * 8);
        // 
        // FixedEventClassInfo.Add(TempFixedData1);
        // ////
        // 
        // 
        // // 선택이벤트
        // // (임시로 여기서)2. 전체 이벤트 리스트에서 사용가능한 이벤트들 사용가능이벤트 리스트 변수에 담기
        // // 제이슨 파일 생성 전 테스트를 위해 만든 변수
        // SaveEventData TempEventData = new SaveEventData();
        // SaveEventData TempEventData1 = new SaveEventData();
        // SaveEventData TempEventData2 = new SaveEventData();
        // SaveEventData TempEventData3 = new SaveEventData();
        // 
        // 
        // // 이벤트 struct 관련 초기화 해주기
        // TempEventData.EventClassName = "test 0";
        // TempEventData.EventDate[0] = 1;
        // 
        // TempEventData.EventDate[1] = 3;
        // 
        // TempEventData.IsPossibleUseEvent = false;
        // TempEventData.IsFixedEvent = false;      // 고정이벤트인지 선택이벤트인지 구별할 키워드
        // TempEventData.EventRewardStatName[0] = "StatName0";
        // TempEventData.EventRewardStat[0] = 5 + (1 * 3);
        // 
        // TempEventData.EventRewardStatName[1] = "StatName1";
        // TempEventData.EventRewardStat[1] = 30 + (1 * 7);
        // 
        // TempEventData.EventRewardStatName[2] = "StatName2";
        // TempEventData.EventRewardStat[2] = 100 + (2 * 2);
        // 
        // TempEventData.EventRewardMoney = 5247 + (3 * 4);
        // 
        // SelectEventClassInfo.Add(TempEventData);
        // ////
        // TempEventData1.EventClassName = "test 1";
        // TempEventData1.EventDate[0] = 1;
        // 
        // TempEventData1.EventDate[1] = 3;
        // TempEventData1.IsPossibleUseEvent = false;
        // TempEventData1.IsFixedEvent = false;      // 고정이벤트인지 선택이벤트인지 구별할 키워드
        // TempEventData1.EventRewardStatName[0] = "StatName0";
        // TempEventData1.EventRewardStat[0] = 5 + (1 * 3);
        // 
        // TempEventData1.EventRewardStatName[1] = "StatName1";
        // TempEventData1.EventRewardStat[1] = 30 + (1 * 7);
        // 
        // TempEventData1.EventRewardStatName[2] = "StatName2";
        // TempEventData1.EventRewardStat[2] = 100 + (2 * 2);
        // 
        // TempEventData1.EventRewardMoney = 5247 + (3 * 4);
        // 
        // SelectEventClassInfo.Add(TempEventData1);
        // ////
        // TempEventData2.EventClassName = "test 2";
        // TempEventData2.EventDate[0] = 1;
        // 
        // TempEventData2.EventDate[1] = 3;
        // 
        // TempEventData2.IsPossibleUseEvent = false;
        // TempEventData2.IsFixedEvent = false;      // 고정이벤트인지 선택이벤트인지 구별할 키워드
        // TempEventData2.EventRewardStatName[0] = "StatName0";
        // TempEventData2.EventRewardStat[0] = 5 + (1 * 3);
        // 
        // TempEventData2.EventRewardStatName[1] = "StatName1";
        // TempEventData2.EventRewardStat[1] = 30 + (1 * 7);
        // 
        // TempEventData2.EventRewardStatName[2] = "StatName2";
        // TempEventData2.EventRewardStat[2] = 100 + (2 * 2);
        // 
        // TempEventData2.EventRewardMoney = 5247 + (3 * 4);
        // 
        // SelectEventClassInfo.Add(TempEventData2);
        // ////
        // TempEventData3.EventClassName = "test 3";
        // TempEventData3.EventDate[0] = 1;
        // 
        // TempEventData3.EventDate[1] = 5;
        // 
        // TempEventData3.IsPossibleUseEvent = false;
        // TempEventData3.IsFixedEvent = false;      // 고정이벤트인지 선택이벤트인지 구별할 키워드
        // TempEventData3.EventRewardStatName[0] = "StatName0";
        // TempEventData3.EventRewardStat[0] = 5 + (1 * 3);
        // 
        // TempEventData3.EventRewardStatName[1] = "StatName1";
        // TempEventData3.EventRewardStat[1] = 30 + (1 * 7);
        // 
        // TempEventData3.EventRewardStatName[2] = "StatName2";
        // TempEventData3.EventRewardStat[2] = 100 + (2 * 2);
        // 
        // TempEventData3.EventRewardMoney = 5247 + (3 * 4);
        // 
        // SelectEventClassInfo.Add(TempEventData3);

        #endregion
    }

    // Update is called once per frame
    void Update()
    {
        // 달이 바뀔 때마다 이전과 현재 비교 후 그 달의 고정 이벤트 정보를 받겠지
        if (IsSetEventList == false && month != GameTime.Instance.FlowTime.NowMonth)
        {
            month = GameTime.Instance.FlowTime.NowMonth;

            PutOnFixedEventData(month); //고정이벤트
            PutOnPossibleEventData(month); // 선택가능이벤트 넣어주기
            InGameUI.Instance.DrawEventScreenUIText();
        }

        if (EventSetPanel.activeSelf == true)
        {
            NowMoneyText.text = PlayerInfo.Instance.MyMoney.ToString();
            NowSpecialPointText.text = PlayerInfo.Instance.SpecialPoint.ToString();
        }
    }

    // 조건에 맞는 고정이벤트데이터를 MyFixedEventList 에 넣어주기
    public void PutOnFixedEventData(int month)
    {
        GameObject CalenderEventList; // 고정이벤트들 부모

        // 오브젝트풀에 다시 넣기
        // 이미 null이면 넘어가고
        if (m_ParentCalenderScroll.transform.childCount != 0)
        {
            for (int i = 0; i < m_ParentCalenderScroll.childCount; i++)
            {
                MailObjectPool.ReturnFixedEventObject(m_ParentCalenderScroll.gameObject);
            }
        }

        // 고정이벤트 -> 월 조건만 맞춰서 MyFixedEventList에 넣으면 된다
        for (int i = 0; i < SuddenEventClassInfo.Count; i++)
        {
            // - 월별 & 고정이벤트인지 & 사용가능한지
            if (month == SuddenEventClassInfo[i].SuddenEventDate[1]
                && SuddenEventClassInfo[i].SuddenEventOpen)
            {
                // 부모 안에 객체를 옮기기
                CalenderEventList = MailObjectPool.GetFixedEventObject(m_ParentCalenderScroll);
                CalenderEventList.transform.localScale = new Vector3(1f, 1f, 1f);
                ThisMonthMySelectEventList.Add(SuddenEventClassInfo[i]);

                EventSchedule.Instance.ShowFixedEventOnCalender(); // 고정 이벤트 만들어두기
            }
        }
    }

    // 선택가능 이벤트 리스트에 정보 넣기
    public void PutOnPossibleEventData(int month)
    {
        GameObject PossibleEventList; // 선택가능이벤트들 부모

        int tempScrollChild = 0;

        for (int i = 0; i < SelectEventClassInfo.Count; i++)
        {
            // int statArrow = 0;

            // 선택 이벤트 -> 선택이벤트 && 그 달에 사용 가능한지
            if ((month == SelectEventClassInfo[i].SuddenEventDate[1] &&
                 SelectEventClassInfo[i].SuddenEventOpen == true))
            {
                PossibleEventList = MailObjectPool.GetPossibleEventObject(m_PossibleParentScroll); // 여기서 바꿔줘야 함?
                PossibleEventList.transform.localScale = new Vector3(1f, 1f, 1f);

                PossibleEventList.name = SelectEventClassInfo[i].SuddenEventID.ToString();
                PossibleEventList.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text =
                    SelectEventClassInfo[i].SuddenEventID.ToString();

                // 이벤트 사용에 드는 비용 추가 필요

                int eventStatCount = 0;

                //for (int j = 0; j < SelectEventClassInfo[i].EventRewardStat.Length; j++)
                //{
                //    if (SelectEventClassInfo[i].EventRewardStat[j] != 0)
                //    {
                //        // 스탯의 종류는 아직 정해진 것이 없어서 우선은 제외하고 하겠음. 보이기만 하는 것이므로 우선은 제외
                //        // PossibleEventList.transform.GetChild(3).GetChild(statArrow).GetChild(1).GetComponent<TextMeshProUGUI>().text = SelectEventClassInfo[i].EventRewardStatName[j];
                //        PossibleEventList.transform.GetChild(3).GetChild(statArrow).GetChild(2).GetComponent<TextMeshProUGUI>().text = SelectEventClassInfo[i].EventRewardStat[j].ToString();
                //        statArrow += 1;
                //
                //        eventStatCount += 1;
                //    }
                //}

                // 사용되지 않는 stat 칸을 꺼주기 위한 처리
                if (eventStatCount == 0)
                {
                    PossibleEventList.transform.GetChild(3).GetChild(0).gameObject.SetActive(false);
                    PossibleEventList.transform.GetChild(3).GetChild(1).gameObject.SetActive(false); //  이 이론이 맞다면 맞겠지
                }
                else if (eventStatCount == 1)
                {
                    PossibleEventList.transform.GetChild(3).GetChild(1).gameObject.SetActive(false);
                }

                // 머니랑 스페셜 포인트 리워드 처리
                // if (SelectEventClassInfo[i].EventRewardMoney != 0 && SelectEventClassInfo[i].EventRewardSpecialPoint == 0)
                // {
                //     PossibleEventList.transform.GetChild(3).GetChild(2).GetChild(2).GetComponent<TextMeshProUGUI>().text = SelectEventClassInfo[i].EventRewardMoney.ToString();
                //     PossibleEventList.transform.GetChild(3).GetChild(2).GetChild(3).gameObject.SetActive(false);
                // }
                // else if (SelectEventClassInfo[i].EventRewardMoney == 0 && SelectEventClassInfo[i].EventRewardSpecialPoint != 0)
                // {
                //     PossibleEventList.transform.GetChild(3).GetChild(2).GetChild(2).GetComponent<TextMeshProUGUI>().text = SelectEventClassInfo[i].EventRewardSpecialPoint.ToString();
                //     PossibleEventList.transform.GetChild(3).GetChild(2).GetChild(3).gameObject.SetActive(false);
                // }
                // else if (SelectEventClassInfo[i].EventRewardMoney != 0 && SelectEventClassInfo[i].EventRewardSpecialPoint != 0)
                // {
                //     PossibleEventList.transform.GetChild(3).GetChild(2).GetChild(2).GetComponent<TextMeshProUGUI>().text = SelectEventClassInfo[i].EventRewardMoney.ToString();
                //     PossibleEventList.transform.GetChild(3).GetChild(2).GetChild(3).GetComponent<TextMeshProUGUI>().text = SelectEventClassInfo[i].EventRewardSpecialPoint.ToString();
                // }
                // else if (SelectEventClassInfo[i].EventRewardMoney == 0 && SelectEventClassInfo[i].EventRewardSpecialPoint == 0)
                // {
                //     PossibleEventList.transform.GetChild(3).GetChild(2).gameObject.SetActive(false);
                // }

                // 선택가능 이벤트 스크롤뷰에서 버튼을 클릭시 지정 함수로 넘어가도록 한다.
                PossibleEventList.GetComponent<Button>().onClick.AddListener(WhenISelectedPossibleEvent);

                PossibleChooseEventClassList.Add(SelectEventClassInfo[i]);

                tempScrollChild += 1;
            }

            // statArrow = 0;
        }

        IsSetEventList = true;
    }


    /// <summary>
    /// 여기서 해야 할 것 : 지금 세가지 MyEventList 체크해서 있는 데이터 인지 없는 데이터인지 구분. 
    /// 아웃라인 제거 / 생성하는거 
    /// 코드 정리
    /// </summary>
    /// <param name="_NowEvent"></param>
    public void AfterCheckEventListDecideAddDataOrDeleteData(GameObject _NowEvent)
    {
        if (ThisMonthMySelectEventList.Count != 0) // 해당 목록 이벤트의 데이터가 저장 되어있을때
        {
            for (int i = 0; i < ThisMonthMySelectEventList.Count; i++)
            {
                Color color = new Color(1.0f, 0.0f, 0.0f, 0.0f);
                _NowEvent.GetComponent<Outline>().effectColor = color;
                _NowEvent.GetComponent<Outline>().effectDistance = new Vector2(10f, 10f);

                if (ClickedEventDate != null)
                {
                    ClickedEventDate.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
                    ClickedEventDate.transform.GetComponent<Button>().interactable = true;
                }

                ThisMonthMySelectEventList.RemoveAt(i); // 삭제하기
                _PrevSelect = null;
            }
        }
        else // 해당 목록 이벤트의 데이터가 저장 안되어있을때
        {
            // 선택한 목록 적용시킬 함수
            for (int i = 0; i < PossibleChooseEventClassList.Count; i++)
            {
                //if (_NowEvent.name == PossibleChooseEventClassList[i].EventClassName)
                //{
                //    //// 여기서 임시로 내가 선택한 데이터를 담아둔다
                //    TempIChoosed = PossibleChooseEventClassList[i];
                //    EventSchedule.Instance.tempEventList = PossibleChooseEventClassList[i];
                //
                //    _NowEvent.transform.GetComponent<TextMeshProUGUI>().text = TempIChoosed.EventClassName;
                //}
            }
        }
    }

    public GameObject ClickedEventDate;
    int nowSelectedEventListCount = 0;

    public void ResetSelectedEvent()
    {
        for (int i = 0; i < m_PossibleParentScroll.childCount; i++)
        {
            Color color = new Color(1.0f, 0.0f, 0.0f, 0.0f);
            m_PossibleParentScroll.GetChild(i).gameObject.GetComponent<Outline>().effectColor = color;
            m_PossibleParentScroll.GetChild(i).gameObject.GetComponent<Outline>().effectDistance =
                new Vector2(10f, 10f);

            PayMoneyText.text = "";
            PaySpecialPointText.text = "";
        }

        for (int i = 0; i < m_PossibleParentScroll.childCount; i++)
        {
            m_PossibleParentScroll.GetChild(i).GetComponent<Button>().interactable = true;
        }

        for (int i = 0; i < m_ParentCalenderButton.transform.childCount; i++)
        {
            for (int j = 0; j < PossibleChooseEventClassList.Count; j++)
            {
                // if (PossibleChooseEventClassList[j].EventClassName == m_ParentCalenderButton.transform.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().text)
                {
                    m_ParentCalenderButton.transform.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
                    m_ParentCalenderButton.transform.GetChild(i).GetComponent<Button>().interactable = true;
                }
            }
        }

        PrevIChoosedEvent.Clear();

        for (int i = 0; i < ThisMonthMySelectEventList.Count; i++)
        {
            //if (ThisMonthMySelectEventList[i].IsFixedEvent == false)
            {
                ThisMonthMySelectEventList.RemoveAt(i);
            }
        }


        EventSchedule.Instance.TempEventData = null;

        EventSchedule.Instance.nowPossibleCount = 2; // 지정 가능 횟수
        EventSchedule.Instance._nowPossibleCountImg.transform.GetChild(0).gameObject.SetActive(true);
        EventSchedule.Instance._nowPossibleCountImg.transform.GetChild(1).gameObject.SetActive(true);
    }

    void CheckListOutLine(GameObject nowclick, GameObject prevselect)
    {
        // 아웃라인 표시
        if (_PrevSelect == null)
        {
            Debug.Log("현재선택 아웃라인 작동!");
            Color color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
            nowclick.GetComponent<Outline>().effectColor = color;
            nowclick.GetComponent<Outline>().effectDistance = new Vector2(10f, 10f);
        }
        else if (nowclick != _PrevSelect)
        {
            Color color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
            nowclick.GetComponent<Outline>().effectColor = color;
            nowclick.GetComponent<Outline>().effectDistance = new Vector2(10f, 10f);

            color = new Color(1.0f, 0.0f, 0.0f, 0.0f);
            _PrevSelect.GetComponent<Outline>().effectColor = color;
            _PrevSelect.GetComponent<Outline>().effectDistance = new Vector2(10f, 10f);
        }
    }

    // 내가 선택가능한 이벤트를 클릭 했을 때 아웃라인 체크, 
    public void WhenISelectedPossibleEvent()
    {
        GameObject _NowEvent = EventSystem.current.currentSelectedGameObject;

        Debug.Log("이벤트 선택");

        if (PrevIChoosedEvent.Count == 0 || PrevIChoosedEvent.Count == 1) // 선택 이벤트 선택 가능
        {
            // 아웃라인 표시
            CheckListOutLine(_NowEvent, _PrevSelect);

            // 선택한 이벤트 임시저장
            for (int i = 0; i < PossibleChooseEventClassList.Count; i++)
            {
                // if (_NowEvent.name == PossibleChooseEventClassList[i].EventClassName)
                // {
                // 
                //     if (PossibleChooseEventClassList[i].NeedMoney < PlayerInfo.Instance.MyMoney &&
                //         PossibleChooseEventClassList[i].NeedSpecialPoint < PlayerInfo.Instance.SpecialPoint)
                //     {
                //         NoticeMoneyText.gameObject.SetActive(false);
                //         // PayMoneyText.text = PossibleChooseEventClassList[i].NeedMoney.ToString();
                //         // PaySpecialPointText.text = PossibleChooseEventClassList[i].NeedSpecialPoint.ToString();
                // 
                //         _PrevSelect = _NowEvent;
                // 
                //         //// 여기서 임시로 내가 선택한 데이터를 담아둔다
                //         TempIChoosed = PossibleChooseEventClassList[i];
                // 
                //         EventSchedule.Instance.tempEventList = PossibleChooseEventClassList[i];
                //     }
                //     else
                //     {
                //         NoticeMoneyText.gameObject.SetActive(true);
                //     }
                // }
            }
        }
        else if (PrevIChoosedEvent.Count == 2) // 선택 이벤트 제한갯수 2개 꽉 참
        {
            // 아웃라인 표시
            CheckListOutLine(_NowEvent, _PrevSelect);

            _PrevSelect = _NowEvent;
        }
    }

    // 사용가능 이벤트를 몇개를 했는지 체크해서 보여주는 함수
    public void CountPossibleEventSetting(GameObject _NowEvent)
    {
        // 내가 선택한 이벤트의 갯수에 따라 선택 여부가 달라지는 조건문
        if (PrevIChoosedEvent.Count != 0)
        {
            for (int i = 0; i < PrevIChoosedEvent.Count; i++)
            {
                // if (_NowEvent.name == PrevIChoosedEvent[i].EventClassName)
                // {
                //     EventSchedule.Instance.Choose_Button.transform.GetComponent<Button>().interactable = false;
                //     break;
                // }
                // else if (_NowEvent.name != PrevIChoosedEvent[i].EventClassName)
                // {
                //     EventSchedule.Instance.Choose_Button.transform.GetComponent<Button>().interactable = true;
                // }
            }

            if (PrevIChoosedEvent.Count == 2)
            {
                EventSchedule.Instance.Choose_Button.transform.GetComponent<Button>().interactable = false;
            }
        }
        else if (PrevIChoosedEvent.Count == 0)
        {
            EventSchedule.Instance.Choose_Button.transform.GetComponent<Button>().interactable = true;
        }
    }

    // 선택 이벤트 - 내가 전에 선택했던 이벤트들을 모아 둔 리스트에서 삭제하기 /  달력창 정보에서 프리팹 삭제하기
    public void PushCancleButton()
    {
        Color color = new Color(1.0f, 0.0f, 0.0f, 0.0f);
        _PrevSelect.GetComponent<Outline>().effectColor = color;
        _PrevSelect.GetComponent<Outline>().effectDistance = new Vector2(10f, 10f);

        for (int i = 0; i < PrevIChoosedEvent.Count; i++)
        {
            // if (PrevIChoosedEvent[i].EventClassName == TempIChoosed.EventClassName)
            // {
            //     EventSchedule.Instance.Choose_Button.transform.GetComponent<Button>().interactable = true;
            //     PrevIChoosedEvent.RemoveAt(i);
            // 
            //     for (int j = 0; j < m_ParentCalenderScroll.childCount; j++)
            //     {
            //         if (m_ParentCalenderScroll.GetChild(j).name == TempIChoosed.EventClassName)
            //         {
            //             MailObjectPool.ReturnSelectedEventObject(m_ParentCalenderScroll.GetChild(j).gameObject);
            // 
            //             break;
            //         }
            //     }
            // }
        }
    }

    // 고정, 선택 이벤트를 첫번째 조건 월에 맞춰 넣었으므로 고정은 바로 달력창의 Scroll View에 넣기
    // 선택은 Possible Scrollview 에 넣기
    public void PutMySelectedEventOnCalender()
    {
        // PrevIChoosedEvent.Add(TempIChoosed);        // 여기서 임시로 내가 선택한 데이터를 담아둔다

        // 선택이벤트 정보
        for (int i = 0; i < PossibleChooseEventClassList.Count; i++)
        {
            int statArrow = 0;

            // if (TempIChoosed.EventRewardMoney != 0)
            // {
            //     statArrow += 1;
            // }
            // 
            // for (int j = 0; j < TempIChoosed.EventRewardStat.Length; j++)
            // {
            //     if (TempIChoosed.EventRewardStat[j] != 0)
            //     {
            // 
            //         statArrow += 1;
            //     }
            //     else
            //     {
            //         statArrow += 1;
            //     }
            // }
        }
    }

    // 오브젝트를 다시 오브젝트 풀에 되돌려 줘야하는데 각각의 큐애 인큐 해야함
    public void ReturnEventPrefabToPool()
    {
        int _DestroyMailObj = m_PossibleParentScroll.childCount; // 

        for (int i = _DestroyMailObj - 1; i >= 0; i--)
        {
            MailObjectPool.ReturnSelectedEventObject(m_PossibleParentScroll.GetChild(0).gameObject);
        }
    }

    //  선택이벤트 가라를 두개 만들어 둔다 - 이거 아니다!
    public void PutMyEventListOnCalenderPage()
    {
        GameObject SetEventList;

        for (int i = 0; i < 2; i++)
        {
            SetEventList = MailObjectPool.GetPossibleEventObject(m_ParentCalenderScroll);
            SetEventList.transform.localScale = new Vector3(1f, 1f, 1f);

            SetEventList.transform.gameObject.SetActive(false);
        }
    }

    // 날짜를 클릭 후 실행 버튼을 눌러 내가 이벤트를 사용할지 확정을 짓는다
    public void IfIGaveTheEventDate()
    {
        if (EventSchedule.Instance.TempEventData.SuddenEventDate[2] != 0)
        {
            GameObject SetEventList;
            SetEventList = MailObjectPool.GetSelectedEventObject(m_ParentCalenderScroll); //선택한 이벤트 프리팹 생성했으니
            SetEventList.transform.localScale = new Vector3(1f, 1f, 1f);

            PrevIChoosedEvent.Add(TempIChoosed); // 여기서 임시로 내가 선택한 데이터를 담아둔다

            // 선택 이벤트 정보 넣기
            // SetEventList.name = TempIChoosed.EventClassName;
            SetEventList.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text =
                TempIChoosed.SuddenEventDate[2].ToString() + "주차"; // 해당 주차

            int dayIndex = TempIChoosed.SuddenEventDate[3];
            switch (dayIndex)
            {
                case 1:
                    SetEventList.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = "월요일";
                    break;
                case 2:
                    SetEventList.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = "화요일";
                    break;
                case 3:
                    SetEventList.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = "수요일";
                    break;
                case 4:
                    SetEventList.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = "목요일";
                    break;
                case 5:
                    SetEventList.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = "금요일";
                    break;
            }
            // SetEventList.transform.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>().text = TempIChoosed.EventClassName;

            // 데이터 넣은 후에 해당 패널 setActive(false) 해주기
            _PopOfEventCalenderPanel.TurnOffUI();
        }
    }

    public void ResetMyEventList()
    {
        int possibleEvent = m_PossibleParentScroll.childCount;
        // 
        for (int i = possibleEvent - 1; i >= 0; i--)
        {
            MailObjectPool.ReturnPossibleEventObject(m_PossibleParentScroll.GetChild(i).gameObject);
        }

        PossibleChooseEventClassList.Clear();
    }
}