using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityCommon.Events;
using UnityCommon.Modules;
using UnityCommon.Singletons;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;

namespace UnityCommon.Variables
{
	public static class Var
	{
		public static T Get<T>(string name, bool allowNull = false) where T : Variable =>
			Variable.Get<T>(name, allowNull);

		public static T Get<T>(int hash, bool allowNull = false) where T : Variable => Variable.Get<T>(hash, allowNull);
		public static T GetVal<T>(string name, T fallbackValue = default) => Variable.GetValue(name, fallbackValue);
		public static T GetVal<T>(int hash, T fallbackValue = default) => Variable.GetValue(hash, fallbackValue);

		public static bool GetVal<T>(string name, out T val) => Variable.GetValue(name, out val);
		public static bool GetVal<T>(int hash, out T val)    => Variable.GetValue(hash, out val);
	}

	public abstract class Variable : ScriptableObject
	{
		private static VariablesModule Module => VariablesModule.Instance;

		[HideInInspector]
		public int hash;

		[Space(5)]
		public bool logStackTrace = false;

		[HideInInspector]
		[SerializeField]
		public bool bindToPlayerPrefs = false;


		[HideInInspector]
		[SerializeField]
		public string PrefsKey;

		public static int NameToHash(string name)
		{
			unchecked
			{
				int hash = 23;
				int len = name.Length;
				for (var i = 0; i < len; i++)
				{
					hash = hash * 31 + name[i];
				}

				return hash;
			}
		}

		public abstract object GetValueAsObject();

		public abstract T GetValue<T>(T fallbackValue = default);

		public abstract bool GetValue<T>(out T val);


		public abstract void SetValueAsObject(object obj);

		public abstract void InvokeModified();

		public abstract void ResetToEditorValue();

		public virtual string Serialize()
		{
			return GetValueAsObject().ToString();
		}

		public virtual void Deserialize(string s)
		{
			var val = Convert.ChangeType(s, GetType());
			SetValueAsObject(val);
		}

		public virtual bool CanBeBoundToPlayerPrefs() => true;


		public virtual void OnInspectorChanged()
		{
		}

		public abstract void RaiseModifiedEvent();

		public static void Initialize()
		{
			// if (prefsVariables != null)
			// {
			// 	foreach (var var in prefsVariables)
			// 	{
			// 		var.ResetToEditorValue();
			// 	}
			// }

			var module = Module;

			var variables = module.variables = new Dictionary<int, Variable>(32);
			var prefsVariables = module.prefsVariables = new List<Variable>(8);

			var loadedVariables = Resources.LoadAll<Variable>("Variables");

			for (int i = 0; i < loadedVariables.Length; i++)
			{
				var variable = loadedVariables[i];

				variable.PrefsKey = $"Variable_{variable.name}";

				int hash = variable.hash;

				if (variables.ContainsKey(hash))
				{
					Debug.Log("Variables already contain name " + variable.name);
					if (variables[hash] == null)
					{
						variables[hash] = variable;
					}
				}
				else
				{
					variables.Add(hash, variable);
				}

				variable.hideFlags = HideFlags.DontUnloadUnusedAsset;
				DontDestroyOnLoad(variable);

				if (variable.bindToPlayerPrefs)
				{
					try
					{
						prefsVariables.Add(variable);

						var key = variable.PrefsKey;
						if (PlayerPrefs.HasKey(key))
						{
							var strVal = PlayerPrefs.GetString(key);

							variable.Deserialize(strVal);
#if UNITY_EDITOR
							if (variable.logStackTrace)
								Debug.Log(variable.name + ": " + variable.GetValueAsObject(), variable);
#endif
						}
						else
						{
							variable.ResetToEditorValue();
#if UNITY_EDITOR
							if (variable.logStackTrace)
								Debug.Log(variable.name + ": " + variable.GetValueAsObject(), variable);
#endif
						}
					}
					catch (Exception ex)
					{
						UnityEngine.Debug.LogError(ex);
					}
				}
				else
				{
					variable.ResetToEditorValue();
				}

				variable.RaiseModifiedEvent();
			} // For end
		}


		public static void SavePlayerPrefs()
		{
			var prefsVariables = Module.prefsVariables;
			for (var i = 0; i < prefsVariables.Count; i++)
			{
				var v = prefsVariables[i];
				var key = v.PrefsKey;
				var value = v.Serialize();
				PlayerPrefs.SetString(key, value);
			}

			PlayerPrefs.Save();
		}


