using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using StatData.Runtime;
using Newtonsoft.Json;
using BehaviorDesigner.Runtime;
using System.Linq;
using System.Reflection;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

/// <summary>
/// 학생과 교수의 생성, 삭제를 관리해주는 스크립트
/// 
/// 원본은 프리팹으로 리소스로서 존재하며, 끌어넣기를 썼다.
/// 
/// 2022. 10. 31 Ocean
/// </summary>
/// 
public class StudentNameListClass
{
    public List<string> _studentName;
}

public class ObjectManager : MonoBehaviour
{
    private static ObjectManager _instance = null;

    [SerializeField] private ExternalBehaviorTree studentTree;

    public List<Student> m_StudentList = new List<Student>(); // 현재 학원에 있는 학생 리스트
    public List<Instructor> m_InstructorList = new List<Instructor>(); // 현재 학원의 강사 리스트
    //public List<GameObject> m_StudentBehaviorList = new List<GameObject>();
    private List<string> m_OriginalFeMaleLastName = new List<string>();
    private List<string> m_OriginalMaleLastName = new List<string>();
    private List<string> m_OriginalFirstName = new List<string>();

    private List<string> m_FeMaleName = new List<string>();
    private List<string> m_MaleName = new List<string>();

    private List<StudentRandomStatRange> m_StudentRandomStatRangeList = new List<StudentRandomStatRange>();
    private List<StudentPersonality> m_StudentPersonality = new List<StudentPersonality>();
    private List<StudentBasicSkills> m_SkillRangeList = new List<StudentBasicSkills>();
    private List<IncreaseStat> m_IncreaseStatData = new List<IncreaseStat>();

    public List<List<int>> m_Friendship = new List<List<int>>(); // 모든 학생과 강사의 친밀도 (학생18 강사3)

    // 각 과별 학생 수 파악
    private int m_nArt;
    private int m_nProgramming;
    private int m_nProductManager;

    // 현재 생성되어있는 강사 오브젝트(임의로 데이터 연결 하려 만듬)
    [SerializeField] private GameObject Professor1;
    [SerializeField] private GameObject Professor2;
    [SerializeField] private GameObject Professor3;
    // 모든 교사 정보를 담은 거니까 이거는 리스트<professorStat>
    // [SerializeField] private List<ProfessorStat> AllProfessor = new List<ProfessorStat>();

    [SerializeField] private Transform AcademyEntrance;
    [SerializeField] private Transform SpawnPoint1;
    [SerializeField] private Transform SpawnPoint2;

    private List<ExternalBehavior> m_ArtBehaviorPool = new List<ExternalBehavior>();
    private List<ExternalBehavior> m_ProductManagerBehaviorPool = new List<ExternalBehavior>();
    private List<ExternalBehavior> m_ProgrammingBehaviorPool = new List<ExternalBehavior>();

    public List<ExternalBehavior> ArtBehaviorPool
    {
        get;
        private set;
    }
    public List<ExternalBehavior> ProductManagerBehaviorPool
    {
        get;
        private set;
    }
    public List<ExternalBehavior> ProgrammingBehaviorPool
    {
        get;
        private set;
    }

    private bool LoadStudentOnce = true;

    public static ObjectManager Instance
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
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ArtBehaviorPool = new List<ExternalBehavior>();
        ProductManagerBehaviorPool = new List<ExternalBehavior>();
        ProgrammingBehaviorPool = new List<ExternalBehavior>();

        InitStudentStatRangeByRank();
        InitStudentPersonality();
        InitFemaleAndMaleName();
        InitBasicSkills();
        InitExternalBehaviorPool();
        InitIncreaseStatData();

