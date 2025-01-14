using System.Collections;
//using System.Diagnostics.Eventing.Reader;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine.AI;

public enum Category
{
    Facility,
    GenreRoom,
    ClassRoom,
    Object,
    Free,
    Vacation,
    Nothing,
}

public class Destination : Action
{
    private string m_NowDestination = " ";
    private Category m_goingCategory;
    private bool m_isWaiting = false;
    private bool m_isTalking = false;
    private int m_RandomNumTalkAniIndex;

    private Transform m_canvas;

    private const int IdlePercent = 20;
    private const int ClassIdlePercent = 70;

    private const float DesChangeDis = 1;
    private const float DesStoreDis = 1;

    private NavMeshAgent m_Agent;
    private Animator m_Animator;

    private bool m_IsWaiting;

    private bool m_IsCoroutineRunning = false;
    private bool m_NowTalking = false;

    public override void OnStart()
    {
        m_canvas = GameObject.Find("InGameCanvas").transform;
        m_Agent = gameObject.GetComponent<NavMeshAgent>();
        m_Animator = gameObject.GetComponent<Animator>();
    }

    public override TaskStatus OnUpdate()
    {
        bool _bNewDestination = false;

        if (InGameTest.Instance == null)
        {
            return TaskStatus.Failure;
        }

        if (InGameTest.Instance.m_ClassState != ClassState.nothing && InGameTest.Instance.m_ClassState != ClassState.ClassEnd)
        {
            return TaskStatus.Failure;
        }

        if (gameObject.GetComponent<Student>().DoingValue != Student.Doing.FreeWalk)
        {
            return TaskStatus.Failure;
        }

        if (!PlayerInfo.Instance.IsFirstAcademySetting)
        {
            return TaskStatus.Failure;
        }

        // 2월 4주 방학
        if (GameTime.Instance.FlowTime.NowMonth == 2 && GameTime.Instance.FlowTime.NowWeek == 3 && GameTime.Instance.FlowTime.NowDay == 5)
        {
            _bNewDestination = SetVacation();
        }
        // 2월 1주~3주 목적지 설정
        else if (GameTime.Instance.FlowTime.NowMonth == 2 && (GameTime.Instance.FlowTime.NowWeek == 1 || GameTime.Instance.FlowTime.NowWeek == 2 || GameTime.Instance.FlowTime.NowWeek == 3))
        {
            if (m_goingCategory == Category.GenreRoom)
            {
                m_goingCategory = Category.Nothing;
                m_NowDestination = " ";
            }
            // 갈 곳이 정해지지 않은경우
            if (m_NowDestination == " ")
            {
                int randGoing = Random.Range(1, 101);

                // 강의실로 이동
                if (randGoing <= 50)
                {
                    _bNewDestination = SetClass();
                }
                // 시설(서점, 자습실, 매점, 라운지)로 이동
                else if (randGoing <= 80)
                {
                    _bNewDestination = SetFacility();
                }
                // 오브젝트(정수기, 자판기, 소파, 의자, 화분)로 이동
                else if (randGoing <= 90)
                {
                    _bNewDestination = SetObject();
                }
                // 복도 자유이동
                else
                {
                    _bNewDestination = SetFree();
                }
            }
            // 가는 곳이 정해져 있는 경우
            else
            {
                switch (m_goingCategory)
                {
                    case Category.ClassRoom:
                        _bNewDestination = SetClass();
                        break;

                    case Category.Facility:
                        _bNewDestination = SetFacility();
                        break;

                    case Category.Object:
                        _bNewDestination = SetObject();
                        break;

                    case Category.Free:
                        _bNewDestination = SetFree();
                        break;

                    default:
                        Debug.Log("목적지 설정 오류");
                        break;
                }
            }
        }
        // 2월이 아닐때.
        else
        {
            // 갈 곳이 정해지지 않은경우
            if (m_NowDestination == " ")
            {
                int randGoing = Random.Range(1, 101);

                // 시설(서점, 자습실, 매점, 라운지)로 이동
                if (randGoing <= 30)
                {
                    _bNewDestination = SetFacility();
                }
                // 장르방(RPG, 스포츠, 시뮬레이션, 액션, 리듬, 어드벤쳐, 슈팅, 퍼즐)으로 이동
                else if (randGoing <= 70)
                {
                    _bNewDestination = SetGenreRoom();
                }
                // 오브젝트(정수기, 자판기, 소파, 의자, 화분)로 이동
                else if (randGoing <= 90)
                {
                    _bNewDestination = SetObject();
                }
                // 복도 자유이동
                else
                {
                    _bNewDestination = SetFree();
                }
            }
            // 가는 곳이 정해져 있는 경우
            else
            {
                switch (m_goingCategory)
                {
                    case Category.Facility:
                        _bNewDestination = SetFacility();
                        break;

                    case Category.GenreRoom:
                        _bNewDestination = SetGenreRoom();
                        break;

                    case Category.Object:
                        _bNewDestination = SetObject();
                        break;

                    case Category.Free:
                        _bNewDestination = SetFree();
                        break;

                    default:
                        Debug.Log("목적지 설정 오류");
                        break;
                }
            }
        }

        return _bNewDestination ? TaskStatus.Success : TaskStatus.Failure;
    }

