using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Factorization;

public class AwwYeahMath
{
	private AwwYeahMath() { }
	public static AwwYeahMath Instance = new AwwYeahMath();

	public List<double[]> PreX { get; private set; } = new List<double[]>();
	public List<double[]> PreY { get; private set; } = new List<double[]>();

	public Matrix<double> T { get; private set; }

	public bool Calibrated { get; private set; } = false;

	public void AddPreXPoint(double[] point)
	{
		System.Diagnostics.Debug.WriteLine("PreX point added: " + new Point(point));
		PreX.Add(point);
	}

	public void AddPreYPoint(double[] point)
	{
		System.Diagnostics.Debug.WriteLine("PreY point added: " + new Point((float)point[0], (float)point[1], (float)point[2]));
		PreY.Add(point);
	}

	public double[] CalculateCentroidPoint(List<double[]> pointList)
	{
		double avgX = 0;
		double avgY = 0;
		double avgZ = 0;

		double length = pointList.Count;

		for (int i = 0; i < length; i++)
		{
			avgX += pointList[i][0];
			avgY += pointList[i][1];
			avgZ += pointList[i][2];
		}

		return new double[] { avgX / length, avgY / length, avgZ / length };
	}

	public List<double[]> GetCentroidDistanceMatrix(double[] centroid, List<double[]> pointList)
	{
		int length = pointList.Count;
		List<double[]> result = new List<double[]>();

		for (int i = 0; i < length; i++)
			result.Add(FromToVector(centroid, pointList[i]));

		return result;
	}

	public double[] FromToVector(double[] from, double[] to)
	{
		return new double[] { to[0] - from[0], to[1] - from[1], to[2] - from[2] };
	}

	public Matrix<double> ListToMatrix(List<double[]> pointList)
	{
		Matrix<double> result = CreateMatrix.Dense<double>(3, pointList.Count);

		for (int i = 0; i < pointList.Count; i++)
			result.SetColumn(i, pointList[i]);

		return result;
	}

	void Debug(string message)
	{
		System.Diagnostics.Debug.WriteLine(message);
	}

	void DebugList(string title, List<double[]> list)
	{
		Debug(title);

		foreach (double[] point in list)
			Debug("\t" + new Point(point).ToString());
	}

	public void DoTheThing()
	{
		Debug("DOTHETHING~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
		Debug("\n~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\n");

		DebugList("PreX List: (Master)", PreX);
		DebugList("PreY List: (Slave)", PreY);

		Debug("\n~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\n");

		double[] masterCentroid = CalculateCentroidPoint(PreX);
		double[] slaveCentroid = CalculateCentroidPoint(PreY);

		Debug("Master Centroid: " + new Point(masterCentroid));
		Debug("Slave Centroid:  " + new Point(slaveCentroid));

		Debug("\n~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\n");

		List<double[]> realXList = GetCentroidDistanceMatrix(masterCentroid, PreX);
		List<double[]> realYList = GetCentroidDistanceMatrix(slaveCentroid, PreY);

		DebugList("Distance From Centroid List X: (Master)", realXList);
		DebugList("Distance From Centroid List Y: (Slave)", realYList);

		Debug("\n~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\n");

		Matrix<double> X = ListToMatrix(realXList);
		Matrix<double> Y = ListToMatrix(realYList);

		Matrix<double> S = X.TransposeAndMultiply(Y);

		Debug("S = X * YT: " + S.ToString());

		Debug("\n~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\n");

		Svd<double> SVD = S.Svd();

		Debug("U:  " + SVD.U);
		Debug("VT: " + SVD.VT);

		Debug("\n~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\n");

		Matrix<double> V = SVD.VT.Transpose();

		Matrix<double> R = V.TransposeAndMultiply(SVD.U);

		Debug("R:  " + R);
		Debug("\n~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\n");

		Vector<double> mBar = CreateVector.Dense(masterCentroid);
		Vector<double> sBar = CreateVector.Dense(slaveCentroid);

		Debug("mBar: " + mBar);
		Debug("sBar: " + sBar);
		Debug("\n~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\n");

		Vector<double> tArrow = sBar - R.Multiply(mBar);

		Debug("tArrow = sBar - R * mBar: " + tArrow);
		Debug("\n~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\n");


		T = CreateMatrix.Dense<double>(4, 4);

		double[] column0 = new double[4] { R[0, 0], R[1, 0], R[2, 0], 0 };
		double[] column1 = new double[4] { R[0, 1], R[1, 1], R[2, 1], 0 };
		double[] column2 = new double[4] { R[0, 2], R[1, 2], R[2, 2], 0 };
		double[] column3 = new double[4] { tArrow[0], tArrow[1], tArrow[2], 1 };

		T.SetColumn(0, column0);
		T.SetColumn(1, column1);
		T.SetColumn(2, column2);
		T.SetColumn(3, column3);

		Debug("T: " + T);

		Calibrated = true;
	}

	public Point TranslateToMaster(Point slavePoint)
	{
		double[] pArray = slavePoint.ToArray();
		double[] pArrayWith1 = new double[] { pArray[0], pArray[1], pArray[2], 1 };

		Vector<double> s = CreateVector.Dense(pArrayWith1);

		Vector<double> master = T.Inverse().Multiply(s);

		return new Point((float)master[0], (float)master[1], (float)master[2]);
	}

	/// <summary>
	/// 
	/// </summary>
	public void RunMathTest()
	{
		// What we should get for our master and slave centroids
		double[] idealCentroid = { 0.0, 0.0, 1.0 };

		// Scenario master points
		List<double[]> masterTestPoints = new List<double[]>();
		masterTestPoints.Add(new double[] { -0.25, 0, 0.75 });
		masterTestPoints.Add(new double[] { 0.25, 0, 0.75 });
		masterTestPoints.Add(new double[] { -0.25, 0, 1.25 });
		masterTestPoints.Add(new double[] { 0.25, 0, 1.25 });
		masterTestPoints.Add(idealCentroid);

		// Scenario slave points
		List<double[]> slaveTestPoints = new List<double[]>();
		slaveTestPoints.Add(new double[] { -0.25, 0, 0.75 });
		slaveTestPoints.Add(new double[] { -0.25, 0, 1.25 });
		slaveTestPoints.Add(new double[] { 0.25, 0, 1.25 });
		slaveTestPoints.Add(new double[] { 0.25, 0, 0.75 });
		slaveTestPoints.Add(idealCentroid);

		// Calculate master and slave centroids
		double[] masterCentroid = CalculateCentroidPoint(masterTestPoints);
		double[] slaveCentroid = CalculateCentroidPoint(slaveTestPoints);

		// Get (points - centroid) points
		List<double[]> xPoints = GetCentroidDistanceMatrix(masterCentroid, masterTestPoints);
		List<double[]> yPoints = GetCentroidDistanceMatrix(slaveCentroid, slaveTestPoints);

		// Get X and Y matrices
		Matrix<double> X = ListToMatrix(xPoints);
		Matrix<double> Y = ListToMatrix(yPoints);

		System.Diagnostics.Debug.WriteLine(X);
	}
}