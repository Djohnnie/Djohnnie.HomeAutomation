using System;

namespace Djohnnie.HomeAutomation.DataAccess.Smappee.Model
{
    public class SmappeeToken
    {
        public String AccessToken { get; set; }
        public String RefreshToken { get; set; }
        public Int32 ExpiresIn { get; set; }
    }
}