    bool SetFacility()
    {
        string entrance = " ";
        if (m_NowDestination.Length > 8)
        {
            entrance = m_NowDestination.Substring(m_NowDestination.Length - 8);
        }

        if (entrance == "Entrance")
        {
            Vector3 myPos = gameObject.transform.position;
            Vector3 desPos = m_Agent.destination;

            float dis = Vector3.Distance(myPos, desPos);

            if (dis < DesChangeDis)
            {
                if (m_NowDestination == "StoreEntrance")
                {
                    int pointNum = InteractionManager.Instance.EnterStore((int)InteractionManager.SpotName.Store);
                    if (pointNum < 0)
                    {
                        return false;
                    }

                    m_Agent.SetDestination(InteractionManager.Instance.StorePoints[pointNum].position);
                    gameObject.GetComponent<Student>().NowRoom = (int)InteractionManager.SpotName.Store;
                    m_NowDestination = "StorePoint";
                    m_goingCategory = Category.Facility;
                    return true;
                }
                else if (m_NowDestination == "BookEntrance")
                {
                    int pointNum = InteractionManager.Instance.EnterStore((int)InteractionManager.SpotName.BookStore);
                    if (pointNum < 0)
                    {
                        return false;
                    }

                    m_Agent.SetDestination(InteractionManager.Instance.BookStorePoints[pointNum].position);
                    gameObject.GetComponent<Student>().NowRoom = (int)InteractionManager.SpotName.BookStore;
                    m_NowDestination = "BookPoint";
                    m_goingCategory = Category.Facility;
                    return true;
                }
                else if (m_NowDestination == "StudyRoomEntrance")
                {
                    int seatNum = InteractionManager.Instance.EnterStudyRoom();
                    if (seatNum < 0)
                    {
                        StudentTalk("자습실 자리가 없네");
                        return false;
                    }

                    m_Agent.SetDestination(InteractionManager.Instance.StudyRoomSeats[seatNum].position);
                    gameObject.GetComponent<Student>().LookTransform =
                        InteractionManager.Instance.StudyRoomSeats[seatNum];
                    gameObject.GetComponent<Student>().NowRoom = (int)InteractionManager.SpotName.StudyRoom;
                    m_NowDestination = "StudyRoomSeat";
                    m_goingCategory = Category.Facility;
                    return true;
                }
                else if (m_NowDestination == "1LoungeEntrance")
                {
                    int seatNum = InteractionManager.Instance.EnterLounge(1);
                    if (seatNum < 0)
                    {
                        StudentTalk("라운지 자리가 없네");
                        return false;
                    }

                    m_Agent.SetDestination(InteractionManager.Instance.LoungeSeats[seatNum].position);
                    gameObject.GetComponent<Student>().LookTransform = InteractionManager.Instance.LoungeSeats[seatNum];
                    gameObject.GetComponent<Student>().NowRoom = (int)InteractionManager.SpotName.Lounge1;
                    m_NowDestination = "1LoungeSeat";
                    m_goingCategory = Category.Facility;
                    return true;
                }
                else if (m_NowDestination == "2LoungeEntrance")
                {
                    int seatNum = InteractionManager.Instance.EnterLounge(2);
                    if (seatNum < 0)
                    {
                        StudentTalk("라운지 자리가 없네");
                        return false;
                    }

                    m_Agent.SetDestination(InteractionManager.Instance.Lounge2Seats[seatNum].position);
                    gameObject.GetComponent<Student>().LookTransform = InteractionManager.Instance.Lounge2Seats[seatNum];
                    gameObject.GetComponent<Student>().NowRoom = (int)InteractionManager.SpotName.Lounge1;
                    m_NowDestination = "2LoungeSeat";
                    m_goingCategory = Category.Facility;
                    return true;
                }
            }
            return true;
        }

        if (m_NowDestination == "StorePoint")
        {
            Vector3 myPos = gameObject.transform.position;
            Vector3 desPos = m_Agent.destination;

            float dis = Vector3.Distance(myPos, desPos);

            if (dis < DesStoreDis)
            {
                int randomAnim = Random.Range(1, 3);
                if (randomAnim == 1)
                {
                    m_Animator.SetTrigger("ToIdle");
                }
                else
                {
                    m_Animator.SetTrigger("ToPointing");
                }
                gameObject.GetComponent<Student>().DoingValue = Student.Doing.Idle;
                m_Agent.isStopped = true;

                // 30퍼 확률로 이모티콘 또는 스크립트 출력
                int randomScript = Random.Range(1, 101);
                if (randomScript <= 20)
                {
                    int randomSelect = Random.Range(0,
                        ScriptsManager.Instance.FacilityScripts[(int)InteractionManager.SpotName.Store - 10].Count - 2);
                    StoreTalk(ScriptsManager.Instance.FacilityScripts[(int)InteractionManager.SpotName.Store - 10][randomSelect]);
                }
                else if (randomScript <= 30)
                {
                    int randomSelect = Random.Range(1, 3);
                    if (randomSelect == 1)
                    {
                        InteractionManager.Instance.ShowEmoticon(6, this.transform);
                    }
                    else
                    {
                        InteractionManager.Instance.ShowEmoticon(3, this.transform);
                    }
                }

                StartCoroutine(ChooseSomething());
            }
            return true;
        }

        else if (m_NowDestination == "StoreCounter")
        {
            Vector3 myPos = gameObject.transform.position;
            Vector3 desPos = m_Agent.destination;

            float dis = Vector3.Distance(myPos, desPos);

            if (dis < DesStoreDis)
            {
                if (InteractionManager.Instance.FacilityList[0].IsCalculating)
                {
                    gameObject.GetComponent<Student>().DoingValue = Student.Doing.Idle;
                    int waitingNum = InteractionManager.Instance.StoreWaitingQ.Count;
                    InteractionManager.Instance.StoreWaitingQ.Add(this.gameObject);
                    if (waitingNum >= 3)
                        waitingNum = 2;
                    
                    m_Animator.SetTrigger("ToIdle");
                    m_Agent.SetDestination(InteractionManager.Instance.StorePoints[4 + waitingNum].position);
                }
                else
                {
                    gameObject.GetComponent<Student>().DoingValue = Student.Doing.InFacility;
                    gameObject.GetComponent<Student>().NowRoom = (int)InteractionManager.SpotName.Store;
                    m_NowDestination = " ";
                    m_goingCategory = Category.Nothing;
                    m_IsWaiting = false;
                }
            }
            else
            {
                int randomAnim = Random.Range(1, 3);
                m_Animator.SetTrigger("ToWalk" + randomAnim);
            }
            return true;
        }

        else if (m_NowDestination == "BookPoint")
        {
            Vector3 myPos = gameObject.transform.position;
            Vector3 desPos = m_Agent.destination;

            float dis = Vector3.Distance(myPos, desPos);

            if (dis < DesStoreDis)
            {
                int randomAnim = Random.Range(1, 3);
                if (randomAnim == 1)
                {
                    m_Animator.SetTrigger("ToIdle");
                }
                else
                {
                    m_Animator.SetTrigger("ToPointing");
                }
                gameObject.GetComponent<Student>().DoingValue = Student.Doing.Idle;
                m_Agent.isStopped = true;

                // 30퍼 확률로 이모티콘 또는 스크립트 출력
                int randomScript = Random.Range(1, 101);
                if (randomScript <= 20)
                {
                    int randomSelect = Random.Range(0,
                        ScriptsManager.Instance.FacilityScripts[(int)InteractionManager.SpotName.BookStore - 10].Count - 2);
                    StoreTalk(ScriptsManager.Instance.FacilityScripts[(int)InteractionManager.SpotName.BookStore - 10][randomSelect]);
                }
                else if (randomScript <= 30)
                {
                    int randomSelect = Random.Range(1, 3);
                    if (randomSelect == 1)
                    {
                        InteractionManager.Instance.ShowEmoticon(2, this.transform);
                    }
                    else
                    {
                        InteractionManager.Instance.ShowEmoticon(8, this.transform);
                    }
                }

                StartCoroutine(ChooseBook());
            }
            return true;
        }

        else if (m_NowDestination == "BookCounter")
        {
            Vector3 myPos = gameObject.transform.position;
            Vector3 desPos = m_Agent.destination;

            float dis = Vector3.Distance(myPos, desPos);

            if (dis < DesStoreDis)
            {
                if (InteractionManager.Instance.FacilityList[1].IsCalculating)
                {
                    gameObject.GetComponent<Student>().DoingValue = Student.Doing.Idle;
                    int waitingNum = InteractionManager.Instance.BookStoreWaitingQ.Count;
                    InteractionManager.Instance.BookStoreWaitingQ.Add(this.gameObject);
                    if (waitingNum >= 3)
                        waitingNum = 2;

                    m_Animator.SetTrigger("ToIdle");
                    m_Agent.SetDestination(InteractionManager.Instance.BookStorePoints[4 + waitingNum].position);
                }
                else
                {
                    gameObject.GetComponent<Student>().DoingValue = Student.Doing.InFacility;
                    gameObject.GetComponent<Student>().NowRoom = (int)InteractionManager.SpotName.BookStore;
                    m_NowDestination = " ";
                    m_goingCategory = Category.Nothing;
                    m_IsWaiting = false;
                }
            }
            return true;
        }

        else if (m_NowDestination == "StudyRoomSeat")
        {
            Vector3 myPos = gameObject.transform.position;
            Vector3 desPos = m_Agent.destination;

            float dis = Vector3.Distance(myPos, desPos);

            if (dis < DesStoreDis)
            {
                m_Agent.isStopped = true;
                gameObject.GetComponent<Student>().DoingValue = Student.Doing.InFacility;
                gameObject.GetComponent<Student>().NowRoom = (int)InteractionManager.SpotName.StudyRoom;
                m_NowDestination = " ";
                m_goingCategory = Category.Nothing;
            }
            return true;
        }

        else if (m_NowDestination == "1LoungeSeat")
        {
            Vector3 myPos = gameObject.transform.position;
            Vector3 desPos = m_Agent.destination;

            float dis = Vector3.Distance(myPos, desPos);

            if (dis < DesStoreDis)
            {
                m_Agent.isStopped = true;
                gameObject.GetComponent<Student>().DoingValue = Student.Doing.InFacility;
                gameObject.GetComponent<Student>().NowRoom = (int)InteractionManager.SpotName.Lounge1;
                m_NowDestination = " ";
                m_goingCategory = Category.Nothing;
            }
            return true;
        }

        else if (m_NowDestination == "2LoungeSeat")
        {
            Vector3 myPos = gameObject.transform.position;
            Vector3 desPos = m_Agent.destination;

            float dis = Vector3.Distance(myPos, desPos);

            if (dis < DesStoreDis)
            {
                m_Agent.isStopped = true;
                gameObject.GetComponent<Student>().DoingValue = Student.Doing.InFacility;
                gameObject.GetComponent<Student>().NowRoom = (int)InteractionManager.SpotName.Lounge2;
                m_NowDestination = " ";
                m_goingCategory = Category.Nothing;
            }
            return true;
        }

        else
        {
            // 2월엔 이동 자습실 이동확률 증가
            if (GameTime.Instance.FlowTime.NowMonth == 2)
            {
                // 시설 4종류 랜덤
                float randomGoing = Random.Range(1f, 100f);
                // 매점 이동
                if (randomGoing <= 20/* - (gameObject.GetComponent<Student>().m_StudentStat.m_Health - 100) / 3f*/)
                {
                    m_NowDestination = "StoreEntrance";
                    m_goingCategory = Category.Facility;
                    m_Agent.SetDestination(InteractionManager.Instance.StoreEntrance.position);
                }
                // 서점 이동
                else if (randomGoing <= 40/* - (gameObject.GetComponent<Student>().m_StudentStat.m_Health - 100) / 3f * 2f*/)
                {
                    m_NowDestination = "BookEntrance";
                    m_Agent.SetDestination(InteractionManager.Instance.BookStoreEntrance.position);
                }
                // 자습실 이동
                else if (randomGoing <= 100/* - gameObject.GetComponent<Student>().m_StudentStat.m_Health - 100*/)
                {
                    m_NowDestination = "StudyRoomEntrance";
                    m_Agent.SetDestination(InteractionManager.Instance.StudyRoomEntrance.position);
                }
            }
            else
            {
                // 시설 4종류 랜덤
                float randomGoing = Random.Range(0f, 100f);
                // 매점 이동
                if (randomGoing <= 33 - (100 - gameObject.GetComponent<Student>().m_StudentStat.m_Health) / 3f)
                {
                    m_NowDestination = "StoreEntrance";
                    m_Agent.SetDestination(InteractionManager.Instance.StoreEntrance.position);
                }
                // 서점 이동
                else if (randomGoing <= 66 - (100 - gameObject.GetComponent<Student>().m_StudentStat.m_Health) / 3f * 2f)
                {
                    m_NowDestination = "BookEntrance";
                    m_Agent.SetDestination(InteractionManager.Instance.BookStoreEntrance.position);
                }
                // 자습실 이동
                else if (randomGoing <= 100 - (100 - gameObject.GetComponent<Student>().m_StudentStat.m_Health))
                {
                    m_NowDestination = "StudyRoomEntrance";
                    m_Agent.SetDestination(InteractionManager.Instance.StudyRoomEntrance.position);
                }
                // 라운지 이동
                else
                {
                    int randomLounge = Random.Range(1, 3);
                    m_NowDestination = randomLounge + "LoungeEntrance";
                    if (randomLounge == 1)
                    {
                        m_Agent.SetDestination(InteractionManager.Instance.LoungeEntrance.position);
                    }
                    else
                    {
                        m_Agent.SetDestination(InteractionManager.Instance.Lounge2Entrance.position);
                    }
                }
            }
            m_goingCategory = Category.Facility;
            gameObject.GetComponent<Student>().DoingValue = Student.Doing.FreeWalk;

            int randomAnim = Random.Range(1, 3);
            m_Animator.SetTrigger("ToWalk" + randomAnim);

            return true;
        }
    }

