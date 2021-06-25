using System;
using UnityEngine;

namespace UnityCommon.Runtime.Extensions
{
	public static class GameObjectExtensions
	{
		public static T   GC<T>(this GameObject go)    => go.GetComponent<T>();
		public static T   GC<T>(this MonoBehaviour go) => go.GetComponent<T>();
		public static T[] GCs<T>(this GameObject go)   => go.GetComponents<T>();

		public static T GCIC<T>(this GameObject go, bool includeInactive = false) =>
			go.GetComponentInChildren<T>(includeInactive);

		public static T[] GCsIC<T>(this GameObject go, bool includeInactive = false) =>
			go.GetComponentsInChildren<T>(includeInactive);


		public static T GCIP<T>(this GameObject go, bool includeInactive = false) =>
			go.GetComponentInParent<T>(includeInactive);

		public static T[] GCsIP<T>(this GameObject go, bool includeInactive = false) =>
			go.GetComponentsInParent<T>(includeInactive);

		public static void ForEachChild(this GameObject go, Action<GameObject, int> act)
		{
			var t = go.transform;
			int childCount = t.childCount;
			for (int i = 0; i < childCount; i++)
			{
				act.Invoke(t.GetChild(i).gameObject, i);
			}
		}

		public static void ForEachChild(this Transform t, Action<Transform, int> act)
		{
			int childCount = t.childCount;
			for (int i = 0; i < childCount; i++)
			{
				act.Invoke(t.GetChild(i), i);
			}
		}


		public static void SetLayer(this GameObject go, int layer, bool includeChildren = true)
		{
			go.layer = layer;

			if (includeChildren)
			{
				go.ForEachChild((child, i) => { child.SetLayer(layer); });
			}
		}

		public static void Destroy(this UnityEngine.Object obj)              => UnityEngine.Object.Destroy(obj);
		public static void Destroy(this UnityEngine.Object obj, float delay) => UnityEngine.Object.Destroy(obj, delay);
	}
}
