using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class NowTime
{
    public int NowYear;
    public int NowMonth;
    public int NowWeek;
    public int NowDay;
}

public class TimeDefine
{
    // const int 
}

/// <summary>
///  2023. 04. 03 Mang
/// 
/// 시간 : string -> int 로 변경. UI 는 보이기만 하도록
/// 
/// (시간변경)
/// </summary>
public class GameTime : MonoBehaviour
{
    private static GameTime instance = null;

    public static GameTime Instance
    {
        get
        {
            return instance;
        }

        set { instance = value; }
    }

    public TextMeshProUGUI m_DrawnowTime;
    public TextMeshProUGUI m_TimeText;


    public NowTime FlowTime = new NowTime();

    const float LimitTime1 = 10.0f;      // 1 ~ 2주 제한시간
    const float LimitTime2 = 100.0f;     // 3 ~ 4주 제한시간
    float PrevTime = 0;

    public int Year;           // 1 ~ 3년(게임모드) - 무한(무한모드)
    public int Month;          // 1 ~ 12월(12)
    public int Week;          // 1 ~ 4주(4)
    public int Day;            // 월 ~ 금(5)

    public Image TimeBarImg;
    public Button m_ClassOpenButton;

    [SerializeField] private int FirstHalfPerSecond;       //  (1주 - 2주) 하루의 시간 2초(한 주 총 10초)
    [SerializeField] private int SecondHalfPerSecond;      // (3주 - 4주)하루의 시간은 20초                  
    [SerializeField] private int VacationDelay;
    [SerializeField] private int RecommendDelay;

    [SerializeField] private TreeChanger m_TreeChanger;

    public int ThirdFourthWeekTime
    {
        get { return SecondHalfPerSecond; }
    }

    public bool IsGameMode = false;                 // 메인게임화면 or UI 창 화면 체크해서 각 모드 때만 가능한 것들을 하기 위한 변수

    public bool IsMonthCycleCompleted = false;      // 월 - 한 사이클 돌았는지 체크할 변수
    public bool IsOneSemesterCompleted = false;     // 3개월(한 학기) 사이클 돌았는지 체크
    public bool IsYearCycleCompleted = false;       // 년 - 한 사이클 돌았는지 체크할 변수
    public bool IsGameEnd = false;                  // 게임이 끝났는 지 체크할 변수

    public bool isChangeWeek = false;
    private bool FirstGameOpen = true;          // 처음 게임이 실행되었다는 것을 판가름 해 줄 친구
    private bool CreateStudent = false;

    public Alarm AlarmControl;

    public UIManager UiManager;
    private bool _autosaveTimer;

    [SerializeField] private PopUpUI m_VacationPopUp;

    public bool m_IsVacationNotice = false;
    public bool m_IsRecommendNotice = false;

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        m_TreeChanger = GameObject.Find("SeasonChanger").GetComponent<TreeChanger>();

        if (GameObject.Find("LoadingCanvas") != null)
        {
            if (GameObject.Find("LoadingCanvas").activeSelf)
            {
                GameObject.Find("LoadingCanvas").SetActive(false);
            }
        }

        Debug.Log("GameTime");

        if (Json.Instance.UseLoadingData)
        {
            Year = AllInOneData.Instance.PlayerData.Year;
            Month = AllInOneData.Instance.PlayerData.Month;
            Week = AllInOneData.Instance.PlayerData.Week;
            Day = AllInOneData.Instance.PlayerData.Day;


        }
        else
        {
            Year = 1;           // 1 ~ 3년(게임모드) - 무한(무한모드)
            Month = 3;          // 1 ~ 12월(12)
            Week = 1;           // 1 ~ 4주(4)
            Day = 1;            // 월 ~ 금(5)  -> 시간이 바로 흐르므로 처음에 0으로 시작해준다.

            TimeBarImg.fillAmount = 0.2f;
            
            // 튜토리얼용 초기화
            PlayerInfo.Instance.IsFirstClassSetting = true;
            PlayerInfo.Instance.IsFirstGameJam = true;
            PlayerInfo.Instance.IsFirstGameShow = true;
            PlayerInfo.Instance.IsFirstClassEnd = false;
            PlayerInfo.Instance.IsFirstVacation = true;
            PlayerInfo.Instance.IsFirstInJaeRecommend = true;
            PlayerInfo.Instance.IsGameJamMiniGameFirst = true;
        }