    bool SetGenreRoom()
    {
        m_goingCategory = Category.GenreRoom;

        string entrance = " ";
        if (m_NowDestination.Length > 8)
        {
            entrance = m_NowDestination.Substring(m_NowDestination.Length - 8);
        }

        if (entrance == "Entrance")
        {
            Vector3 myPos = gameObject.transform.position;
            Vector3 desPos = m_Agent.destination;

            float dis = Vector3.Distance(myPos, desPos);

            if (dis < DesChangeDis)
            {
                if (m_NowDestination == "PuzzleEntrance")
                {
                    int seatNum = InteractionManager.Instance.EnterRoom(InteractionManager.SpotName.PuzzleRoom);
                    if (seatNum < 0)
                    {
                        SeatCheck(seatNum);
                        return false;
                    }
                    m_Agent.SetDestination(InteractionManager.Instance.PuzzleSeats[seatNum].position);
                    gameObject.GetComponent<Student>().LookTransform = InteractionManager.Instance.PuzzleSeats[seatNum];
                    gameObject.GetComponent<Student>().NowRoom = (int)InteractionManager.SpotName.PuzzleRoom;
                    gameObject.GetComponent<Student>().m_NowSeatNum = seatNum;
                }
                else if (m_NowDestination == "SimulationEntrance")
                {
                    int seatNum = InteractionManager.Instance.EnterRoom(InteractionManager.SpotName.SimulationRoom);
                    if (seatNum < 0)
                    {
                        SeatCheck(seatNum);
                        return false;
                    }
                    m_Agent.SetDestination(InteractionManager.Instance.SimulationSeats[seatNum].position);
                    gameObject.GetComponent<Student>().LookTransform = InteractionManager.Instance.SimulationSeats[seatNum];
                    gameObject.GetComponent<Student>().NowRoom = (int)InteractionManager.SpotName.SimulationRoom;
                    gameObject.GetComponent<Student>().m_NowSeatNum = seatNum;
                }
                else if (m_NowDestination == "RhythmEntrance")
                {
                    int seatNum = InteractionManager.Instance.EnterRoom(InteractionManager.SpotName.RhythmRoom);
                    if (seatNum < 0)
                    {
                        SeatCheck(seatNum);
                        return false;
                    }
                    m_Agent.SetDestination(InteractionManager.Instance.RhythmSeats[seatNum].position);
                    gameObject.GetComponent<Student>().LookTransform = InteractionManager.Instance.RhythmSeats[seatNum];
                    gameObject.GetComponent<Student>().NowRoom = (int)InteractionManager.SpotName.RhythmRoom;
                    gameObject.GetComponent<Student>().m_NowSeatNum = seatNum;
                }
                else if (m_NowDestination == "AdventureEntrance")
                {
                    int seatNum = InteractionManager.Instance.EnterRoom(InteractionManager.SpotName.AdventureRoom);
                    if (seatNum < 0)
                    {
                        SeatCheck(seatNum);
                        return false;
                    }
                    m_Agent.SetDestination(InteractionManager.Instance.AdventureSeats[seatNum].position);
                    gameObject.GetComponent<Student>().LookTransform = InteractionManager.Instance.AdventureSeats[seatNum];
                    gameObject.GetComponent<Student>().NowRoom = (int)InteractionManager.SpotName.AdventureRoom;
                    gameObject.GetComponent<Student>().m_NowSeatNum = seatNum;
                }
                else if (m_NowDestination == "RPGEntrance")
                {
                    int seatNum = InteractionManager.Instance.EnterRoom(InteractionManager.SpotName.RPGRoom);
                    if (seatNum < 0)
                    {
                        SeatCheck(seatNum);
                        return false;
                    }
                    m_Agent.SetDestination(InteractionManager.Instance.RPGSeats[seatNum].position);
                    gameObject.GetComponent<Student>().LookTransform = InteractionManager.Instance.RPGSeats[seatNum];
                    gameObject.GetComponent<Student>().NowRoom = (int)InteractionManager.SpotName.RPGRoom;
                    gameObject.GetComponent<Student>().m_NowSeatNum = seatNum;
                }
                else if (m_NowDestination == "SportsEntrance")
                {
                    int seatNum = InteractionManager.Instance.EnterRoom(InteractionManager.SpotName.SportsRoom);
                    if (seatNum < 0)
                    {
                        SeatCheck(seatNum);
                        return false;
                    }
                    m_Agent.SetDestination(InteractionManager.Instance.SportsSeats[seatNum].position);
                    gameObject.GetComponent<Student>().LookTransform = InteractionManager.Instance.SportsSeats[seatNum];
                    gameObject.GetComponent<Student>().NowRoom = (int)InteractionManager.SpotName.SportsRoom;
                    gameObject.GetComponent<Student>().m_NowSeatNum = seatNum;
                }
                else if (m_NowDestination == "ActionEntrance")
                {
                    int seatNum = InteractionManager.Instance.EnterRoom(InteractionManager.SpotName.ActionRoom);
                    if (seatNum < 0)
                    {
                        SeatCheck(seatNum);
                        return false;
                    }
                    m_Agent.SetDestination(InteractionManager.Instance.ActionSeats[seatNum].position);
                    gameObject.GetComponent<Student>().LookTransform = InteractionManager.Instance.ActionSeats[seatNum];
                    gameObject.GetComponent<Student>().NowRoom = (int)InteractionManager.SpotName.ActionRoom;
                    gameObject.GetComponent<Student>().m_NowSeatNum = seatNum;
                }
                else if (m_NowDestination == "ShootingEntrance")
                {
                    int seatNum = InteractionManager.Instance.EnterRoom(InteractionManager.SpotName.ShootingRoom);
                    if (seatNum < 0)
                    {
                        SeatCheck(seatNum);
                        return false;
                    }
                    m_Agent.SetDestination(InteractionManager.Instance.ShootingSeats[seatNum].position);
                    gameObject.GetComponent<Student>().LookTransform = InteractionManager.Instance.ShootingSeats[seatNum];
                    gameObject.GetComponent<Student>().NowRoom = (int)InteractionManager.SpotName.ShootingRoom;
                    gameObject.GetComponent<Student>().m_NowSeatNum = seatNum;
                }
                m_NowDestination = "Seat";
            }
            gameObject.GetComponent<Student>().DoingValue = Student.Doing.FreeWalk;
            return true;
        }

        if (m_NowDestination == "Seat")
        {
            Vector3 myPos = gameObject.transform.position;
            Vector3 desPos = m_Agent.destination;

            float dis = Vector3.Distance(myPos, desPos);

            if (dis < DesChangeDis)
            {
                m_Animator.SetTrigger("Idle");

                gameObject.GetComponent<Student>().DoingValue = Student.Doing.InGenreRoom;
                m_NowDestination = " ";
                m_goingCategory = Category.Nothing;
                m_Agent.isStopped = true;
            }
            return true;
        }

        else
        {
            int randomGenre = Random.Range(0, 8);
            switch (randomGenre)
            {
                case 0:
                    m_NowDestination = "PuzzleEntrance";
                    m_Agent.SetDestination(InteractionManager.Instance.GenreRoomEntrances[0].position);
                    break;

                case 1:
                    m_NowDestination = "SimulationEntrance";
                    m_Agent.SetDestination(InteractionManager.Instance.GenreRoomEntrances[1].position);
                    break;

                case 2:
                    m_NowDestination = "RhythmEntrance";
                    m_Agent.SetDestination(InteractionManager.Instance.GenreRoomEntrances[2].position);
                    break;

                case 3:
                    m_NowDestination = "AdventureEntrance";
                    m_Agent.SetDestination(InteractionManager.Instance.GenreRoomEntrances[3].position);
                    break;

                case 4:
                    m_NowDestination = "RPGEntrance";
                    m_Agent.SetDestination(InteractionManager.Instance.GenreRoomEntrances[4].position);
                    break;

                case 5:
                    m_NowDestination = "SportsEntrance";
                    m_Agent.SetDestination(InteractionManager.Instance.GenreRoomEntrances[5].position);
                    break;

                case 6:
                    m_NowDestination = "ActionEntrance";
                    m_Agent.SetDestination(InteractionManager.Instance.GenreRoomEntrances[6].position);
                    break;

                case 7:
                    m_NowDestination = "ShootingEntrance";
                    m_Agent.SetDestination(InteractionManager.Instance.GenreRoomEntrances[7].position);
                    break;
            }
            gameObject.GetComponent<Student>().DoingValue = Student.Doing.FreeWalk;
            m_goingCategory = Category.GenreRoom;

            int randomAnim = Random.Range(1, 3);
            m_Animator.SetTrigger("ToWalk" + randomAnim);

            return true;
        }
    }

