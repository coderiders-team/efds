using System;
using System.IO;
using System.Reflection;
using Topshelf;

namespace ExpiredFilesDestructorService
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Directory.SetCurrentDirectory(path);
            var rc = HostFactory.Run(x =>                                  
            {
                x.Service<ExpiredFilesDestructor>(s =>                                   
                {
                    s.ConstructUsing(name => new ExpiredFilesDestructor(Helper.getAppSetting("folder"), Convert.ToInt32(Helper.getAppSetting("expirationHours"))));               
                    s.WhenStarted(etc => etc.Start());                        
                    s.WhenStopped(etc => etc.Stop());                        
                });
                x.RunAsLocalSystem();                                    

                x.SetDescription("Expired Files Destructor");                  
                x.SetDisplayName("Expired Files Destructor");                         
                x.SetServiceName("ExpiredFilesDestructor");                             
            });                                                         

            var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());
            Environment.ExitCode = exitCode;
        }
    }
}