        m_InstructorList.Add(Professor1.GetComponent<Instructor>());
        m_InstructorList.Add(Professor2.GetComponent<Instructor>());
        m_InstructorList.Add(Professor3.GetComponent<Instructor>());
    }

    /// 성격에 따른 수업 가중치 
    private void InitIncreaseStatData()
    {
        m_IncreaseStatData.Add(new IncreaseStat("평범", 1, 1, 1, 1, 1));
        m_IncreaseStatData.Add(new IncreaseStat("완벽주의자", 1, 1, 1, 1, 1));
        m_IncreaseStatData.Add(new IncreaseStat("먹쟁이", 1.2f, 1.2f, 1.1f, 0.7f, 1.2f));
        m_IncreaseStatData.Add(new IncreaseStat("괴짜", 1, 1.3f, 0.7f, 1.3f, 0.7f));
        m_IncreaseStatData.Add(new IncreaseStat("현실주의자", 0.7f, 1, 1.3f, 0.7f, 1.3f));
        m_IncreaseStatData.Add(new IncreaseStat("낭만주의자", 0.8f, 0.8f, 1.2f, 1, 1.2f));
        m_IncreaseStatData.Add(new IncreaseStat("낙관주의자", 1.3f, 0.9f, 0.8f, 1, 1));
        m_IncreaseStatData.Add(new IncreaseStat("동물 애호가", 1.2f, 1.1f, 0.9f, 0.8f, 1));
        m_IncreaseStatData.Add(new IncreaseStat("선량함", 1.1f, 1, 1, 1.1f, 0.8f));
        m_IncreaseStatData.Add(new IncreaseStat("자아도취", 0.9f, 1.1f, 0.9f, 1, 1.1f));
        m_IncreaseStatData.Add(new IncreaseStat("사악함", 1, 1.1f, 1.3f, 0.9f, 0.7f));
        m_IncreaseStatData.Add(new IncreaseStat("지저분함", 1.3f, 0.8f, 0.8f, 1.1f, 1));
        m_IncreaseStatData.Add(new IncreaseStat("인싸", 1, 0.7f, 1.1f, 0.9f, 1.3f));
        m_IncreaseStatData.Add(new IncreaseStat("빠른 학습자", 1, 1, 1, 1, 1));
        m_IncreaseStatData.Add(new IncreaseStat("사색가", 1.2f, 1, 1.1f, 0.8f, 0.9f));
        m_IncreaseStatData.Add(new IncreaseStat("수집가", 1.3f, 0.7f, 1f, 0.8f, 1.2f));
        m_IncreaseStatData.Add(new IncreaseStat("야심가", 1, 0.9f, 0.7f, 1.3f, 1.1f));
        m_IncreaseStatData.Add(new IncreaseStat("궤변론자", 0.8f, 0.8f, 1.2f, 1, 1.2f));
        m_IncreaseStatData.Add(new IncreaseStat("분위기 메이커", 0.9f, 1, 1, 1.2f, 0.9f));
        m_IncreaseStatData.Add(new IncreaseStat("식물 애호가", 0.7f, 1.2f, 1, 1.2f, 0.9f));
    }

    private void InitStudentStatRangeByRank()
    {
        m_StudentRandomStatRangeList.Add(new StudentRandomStatRange(Rank.SSS, 25, 37, 18, 26));
        m_StudentRandomStatRangeList.Add(new StudentRandomStatRange(Rank.SS, 22, 34, 15, 24));
        m_StudentRandomStatRangeList.Add(new StudentRandomStatRange(Rank.S, 19, 31, 13, 22));
        m_StudentRandomStatRangeList.Add(new StudentRandomStatRange(Rank.A, 16, 28, 11, 20));
        m_StudentRandomStatRangeList.Add(new StudentRandomStatRange(Rank.B, 13, 25, 9, 18));
        m_StudentRandomStatRangeList.Add(new StudentRandomStatRange(Rank.C, 10, 22, 7, 15));
        m_StudentRandomStatRangeList.Add(new StudentRandomStatRange(Rank.D, 7, 19, 5, 13));
        m_StudentRandomStatRangeList.Add(new StudentRandomStatRange(Rank.E, 4, 16, 3, 11));
        m_StudentRandomStatRangeList.Add(new StudentRandomStatRange(Rank.F, 1, 13, 1, 9));
    }

    private void InitStudentPersonality()
    {
        m_StudentPersonality.Add(new StudentPersonality(1, "평범", "지극히 평범합니다."));
        m_StudentPersonality.Add(new StudentPersonality(2, "완벽주의자", "짝짝이 양말을 신으면 3일내내 악몽을꿉니다."));
        m_StudentPersonality.Add(new StudentPersonality(3, "먹쟁이", "배고픈 소크라테스보다 배부른 돼지가 되기로 했습니다."));
        m_StudentPersonality.Add(new StudentPersonality(4, "괴짜", "1+1은 창문이라고 우깁니다."));
        m_StudentPersonality.Add(new StudentPersonality(5, "현실주의자", "MBTI를 안믿습니다."));
        m_StudentPersonality.Add(new StudentPersonality(6, "낭만주의자", "연인과 달빛아래서 블루스추는걸 즐깁니다."));
        m_StudentPersonality.Add(new StudentPersonality(7, "낙관주의자", "머리에 꽃달았습니다."));
        m_StudentPersonality.Add(new StudentPersonality(8, "동물 애호가", "집에 168종의 동물을 키우고 있습니다."));
        m_StudentPersonality.Add(new StudentPersonality(9, "선량함", "착한아이라는 소리를 많이 듣습니다."));
        m_StudentPersonality.Add(new StudentPersonality(10, "자아도취", "난 너무이뻐 난 너무잘생겼어 난 최고야"));
        m_StudentPersonality.Add(new StudentPersonality(11, "사악함", "지금 옆 친구 가방을 노려보는 중입니다."));
        m_StudentPersonality.Add(new StudentPersonality(12, "지저분함", "세상이 너무 깨끗한거라고 우깁니다."));
        m_StudentPersonality.Add(new StudentPersonality(13, "인싸", "혼자 놀고 혼자 밥먹는걸 이해못합니다."));
        m_StudentPersonality.Add(new StudentPersonality(14, "빠른 학습자", "하나를 알면 열을 깨우칩니다."));
        m_StudentPersonality.Add(new StudentPersonality(15, "사색가", "얘는 뭔생각을 하고있는거지?"));
        m_StudentPersonality.Add(new StudentPersonality(16, "수집가", "최근에 인어 비늘을 수집하겠다고 난리입니다."));
        m_StudentPersonality.Add(new StudentPersonality(17, "야심가", "지구정복이 꿈입니다."));
        m_StudentPersonality.Add(new StudentPersonality(18, "궤변론자", "지구는 네모납니다. 모르셨다고요? 무지하기는."));
        m_StudentPersonality.Add(new StudentPersonality(19, "분위기 메이커", "모임에 필수 유형입니다. 근데 연애는 못하는.."));
        m_StudentPersonality.Add(new StudentPersonality(20, "식물 애호가", "선인장 쓰다듬다가 찔렸는데 웃고있어요!"));
    }

    private void InitFemaleAndMaleName()
    {
        foreach (StudentLastName name in AllOriginalJsonData.Instance.OriginalStudentLastNameData)
        {
            if (name.IsMale)
                m_OriginalMaleLastName.Add(name.Name);
            else
                m_OriginalFeMaleLastName.Add(name.Name);
        }

        m_OriginalFirstName.Add("김");
        m_OriginalFirstName.Add("이");
        m_OriginalFirstName.Add("박");
        m_OriginalFirstName.Add("최");
        m_OriginalFirstName.Add("정");
        m_OriginalFirstName.Add("강");
        m_OriginalFirstName.Add("조");
        m_OriginalFirstName.Add("윤");
        m_OriginalFirstName.Add("장");
        m_OriginalFirstName.Add("임");
        m_OriginalFirstName.Add("한");
        m_OriginalFirstName.Add("오");
        m_OriginalFirstName.Add("서");
        m_OriginalFirstName.Add("신");
        m_OriginalFirstName.Add("권");
        m_OriginalFirstName.Add("황");
        m_OriginalFirstName.Add("안");
        m_OriginalFirstName.Add("편");
    }

    private void InitBasicSkills()
    {
        m_SkillRangeList.Add(new StudentBasicSkills(1, 1, 10));
        m_SkillRangeList.Add(new StudentBasicSkills(2, 11, 20));
        m_SkillRangeList.Add(new StudentBasicSkills(3, 21, 30));
        m_SkillRangeList.Add(new StudentBasicSkills(4, 31, 40));
        m_SkillRangeList.Add(new StudentBasicSkills(5, 41, 50));
    }

    private void InitExternalBehaviorPool()
    {
        for (var i = 1; i < 7; i++)
        {
            ExternalBehavior studentTreeInstance = Instantiate(studentTree);
            studentTreeInstance.Init();
            Vector3 classEntrance = GameObject.Find("CheckArtClass").transform.position;
            studentTreeInstance.SetVariableValue("MyClassEntrance", classEntrance);
            Vector3 classSeat = GameObject.Find("ArtFixedSeat" + i).transform.position;
            studentTreeInstance.SetVariableValue("MyClassSeat", classSeat);
            Transform SeatTransform = GameObject.Find("ArtFixedSeat" + i).transform;
            studentTreeInstance.SetVariableValue("SeatTransform", SeatTransform);
            ArtBehaviorPool.Add(studentTreeInstance);
        }

        for (var i = 1; i < 7; i++)
        {
            ExternalBehavior studentTreeInstance = Instantiate(studentTree);
            studentTreeInstance.Init();
            Vector3 classEntrance = GameObject.Find("CheckProductManagerClass").transform.position;
            studentTreeInstance.SetVariableValue("MyClassEntrance", classEntrance);
            Vector3 classSeat = GameObject.Find("ProductManagerFixedSeat" + i).transform.position;
            studentTreeInstance.SetVariableValue("MyClassSeat", classSeat);
            Transform SeatTransform = GameObject.Find("ProductManagerFixedSeat" + i).transform;
            studentTreeInstance.SetVariableValue("SeatTransform", SeatTransform);
            ProductManagerBehaviorPool.Add(studentTreeInstance);
        }

        for (var i = 1; i < 7; i++)
        {
            ExternalBehavior studentTreeInstance = Instantiate(studentTree);
            studentTreeInstance.Init();
            Vector3 classEntrance = GameObject.Find("CheckProgrammingClass").transform.position;
            studentTreeInstance.SetVariableValue("MyClassEntrance", classEntrance);
            Vector3 classSeat = GameObject.Find("ProgrammingFixedSeat" + i).transform.position;
            studentTreeInstance.SetVariableValue("MyClassSeat", classSeat);
            Transform SeatTransform = GameObject.Find("ProgrammingFixedSeat" + i).transform;
            studentTreeInstance.SetVariableValue("SeatTransform", SeatTransform);
            ProgrammingBehaviorPool.Add(studentTreeInstance);
        }
    }

    // 내가 찾으려는 학생을 이름으로 찾아서 반환해주는 함수
    public Student SearchStudentInfo(string _studentName)
    {
        foreach (Student student in m_StudentList)
        {
            if (student.m_StudentStat.m_StudentName == _studentName || student.m_StudentStat.m_UserSettingName == _studentName)
            {
                return student;
            }
        }

        return null;
    }

    public float CheckIncreaseStat(string _personalityName, int _statIndex)
    {
        foreach (IncreaseStat stat in m_IncreaseStatData)
        {
            if (stat.personalityName == _personalityName)
            {
                switch (_statIndex)
                {
                    case 0:
                    return stat.Insight;

                    case 1:
                    return stat.Concentration;

                    case 2:
                    return stat.Sense;

                    case 3:
                    return stat.Technique;

                    case 4:
                    return stat.Wit;
                }
            }
        }

        return 0;
    }

    public int CheckStatSkills(int _statValue)
    {
        foreach (StudentBasicSkills table in m_SkillRangeList)
        {
            if (_statValue >= table.MinStat && _statValue <= table.MaxStat)
            {
                return table.Level;
            }
        }

        return 0;
    }

    public void CreateAllStudent()
    {
        // 2년차에 저장 데이터를 불러오면 안되기 떄문에 수정 필요
        if (Json.Instance.UseLoadingData && LoadStudentOnce)
        {
            CreateLoadedStudent();
            LoadStudentOnce = false;
        }

        // 로딩에 실패하거나 모종의 이유로 학생이 없다면 암튼 만든다.
        if (m_StudentList.Count == 0)
        {
            m_MaleName.Clear();
            m_FeMaleName.Clear();

            string _firstName = "";
            m_MaleName = m_OriginalMaleLastName.ToList();
            m_FeMaleName = m_OriginalFeMaleLastName.ToList();

            m_nArt = 1;
            m_nProgramming = 1;
            m_nProductManager = 1;

            int StudentCount = 0;

            for (int i = 0; i < 2; i++)
            {
                int _nameIndex = Random.Range(0, m_MaleName.Count);
                int _firstnameIndex = Random.Range(0, m_OriginalFirstName.Count);
                _firstName = m_OriginalFirstName[_firstnameIndex];

                StartCoroutine(DelayCreateStudent(_firstName + m_MaleName[_nameIndex], StudentType.Art, 1, StudentCount));
                StudentCount++;
                //CreateStudent(_nameIndex, StudentType.Art, 1);
                m_MaleName.RemoveAt(_nameIndex);
            }

            for (int i = 0; i < 4; i++)
            {
                int _nameIndex = Random.Range(0, m_FeMaleName.Count);
                int _firstnameIndex = Random.Range(0, m_OriginalFirstName.Count);
                _firstName = m_OriginalFirstName[_firstnameIndex];

                StartCoroutine(DelayCreateStudent(_firstName + m_FeMaleName[_nameIndex], StudentType.Art, 2, StudentCount));
                StudentCount++;
                //CreateStudent(_nameIndex, StudentType.Art, 2);
                m_FeMaleName.RemoveAt(_nameIndex);
            }

            for (int i = 0; i < 3; i++)
            {
                int _nameIndex = Random.Range(0, m_MaleName.Count);
                int _firstnameIndex = Random.Range(0, m_OriginalFirstName.Count);
                _firstName = m_OriginalFirstName[_firstnameIndex];

                StartCoroutine(DelayCreateStudent(_firstName + m_MaleName[_nameIndex], StudentType.GameDesigner, 1, StudentCount));
                StudentCount++;
                //CreateStudent(_nameIndex, StudentType.GameDesigner, 1);
                m_MaleName.RemoveAt(_nameIndex);
            }

            for (int i = 0; i < 3; i++)
            {
                int _nameIndex = Random.Range(0, m_FeMaleName.Count);
                int _firstnameIndex = Random.Range(0, m_OriginalFirstName.Count);
                _firstName = m_OriginalFirstName[_firstnameIndex];

                StartCoroutine(DelayCreateStudent(_firstName + m_FeMaleName[_nameIndex], StudentType.GameDesigner, 2, StudentCount));
                StudentCount++;
                //CreateStudent(_nameIndex, StudentType.GameDesigner, 2);
                m_FeMaleName.RemoveAt(_nameIndex);
            }

            for (int i = 0; i < 4; i++)
            {
                int _nameIndex = Random.Range(0, m_MaleName.Count);
                int _firstnameIndex = Random.Range(0, m_OriginalFirstName.Count);
                _firstName = m_OriginalFirstName[_firstnameIndex];

                StartCoroutine(DelayCreateStudent(_firstName + m_MaleName[_nameIndex], StudentType.Programming, 1, StudentCount));
                StudentCount++;
                //CreateStudent(_nameIndex, StudentType.Programming, 1);
                m_MaleName.RemoveAt(_nameIndex);
            }

            for (int i = 0; i < 2; i++)
            {
                int _nameIndex = Random.Range(0, m_FeMaleName.Count);
                int _firstnameIndex = Random.Range(0, m_OriginalFirstName.Count);
                _firstName = m_OriginalFirstName[_firstnameIndex];

                StartCoroutine(DelayCreateStudent(_firstName + m_FeMaleName[_nameIndex], StudentType.Programming, 2, StudentCount));
                StudentCount++;
                //CreateStudent(_nameIndex, StudentType.Programming, 2);
                m_FeMaleName.RemoveAt(_nameIndex);
            }

            m_Friendship.Clear();

            // 학생18, 강사3 친밀도 초기화
            for (int i = 0; i < 18; i++)
            {
                List<int> studentFriendship = new List<int>();
                for (int j = 0; j < 21; j++)
                {
                    studentFriendship.Add(0);
                }

                m_Friendship.Add(studentFriendship);
            }
        }
    }

    // 학생 오브젝트를 필요할 때 동적으로 생성해주기 위한 함수
    // 랜덤한 숫자를 가져와서 데이터베이스에 있는 학생의 정보를 무작위로 가져온다
    // 학생을 생성할 때 파츠를 오브젝트로 생성하는게 아니라 Mesh를 바꿔주는걸로 해주기
    public void CreateStudent(string _name, StudentType _type, int _gender)
    {
        /// S. Monobehavior를 사용한 녀석들은 new를 하면 안된다.
        // GameObject student = new GameObject();
        GameObject _newStudentObject;
        /// Instantiate를 하는 방법
        /// (사본으로부터 원본을 만든다.)
        /// 1. 리소스화 된 프리팹을 특정 오브젝트로 끌어넣고, 그것을 원본으로 사용한다.
        //GameObject _newStudentObject = GameObject.Instantiate(StudentOriginal) as GameObject;   // 범용적인 함수

        // 모델링을 지정해준다.
        if (_gender == 1)
        {
            _newStudentObject = InGameObjectPool.GetMaleStudentObject(transform);
        }
        else
        {
            _newStudentObject = InGameObjectPool.GetFemaleStudentObject(transform);
        }

        //GameObject _newStudentObjec2 = GameObject.Instantiate(StudentOriginal);                 // 오리지널이 게임오브젝트일 때
        //GameObject _newStudentObject3 = GameObject.Instantiate<GameObject>(StudentOriginal);    // 타입을 Generic으로 지정해주는 버전

        #region _프리팹을 동적으로 만드는 방법

        /// (사본으로부터 원본을 만든다.)
        /// 2. 리소스화 된 프리팹을 리소스 폴더로부터 바로 로드해서, 그것을 원본으로 사용한다.
        //GameObject _newStudentObject = GameObject.Instantiate(Resources.Load("Student")) as GameObject;

        /// (아무것도 없는 상태에서, 하나하나 다 만들고 싶을 때)
        /// 3. GameObject를 하나 만들고, 컴포넌트들을 하나하나 만들어서 붙인다.
        //GameObject _newStudentObject2 = new GameObject();
        //_newStudentObject2.AddComponent<Student>();
        //_newStudentObject2.AddComponent<MeshFilter>();
        //MeshFilter _newFilter = _newStudentObject2.GetComponent<MeshFilter>();
        //_newFilter.mesh = new Mesh();

        // (참고) 컴포넌트만 가져오고 싶은 경우
        //Student _student2 = StudentOriginal.GetComponent<Student>();
        //Student _newStudentComponent = GameObject.Instantiate(_student2);   // 범용적인 함수
        // 캐릭터를 생성할 때 헤어랑 옷을 랜덤으로 만들어준다.
        //int _hairNum = Random.Range(0, m_CharacterPartsHair.Count);
        //int _topNum = Random.Range(0, m_CharacterPartsTop.Count);
        //int _bottomNum = Random.Range(0, m_CharacterPartsBottom.Count);

        // 머리카락과 옷을 생성할 때 부모를 _newStudentObject로 설정해준다.
        //GameObject.Instantiate(m_CharacterPartsHair[_hairNum], _newStudentObject.transform.GetChild(0).transform);
        //GameObject.Instantiate(m_CharacterPartsTop[_topNum], _newStudentObject.transform.GetChild(0).transform);
        //GameObject.Instantiate(m_CharacterPartsBottom[_bottomNum], _newStudentObject.transform.GetChild(0).transform);

        #endregion

        // 엔티티로부터 스크립트를 얻어낸다
        Student _student = _newStudentObject.GetComponent<Student>();
        // 그 스크립트로부터 이런 저런 처리를 한다.

        // 처음 생성 시 3명이니 3,3 근데 이건 학생 생성 완료후로 넘겨야 할 듯.

        StudentStat _studentStat = new StudentStat();

        // if (_loadData)
        // {
        //     _studentStat = _loadStat;
        // }
        // else
        {
            // 랜덤한 int값이 들어가는 스탯들
            RandomStat(_studentStat, _name, _gender);
            _studentStat.m_Skills = new List<string>();

            // 학생들의 기본 스킬을 스탯에 맞게 초기화 해준다.
            for (int i = 0; i < 5; i++)
            {
                _studentStat.m_AbilitySkills[i] = CheckStatSkills(_studentStat.m_AbilityAmountArr[i]);
                if (_studentStat.m_AbilitySkills[i] <= 0)
                {
                    Debug.LogWarning("스탯스킬 이상함 : " + _studentStat.m_AbilitySkills[i]);
                }
            }

            _studentStat.m_StudentType = _type;
            _studentStat.m_NumberOfEntries = 1;
            _studentStat.m_Gender = _gender;
            //StudentCondition _studentCondition = new StudentCondition(m_conditionData.dataBase.studentCondition[0]);
        }

        _student.Initialize(_studentStat);


        // 학생에 BT컴포넌트를 붙여준다
        _newStudentObject.GetComponent<BehaviorTree>().StartWhenEnabled = true;
        _newStudentObject.GetComponent<BehaviorTree>().PauseWhenDisabled = true;
        _newStudentObject.GetComponent<BehaviorTree>().RestartWhenComplete = true;

        _newStudentObject.GetComponent<BehaviorTree>().DisableBehavior();

        //if (_student.m_StudentStat.m_StudentType == StudentType.Art)
        //{
        //    _newStudentObject.GetComponent<BehaviorTree>().ExternalBehavior = ArtBehaviorPool[m_nArt];
        //    _newStudentObject.GetComponent<BehaviorTree>().ExternalBehavior.GetVariable("")
        //    m_nArt++;
        //}
        //else if (_student.m_StudentStat.m_StudentType == StudentType.GameDesigner)
        //{
        //    _newStudentObject.GetComponent<BehaviorTree>().ExternalBehavior = ProductManagerBehaviorPool[m_nProductManager];
        //    m_nProductManager++;
        //}
        //else if (_student.m_StudentStat.m_StudentType == StudentType.Programming)
        //{
        //    _newStudentObject.GetComponent<BehaviorTree>().ExternalBehavior = ProgrammingBehaviorPool[m_nProgramming];
        //    m_nProgramming++;
        //}
        //_newStudentObject.GetComponent<BehaviorTree>().EnableBehavior();
        ExternalBehavior studentTreeInstance = Instantiate(studentTree);
        studentTreeInstance.Init();
        _newStudentObject.GetComponent<BehaviorTree>().ExternalBehavior = studentTreeInstance;

        if (_student.m_StudentStat.m_StudentType == StudentType.Art)
        {
            Vector3 classEntrance = GameObject.Find("CheckArtClass").transform.position;
            _newStudentObject.GetComponent<BehaviorTree>().ExternalBehavior.SetVariableValue("MyClassEntrance", classEntrance);
            Vector3 classSeat = GameObject.Find("ArtFixedSeat" + m_nArt).transform.position;
            _newStudentObject.GetComponent<BehaviorTree>().ExternalBehavior.SetVariableValue("MyClassSeat", classSeat);
            Transform seatTransform = GameObject.Find("ArtFixedSeat" + m_nArt).transform;
            _newStudentObject.GetComponent<BehaviorTree>().ExternalBehavior.SetVariableValue("SeatTransform", seatTransform);
            m_nArt++;
        }
        else if (_student.m_StudentStat.m_StudentType == StudentType.GameDesigner)
        {
            Vector3 classEntrance = GameObject.Find("CheckProductManagerClass").transform.position;
            _newStudentObject.GetComponent<BehaviorTree>().ExternalBehavior.SetVariableValue("MyClassEntrance", classEntrance);
            Vector3 classSeat = GameObject.Find("ProductManagerFixedSeat" + m_nProductManager).transform.position;
            _newStudentObject.GetComponent<BehaviorTree>().ExternalBehavior.SetVariableValue("MyClassSeat", classSeat);
            Transform seatTransform = GameObject.Find("ProductManagerFixedSeat" + m_nProductManager).transform;
            _newStudentObject.GetComponent<BehaviorTree>().ExternalBehavior.SetVariableValue("SeatTransform", seatTransform);
            m_nProductManager++;
        }
        else if (_student.m_StudentStat.m_StudentType == StudentType.Programming)
        {
            Vector3 classEntrance = GameObject.Find("CheckProgrammingClass").transform.position;
            _newStudentObject.GetComponent<BehaviorTree>().ExternalBehavior.SetVariableValue("MyClassEntrance", classEntrance);
            Vector3 classSeat = GameObject.Find("ProgrammingFixedSeat" + m_nProgramming).transform.position;
            _newStudentObject.GetComponent<BehaviorTree>().ExternalBehavior.SetVariableValue("MyClassSeat", classSeat);
            Transform seatTransform = GameObject.Find("ProgrammingFixedSeat" + m_nProgramming).transform;
            _newStudentObject.GetComponent<BehaviorTree>().ExternalBehavior.SetVariableValue("SeatTransform", seatTransform);
            m_nProgramming++;
        }
        _newStudentObject.GetComponent<BehaviorTree>().EnableBehavior();


        // 새로 만든 학생 오브젝트의 위치를 0으로 돌린다. 네비매쉬가 켜진상태로 바꾸면 안바껴진다.
        _newStudentObject.GetComponent<NavMeshAgent>().enabled = false;

        int randomSpawn = Random.Range(1, 3);

        if (randomSpawn == 1)
        {
            _newStudentObject.transform.position = SpawnPoint1.position;
        }
        else if (randomSpawn == 2)
        {
            _newStudentObject.transform.position = SpawnPoint2.position;
        }
        _newStudentObject.GetComponent<NavMeshAgent>().enabled = true;

        // 만들어진 오브젝트를 특정 풀에 넣는다.
        m_StudentList.Add(_student);
        //m_StudentBehaviorList.Add(_newStudentObject);
    }

    // 로딩한 데이터로 학생을 생성해보자.
    private void CreateLoadedStudent()
    {
        List<StudentStat> forTest = new List<StudentStat>();

        m_nArt = 1;
        m_nProgramming = 1;
        m_nProductManager = 1;

        // 호감도를 저장하기 위한 빈 리스트 생성
        for (int i = 0; i < 18; i++)
        {
            List<int> studentFriendship = new List<int>();
            m_Friendship.Add(studentFriendship);
        }

        // 불러온 학생정보 한명한명 가져온다.
        foreach (var nowStudentData in AllInOneData.Instance.StudentData)
        {
            GameObject _newStudentObject;

            // 호감도 설정

            List<int> studentFriendship = nowStudentData.Friendship;
            m_Friendship[nowStudentData.FriendshipIndex] = studentFriendship;


            // 모델링을 지정해준다.
            if (nowStudentData.Gender == 1)
            {
                _newStudentObject = InGameObjectPool.GetMaleStudentObject(transform);
            }
            else
            {
                _newStudentObject = InGameObjectPool.GetFemaleStudentObject(transform);
            }

            // 엔티티로부터 스크립트를 얻어낸다
            Student _newStudent = _newStudentObject.GetComponent<Student>();



            // 스텟 값을 넣어주기 위해 Reflection 사용해보았다!
            StudentStat _newStudentStat = new StudentStat();
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            var statFields = typeof(StudentStat).GetFields(flags);
            var saveFields = typeof(StudentSaveData).GetProperties(flags);

            foreach (var newStat in statFields)
            {
                foreach (var saveStat in saveFields)
                {
                    if (newStat.Name == "m_" + saveStat.Name)
                    {
                        var value = saveStat.GetValue(nowStudentData);
                        newStat.SetValue(_newStudentStat, value);
                        break;
                    }
                }
            }

            _newStudent.Initialize(_newStudentStat);

            _newStudent.DoingValue = Student.Doing.EndInteracting;

            // 학생에 BT컴포넌트를 붙여준다
            _newStudentObject.GetComponent<BehaviorTree>().StartWhenEnabled = true;
            _newStudentObject.GetComponent<BehaviorTree>().PauseWhenDisabled = true;
            _newStudentObject.GetComponent<BehaviorTree>().RestartWhenComplete = true;

            _newStudentObject.GetComponent<BehaviorTree>().DisableBehavior();

            ExternalBehavior studentTreeInstance = Instantiate(studentTree);
            studentTreeInstance.Init();

            if (_newStudent.m_StudentStat.m_StudentType == StudentType.Art)
            {
                Vector3 classEntrance = GameObject.Find("CheckArtClass").transform.position;
                studentTreeInstance.SetVariableValue("MyClassEntrance", classEntrance);
                Vector3 classSeat = GameObject.Find("ArtFixedSeat" + m_nArt).transform.position;
                studentTreeInstance.SetVariableValue("MyClassSeat", classSeat);
                Transform SeatTransform = GameObject.Find("ArtFixedSeat" + m_nArt).transform;
                studentTreeInstance.SetVariableValue("SeatTransform", SeatTransform);
                m_nArt++;
            }
            else if (_newStudent.m_StudentStat.m_StudentType == StudentType.GameDesigner)
            {
                Vector3 classEntrance = GameObject.Find("CheckProductManagerClass").transform.position;
                studentTreeInstance.SetVariableValue("MyClassEntrance", classEntrance);
                Vector3 classSeat = GameObject.Find("ProductManagerFixedSeat" + m_nProductManager).transform.position;
                studentTreeInstance.SetVariableValue("MyClassSeat", classSeat);
                Transform SeatTransform = GameObject.Find("ProductManagerFixedSeat" + m_nProductManager).transform;
                studentTreeInstance.SetVariableValue("SeatTransform", SeatTransform);
                m_nProductManager++;
            }
            else if (_newStudent.m_StudentStat.m_StudentType == StudentType.Programming)
            {
                Vector3 classEntrance = GameObject.Find("CheckProgrammingClass").transform.position;
                studentTreeInstance.SetVariableValue("MyClassEntrance", classEntrance);
                Vector3 classSeat = GameObject.Find("ProgrammingFixedSeat" + m_nProgramming).transform.position;
                studentTreeInstance.SetVariableValue("MyClassSeat", classSeat);
                Transform SeatTransform = GameObject.Find("ProgrammingFixedSeat" + m_nProgramming).transform;
                studentTreeInstance.SetVariableValue("SeatTransform", SeatTransform);
                m_nProgramming++;
            }

            _newStudentObject.GetComponent<BehaviorTree>().ExternalBehavior = studentTreeInstance;
            _newStudentObject.GetComponent<BehaviorTree>().EnableBehavior();

            // 새로 만든 학생 오브젝트의 위치를 0으로 돌린다. 네비매쉬가 켜진상태로 바꾸면 안바껴진다.
            _newStudentObject.GetComponent<NavMeshAgent>().enabled = false;
            _newStudentObject.transform.position = AcademyEntrance.position;
            _newStudentObject.GetComponent<NavMeshAgent>().enabled = true;

            // 만들어진 오브젝트를 특정 풀에 넣는다.
            m_StudentList.Add(_newStudent);
            //m_StudentBehaviorList.Add(_newStudentObject);
        }
        Debug.Log("저장된 학생 생성 완료...!!");
        // 불러오기 이후 시간이 멈추는 현상을 임시로 해결함
        Time.timeScale = 1;
    }

    public void RandomStat(StudentStat _studentStat, string _name, int _gender)
    {
        int _randomIndex = Random.Range(0, 20);

        // 이름은 나중에 바꿔줘야함.
        //if (_gender == 1)
        //{
        //    _studentStat.m_StudentName = m_MaleName[_index];
        //}
        //else
        //{
        //    _studentStat.m_StudentName = m_FeMaleName[_index];
        //}
        _studentStat.m_StudentName = _name;
        _studentStat.m_Health = 100;
        _studentStat.m_Passion = 100;
        _studentStat.m_IsActiving = false;

        _studentStat.m_Personality = m_StudentPersonality[_randomIndex].Name;

        for (int i = 0; i < (int)AbilityType.Count; ++i)
        {
            _studentStat.m_AbilityAmountArr[i] = GetRandomStat(PlayerInfo.Instance.CurrentRank, "스탯");

            if (_studentStat.m_AbilityAmountArr[i] <= 0)
            {
                Debug.LogWarning("스탯값이 이상하게 들어감 : " + _studentStat.m_AbilityAmountArr[i]);
            }
        }

        for (int i = 0; i < (int)GenreStat.Count; ++i)
        {
            _studentStat.m_GenreAmountArr[i] = GetRandomStat(PlayerInfo.Instance.CurrentRank, "장르");
        }
        // Random.Range(5, 101)

        // 이제 나중에 학생의 스탯에 맞추어서 스킬들을 부여해주자
        // 조금 나중으로 미루자
    }

    // 아카데미 등급에 따라 학생 생성시 능력치 다르게 해주기
    private int GetRandomStat(Rank myAcademyRank, string genreOrStat)
    {
        foreach (StudentRandomStatRange table in m_StudentRandomStatRangeList)
        {
            if (table.AcademyRank == myAcademyRank && genreOrStat == "스탯")
            {
                return Random.Range(table.StatMinValue, table.StatMaxValue + 1);
            }
            else if (table.AcademyRank == myAcademyRank && genreOrStat == "장르")
            {
                return Random.Range(table.GenreMinValue, table.GenreMaxValue + 1);
            }
        }

        return 0;
    }

    // 학생의 성격 이름을 넣으면 해당하는 스크립트를 내보낸다.
    public string GetPersonalityScript(string _name)
    {
        string _script = "";

        foreach (StudentPersonality table in m_StudentPersonality)
        {
            if (_name == table.Name)
            {
                return _script = table.Script;
            }
        }

        return _script;
    }

    // 임의로 생성해줄 강사 오브젝트와 데이터 연결
    public void LinkInstructorDataToObject()
    {
        // 교수 각각으로 스탯을 받아와서 데이터를 넣어준다 한번에 하면 레퍼런스라서 다 하나의 중복된 값이 들어가게 된다

        // 기획 데이터 넣기
        for (int i = 0; i < Professor.Instance.GameManagerProfessor.Count; i++)
        {
            if (Professor.Instance.GameManagerProfessor[i].m_ProfessorType == StudentType.GameDesigner &&
                Professor.Instance.GameManagerProfessor[i].m_ProfessorSet == "전임")
            {
                // 여기서 설정된 강사 데이터를 다 넣어주는 것이다.
                Professor1.GetComponent<Instructor>().Initialize(Professor.Instance.GameManagerProfessor[i]);
            }
        }

        // 아트 데이터 넣기
        for (int i = 0; i < Professor.Instance.ArtProfessor.Count; i++)
        {
            if (Professor.Instance.ArtProfessor[i].m_ProfessorType == StudentType.Art &&
                Professor.Instance.ArtProfessor[i].m_ProfessorSet == "전임")
            {
                // 여기서 설정된 강사 데이터를 다 넣어주는 것이다.
                Professor2.GetComponent<Instructor>().Initialize(Professor.Instance.ArtProfessor[i]);
            }
        }

        // 플밍 데이터 넣기
        for (int i = 0; i < Professor.Instance.ProgrammingProfessor.Count; i++)
        {
            if (Professor.Instance.ProgrammingProfessor[i].m_ProfessorType == StudentType.Programming &&
                Professor.Instance.ProgrammingProfessor[i].m_ProfessorSet == "전임") // 외래로 임시로 한다
            {
                // 여기서 설정된 강사 데이터를 다 넣어주는 것이다.
                Professor3.GetComponent<Instructor>().Initialize(Professor.Instance.ProgrammingProfessor[i]);
            }
        }
    }

    IEnumerator DelayCreateStudent(string _name, StudentType _type, int _gender, int delayTime)
    {
        yield return new WaitForSeconds(delayTime);

        CreateStudent(_name, _type, _gender);
    }
}