    bool SetObject()
    {
        m_goingCategory = Category.Object;

        if (m_NowDestination == "VendingMachine")
        {
            Vector3 myPos = gameObject.transform.position;
            Vector3 desPos = m_Agent.destination;

            float dis = Vector3.Distance(myPos, desPos);

            if (dis < DesChangeDis)
            {
                gameObject.GetComponent<Student>().DoingValue = Student.Doing.ObjectInteraction;
                gameObject.GetComponent<Student>().NowRoom = (int)InteractionManager.SpotName.VendingMachine;
                m_NowDestination = " ";
                m_goingCategory = Category.Object;
            }
            return true;
        }
        else if (m_NowDestination == "Pot")
        {
            Vector3 myPos = gameObject.transform.position;
            Vector3 desPos = m_Agent.destination;

            float dis = Vector3.Distance(myPos, desPos);

            if (dis < DesChangeDis)
            {
                gameObject.GetComponent<Student>().DoingValue = Student.Doing.ObjectInteraction;
                gameObject.GetComponent<Student>().NowRoom = (int)InteractionManager.SpotName.Pot;
                m_NowDestination = " ";
                m_goingCategory = Category.Object;
            }
            return true;
        }
        else if (m_NowDestination == "AmusementMachine")
        {
            Vector3 myPos = gameObject.transform.position;
            Vector3 desPos = m_Agent.destination;

            float dis = Vector3.Distance(myPos, desPos);

            if (dis < DesChangeDis)
            {
                m_Animator.SetTrigger("ToIdle");
                gameObject.GetComponent<Student>().DoingValue = Student.Doing.ObjectInteraction;
                gameObject.GetComponent<Student>().NowRoom = (int)InteractionManager.SpotName.AmusementMachine;
                m_NowDestination = " ";
                m_goingCategory = Category.Object;
            }
            return true;
        }
        else if (m_NowDestination == "WaterPurifier")
        {
            Vector3 myPos = gameObject.transform.position;
            Vector3 desPos = m_Agent.destination;

            float dis = Vector3.Distance(myPos, desPos);

            if (dis < DesChangeDis)
            {
                m_Animator.SetTrigger("ToIdle");
                gameObject.GetComponent<Student>().DoingValue = Student.Doing.ObjectInteraction;
                gameObject.GetComponent<Student>().NowRoom = (int)InteractionManager.SpotName.WaterPurifier;
                m_NowDestination = " ";
                m_goingCategory = Category.Object;
            }
            return true;
        }
        else if (m_NowDestination == "NoticeBoard")
        {
            Vector3 myPos = gameObject.transform.position;
            Vector3 desPos = m_Agent.destination;

            float dis = Vector3.Distance(myPos, desPos);

            if (dis < DesChangeDis)
            {
                m_Animator.SetTrigger("ToIdle");
                gameObject.GetComponent<Student>().DoingValue = Student.Doing.ObjectInteraction;
                gameObject.GetComponent<Student>().NowRoom = (int)InteractionManager.SpotName.NoticeBoard;
                m_NowDestination = " ";
                m_goingCategory = Category.Object;
            }
            return true;
        }
        else
        {
            int randomObject = Random.Range(0, 5);      // 5종류 오브젝트 중 랜덤 선택
            int randomSelect;

            Vector3 objectPoint = Vector3.zero;

            if (randomObject == 0)
            {
                randomSelect = Random.Range(0, InteractionManager.Instance.VendingMachines.Length);
                if (!InteractionManager.Instance.VendingMachineList[randomSelect].IsUsed)
                {
                    objectPoint = InteractionManager.Instance.VendingMachines[randomSelect].position;
                    gameObject.GetComponent<Student>().LookTransform = InteractionManager.Instance.VendingMachines[randomSelect];
                    m_NowDestination = "VendingMachine";
                    InteractionManager.Instance.UseObject((int)InteractionManager.SpotName.VendingMachine, randomSelect);
                    gameObject.GetComponent<Student>().UsingObjNum = randomSelect;
                }
            }
            else if (randomObject == 1)
            {
                randomSelect = Random.Range(0, InteractionManager.Instance.Pots.Length);
                if (!InteractionManager.Instance.PotList[randomSelect].IsUsed)
                {
                    objectPoint = InteractionManager.Instance.Pots[randomSelect].position;
                    gameObject.GetComponent<Student>().LookTransform = InteractionManager.Instance.Pots[randomSelect];
                    m_NowDestination = "Pot";
                    InteractionManager.Instance.UseObject((int)InteractionManager.SpotName.Pot, randomSelect);
                    gameObject.GetComponent<Student>().UsingObjNum = randomSelect;
                }
            }
            else if (randomObject == 2)
            {
                randomSelect = Random.Range(0, InteractionManager.Instance.AmusementMachines.Length);
                if (!InteractionManager.Instance.AmusementMachineList[randomSelect].IsUsed)
                {
                    objectPoint = InteractionManager.Instance.AmusementMachines[randomSelect].position;
                    gameObject.GetComponent<Student>().LookTransform = InteractionManager.Instance.AmusementMachines[randomSelect];
                    m_NowDestination = "AmusementMachine";
                    InteractionManager.Instance.UseObject((int)InteractionManager.SpotName.AmusementMachine, randomSelect);
                    gameObject.GetComponent<Student>().UsingObjNum = randomSelect;
                }
            }
            else if (randomObject == 3)
            {
                randomSelect = Random.Range(0, InteractionManager.Instance.WaterPurifier.Length);
                if (!InteractionManager.Instance.WaterPurifierList[randomSelect].IsUsed)
                {
                    objectPoint = InteractionManager.Instance.WaterPurifier[randomSelect].position;
                    gameObject.GetComponent<Student>().LookTransform = InteractionManager.Instance.WaterPurifier[randomSelect];
                    m_NowDestination = "WaterPurifier";
                    InteractionManager.Instance.UseObject((int)InteractionManager.SpotName.WaterPurifier, randomSelect);
                    gameObject.GetComponent<Student>().UsingObjNum = randomSelect;
                }
            }
            else if (randomObject == 4)
            {
                randomSelect = Random.Range(0, InteractionManager.Instance.NoticeBoard.Length);
                if (!InteractionManager.Instance.NoticeBoardList[randomSelect].IsUsed)
                {
                    objectPoint = InteractionManager.Instance.NoticeBoard[randomSelect].position;
                    gameObject.GetComponent<Student>().LookTransform = InteractionManager.Instance.NoticeBoard[randomSelect];
                    m_NowDestination = "NoticeBoard";
                    InteractionManager.Instance.UseObject((int)InteractionManager.SpotName.NoticeBoard, randomSelect);
                    gameObject.GetComponent<Student>().UsingObjNum = randomSelect;
                }
            }
            m_Agent.SetDestination(objectPoint);
            gameObject.GetComponent<Student>().DoingValue = Student.Doing.FreeWalk;

            int randomAnim = Random.Range(1, 3);
            m_Animator.SetTrigger("ToWalk" + randomAnim);

            return true;
        }
    }

