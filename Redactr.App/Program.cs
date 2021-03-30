using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Abstractions;
using System.Collections.Generic;

namespace Redactr
{
    public struct RedactData
    {
        public RedactData(IFileSystem fs, string fn, string rw, char rc)
        {
            FS = fs;
            FN = fn;
            RW = rw;
            RC = rc;
        }
        public IFileSystem FS; // Filesystem
        public string FN; // File name
        public string RW; // Redacted Word
        public char RC; // Replacement Char
    }

    public struct TraverseData
    {
        public TraverseData(string dn, string rw)
        {
            DN = dn;
            RW = rw;
        }
        public string DN; // Directory Name
        public string RW; // Redacted Word
    }

    public class Redactor
    {
        public static void Redact(object data)
        {
            RedactData rd = (RedactData)data;
            String str = rd.FS.File.ReadAllText(rd.FN);
            str = str.Replace(rd.RW, new String(rd.RC, rd.RW.Length));
            rd.FS.File.WriteAllText(rd.FN, str);
        }
    }

    public class Redactr
    {
        public static List<Task> tasks = new List<Task>();

        readonly IFileSystem filesystem;

        public Redactr(IFileSystem filesystem)
        {
            this.filesystem = filesystem;
        }

        public Redactr() : this(
            filesystem : new FileSystem()
        )
        {

        }

        public void TraverseDirectory(object data)
        {
            TraverseData td = (TraverseData)data;


            IDirectoryInfo dirInfo = this.filesystem.DirectoryInfo.FromDirectoryName(td.DN);
            
            foreach (IDirectoryInfo di in dirInfo.EnumerateDirectories())
            {
                Redactr r = new Redactr(this.filesystem);
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    r.TraverseDirectory(new TraverseData(di.FullName, td.RW));
                }));
                
            }

            foreach (IFileInfo fi in dirInfo.EnumerateFiles())
            {
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    Redactor.Redact(new RedactData(this.filesystem, fi.FullName, td.RW, '*'));
                }));
            }
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            if(args.Length < 2)
            {
                Console.WriteLine("Syntax: Redactr <Directory Name> <Word To Redact>");
                return;
            }

            Redactr r = new Redactr();
            r.TraverseDirectory(new TraverseData(args[0], args[1]));
        }
    }
}
