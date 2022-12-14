using UnityEngine;

namespace Xsolla.Core
{
	public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
	{
		private const string PATH_TO_PREFABS = "";
		private static T _instance;
		private bool _isInitialized = false;

		public static T Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = InstantiateThis();
					_instance?.OneTimeInit();
				}

				return _instance;
			}
		}

		public static bool IsExist => _instance != null;

		private void Awake()
		{
			if (_instance == null)
			{
				DontDestroyOnLoad(gameObject);
				_instance = this as T;
				_instance.OneTimeInit();
			}
			else if (_instance != this)
			{
				Destroy(gameObject);
			}
		}

		private static T InstantiateThis()
		{
			string componentName = typeof(T).Name;
			Debug.Log("No instance of " + componentName);
			T result = InstantiateFromScene();

			if (result == null)
			{
				result = InstantiateFromResources();
				if (result != null)
					Debug.Log(componentName + " loaded from resources.");
			}
			else
				Debug.Log(componentName + " found in the scene.");

			if (result == null)
			{
				result = InstantiateAsNewObject();
				if (result != null)
					Debug.Log(componentName + " a temporary one is created.");
			}
			return result;
		}

		private static T InstantiateFromScene()
		{
			return FindObjectOfType(typeof(T)) as T;
		}

		private static T InstantiateFromResources()
		{
			string path = PATH_TO_PREFABS + typeof(T).Name;
			
			var prefab = Resources.Load(path);
			if (prefab == null) return null;
			
			GameObject instance = Instantiate(prefab) as GameObject;
			if (instance != null)
			{
				instance.name = typeof(T).Name;
			}
			return instance == null ? null : instance.GetComponent<T>();
		}

		private static T InstantiateAsNewObject()
		{
			GameObject instance = new GameObject("Temp Instance of " + typeof(T));
			return instance.AddComponent<T>();
		}

		protected virtual void OnDestroy()
		{
			Debug.Log(typeof(T) + " is destroyed.");
			_instance = null;
		}

		private void OneTimeInit()
		{
			if (!_isInitialized)
			{
				_isInitialized = true;
				Init();
			}
		}

		public virtual void Init()
		{

		}

		private void OnApplicationQuit()
		{
			Destroy(gameObject);
		}
	}
}
