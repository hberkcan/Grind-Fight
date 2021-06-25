using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityCommon.Variables;
using UnityEngine;

namespace UnityCommon.DataBinding
{
	public class ColorObserver : DataObserver<ColorReference, ColorVariable, ColorEvent, Color>
	{
		protected override void OnDataModified(Color val)
		{
			base.OnDataModified(val);
#if UNITY_EDITOR
			if (stackTrace)
				Debug.Log("Modified: " + val, this.gameObject);
#endif
		}
	}
}
