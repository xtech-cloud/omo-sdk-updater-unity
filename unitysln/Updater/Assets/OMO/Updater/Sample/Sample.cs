using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using OMO.SDK.Updater;

public class Sample : MonoBehaviour {

	private Agent updater = null;
	// Use this for initialization
	void Start () {
		Config config = new Config();
		config.domain = "http://localhost:8080";
		config.dir = Application.persistentDataPath;
		config.mono = this;
		
		updater = new Agent();
		updater.onStatusUpdate = (_status)=>{
			Debug.Log(string.Format("{0}/{1}   {2}", _status.finish, _status.total, _status.progress));
		};

		updater.Setup(config);

		updater.Fetch("omo-updater", "dev", onFetchSuccess, onError);
	}
	void onFetchSuccess(List<Task> _tasks)
	{
		foreach(Task task in _tasks)
			Debug.Log(string.Format("{0} {1} {2} {3} {4}", task.uuid, task.path, task.file, task.md5, task.size));
		updater.Upgrade(_tasks, onFinish, onError);
	}

	void onFinish()
	{
		Debug.Log("upgrade finish");
	}

	void onError(string _err)
	{
		Debug.LogError(_err);
	}
}
