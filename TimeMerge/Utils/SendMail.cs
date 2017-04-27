using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeMerge.Utils
{
    class SendMail
    {
        public static void Send(string inlineMailMessage)
        {
            // Encode some special characters so that 'mailto:' protocol will understand it
            string encodedMailMessage = inlineMailMessage;
            encodedMailMessage = encodedMailMessage.Replace("\n", "%0A");
            encodedMailMessage = encodedMailMessage.Replace("\t", "%09");

            // Start the user's default Mail Client
            using (var mailClientProcess = new System.Diagnostics.Process())
            {
                mailClientProcess.StartInfo.FileName = encodedMailMessage;
                mailClientProcess.StartInfo.UseShellExecute = true;
                mailClientProcess.StartInfo.RedirectStandardOutput = false;
                mailClientProcess.Start();
            }
        }
    }
}
