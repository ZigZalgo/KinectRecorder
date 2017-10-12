using Newtonsoft.Json;
using System.Collections.Generic;

public class Person
{
	public int PersonID;

	public Point HandLocationRight;
	public Point HandLocationLeft;
	public Point Location;

	public Dictionary<BodyJoint, Point> Joints;

	public HandState HandStateRight;
	public HandState HandStateLeft;

	public Person(int id, Dictionary<BodyJoint, Point> skeleton, HandState rhs, HandState lhs)
	{
		PersonID = id;
		foreach (var p in skeleton)
		{
			p.Value.z *= -1;

		}
		Joints = skeleton;

		HandLocationRight = skeleton[BodyJoint.HandRight];
		HandLocationLeft = skeleton[BodyJoint.HandLeft];
		Location = skeleton[BodyJoint.SpineBase];

		HandStateRight = rhs;
		HandStateLeft = lhs;
	}

	public enum HandState
	{
		Open,
		Closed
	}

	public override string ToString()
	{
		return string.Format("Person {0} \n\tLocation: {1}\n\tRight Hand Location: {2}\tState: {3}\n\tLeft Hand Location:  {4}\tState{5}",
			PersonID,
			Location,
			HandLocationRight,
			HandStateRight,
			HandLocationLeft,
			HandStateLeft);
	}
}