using System.Collections;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine.AI;

public class ProfClassDestination : Action
{
    private string m_NowDestination = " ";

    private const float DesClassDis = 2;

    private bool m_isCoroutineRunning = false;

    private IEnumerator studyTalkCoroutine;

    private NavMeshAgent m_Agent;
    private Animator m_Animator;

    private float m_ResetTimer;
    private const float m_ResetTime = 5;

    public override void OnStart()
    {
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

        if (InGameTest.Instance.m_ClassState == ClassState.ClassStart)
        {
            _bNewDestination = SetClassDestination();
        }

        else if (InGameTest.Instance.m_ClassState == ClassState.ClassEnd)
        {
            if (gameObject.GetComponent<Instructor>().m_IsDesSetting == false)
            {
                //_bNewDestination = SetClassEndDestination();
                StartCoroutine(SetClassEnd());
            }
            gameObject.GetComponent<Instructor>().m_IsDesSetting = true;

            if (m_isCoroutineRunning)
            {
                m_isCoroutineRunning = false;
                StopCoroutine(studyTalkCoroutine);
            }
        }

        else if (InGameTest.Instance.m_ClassState == ClassState.StudyStart)
        {
            _bNewDestination = true;

        }

        else if (InGameTest.Instance.m_ClassState == ClassState.Studying)
        {
            _bNewDestination = true;

            if (m_isCoroutineRunning == false)
            {
                int randomAnim = Random.Range(1, 4);
                m_Animator.SetTrigger("ToTeach" + randomAnim);

                studyTalkCoroutine = ProfessorScript();
                StartCoroutine(studyTalkCoroutine);
            }
        }

        return _bNewDestination ? TaskStatus.Success : TaskStatus.Failure;
    }

    private bool SetClassDestination()
    {
        if (gameObject.GetComponent<Instructor>().m_IsArrivedClass)
            return true;

        if (m_NowDestination != "ClassEntrance" && m_NowDestination != "ClassSeat")
        {
            m_NowDestination = "ClassEntrance";
            Vector3 entrancePos = (Vector3)gameObject.GetComponent<BehaviorTree>().ExternalBehavior.GetVariable("MyClassEntrance").GetValue();
            m_Agent.SetDestination(entrancePos);
            m_Agent.isStopped = false;

            m_Animator.SetTrigger("ToWalk");

            return true;
        }

        if (m_NowDestination == "ClassEntrance")
        {
            Vector3 myPos = gameObject.transform.position;
            Vector3 entrancePos = (Vector3)gameObject.GetComponent<BehaviorTree>().ExternalBehavior.GetVariable("MyClassEntrance").GetValue();
            float dis = Vector3.Distance(myPos, entrancePos);

            if (dis < DesClassDis)
            {
                m_NowDestination = "ClassSeat";
                Vector3 seatPos = (Vector3)gameObject.GetComponent<BehaviorTree>().ExternalBehavior.GetVariable("MyClassSeat").GetValue();
                m_Agent.SetDestination(seatPos);
            }
            else
            {
                m_ResetTimer += Time.deltaTime;

                if (m_ResetTimer >= m_ResetTime)
                {
                    m_ResetTimer = 0;
                    m_Agent.SetDestination(entrancePos);
                }
            }

            return true;
        }

        if (m_NowDestination == "ClassSeat")
        {
            Vector3 myPos = gameObject.transform.position;
            Vector3 seatPos = (Vector3)gameObject.GetComponent<BehaviorTree>().ExternalBehavior.GetVariable("MyClassSeat").GetValue();
            float dis = Vector3.Distance(myPos, seatPos);
            
            if (dis < DesClassDis)
            {
                m_Animator.SetTrigger("ToIdle");

                gameObject.GetComponent<Instructor>().m_IsDesSetting = false;
                m_NowDestination = " ";
                gameObject.GetComponent<Instructor>().m_IsArrivedClass = true;
                m_Agent.velocity = Vector3.zero;
            }

            return true;
        }

        return false;
    }

    private bool SetClassEndDestination()
    {
        m_NowDestination = " ";
        gameObject.GetComponent<Instructor>().DoingValue = Instructor.Doing.FreeWalk;

        return true;
    }

    IEnumerator SetClassEnd()
    {
        m_Animator.SetTrigger("ToIdle");

        yield return new WaitForSeconds(1.5f);

        m_NowDestination = " ";
        gameObject.GetComponent<Instructor>().m_IsCoolDown = true;
        gameObject.GetComponent<Instructor>().DoingValue = Instructor.Doing.FreeWalk;
    }

    IEnumerator ProfessorScript()
    {
        m_isCoroutineRunning = true;

        while (true)
        {
            int random = Random.Range(1, 101);

            if (random <= 30)
            {
                InteractionManager.Instance.ShowClassScript(gameObject.GetComponent<Instructor>().m_InstructorData.m_ProfessorType, transform, true);
            }

            float randomCoolTime = Random.Range(1f, 3f);

            yield return new WaitForSeconds(randomCoolTime);
        }
    }
}