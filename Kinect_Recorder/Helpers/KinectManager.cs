using Microsoft.Kinect;
using Kinect_Recorder.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Kinect_Recorder.Helpers
{
	public class KinectManager
	{

		#region Static Singleton Instance

		public static KinectManager Instance = new KinectManager();

		#endregion

		#region Constructors

		private KinectManager()
		{
			KinectSensor = KinectSensor.GetDefault();
			bodies = new Body[KinectSensor.BodyFrameSource.BodyCount];
			ourGeneratedBodyIDs = new Dictionary<ulong, int>();

			BodyReader = KinectSensor.BodyFrameSource.OpenReader();
			DepthReader = KinectSensor.DepthFrameSource.OpenReader();

			KinectSensor.Open();

			if (BodyReader != null)
				BodyReader.FrameArrived += Reader_FrameArrived;
		}

		#endregion

		#region Variables

		/// <summary>
		/// Reference to the kinect sensor
		/// </summary>
		public KinectSensor KinectSensor;

		/// <summary>
		/// List of current detected bodies
		/// Each body represents a person
		/// </summary>
		private Body[] bodies;

		/// <summary>
		/// The ids created by us for each body detected (for simplicity. Default ids are unnecessarily long)
		/// </summary>
		private Dictionary<ulong, int> ourGeneratedBodyIDs;


		//Some variables to help receive the depth/skeleton data
		public BodyFrameReader BodyReader = null;      //Reader for body frames
		public DepthFrameReader DepthReader = null;    //Reader for body frames

		#endregion

		#region Methods

		/// <summary>
		/// Gets/Generates our id that represents a body
		/// </summary>
		/// <param name="id">Tracking id of the body instance</param>
		/// <returns>Our simpler generated ID</returns>
		public int GetSimpleID(ulong id)
		{
			if (!ourGeneratedBodyIDs.ContainsKey(id))
			{
				int newID = 1;
				if (ourGeneratedBodyIDs.Count() > 0)
					newID = ourGeneratedBodyIDs.Values.Max() + 1;
				ourGeneratedBodyIDs.Add(id, newID);
			}
			return ourGeneratedBodyIDs[id];
		}

		/// <summary>
		/// Convert a body instance to a person instance
		/// </summary>
		public Person ConvertBodyToPerson(Body body)
		{
			Person.HandState handStateRight = (body.HandRightState == HandState.Closed) ? Person.HandState.Closed : Person.HandState.Open;
			Person.HandState handStateLeft = (body.HandLeftState == HandState.Closed) ? Person.HandState.Closed : Person.HandState.Open;

			return new Person(GetSimpleID(body.TrackingId), CreateSkeletonFromBody(body), handStateRight, handStateLeft);
		}

		public Dictionary<BodyJoint, Point> CreateSkeletonFromBody(Body body)
		{
			Dictionary<BodyJoint, Point> skeleton = new Dictionary<BodyJoint, Point>();

			foreach (KeyValuePair<JointType, Joint> pair in body.Joints)
				skeleton.Add((BodyJoint)pair.Key, GetPointFromJoint(pair.Value));

			return skeleton;
		}

		Point GetPointFromJoint(Joint joint)
		{
			return new Point(joint.Position.X, joint.Position.Y, joint.Position.Z);
		}

		/// <summary>
		/// Parse the data received from kinect
		/// </summary>
		void Reader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
		{
			BodyFrameReference frameReference = e.FrameReference;
			BodyFrame frame = frameReference.AcquireFrame();

			if (frame == null)
				return;

			// Make first list of people to compare to determine who has left the scene
			List<Person> oldPersonList = new List<Person>();
			foreach (Body body in bodies)
			{
				if (body != null)
					oldPersonList.Add(ConvertBodyToPerson((body)));
			}

			frame.GetAndRefreshBodyData(bodies);
			frame.Dispose();

			// Second list of people to compare with first to determine who has left the scene
			List<int> currentIDList = new List<int>();
			foreach (Body body in bodies)
				currentIDList.Add(GetSimpleID(body.TrackingId));


			List<Person> persons = new List<Person>();

			foreach (Body body in bodies)
			{
				if (body.IsTracked)
				{
					Person person = ConvertBodyToPerson(body);

					persons.Add(person);
                    if (FileRecording.recording)
                    {
                        int currentId = GetSimpleID(body.TrackingId);;
                        FileRecording.Record_Joint_Points(currentId, person.Joints, DateTime.Now);
                    }
                        
				}
			}
		}

        #endregion
    }
}