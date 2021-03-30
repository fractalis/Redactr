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

        readonly IFileSystem fileSystem;

        public Redactr(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public Redactr() : this(
            fileSystem: new FileSystem()
        )
        {
        }

        public void TraverseDirectory(string dn, string rw)
        {
            IDirectoryInfo dirInfo = this.fileSystem.DirectoryInfo.FromDirectoryName(dn);
            
            foreach (IDirectoryInfo di in dirInfo.EnumerateDirectories())
            {
                Redactr r = new Redactr(this.fileSystem);
                r.TraverseDirectory(di.FullName, rw);
            }

            foreach (IFileInfo fi in dirInfo.EnumerateFiles())
            {
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    Redactor.Redact(new RedactData(this.fileSystem, fi.FullName, rw, '*'));
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
            r.TraverseDirectory(args[0], args[1]);

            while (Redactr.tasks.Any(t => !t.IsCompleted)) { } // Spin-Wait
        }
    }
}