    bool SetFree()
    {
        m_goingCategory = Category.Free;

        if (m_NowDestination == " ")
        {
            int randomFreePoint = Random.Range(0, InGameTest.Instance.FreeWalkPointList.Count);

            Vector3 resPoint = InGameTest.Instance.FreeWalkPointList[randomFreePoint];

            m_NowDestination = "FreeWalk" + (randomFreePoint + 1);
            m_Agent.SetDestination(resPoint);
            gameObject.GetComponent<Student>().DoingValue = Student.Doing.FreeWalk;

            int randomAnim = Random.Range(1, 3);
            m_Animator.SetTrigger("ToWalk" + randomAnim);

            return true;
        }
        else
        {
            Vector3 myPos = gameObject.transform.position;
            Vector3 desPos = m_Agent.destination;

            float dis = Vector3.Distance(myPos, desPos);

            if (dis < DesChangeDis)
            {
                int randomAnim = Random.Range(1, 3);

                if (randomAnim == 1)
                {
                    m_Animator.SetTrigger("ToIdleYawn");
                }
                else
                {
                    m_Animator.SetTrigger("ToIdle");
                }

                int scriptsPlay = Random.Range(1, 101);

                if (scriptsPlay <= 30)
                {
                    if (GameTime.Instance.FlowTime.NowMonth == 2)
                    {
                        int randomScripts = Random.Range(0, ScriptsManager.Instance.FreeWalkScripts2.Count);
                        StudentTalk(ScriptsManager.Instance.FreeWalkScripts2[randomScripts]);
                    }
                    else
                    {
                        int randomScripts = Random.Range(0, ScriptsManager.Instance.FreeWalkScripts.Count);
                        StudentTalk(ScriptsManager.Instance.FreeWalkScripts[randomScripts]);
                    }
                }

                int randomValue = Random.Range(0, 101);
                if (randomValue <= IdlePercent)
                {
                    gameObject.GetComponent<Student>().DoingValue = Student.Doing.Idle;
                    m_Agent.isStopped = true;

                    StartCoroutine(StudentWait());
                }
                else
                {
                    gameObject.GetComponent<Student>().DoingValue = Student.Doing.FreeWalk;
                    m_NowDestination = " ";
                    m_goingCategory = Category.Nothing;
                }
            }
            return true;
        }
    }

