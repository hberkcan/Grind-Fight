using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace UnityCommon.Runtime.Utility
{
	public static class Helpers
	{
		public static string SplitCamelCase(this string input)
		{
			return System.Text.RegularExpressions.Regex
			             .Replace(input, "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
		}

		public static int ExtractInt(this string s)
		{
			return int.Parse(Regex.Match(s, @"\d+").Value);
		}

		public static Color AsTransparent(this Color c)
		{
			c.a = 0f;
			return c;
		}

		public static Color AsOpaque(this Color c)
		{
			c.a = 1f;
			return c;
		}

		public static Color WithRed(this Color c, float r)
		{
			c.r = r;
			return c;
		}

		public static Color WithGreen(this Color c, float g)
		{
			c.g = g;
			return c;
		}

		public static Color WithBlue(this Color c, float b)
		{
			c.b = b;
			return c;
		}

		public static Color WithAlpha(this Color c, float a)
		{
			c.a = a;
			return c;
		}


		public static Vector3 WithX(this Vector3 v, float x)
		{
			v.x = x;
			return v;
		}

		public static Vector3 WithY(this Vector3 v, float y)
		{
			v.y = y;
			return v;
		}

		public static Vector3 WithZ(this Vector3 v, float z)
		{
			v.z = z;
			return v;
		}

		public static Vector3 WithXY(this Vector3 v, float x, float y)
		{
			v.x = x;
			v.y = y;
			return v;
		}

		public static Vector3 WithXZ(this Vector3 v, float x, float z)
		{
			v.x = x;
			v.z = z;
			return v;
		}

		public static Vector3 WithYZ(this Vector3 v, float y, float z)
		{
			v.y = y;
			v.z = z;
			return v;
		}

		public static Vector3 WithXRelative(this Vector3 v, float x)
		{
			v.x += x;
			return v;
		}

		public static Vector3 WithYRelative(this Vector3 v, float y)
		{
			v.y += y;
			return v;
		}

		public static Vector3 WithZRelative(this Vector3 v, float z)
		{
			v.z += z;
			return v;
		}

		public static Vector3 WithXYRelative(this Vector3 v, float x, float y)
		{
			v.x += x;
			v.y += y;
			return v;
		}

		public static Vector3 WithYZRelative(this Vector3 v, float y, float z)
		{
			v.y += y;
			v.z += z;
			return v;
		}

		public static Vector3 WithXZRelative(this Vector3 v, float x, float z)
		{
			v.x += x;
			v.z += z;
			return v;
		}


		public static Vector3 WithLength(this Vector3 v, float len)
		{
			return v.normalized * len;
		}


		public static void SetPositionX(this Transform t, float val)
		{
			t.position = t.position.WithX(val);
		}

		public static void SetPositionY(this Transform t, float val)
		{
			t.position = t.position.WithY(val);
		}

		public static void SetPositionZ(this Transform t, float val)
		{
			t.position = t.position.WithZ(val);
		}

		public static void SetLocalPositionX(this Transform t, float val)
		{
			t.localPosition = t.localPosition.WithX(val);
		}

		public static void SetLocalPositionY(this Transform t, float val)
		{
			t.localPosition = t.localPosition.WithY(val);
		}

		public static void SetLocalPositionZ(this Transform t, float val)
		{
			t.localPosition = t.localPosition.WithZ(val);
		}

		public static void AddPositionX(this Transform t, float val)
		{
			t.position = t.position.WithXRelative(val);
		}

		public static void AddPositionY(this Transform t, float val)
		{
			t.position = t.position.WithYRelative(val);
		}

		public static void AddPositionZ(this Transform t, float val)
		{
			t.position = t.position.WithZRelative(val);
		}

		public static void SetScaleX(this Transform t, float val)
		{
			t.localScale = t.localScale.WithX(val);
		}

		public static void SetScaleY(this Transform t, float val)
		{
			t.localScale = t.localScale.WithY(val);
		}

		public static void SetScaleZ(this Transform t, float val)
		{
			t.localScale = t.localScale.WithZ(val);
		}


		public static void AddScaleX(this Transform t, float val)
		{
			t.localScale = t.localScale.WithXRelative(val);
		}

		public static void AddScaleY(this Transform t, float val)
		{
			t.localScale = t.localScale.WithYRelative(val);
		}

		public static void AddScaleZ(this Transform t, float val)
		{
			t.localScale = t.localScale.WithZRelative(val);
		}

		public static int RoundTo5(this int x)
		{
			if (x < 0)
				return x;

			int r = x % 5;
			return r >= 3 ? x + (5 - r) : x - r;
		}

		public static int RoundTo5(this float x)
		{
			return RoundTo5((int) x);
		}

		public static Vector2 GetPositionToBringChildIntoView(this ScrollRect instance, RectTransform child)
		{
			Canvas.ForceUpdateCanvases();
			Vector2 viewportLocalPosition = instance.viewport.localPosition;
			Vector2 childLocalPosition = child.localPosition;
			Vector2 result = new Vector2(
				0 - (viewportLocalPosition.x + childLocalPosition.x),
				0 - (viewportLocalPosition.y + childLocalPosition.y)
			);
			return result;
		}

		public static Dictionary<TVal, TKey> ReverseMap<TKey, TVal>(this Dictionary<TKey, TVal> source)
		{
			var rev = new Dictionary<TVal, TKey>();
			foreach (var entry in source)
			{
				rev[entry.Value] = entry.Key;
			}

			return rev;
		}

		public static IEnumerable<T> RandomTake<T>(this IEnumerable<T> source, int n)
		{
			try
			{
				return source.OrderBy(el => UnityEngine.Random.Range(0, 999999)).Take(n);
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static IEnumerable<T> Except<T>(this IEnumerable<T> source, T element)
		{
			return source.Except(new[] {element});
		}
	}
}
