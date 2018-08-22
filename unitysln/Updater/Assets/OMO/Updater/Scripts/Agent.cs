using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

namespace OMO.SDK.Updater
{
    public class Agent
    {
        protected struct ReqParams
        {
            public string bucket;
            public string channel;
        }

        public delegate void OnStatusUpdateCallback(Status _status);
        public delegate void OnErrorCallback(string _error);
        public delegate void OnSuccessCallback(List<Task> _tasks);
		public delegate void OnFinishCallback();
        public OnStatusUpdateCallback onStatusUpdate;
		

		public delegate void ProcessorDelegate();

		private Config config = null;
		private Processor processor = null;

		public void Setup(Config _config)
		{
			config = _config;
		}

		public void InstallProcessor(Processor _processor)
		{
			processor = _processor;
		}

		public void Fetch(string _bucket, string _channel, OnSuccessCallback _onSuccess, OnErrorCallback _onError)
		{
			config.mono.StartCoroutine(fetch(_bucket, _channel, _onSuccess, _onError));
		}

		public void Upgrade(List<Task> _tasks, OnFinishCallback _onFinish, OnErrorCallback _onError)
		{
			Downloader downloader = new Downloader();
			downloader.onStatusUpdate = (_status)=>{
				if(null != onStatusUpdate)
					onStatusUpdate(_status);
			};
			downloader.Setup(config);
			downloader.InstallProcessor(processor);
			downloader.Download(_tasks, ()=>{_onFinish();}, (_err)=>{_onError(_err);});
		}
		

        private IEnumerator fetch(string _bucket, string _channel, OnSuccessCallback _onSuccess, OnErrorCallback _onError)
        {
            ReqParams reqParams = new ReqParams();
            reqParams.bucket = _bucket;
            reqParams.channel = _channel;

            string url = string.Format("{0}/fetch", config.domain);
            string json = JsonMapper.ToJson(reqParams);

			Dictionary<string, string> header = new Dictionary<string, string>();
        	header.Add("Content-Type", "application/json"); 

            WWW www = new WWW(url, Encoding.UTF8.GetBytes(json), header);
            yield return www;
            if (null != www.error)
            {
				Debug.LogError(www.error);
                _onError(www.error);
                yield break;
            }

            string manifest = www.text;
            try
            {
                Parser parser = new Parser();
                Task[] tasks = parser.ParseJSON(manifest);
				List<Task> taskList = new List<Task>();
				taskList.AddRange(tasks);
				_onSuccess(taskList);
            }
            catch (System.Exception e)
            {
                _onError(e.Message);
            }
        }
    }//class
}//namespace OMO.SDK.Updater
