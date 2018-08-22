using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace OMO.SDK.Updater
{
    public class Processor
    {
        public delegate string RenameDelegate(string _file);

        public RenameDelegate Rename;
    }//class
}//namespace OMO.SDK