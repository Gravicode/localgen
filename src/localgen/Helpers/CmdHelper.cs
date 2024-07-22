using System.Diagnostics;
using System.Runtime.InteropServices;

namespace localgen.Helpers
{


    public class CmdHelper
    {
        Process process { set; get; }
        CancellationTokenSource sourceToken = new CancellationTokenSource();
        //Thread th1 { set; get; }
        public bool IsRunning { get; set; }
        //import in the declaration for GenerateConsoleCtrlEvent
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GenerateConsoleCtrlEvent(ConsoleCtrlEvent sigevent, int dwProcessGroupId);
        public enum ConsoleCtrlEvent
        {
            CTRL_C = 0,
            CTRL_BREAK = 1,
            CTRL_CLOSE = 2,
            CTRL_LOGOFF = 5,
            CTRL_SHUTDOWN = 6
        }

        //set up the parents CtrlC event handler, so we can ignore the event while sending to the child
        public static volatile bool SENDING_CTRL_C_TO_CHILD = false;
        static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = SENDING_CTRL_C_TO_CHILD;
        }
        public CmdHelper()
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);

        }
        
        public void Stop()
        {
            if (IsRunning && process!=null)
            {
                try
                {
                    var Id = process.Id;
                    //SENDING_CTRL_C_TO_CHILD = true;
                    //GenerateConsoleCtrlEvent(ConsoleCtrlEvent.CTRL_C, process.SessionId);
                    //process.WaitForExit();
                    //SENDING_CTRL_C_TO_CHILD = false;
                    process.Kill();
                    process.Close();
                    //process.CloseMainWindow();
                    process.Dispose();
                    Debug.WriteLine($"CLOSE APP: {Id}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("stop failed:" + ex);
                }
                finally
                {
                    IsRunning = false;
                }
               
            }
        }
        
        public delegate void LogDataUpdateHandler(string LogType, string Message);
        public event LogDataUpdateHandler LogData;

        private void OnLogDataUpdate(string LogType, string Message)
        {
            LogData?.Invoke(LogType, Message);
        }
        public bool ExecuteCommand(string workingDirectory, List<string> cmds, string CmdArg="" )
        {
            try
            {
                if (IsRunning) return false;
                process = new Process();
                var psi = new ProcessStartInfo();
                psi.FileName = "cmd.exe";
                psi.RedirectStandardInput = true;
                psi.RedirectStandardOutput = true;
                psi.CreateNoWindow = true;
                psi.RedirectStandardError = true;
                psi.UseShellExecute = false;
                psi.WorkingDirectory = workingDirectory;
                psi.Arguments = CmdArg;
                process.StartInfo = psi;
                process.Start();
                process.OutputDataReceived += (sender, e) =>
                {
                    var message = e.Data;
                    Debug.WriteLine(message); 
                    OnLogDataUpdate("Process", message);
                };
                process.ErrorDataReceived += (sender, e) =>
                {
                    var message = e.Data;
                    Debug.WriteLine(message);
                    OnLogDataUpdate("Error", message);
                };
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                using (StreamWriter sw = process.StandardInput)
                {
                    foreach (var cmd in cmds)
                    {
                        sw.WriteLine(cmd);
                    }
                }
                IsRunning = true;
                process.WaitForExit();
                process.Close();
                Debug.WriteLine("SELESAI..");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
            finally
            {
                IsRunning = false;
            }
        }

        /*
        public bool ExecuteCommand(string Command)
        {
            try
            {
                process.StandardInput.WriteLine(Command);
                process.Start();
                //var res = process.StandardOutput.ReadToEnd();
                return true;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }*/

    }


}
