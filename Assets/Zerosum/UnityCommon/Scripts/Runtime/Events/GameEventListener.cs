using System.Collections.Generic;
using UnityCommon.Modules;
using UnityEngine;
using UnityEngine.Events;

namespace UnityCommon.Events
{
	public class GameEventListener : MonoBehaviour
	{
		public string listenerName = "Unnamed Listener";

		public List<GameEvent> events;

		public UnityEvent response;
		public float responseDelay = -1f;

		public void OnEnable()
		{
			if (Application.isPlaying)
			{
				foreach (var ev in events)
				{
					ev.AddListener(OnEventFired);
				}
			}
		}

		public void OnDisable()
		{
			foreach (var ev in events)
			{
				ev.RemoveListener(OnEventFired);
			}
		}

		public void OnEventFired(object args)
		{
			if (responseDelay <= 1e-2f)
			{
				response?.Invoke();
			}
			else
			{
				Conditional.Wait(responseDelay).Do(() => { response?.Invoke(); });
			}
		}

		public string GetName() => listenerName;
	}
}