    bool SetClass()
    {
        m_goingCategory = Category.ClassRoom;

        if (m_NowDestination == " ")
        {
            int randomAnim = Random.Range(1, 3);
            m_Animator.SetTrigger("ToWalk" + randomAnim);
            m_NowDestination = "ClassSeat";
            Vector3 seatPos = (Vector3)gameObject.GetComponent<BehaviorTree>().ExternalBehavior.GetVariable("MyClassSeat").GetValue();
            m_Agent.SetDestination(seatPos);
            gameObject.GetComponent<Student>().DoingValue = Student.Doing.FreeWalk;

            return true;
        }
        else
        {
            Vector3 myPos = gameObject.transform.position;
            Vector3 desPos = m_Agent.destination;

            float dis = Vector3.Distance(myPos, desPos);

            if (dis < DesChangeDis)
            {
                m_Animator.SetTrigger("ToIdle");

                int scriptsPlay = Random.Range(1, 101);

                if (scriptsPlay <= 30)
                {
                    int randomScripts = Random.Range(0, ScriptsManager.Instance.ClassScripts[(int)gameObject.GetComponent<Student>().m_StudentStat.m_StudentType + 3].Count);

                    StudentTalk(ScriptsManager.Instance.ClassScripts[(int)gameObject.GetComponent<Student>().m_StudentStat.m_StudentType + 3][randomScripts]);
                }
                gameObject.GetComponent<Student>().DoingValue = Student.Doing.Idle;

                m_Agent.isStopped = true;
                StartCoroutine(StudentWait());
            }
            return true;
        }
    }

