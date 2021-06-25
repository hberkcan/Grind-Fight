using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityCommon.Variables;
using UnityEngine;

namespace UnityCommon.DataBinding
{
	public class IntObserver : DataObserver<IntReference, IntVariable, IntEvent, int>
	{
		public int multiplier = 1;
		public int offset = 0;


		
		protected override void OnDataModified(int val)
		{
			base.OnDataModified(val * multiplier + offset);
#if UNITY_EDITOR
			if (stackTrace)
				Debug.Log("Modified: " + val, this.gameObject);
#endif
		}
	}
}
