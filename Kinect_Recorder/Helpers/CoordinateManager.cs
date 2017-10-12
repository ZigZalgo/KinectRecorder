using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinect_Recorder.Helpers
{
	public class CoordinateManager
	{
		#region Static Singleton Instance

		public static CoordinateManager Instance = new CoordinateManager();

		#endregion

		#region Constructors

		/// <summary>
		/// Private constuctor to prevent creating an instance outside this class
		/// </summary>
		private CoordinateManager()
		{

		}

		#endregion



		#region Math Utilities

		/// <summary>
		/// Finds the distance between two 3D points. Formula of the form:<para/>
		/// distance = sqrt( (x2 - x1)^2 + (y2 - y1)^2 + (z2 - z1)^2 )
		/// </summary>
		/// <param name="p1"></param>
		/// <param name="p2"></param>
		/// <returns></returns>
		public float Distance(Point p1, Point p2)
		{
			float dX = p2.x - p1.x;
			dX = dX * dX;

			float dY = p2.y - p1.y;
			dY = dY * dY;

			float dZ = p2.z - p1.z;
			dZ = dZ * dZ;

			float distance = (float)Math.Sqrt(dX + dY + dZ);
			return distance;
		}

		#endregion

	}

}
