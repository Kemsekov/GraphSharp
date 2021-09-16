using System;
using System.IO;
using System.Text;
namespace GraphSharp.Extensions
{
    public static partial class _Extensions
    {
        static object obj = new();
        public static void WriteUTF8(this FileStream f,string message){
            lock(obj)
                f.Write(Encoding.UTF8.GetBytes(message));
        }
        public static void WriteLine(this FileStream f,string message = "", params object[] prms){
            lock(obj){
                if(prms.Length>0)
                    f.Write(Encoding.UTF8.GetBytes(String.Format(message+"\n",prms)));
                else
                    f.Write(Encoding.UTF8.GetBytes(message+"\n"));
            }
        }
    }
}