        if (Month == 4)
        {
            m_TreeChanger.ChangeMaterial(0);
        }
        else if (Month == 3 || Month == 5 || Month == 6 || Month == 7 || Month == 8 || Month == 9)
        {
            m_TreeChanger.ChangeMaterial(1);
        }
        else if (Month == 10 || Month == 11)
        {
            m_TreeChanger.ChangeMaterial(2);
        }
        else if (Month == 12 || Month == 1 || Month == 2)
        {
            m_TreeChanger.ChangeMaterial(3);
        }

    }

    // Start is called before the first frame update
    public void Start()
    {
        //Debug.Log(SecondHalfPerSecond);
        // Debug.Log(TimeBarImg.fillAmount);
        //Debug.Log(Day);

        IsGameMode = true;
        //Debug.Log(IsGameMode);

        // Month[11] = "12월";
        // Week[0] = "첫째주";
        m_DrawnowTime.text = Year + "년 " + Month + "월 " + Week + "주";

        FlowTime.NowYear = Year;
        FlowTime.NowMonth = Month;
        FlowTime.NowWeek = Week;
        FlowTime.NowDay = Day;

        //Debug.Log(Year + "년" + " " + Month + " " + Week);

        //ShowGameTime();


    }

    // bool call = false;
    // Update is called once per frame
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Time.timeScale = InGameUI.Instance.m_NowGameSpeed;
        }

        if (!CreateStudent)
        {
            ObjectManager.Instance.CreateAllStudent();
            CreateStudent = true;
        }

        if (IsGameMode == true && PlayerInfo.Instance.IsFirstAcademySetting)
        {
            FlowtheTime();

            //ShowGameTime();
        }
    }

    public void FlowtheTime()
    {
        m_DrawnowTime.text = Year + "년 " + Month + "월 " + Week + "주";

        CheckPerSecond();

        // 30초 넘어갔을 때 값 변화
        if (isChangeWeek)     // 
        {
            ChangeMonth();

            ChangeWeek();

            //Debug.Log("Time.time : " + Time.time);

            // nowTime = Year + "년 " + Month[MonthIndex] + " " + Week[WeekIndex];
            if (Week != 4)
            {
                m_DrawnowTime.text = Year + "년 " + Month + "월 " + Week + "주";

                FlowTime.NowWeek = Week;
            }

            FlowTime.NowYear = Year;
            FlowTime.NowMonth = Month;
            FlowTime.NowWeek = Week;

            // 3년이라는 게임 시간이 끝나고 난 후
            if (Year == 3 && Month == 12 && Week == 4)
            {
                IsLimitedGameTimeEnd();
            }

            PrevTime = 0.0f;
            isChangeWeek = false;
        }


        //if (Month == 3 && Week == 1 && Day == 2)
        //{
        //    AlarmControl.AlarmMessageQ.Enqueue("지금은 3월 첫째주 화요일입니다!");
        //}
        if (PrevTime == 0.0f)
        {
            PrevTime = Time.time;
        }
    }

    public void ChangeDay()
    {
        if (Day < 5)
        {
            Day++;
            FlowTime.NowDay = Day;

            PrevTime = Time.time;
            //Debug.Log("요일 : " + Day);
        }
        else if (Day >= 5)
        {
            Day = 1;
            FlowTime.NowDay = Day;

            TimeBarImg.fillAmount = 0.2f;
            isChangeWeek = true;

            PrevTime = Time.time;
        }

        if (Month == 2 && Week == 3 && Day == 5)
        {
            AlarmControl.AlarmMessageQ.Enqueue("인재추천 기간이 종료되었습니다.");
            AlarmControl.AlarmMessageQ.Enqueue("방학이 시작되었습니다.");
        }
    }

    // 주 증가
    public void ChangeWeek()
    {
        // Week 증가
        if (Week < 4)
        {
            Week++;

            //Debug.Log("주 : " + Week);
            if (Month == 2 && Week == 4)
            {
                AlarmControl.AlarmMessageQ.Enqueue("학생들이 졸업하였습니다.");
            }
        }
        else if (Week >= 4)
        {
            Week = 1;
        }

    }

    // 년, 월 증가
    public void ChangeMonth()
    {
        // 연도 변경 시 월 초기화
        if (Month >= 12 && Week >= 4)
        {
            Month = 1;

            Year++;
            //AlarmControl.AlarmMessageQ.Enqueue("겨울이 왔습니다.");
        }
        // 월 증가
        else if (Month < 12 && Week >= 4)
        {
            Month++;
            //Debug.Log("달 : " + Month);
            if (Month == 2)
            {
                AlarmControl.AlarmMessageQ.Enqueue("인재추천 기간이 다가왔습니다. (2월 1주차~ 2월 3주차)");
            }
            else if (Month == 3)
            {
                // 3월이 시작되면 학생을 생성합니다.
                CreateStudent = false;
                AlarmControl.AlarmMessageQ.Enqueue("1학기가 시작되었습니다.");
                m_TreeChanger.ChangeMaterial(1);
            }
            else if (Month == 4)
            {
                //AlarmControl.AlarmMessageQ.Enqueue("봄이 왔습니다.");
                m_TreeChanger.ChangeMaterial(0);
            }
            else if (Month == 5)
            {
                m_TreeChanger.ChangeMaterial(1);
            }
            else if (Month == 6)
            {
                AlarmControl.AlarmMessageQ.Enqueue("2학기가 시작되었습니다.");
            }
            else if (Month == 7)
            {
                //AlarmControl.AlarmMessageQ.Enqueue("여름이 왔습니다.");
            }
            else if (Month == 9)
            {
                AlarmControl.AlarmMessageQ.Enqueue("3학기가 시작되었습니다.");
            }
            else if (Month == 10)
            {
                //AlarmControl.AlarmMessageQ.Enqueue("가을이 왔습니다.");
                m_TreeChanger.ChangeMaterial(2);
            }
            else if (Month == 12)
            {
                AlarmControl.AlarmMessageQ.Enqueue("4학기가 시작되었습니다.");
                m_TreeChanger.ChangeMaterial(3);
            }
        }

    }

    public void CheckPerSecond()
    {
        // 수업을 시작했을 때 캐릭터는 움직이지만 시간은 흐르지않게 할 때 PrevTime을 갱신해주지 않으면 수업 시작 시 시간이 재빠르게 흐른다.
        if ((InGameTest.Instance.m_ClassState == ClassState.nothing && Month == 3 && Week == 1 && Day == 1) || InGameTest.Instance.m_ClassState == ClassState.ClassStart || m_IsVacationNotice)
        {
            PrevTime = Time.time;
        }
        // 수업시작 후 반에 도착할 때 까지는 시간이 흐르면 안된다. TimeScale을 멈추면 캐릭터가 움직이지 않으니 다른 방법으로,,
        if (GameTime.Instance.IsGameMode == true && (InGameTest.Instance.m_ClassState != ClassState.ClassStart || (InGameTest.Instance.m_ClassState == ClassState.nothing && Month == 3 && Week == 1 && Day == 1)))
        {
            if (Week == 1 || Week == 2)
            {
                // (1 ~ 2 주차)2초마다 시간체크
                if (Time.time - PrevTime >= FirstHalfPerSecond)
                {
                    TimeBarImg.fillAmount += 0.2f;


                    // 1초마다 더해주기
                    if (FirstGameOpen == true)
                    {
                        FirstGameOpen = false;
                    }
                    if (FirstGameOpen == false)
                    {
                        ChangeDay();
                    }

                    // FirstHalfPerSecond += 2;

                    // if (FirstHalfPerSecond > LimitTime1)
                    // {
                    //     FirstHalfPerSecond = 2;
                    // }
                    _autosaveTimer = true;
                }
            }
            else if (Week == 3 || Week == 4)
            {
                // 방학 시작용
                if (Month == 2 && Week == 3 && Day == 5 && Time.time - PrevTime >= VacationDelay && !m_IsVacationNotice)
                {
                    m_VacationPopUp.TurnOnUI();
                    m_IsVacationNotice = true;
                    PrevTime = Time.time;
                }
                // 인재 추천 결과용
                if (Month == 2 && Week == 4 && Day == 5 && Time.time - PrevTime >= RecommendDelay && !m_IsRecommendNotice)
                {
                    m_IsRecommendNotice = true;
                }
                // (3 ~ 4 주차)6초마다 시간체크
                if (Time.time - PrevTime >= SecondHalfPerSecond)
                {
                    TimeBarImg.fillAmount += 0.2f;

                    // 6초마다 더해주기
                    ChangeDay();
                    _autosaveTimer = true;
                    // FlowTime.NowDay = Day;

                    // SecondHalfPerSecond += 20;

                    // if (SecondHalfPerSecond > LimitTime2)
                    // {
                    //     SecondHalfPerSecond = 20;
                    // }
                }

                // 2월은 오류방지를 위해 저장하지 말자.
                if (Month != 2)
                {
                    // 자동 저장은 3주차, 4주차에 UI가 없고, 시간이 흐르다가 하루의 초반에 저장 (4초후)
                    if (Time.time - PrevTime >= SecondHalfPerSecond / 5 && _autosaveTimer)
                    {
                        if (Year == 1 && Week == 3 && Day == 1) // 게임잼 튜토 전 한번 저장할 때 용
                        {
                            PlayerInfo.Instance.IsFirstClassEnd = true;
                        }

                        UiManager.CollectDataBtn();
                        Json.Instance.SaveDataInLocal();
                        Debug.Log("자동저장되었습니다."); // 자동저장 UI가 있으면 좋을까?
                        _autosaveTimer = false;
                    }
                }
            }
        }
    }

    //
    public void DrawTimeBar(Image img)
    {
        TimeBarImg = img;
    }

    public void IsLimitedGameTimeEnd()
    {
        if (IsGameEnd == true)
        {
            Debug.Log("게임 스토리 끝~");

        }
    }

    public void ShowGameTime()
    {
        m_TimeText.text = FlowTime.NowYear.ToString() + FlowTime.NowMonth + FlowTime.NowWeek + FlowTime.NowDay;
    }
}