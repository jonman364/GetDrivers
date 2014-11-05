using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.IO;

using ICSharpCode.SharpZipLib.Zip;

namespace GetDrivers {
    class Program {
        static string dismCmd;
        static void Main(string[] args) {
            GetDism();
            List<Driver> drivers = new List<Driver>();

            string rawList = GetDriversListRaw();
            string[] list = GetDriversList(rawList);

            for(int i = 0; i < list.Length; i++) {
                string driver = list[i];
                Console.Write("Getting driver {0}/{1}\r", i + 1, list.Length);
                drivers.Add(GetDriverInfo(driver));
            }
            Console.WriteLine();

            CreateZip(drivers.ToArray());

            Console.WriteLine("Press any key to end.");
            Console.ReadKey(true);
        }

        static string GetDriversListRaw() {
            ProcessStartInfo pi = new ProcessStartInfo(dismCmd, "/online /Get-Drivers");
            pi.RedirectStandardOutput = true;
            pi.UseShellExecute = false;

            Process proc = new Process();
            proc.StartInfo = pi;

            proc.Start();

            return proc.StandardOutput.ReadToEnd();
        }

        static string[] GetDriversList(string raw) {
            List<string> retval = new List<string>();
            Regex regex = new Regex("Published Name : (\\S*)");

            foreach(Match match in regex.Matches(raw)) {
                retval.Add(match.Groups[1].Value);
            }
            

            return retval.ToArray();
        }

        static Driver GetDriverInfo(string inf) {
            ProcessStartInfo pi = new ProcessStartInfo(dismCmd, "/online /Get-DriverInfo:" + inf);
            pi.RedirectStandardOutput = true;
            pi.UseShellExecute = false;

            Process proc = new Process();
            proc.StartInfo = pi;

            proc.Start();

            string driverInfo = proc.StandardOutput.ReadToEnd();

            Regex regexPath = new Regex("Driver Store Path : (\\S*)");
            Regex regexClass = new Regex("Class Name : (\\S*)");

            Driver driver = new Driver();
            driver.Path = regexPath.Match(driverInfo).Groups[1].Value;
            driver.Class = regexClass.Match(driverInfo).Groups[1].Value;

            return driver;
        }

        static void CreateZip(Driver[] drivers) {
            Console.WriteLine("Saving to c:\\drivers.zip ...");
            
            using(ZipOutputStream s = new ZipOutputStream(File.Create(@"C:\drivers.zip"))) {
                s.SetLevel(9);

                foreach(Driver driver in drivers) {
                    DirectoryInfo di = new FileInfo(driver.Path).Directory;
                    int offset = di.FullName.Length - di.Name.Length;
                    AddDir(di, offset, s);
                }

                s.Finish();
                s.Close();
            }
        }

        static void AddDir(DirectoryInfo dir, int offset, ZipOutputStream s){
            bool empty = true;

            foreach(DirectoryInfo idir in dir.EnumerateDirectories()) {
                AddDir(idir, offset, s);
            }

            foreach(FileInfo ifile in dir.EnumerateFiles()) {
                empty = false;
                ZipEntry entry = new ZipEntry(ifile.FullName.Substring(offset));
                
                s.PutNextEntry(entry);

                using(FileStream fs = ifile.OpenRead()) {
                    fs.CopyTo(s);
                }
            }

            if(empty) {
                ZipEntry entry = new ZipEntry(string.Format("{0}/", dir.FullName.Substring(offset)));
                s.PutNextEntry(entry);
                s.CloseEntry();
            }
        }

        static void GetDism() {
            if(System.IO.Directory.Exists(Environment.ExpandEnvironmentVariables("%systemroot%\\sysnative")))
                dismCmd = Environment.ExpandEnvironmentVariables("%systemroot%\\sysnative") + "\\dism.exe";
            else
                dismCmd = Environment.ExpandEnvironmentVariables("%systemroot%\\system32") + "\\dism.exe";
        }
    }
}
