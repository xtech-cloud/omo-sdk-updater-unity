using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

namespace OMO.SDK.Updater
{
    internal class Parser
    {
        public Task[] ParseJSON(string _json)
        {
			return JsonMapper.ToObject<Task[]>(_json);
        }
    }//class
}//namespace OMO.SDK