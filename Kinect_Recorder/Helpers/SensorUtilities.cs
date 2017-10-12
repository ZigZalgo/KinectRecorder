using Microsoft.Kinect;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Kinect_Recorder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;

public class SensorUtilities
{

	#region JSON

	/// <summary>
	/// The serialization/deserialization settings
	/// </summary>
	static JsonSerializerSettings JsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All, MetadataPropertyHandling = MetadataPropertyHandling.Ignore, Formatting = Formatting.Indented };



	/// <summary>
	/// Serialize one instance of an object
	/// </summary>
	public static string Serialize<T>(T obj)
	{
		return JsonConvert.SerializeObject(obj, JsonSettings);
	}

	/// <summary>
	/// Serialize list of an object type
	/// </summary>
	public static string Serialize<T>(List<T> obj)
	{
		string message = string.Empty;
		foreach (T item in obj)
			message += Serialize(item) + "^^^^^";

		if (message.Length > 0)
			return message.Substring(0, message.Length - 5);
		else
			return string.Empty;
	}

	/// <summary>
	/// Deserializes a json string into a single person.
	/// </summary>
	/// <param name="msg"></param>
	/// <returns></returns>
	public static Person Deserialize(string msg)
	{
		JObject JO = JObject.Parse(msg);

		int id = (int)JO["PersonID"];
		Dictionary<BodyJoint, global::Point> skeleton = (Dictionary<BodyJoint, global::Point>)JO["Joints"].ToObject(typeof(Dictionary<BodyJoint, global::Point>));
		Person.HandState rhs = (Person.HandState)(int)JO["HandStateRight"];
		Person.HandState lhs = (Person.HandState)(int)JO["HandStateLeft"];

		Person person = new Person(id, skeleton, rhs, lhs);
		return person;
	}

	#endregion

	#region Kinect Slaving

	static float RADIANS_TO_DEGREES = 180 / (float)Math.PI;
	static float DEGREES_TO_RADIANS = (float)Math.PI / 180;
	static float ROUND_RATIO = 150;                           // the round ratio for dealing with not accurate calculation

	public static Point applyTranslateRule(Point originalLocation, TranslateRule translateRule)
	{

		Point location = new Point(originalLocation.x * 1000, originalLocation.y * 1000, originalLocation.z * 1000);
		Point vectorToStartingPoint = new Point(location.x - translateRule.startingLocation.x, 0, location.z - translateRule.startingLocation.z);

		float rotatedx = (float)(vectorToStartingPoint.x * Math.Cos(translateRule.changeInOrientation * Math.PI / 180) + vectorToStartingPoint.y * Math.Sin(translateRule.changeInOrientation * Math.PI / 180));
		float rotatedy = (float)(vectorToStartingPoint.y * Math.Cos(translateRule.changeInOrientation * Math.PI / 180) - vectorToStartingPoint.x * Math.Sin(translateRule.changeInOrientation * Math.PI / 180));


		Point rotatedPoint = new Point(rotatedx, 0.6f, rotatedy);


		rotatedPoint.x += translateRule.dX + translateRule.startingLocation.x;
		rotatedPoint.z += translateRule.dZ + translateRule.startingLocation.z;
		rotatedPoint.x = (float)Math.Round(rotatedPoint.x * 1000000000) / 1000000000 / 1000;
		rotatedPoint.z = (float)Math.Round(rotatedPoint.z * 1000000000) / 1000000000 / 1000;
		return rotatedPoint;
	}

	public class TranslateRule
	{
		public TranslateRule(float changeInOrientation, float dX, float dZ, float xSpaceTran, float zSpaceTran, Point startingLocation)
		{
			this.changeInOrientation = changeInOrientation;
			this.dX = dX;
			this.dZ = dZ;
			this.xSpace = xSpaceTran;
			this.zSpace = zSpaceTran;
			this.startingLocation = startingLocation;
		}
		public float changeInOrientation { get; set; }
		public float dX { get; set; }
		public float dZ { get; set; }
		public float xSpace { get; set; }
		public float zSpace { get; set; }
		public Point startingLocation;
	}

	private static float getAngleOfTwoVectors(Point vector1, Point vector2)
	{
		var vector1length = Math.Sqrt(Math.Pow(vector1.x, 2) + Math.Pow(vector1.z, 2));
		var vector2length = Math.Sqrt(Math.Pow(vector2.x, 2) + Math.Pow(vector2.z, 2));
		var v1MulV2 = vector1.x * vector2.x + vector1.z * vector2.z;

		return (float)Math.Acos(v1MulV2 / (vector1length * vector2length)) * RADIANS_TO_DEGREES; // Dot product
	}

	private static Point getDistanceVector(Point locationA, Point locationB)
	{
		return new Point(locationB.x - locationA.x, 0, locationB.z - locationA.z);
	}

	private static Point matrixRotation(Point originalLocation, float angle)
	{
		Point returnLocation = new Point(0, 0, 0);
		var returnX = originalLocation.x * Math.Cos(angle * DEGREES_TO_RADIANS) + originalLocation.z * Math.Sin(angle * DEGREES_TO_RADIANS);
		var returnZ = originalLocation.z * Math.Cos(angle * DEGREES_TO_RADIANS) - (originalLocation.x * Math.Sin(angle * DEGREES_TO_RADIANS));
		returnLocation.x = (float)Math.Round(returnX * ROUND_RATIO) / ROUND_RATIO;
		returnLocation.z = (float)Math.Round(returnZ * ROUND_RATIO) / ROUND_RATIO;

		return returnLocation;
	}

	public static TranslateRule GetTranslationRule(Point startingLocation1, Point endingLocation1, Point startingLocation2, Point endingLocation2)
	{
		float degree;
		float xDistance;
		float zDistance;
		float xSpaceTransition;
		float zSpaceTransition;
		Point startingLocation;
		Point dv1 = getDistanceVector(startingLocation1, endingLocation1);
		Point dv2 = getDistanceVector(startingLocation2, endingLocation2);


		//get values
		float angleBetweenVectors = getAngleOfTwoVectors(dv1, dv2);						// using dot product
		Point rotatedVector2 = matrixRotation(dv2, angleBetweenVectors);                // clockwise
		Point counterRotatedVector2 = matrixRotation(dv2, -angleBetweenVectors);
		Point rotatedVectorEndingLocation2 = matrixRotation(endingLocation2, angleBetweenVectors);
		Point counterRotatedVectorEndingLocation2 = matrixRotation(endingLocation2, -angleBetweenVectors);


		//function fixSign
		float spaceTransitionX;
		float spaceTransitionZ;
		if (Math.Abs(rotatedVector2.x - dv1.x) < ROUND_RATIO && Math.Abs(rotatedVector2.z - dv1.z) < ROUND_RATIO)
		{
			spaceTransitionX = (endingLocation1.x - rotatedVectorEndingLocation2.x);
			spaceTransitionZ = (endingLocation1.z - rotatedVectorEndingLocation2.z);

			degree = angleBetweenVectors;
			xDistance = startingLocation1.x - startingLocation2.x;
			zDistance = startingLocation1.z - startingLocation2.z;
			xSpaceTransition = spaceTransitionX;
			zSpaceTransition = spaceTransitionZ;
			startingLocation = startingLocation2;
		}


		else if (Math.Abs(counterRotatedVector2.x - dv1.x) < ROUND_RATIO && Math.Abs(counterRotatedVector2.z - dv1.z) < ROUND_RATIO)
		{
			spaceTransitionX = (endingLocation1.x - counterRotatedVectorEndingLocation2.x);
			spaceTransitionZ = (endingLocation1.z - counterRotatedVectorEndingLocation2.z);

			degree = -angleBetweenVectors;
			xDistance = startingLocation1.x - startingLocation2.x;
			zDistance = startingLocation1.z - startingLocation2.z;
			xSpaceTransition = spaceTransitionX;
			zSpaceTransition = spaceTransitionZ;
			startingLocation = startingLocation2;

		}
		else
		{
			degree = float.NaN;
			xDistance = startingLocation1.x - startingLocation2.x;
			zDistance = startingLocation1.z - startingLocation2.z;
			xSpaceTransition = 0;
			zSpaceTransition = 0;
			startingLocation = startingLocation2;

		}
		return new TranslateRule(degree, xDistance, zDistance, xSpaceTransition, zSpaceTransition, startingLocation);
	}

	#endregion

}
