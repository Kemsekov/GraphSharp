using System;
using System.IO;
using System.Text;
namespace GraphSharp.Extensions
{
    public static partial class _Extensions
    {
        static object obj = new object();
        public static void WriteUTF8(this FileStream f,string message){
            var data = Encoding.UTF8.GetBytes(message);
            lock(obj)
                f.Write(data,0,data.Length);
        }
        public static void WriteLine(this FileStream f,string message = "", params object[] prms){
            lock(obj){
                byte[] data;
                if(prms.Length>0)
                    data = Encoding.UTF8.GetBytes(String.Format(message+"\n",prms));
                else
                    data = Encoding.UTF8.GetBytes(message+"\n");
                f.Write(data,0,data.Length);
            }
        }
    }
}