using UnityEngine;
using System.Collections;

public class CPUMovement : MonoBehaviour
{
		public float m_remainingDist = 2;
		public int m_minWaitSeconds = 1;
		public int m_maxWaitSeconds = 8;
		public Transform target;
		private NavMeshAgent agent;
		public bool m_targetReached = false;

		// Use this for initialization
		void Awake ()
		{
				agent = GetComponent<NavMeshAgent> ();
				StartCoroutine ("TargetReached");
		}
	
		// Update is called once per frame
		void Update ()
		{
				if (!m_targetReached && agent.remainingDistance < m_remainingDist) {
						StartCoroutine ("TargetReached");
				}
		}

		private IEnumerator TargetReached ()
		{
				m_targetReached = true;
				agent.ResetPath ();
				FindNewTarget ();
				yield return new WaitForSeconds (Random.Range (m_minWaitSeconds, m_maxWaitSeconds));
				agent.SetDestination (target.position);
				m_targetReached = false;
		}

		private void FindNewTarget ()
		{
				var targets = GameObject.FindGameObjectsWithTag ("Monument");
				target = targets [Random.Range (0, targets.Length)].transform;
		}
}
