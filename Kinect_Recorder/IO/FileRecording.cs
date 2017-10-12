using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinect_Recorder.IO
{
    public static class FileRecording
    {

        private class JsonJoint
        {
            public long time;
            public int Id;
            public SortedList<BodyJoint, Point> joints;
        }

        public static bool recording = false;
        private static FileStream stream;
        private static StreamWriter writer;
        
        public static void GenNewTempStreamWriter()
        {
            if (writer != null)
            {
                writer.Close();
                stream.Close();
            }
            string path = Path.GetTempPath();
            path += "tmp.JSON";
            stream = File.Create(path);
            writer = new StreamWriter(stream);
        }

        public static void Record_Joint_Points(int id, Dictionary<BodyJoint, Point> Joint_Points, DateTime time)
        {
            JsonJoint j = new JsonJoint();
            j.joints = new SortedList<BodyJoint, Point>();
            if (writer == null)
            {
                GenNewTempStreamWriter();
            }
            j.time = time.Ticks;
            j.Id = id;
            foreach(var pair in Joint_Points)
            {
                j.joints.Add(pair.Key, pair.Value);
            }
            writer.WriteLine(JsonConvert.SerializeObject(j).Trim());
        }


        public static void endFile(string newPath)
        {
            if(writer != null)
            writer.Close();
            if(stream != null)
            stream.Close();
            string path = Path.GetTempPath();
            path += "tmp.JSON";
            File.Copy(path, newPath, true);
            File.Delete(path);
        }
    }
}