    bool SetVacation()
    {
        m_goingCategory = Category.Vacation;

        if (m_NowDestination == "AcademyEntrance")
        {
            Vector3 myPos = gameObject.transform.position;
            Vector3 desPos = m_Agent.destination;

            float dis = Vector3.Distance(myPos, desPos);

            if (dis < DesChangeDis)
            {
                m_NowDestination = "VacationPoint";
                int randomPoint = Random.Range(1, 4);
                Vector3 pointPos = GameObject.Find("VacationPoint" + randomPoint).transform.position;
                m_Agent.SetDestination(pointPos);
                gameObject.GetComponent<Student>().DoingValue = Student.Doing.FreeWalk;

                if (!m_IsCoroutineRunning)
                {
                    StartCoroutine(VacationMoveTalk());
                }
            }
            return true;

        }
        else if (m_NowDestination == "VacationPoint")
        {
            Vector3 myPos = gameObject.transform.position;
            Vector3 desPos = m_Agent.destination;

            float dis = Vector3.Distance(myPos, desPos);

            if (dis < DesChangeDis)
            {
                m_Animator.SetTrigger("ToIdle");
                m_NowDestination = " ";
                gameObject.GetComponent<Student>().DoingValue = Student.Doing.Vacation;
            }
            return true;
        }
        else
        {
            int randomAnim = Random.Range(1, 3);
            m_Animator.SetTrigger("ToWalk" + randomAnim);
            m_NowDestination = "AcademyEntrance";
            Vector3 entrancePos = GameObject.Find("AcademyEntrance").transform.position;
            m_Agent.SetDestination(entrancePos);
            gameObject.GetComponent<Student>().DoingValue = Student.Doing.FreeWalk;

            if (!m_IsCoroutineRunning)
            {
                int randomTalk = Random.Range(1, 19);
                if (randomTalk <= 6)
                {
                    m_IsCoroutineRunning = true;
                    StartCoroutine(VacationTalk());
                }
            }

            return true;
        }
    }