		public static T Get<T>(int hash, bool allowNull = false) where T : Variable
		{
			// if (!VariablesModule.HasInstance())
			// 	Initialize();

			var variables = Module.variables;
			if (variables.TryGetValue(hash, out var variable))
			{
				return (T) variable;
			}

			if (!allowNull)
			{
				Debug.LogError($"Variable is not loaded.");
				//throw new KeyNotFoundException($"Variable with name {name} is not loaded.");
			}

			Debug.LogWarning($"Variable is not loaded, returning null");
			return null;
		}

		public static T Get<T>(string name, bool allowNull = false) where T : Variable =>
			Get<T>(NameToHash(name), allowNull);

		public static T GetValue<T>(string name, T fallbackValue = default) =>
			GetValue(NameToHash(name), fallbackValue);

		public static T GetValue<T>(int hash, T fallbackValue = default)
		{
			// if (!VariablesModule.HasInstance())
			// 	Initialize();

			var variables = Module.variables;
			if (variables.TryGetValue(hash, out var variable))
			{
				return variable.GetValue(fallbackValue);
			}

			//Debug.Log($"Variable does not exist. Returning fallback value.");
			return fallbackValue;
		}

		public static bool GetValue<T>(string name, out T val) => GetValue(NameToHash(name), out val);

		public static bool GetValue<T>(int hash, out T val)
		{
			var variables = Module.variables;
			if (variables.TryGetValue(hash, out var variable))
			{
				if (variable.GetValue(out val))
				{
					return true;
				}
			}

			val = default;
			return false;
		}

		public static T Create<T>() where T : Variable
		{
			return ScriptableObject.CreateInstance<T>();
		}
	}


	public abstract class Variable<T> : Variable
	{
		[Space(16)]
		[HideInInspector]
		[SerializeField]
		private T editorValue;

		[SerializeField]
		[HideInInspector]
		protected T value;


		public T Value
		{
			get { return value; }
			set
			{
				if (EqualityComparer<T>.Default.Equals(this.value, value))
				{
					//Debug.Log($"Setting value of variable {name} without any changes. Value: {value}");
					return;
				}


				this.value = value;

#if UNITY_EDITOR
				if (logStackTrace)
					Debug.Log(this.name + ": " + value, this);

				if (Application.isPlaying == false)
					EditorUtility.SetDirty(this);
#endif

				// if (bindToPlayerPrefs)
				// {
				// 	PlayerPrefs.SetString(PrefsKey, this.Serialize());
				// }

				OnModified.Invoke(this, value);
			}
		}


		public GameEvent<T> OnModified { get; private set; }

		public override void RaiseModifiedEvent()
		{
			OnModified.Invoke(this, Value);
		}

		private void OnEnable()
		{
			OnModified = new GameEvent<T>();

			if (!bindToPlayerPrefs)
				value = editorValue;

			hash = NameToHash(name);
		}

		/*
		private void OnDisable()
		{
			OnModified = new GameEvent<T>();

			if (!BindToPlayerPrefs)
				value = editorValue;
		}
		*/


		public override object GetValueAsObject()
		{
			return value;
		}

		public override TReq GetValue<TReq>(TReq fallbackValue = default)
		{
			var val = Value;
			if (val is TReq valTReq)
			{
				return valTReq;
			}

			Debug.Log(
				$"Variable {name} is of type {val.GetType()} while the requested type is {typeof(T)}. Returning fallback value.");
			return fallbackValue;
		}


		public override bool GetValue<TReq>(out TReq valOut)
		{
			var val = Value;
			if (val is TReq valTReq)
			{
				valOut = valTReq;
				return true;
			}

			valOut = default;
			return false;
		}

		public override void SetValueAsObject(object obj)
		{
			value = (T) obj;
		}

		public void SetValueWithoutNotify(T value)
		{
			this.value = value;
		}


		public override void InvokeModified()
		{
			editorValue = value;

			OnModified?.Invoke(this, value);
		}


		public override void ResetToEditorValue()
		{
			Value = editorValue;
		}

		public static implicit operator T(Variable<T> v)
		{
			return v.Value;
		}
	}
}
