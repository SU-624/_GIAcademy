using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class ChangeState : Action
{
	public override void OnStart()
	{
		//if (gameObject != null)
  //      {
		//	gameObject.GetComponent<Student>().m_IsArrivedClass = true;
  //      }
	}

	public override TaskStatus OnUpdate()
	{
		return TaskStatus.Success;
	}
}