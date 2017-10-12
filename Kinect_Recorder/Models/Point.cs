public class Point
{
	public float x;
	public float y;
	public float z;

	public Point() : this(0, 0, 0) { }

	public Point(float x, float y)
	{
		this.x = x;
		this.y = y;
	}

	public Point(float x, float y, float z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public Point(double[] point)
	{
		x = (float)point[0];
		y = (float)point[1];
		z = (float)point[2];
	}

	public double[] ToArray()
	{
		return new double[] { x, y, z };
	}

	public static Point operator +(Point p1, Point p2)
	{
		return new Point(p1.x + p2.x, p1.y + p2.y, p1.z + p2.z);
	}

	public static Point operator -(Point p1, Point p2)
	{
		return new Point(p1.x - p2.x, p1.y - p2.y, p1.z - p2.z);
	}

	public static Point operator *(Point p1, float val)
	{
		return new Point(p1.x * val, p1.y * val, p1.z * val);
	}

	public static Point operator *(Point p1, Point p2)
	{
		return new Point(p1.x * p2.x, p1.y * p2.y, p1.z * p2.z);
	}
}