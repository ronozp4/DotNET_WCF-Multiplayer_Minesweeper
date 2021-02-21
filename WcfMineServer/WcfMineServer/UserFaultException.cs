using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WcfMineServer
{
    //User Fault Exception

    [DataContract]
    public class UserFaultException
    {
        [DataMember]
        public string Message { get; set; }
    }
}