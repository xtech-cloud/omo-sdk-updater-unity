using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OMO.SDK.Updater
{
    internal class Downloader
    {
        public delegate void OnStatusUpdateCallback(Status _status);
        public delegate void OnFinishCallback();
        public delegate void OnErrorCallback(string _error);

        public OnStatusUpdateCallback onStatusUpdate;

        private WWW www = null;
        private Status status = new Status();
        private bool run = false;
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

        public void Download(List<Task> _tasks, OnFinishCallback _onFinish, OnErrorCallback _onError)
        {
            run = true;
            status.total = _tasks.Count;
            status.finish = 0;
            status.progress = 0f;
            config.mono.StartCoroutine(updateStatus());

            Queue<Task> tasks = new Queue<Task>(_tasks.Count);
            foreach(Task task in _tasks)
                tasks.Enqueue(task);
            config.mono.StartCoroutine(downloadQueue(tasks, _onFinish, _onError));
        }

        private IEnumerator downloadQueue(Queue<Task> _task, OnFinishCallback _onFinish, OnErrorCallback _onError)
        {
            while (_task.Count > 0)
            {
                Task task = _task.Dequeue();
                string uri = config.domain + "/upgrade" + task.path + task.file;
                www = new WWW(uri);
                yield return www;
                if (www.error != null)
                {
                    //retry
                    _task.Enqueue(task);
                    _onError(www.error);
                    continue;
                }
                byte[] data = www.bytes;

                string filename = task.file;
                if(null != processor)
                {
                    if(null != processor.Rename)
                    {
                        filename = processor.Rename(task.file);
                    }
                }

                string outpath = Path.Combine(config.dir, task.path.Remove(0,1));
                Directory.CreateDirectory(outpath);
                string outfile = Path.Combine(outpath, filename);
                File.WriteAllBytes(outfile, data);
                status.finish += 1;

                System.Security.Cryptography.MD5 md5CSP = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5CSP.ComputeHash(data);
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                string md5 = sb.ToString();
                File.WriteAllText(Path.Combine(outpath, filename) + ".md5", md5);
            }
            run = false;
            _onFinish();
        }

        private IEnumerator updateStatus()
        {
            while (run)
            {
                status.progress = (null != www) ? www.progress : 0;
                if (null != onStatusUpdate)
                    onStatusUpdate(status);
                yield return new WaitForSeconds(0.1f);
            }
        }
    }//class
}//namespace OMO.SDK.Updater


