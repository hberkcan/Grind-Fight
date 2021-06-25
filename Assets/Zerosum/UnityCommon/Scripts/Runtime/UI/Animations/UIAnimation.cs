using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityCommon.Variables;
using UnityEngine;

namespace UnityCommon.Runtime.UI.Animations
{
	public abstract class UIAnimation : MonoBehaviour
	{
		[SerializeField]
		protected FloatReference delay;
		
		[SerializeField] public float duration = 0.35f;

		[SerializeField]
		public bool useUnscaledTime = false;

		public void Fade(bool e)
		{
			if (e)
			{
				FadeIn();
			}
			else
			{
				FadeOut();
			}
		}

		public abstract void FadeIn();
		public abstract void FadeOut();
	}
}
