using System;
using System.Collections.Generic;
using System.Text;

namespace AccountManagement
{
    public class SessionData
    {
        public string Session_Token { get; set; }
        public string User_Id { get; set; }
        public string Scope { get; set; } = "user";
        public DateTime Time_Created { get; set; }
        public DateTime Time_Expires { get; set; }
    }
}
