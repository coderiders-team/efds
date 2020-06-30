using System;
using System.IO;
using System.Security.Policy;
using System.Timers;
using Topshelf;

namespace ExpiredFilesDestructorService
{
    public class ExpiredFilesDestructor
    {
        readonly Timer _timer;
        public DateTime CurrentTime {get;set;}
        public DateTime ExpirationDate { get; set; }
        public string Filepath { get; set; }
        public int AddHours { get; set; }

        public bool IsMediaShouldNotBeDeleted { get; set; }

        static string[] mediaExtensions = {
                ".WAV", ".MID", ".MIDI",".MKV", ".WMA", ".MP3", ".OGG", ".RMA",
                ".AVI", ".MP4", ".DIVX", ".WMV", //etc
            };
        public ExpiredFilesDestructor(string filepath, int adddays)
        {
            
            Filepath = filepath;
            AddHours = adddays;
            if (AddHours < 1)
            {
                AddHours = 1;
            }

            _timer = new Timer(60 * 60 * 1000) { AutoReset = true };
            _timer.Elapsed += TimerElapsed;
            CurrentTime = DateTime.Now;
            ExpirationDate = CurrentTime.AddHours(-AddHours);
            /*string filename = @"C:\Users\Admin\Downloads\Billions.2019.S04.720p\Billions.S04E01.WEBRip.720p.rus.2.0.HDREZKA.STUDIO.mkv";
            File.SetAttributes(filename, FileAttributes.Normal);
            File.SetLastAccessTime(filename, DateTime.Now);
            File.SetLastWriteTime(filename, DateTime.Now);
            setMediaFileCanOrCannotBeDeleted(filename);
            Console.WriteLine(IsMediaShouldNotBeDeleted);*/
            DeleteAllExpiredFiles();
            //Console.ReadLine();
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            CurrentTime = DateTime.Now;
            ExpirationDate = CurrentTime.AddHours(-AddHours);
            DeleteAllExpiredFiles();
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }


        private void DeleteAllExpiredFiles()
        {
            checkForFoldersAndDelete(Filepath);
        }

        private void checkForFoldersAndDelete(string filepath)
        {
            string[] folders = Directory.GetDirectories(filepath);
            foreach (var folder in folders)
            {
                if (DateTime.Compare(ExpirationDate, Directory.GetLastAccessTime(folder)) > 0)
                {
                    try
                    {
                        IsMediaShouldNotBeDeleted = false;
                        SetAllFilesInFolderDeletable(folder);
                        if (!IsMediaShouldNotBeDeleted)
                        {
                            Directory.Delete(folder, true);
                            LogDeleteAction(folder);
                            continue;
                        }else
                        {
                            checkForFoldersAndDelete(folder);
                        }
                    }
                    catch(Exception e)
                    {
                        LogErrorAction(e.ToString());
                    }
                   
                }
                else
                {
                    checkForFoldersAndDelete(folder);
                }
            }
            DeleteFiles(filepath);
        }

       
        private void DeleteFiles(string folder)
        {
            string[] files = Directory.GetFiles(folder);
            foreach (var filename in files)
            {
                IsMediaShouldNotBeDeleted = false;
                if (File.Exists(filename))
                {
                    if (DateTime.Compare(ExpirationDate, File.GetLastAccessTime(filename)) > 0)
                    {
                        if(Path.GetFileName(filename) == "deletelog.txt" || Path.GetFileName(filename) == "errorlog.txt")
                        {
                            continue;
                        }
                        try {
                            File.SetAttributes(filename, FileAttributes.Normal);
                            setMediaFileCanOrCannotBeDeleted(filename);
                            
                            if (!IsMediaShouldNotBeDeleted)
                            {
                                File.Delete(filename);
                                LogDeleteAction(folder, filename);
                            }
                            
                        }
                        catch (Exception e)
                        {
                            LogErrorAction(e.ToString());
                        }
                }
                }
            }
        }

        private void LogDeleteAction(string folder, string filename = "" )
        {
            string line;
            if (filename.Length > 0)
            {
                line = $"{ filename } was deleted from { folder } on { DateTime.Now.ToLongDateString() }";
            }else
            {
                line = $"{ folder } was deleted on { DateTime.Now.ToLongDateString() }";
            }
            // Write the string array to a new file named "WriteLines.txt".
            using (StreamWriter logFile = new StreamWriter(Path.Combine(Filepath, "deletelog.txt"), true))
            {
                logFile.WriteLine(line);
            }


        }

        private void SetAllFilesInFolderDeletable(string folder)
        {
            File.SetAttributes(folder, FileAttributes.Normal);
            string[] folders = Directory.GetDirectories(folder);
            foreach (var nested_folder in folders)
            {
                try
                {
                    SetAllFilesInFolderDeletable(nested_folder);
                    File.SetAttributes(nested_folder, FileAttributes.Normal);
                }catch(Exception e)
                {
                    LogErrorAction(e.ToString());
                }

            }

            string[] files = Directory.GetFiles(folder);
            foreach (var file in files)
            {
               
                try
                {
                    
                    File.SetAttributes(file, FileAttributes.Normal);
                    setMediaFileCanOrCannotBeDeleted(file);
                }
                catch (Exception e)
                {
                    LogErrorAction(e.ToString());
                }
            }
        }
            private void LogErrorAction(string e)
        {
            string line;
            line = $"{ e } ON { DateTime.Now.ToLongDateString() }";
            using (StreamWriter errorlogFile = new StreamWriter(Path.Combine(Filepath, "errorlog.txt"), true))
            {
                errorlogFile.WriteLine(line);
            }
        }

            
            private void setMediaFileCanOrCannotBeDeleted(string file)
            {
         
            if (IsMediaFile(file) && isMediaFileNotExpired(file))
                {
                    IsMediaShouldNotBeDeleted = true;
                }
            }

            private bool isMediaFileNotExpired(string file)
            {
            TimeSpan ts = File.GetLastAccessTime(file) - File.GetLastWriteTime(file);
            return ts.TotalSeconds < 3
                         ||
                         DateTime.Compare(ExpirationDate, File.GetLastAccessTime(file)) <= 0;
                      
            }

             private bool IsMediaFile(string path) {
                return -1 != Array.IndexOf(mediaExtensions, Path.GetExtension(path).ToUpperInvariant());
            }
    }
}
