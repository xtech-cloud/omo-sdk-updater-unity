using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace OMO.SDK.Updater
{
    public class FileDownloadHandler : DownloadHandlerScript
    {
        private int expected = -1;
        private int received = 0;
        private string filepath;
        private FileStream fileStream;
        private bool canceled = false;

        public FileDownloadHandler(byte[] buffer, string filepath)
          : base(buffer)
        {
            this.filepath = filepath;
            fileStream = new FileStream(filepath, FileMode.Create, FileAccess.Write);
        }

        protected override byte[] GetData() { return null;}

        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            if (data == null || data.Length < 1)
            {
                return false;
            }
            received += dataLength;
            if (!canceled) fileStream.Write(data, 0, dataLength);
            return true;
        }

        protected override float GetProgress()
        {
            if (expected < 0) return 0;
            return (float)received / expected;
        }

        protected override void CompleteContent()
        {
            fileStream.Close();
        }

        protected override void ReceiveContentLength(int contentLength)
        {
            expected = contentLength;
        }

        public void Cancel()
        {
            canceled = true;
            fileStream.Close();
            File.Delete(filepath);
        }
    }

    internal class Downloader
    {
        public delegate void OnStatusUpdateCallback(Status _status);
        public delegate void OnFinishCallback();
        public delegate void OnErrorCallback(string _error);

        public OnStatusUpdateCallback onStatusUpdate;

        private UnityWebRequest webRequest = null;
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
            foreach (Task task in _tasks)
                tasks.Enqueue(task);
            config.mono.StartCoroutine(downloadQueue(tasks, _onFinish, _onError));
        }

        private IEnumerator downloadQueue(Queue<Task> _task, OnFinishCallback _onFinish, OnErrorCallback _onError)
        {
            while (_task.Count > 0)
            {
                yield return new WaitForEndOfFrame();

                Task task = _task.Dequeue();
                string uri = config.domain + "/upgrade" + task.path + task.file;

                string filename = task.file;
                if (null != processor)
                {
                    if (null != processor.Rename)
                    {
                        filename = processor.Rename(task.file);
                    }
                }

                string outpath = Path.Combine(config.dir, task.path.Remove(0, 1));
                Directory.CreateDirectory(outpath);
                string outfile = Path.Combine(outpath, filename);
                using (webRequest = new UnityWebRequest(uri))
                {
                    byte[] buffer = new byte[64 * 1024];
                    webRequest.downloadHandler = new FileDownloadHandler(buffer, outfile);
                    webRequest.SendWebRequest();
                    while (!webRequest.isDone)
                    {
                        yield return new WaitForEndOfFrame();
                    }

                    if (!string.IsNullOrEmpty(webRequest.error))
                    {
                        //retry
                        _task.Enqueue(task);
                        _onError(webRequest.error);
                        continue;
                    }
                    File.WriteAllText(Path.Combine(outpath, filename) + ".md5", task.md5);
                }
                webRequest = null;

                status.finish += 1;
            }
            run = false;
            _onFinish();
        }

        private IEnumerator updateStatus()
        {
            while (run)
            {
                status.progress = (null != webRequest) ? webRequest.downloadProgress : 0;
                if (null != onStatusUpdate)
                    onStatusUpdate(status);
                yield return new WaitForSeconds(0.1f);
            }
        }
    }//class
}//namespace OMO.SDK.Updater