    IEnumerator VacationTalk()
    {
        yield return new WaitForSeconds(2f);

        int randomScript = Random.Range(0, ScriptsManager.Instance.VacationScripts[0].Count);

        GameObject chat = InGameObjectPool.GetChatObject(m_canvas);

        chat.GetComponent<FollowTarget>().m_Target = gameObject.transform.GetChild(0).transform;
        chat.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = ScriptsManager.Instance.VacationScripts[0][randomScript];

        StartCoroutine(VacationTalkEnd(chat));
    }

    IEnumerator VacationTalkEnd(GameObject chat)
    {
        yield return new WaitForSeconds(2f);

        m_IsCoroutineRunning = false;
        InGameObjectPool.ReturnChatObject(chat);
    }

    IEnumerator VacationMoveTalk()
    {
        m_IsCoroutineRunning = true;

        while (true)
        {
            int random = Random.Range(1, 101);

            float randomCoolTime = Random.Range(1f, 3f);
            if (random <= 30 && !m_NowTalking)
            {
                int randomScript = Random.Range(0, ScriptsManager.Instance.VacationScripts[1].Count);

                GameObject chat = InGameObjectPool.GetChatObject(m_canvas);

                chat.GetComponent<FollowTarget>().m_Target = gameObject.transform.GetChild(0).transform;
                chat.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = ScriptsManager.Instance.VacationScripts[1][randomScript];
                m_NowTalking = true;
                this.GetComponent<Student>().ChatObject = chat;

                StartCoroutine(VacationMoveTalkEnd(chat, randomCoolTime));
            }

            yield return new WaitForSeconds(randomCoolTime);
        }
    }

    IEnumerator VacationMoveTalkEnd(GameObject chat, float returnTime)
    {
        yield return new WaitForSeconds(returnTime);

        InGameObjectPool.ReturnChatObject(chat);
        this.GetComponent<Student>().ChatObject = null;
        m_NowTalking = false;
        m_IsCoroutineRunning = false;
    }

    private void SeatCheck(int num)
    {
        gameObject.GetComponent<Student>().DoingValue = Student.Doing.Idle;
        m_Animator.SetTrigger("ToIdle");
        if (num == -1)
        {
            StudentTalk("자리가없네");
        }
        else if (num == -2)
        {
            int randomScript = Random.Range(0, ScriptsManager.Instance.RepairScripts.Count);
            StudentTalk(ScriptsManager.Instance.RepairScripts[randomScript]);
        }
    }

    private void StoreWait(string sentence)
    {
        GameObject chat = InGameObjectPool.GetChatObject(m_canvas);

        chat.GetComponent<FollowTarget>().m_Target = gameObject.transform.GetChild(0).transform;
        chat.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = sentence;

        StartCoroutine(WaitEnd(chat));
    }

    IEnumerator WaitEnd(GameObject chat)
    {
        yield return new WaitForSeconds(3f);

        InGameObjectPool.ReturnChatObject(chat);
    }

    private void StoreTalk(string sentence)
    {
        GameObject chat = InGameObjectPool.GetChatObject(m_canvas);

        chat.GetComponent<FollowTarget>().m_Target = gameObject.transform.GetChild(0).transform;
        chat.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = sentence;
        StartCoroutine(StoreTalkEnd(chat));
    }

    IEnumerator StoreTalkEnd(GameObject chatBox)
    {
        yield return new WaitForSeconds(2f);

        InGameObjectPool.ReturnChatObject(chatBox);
    }

    private void StudentTalk(string sentence)
    {
        GameObject chat = InGameObjectPool.GetChatObject(m_canvas);

        chat.GetComponent<FollowTarget>().m_Target = gameObject.transform.GetChild(0).transform;
        chat.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = sentence;
        m_NowDestination = " ";
        m_goingCategory = Category.Nothing;
        m_Agent.isStopped = true;

        m_RandomNumTalkAniIndex = Random.Range(0, 2);

        StartCoroutine(TalkEnd(chat));
    }

    IEnumerator ChooseSomething()
    {
        yield return new WaitForSeconds(2f);

        m_Agent.isStopped = false;
        gameObject.GetComponent<Student>().DoingValue = Student.Doing.FreeWalk;
        m_NowDestination = "StoreCounter";

        int randomAnim = Random.Range(1, 3);
        m_Animator.SetTrigger("ToWalk" + randomAnim);
        m_goingCategory = Category.Facility;
        m_Agent.SetDestination(InteractionManager.Instance.StorePoints[3].position);
        gameObject.GetComponent<Student>().LookTransform = InteractionManager.Instance.StorePoints[3];
    }

    IEnumerator ChooseBook()
    {
        yield return new WaitForSeconds(2f);

        m_Agent.isStopped = false;
        gameObject.GetComponent<Student>().DoingValue = Student.Doing.FreeWalk;
        m_NowDestination = "BookCounter";

        int randomAnim = Random.Range(1, 3);
        m_Animator.SetTrigger("ToWalk" + randomAnim);
        m_goingCategory = Category.Facility;
        m_Agent.SetDestination(InteractionManager.Instance.BookStorePoints[3].position);
        gameObject.GetComponent<Student>().LookTransform = InteractionManager.Instance.BookStorePoints[3];
    }

    IEnumerator StudentWait()
    {
        yield return new WaitForSeconds(3f);
        
        m_Animator.SetTrigger("ToIdle");
        m_Agent.isStopped = false;
        gameObject.GetComponent<Student>().DoingValue = Student.Doing.FreeWalk;

        m_NowDestination = " ";
        m_goingCategory = Category.Nothing;
    }

    IEnumerator TalkEnd(GameObject chatBox)
    {
        yield return new WaitForSeconds(2f);

        InGameObjectPool.ReturnChatObject(chatBox);
        m_Agent.isStopped = false;
        gameObject.GetComponent<Student>().DoingValue = Student.Doing.FreeWalk;
    }
}