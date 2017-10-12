using Microsoft.Kinect;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using Kinect_Recorder.Helpers;

namespace Kinect_Recorder
{
    class SkeletonRenderer
    {
        #region instance variables

        /// <summary>
        /// Coordinate mapper to map one type of point to another
        /// </summary>
        private CoordinateMapper coordinateMapper = null;

        /// <summary>
        /// Array for the bodies
        /// </summary>
        private Body[] bodies = null;

        /// <summary>
        /// Width of output drawing
        /// </summary>
        private float RenderWidth;

        /// <summary>
        /// Height of our output drawing
        /// </summary>
        private float RenderHeight;

        /// <summary>
        /// Radius of drawn hand circles
        /// </summary>
        private const double HandSize = 30;

        /// <summary>
        /// Thickness of drawn joint lines
        /// </summary>
        private const double JointThickness = 3;

        /// <summary>
        /// Thickness of clip edge rectangles
        /// </summary>
        private const double ClipBoundsThickness = 10;

        /// <summary>
        /// Brush used for drawing hands that are currently tracked as closed
        /// </summary>
        private readonly Brush handClosedBrush = new SolidColorBrush(Color.FromArgb(128, 255, 0, 0));

        /// <summary>
        /// Brush used for drawing hands that are currently tracked as opened
        /// </summary>
        private readonly Brush handOpenBrush = new SolidColorBrush(Color.FromArgb(128, 0, 255, 0));

        /// <summary>
        /// Brush used for drawing hands that are currently tracked as in lasso (pointer) position
        /// </summary>
        private readonly Brush handLassoBrush = new SolidColorBrush(Color.FromArgb(128, 0, 0, 255));

        /// <summary>
        /// Brush used for drawing joints that are currently tracked
        /// </summary>
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));

        /// <summary>
        /// Brush used for drawing joints that are currently inferred
        /// </summary>        
        private readonly Brush inferredJointBrush = Brushes.Yellow;

        /// <summary>
        /// Pen used for drawing bones that are currently tracked
        /// </summary>
        private readonly Pen trackedBonePen = new Pen(Brushes.Green, 6);

        /// <summary>
        /// Pen used for drawing bones that are currently inferred
        /// </summary>        
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        /// <summary>
        /// Drawing group for body rendering output
        /// </summary>
        private DrawingGroup drawingGroup;

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage imageSource;

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor kinectSensor = null;

        #endregion

        public SkeletonRenderer(Image Image, KinectSensor sensor)
        {
            kinectSensor = sensor;
            bodies = new Body[kinectSensor.BodyFrameSource.BodyCount];
            coordinateMapper = kinectSensor.CoordinateMapper;

            FrameDescription frameDescription = kinectSensor.DepthFrameSource.FrameDescription;
            RenderWidth = frameDescription.Width;
            RenderHeight = frameDescription.Height;

            // Create the drawing group we'll use for drawing
            drawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            imageSource = new DrawingImage(drawingGroup);

            // Display the drawing using our image control
            Image.Source = imageSource;
        }

        /// <summary>
        /// Handles the body frame data arriving from the sensor
        /// </summary>
        /// <param name="e">event arguments</param>
        public void Reader_FrameArrived(BodyFrameArrivedEventArgs e)
        {
            BodyFrame frame = e.FrameReference.AcquireFrame();

            if (frame == null)
                return;

            frame.GetAndRefreshBodyData(bodies);
            frame.Dispose();

            using (DrawingContext drawingContext = drawingGroup.Open())
            {
                // Draw a transparent background to set the render size
                drawingContext.DrawRectangle(Brushes.Transparent, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));

                foreach (Body body in bodies)
                {
                    if (body.IsTracked)
                    {
                        DrawClippedEdges(body, drawingContext);

                        IReadOnlyDictionary<JointType, Joint> joints = body.Joints;

                        // Convert the joint points to depth (display) space
                        Dictionary<JointType, Point> jointPoints = new Dictionary<JointType, Point>();

                        foreach (JointType jointType in joints.Keys)
                        {
                            DepthSpacePoint depthSpacePoint = coordinateMapper.MapCameraPointToDepthSpace(joints[jointType].Position);
                            jointPoints[jointType] = new Point(depthSpacePoint.X, depthSpacePoint.Y);
                        }

                        DrawBody(joints, jointPoints, drawingContext);
                        DrawHand(body.HandLeftState, jointPoints[JointType.HandLeft], drawingContext);
                        DrawHand(body.HandRightState, jointPoints[JointType.HandRight], drawingContext);

                        FormattedText text = new FormattedText(KinectManager.Instance.GetSimpleID(body.TrackingId).ToString(),
                            System.Globalization.CultureInfo.CurrentCulture,
                            FlowDirection.LeftToRight,
                            new Typeface("Calibri"),
                            50,
                            Brushes.Chartreuse);

                        drawingContext.DrawText(text, new System.Windows.Point(jointPoints[JointType.Head].x - 20, jointPoints[JointType.Head].y - 80));

                    }
                }

                drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
            }
        }

        /// <summary>
        /// Draws a body
        /// </summary>
        /// <param name="joints">joints to draw</param>
        /// <param name="jointPoints">translated positions of joints to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private void DrawBody(IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, DrawingContext drawingContext)
        {
            // Draw the bones

            // Torso
            DrawBone(joints, jointPoints, JointType.Head, JointType.Neck, drawingContext);
            DrawBone(joints, jointPoints, JointType.Neck, JointType.SpineShoulder, drawingContext);
            DrawBone(joints, jointPoints, JointType.SpineShoulder, JointType.SpineMid, drawingContext);
            DrawBone(joints, jointPoints, JointType.SpineMid, JointType.SpineBase, drawingContext);
            DrawBone(joints, jointPoints, JointType.SpineShoulder, JointType.ShoulderRight, drawingContext);
            DrawBone(joints, jointPoints, JointType.SpineShoulder, JointType.ShoulderLeft, drawingContext);
            DrawBone(joints, jointPoints, JointType.SpineBase, JointType.HipRight, drawingContext);
            DrawBone(joints, jointPoints, JointType.SpineBase, JointType.HipLeft, drawingContext);

            // Right Arm    
            DrawBone(joints, jointPoints, JointType.ShoulderRight, JointType.ElbowRight, drawingContext);
            DrawBone(joints, jointPoints, JointType.ElbowRight, JointType.WristRight, drawingContext);
            DrawBone(joints, jointPoints, JointType.WristRight, JointType.HandRight, drawingContext);
            DrawBone(joints, jointPoints, JointType.HandRight, JointType.HandTipRight, drawingContext);
            DrawBone(joints, jointPoints, JointType.WristRight, JointType.ThumbRight, drawingContext);

            // Left Arm
            DrawBone(joints, jointPoints, JointType.ShoulderLeft, JointType.ElbowLeft, drawingContext);
            DrawBone(joints, jointPoints, JointType.ElbowLeft, JointType.WristLeft, drawingContext);
            DrawBone(joints, jointPoints, JointType.WristLeft, JointType.HandLeft, drawingContext);
            DrawBone(joints, jointPoints, JointType.HandLeft, JointType.HandTipLeft, drawingContext);
            DrawBone(joints, jointPoints, JointType.WristLeft, JointType.ThumbLeft, drawingContext);

            // Right Leg
            DrawBone(joints, jointPoints, JointType.HipRight, JointType.KneeRight, drawingContext);
            DrawBone(joints, jointPoints, JointType.KneeRight, JointType.AnkleRight, drawingContext);
            DrawBone(joints, jointPoints, JointType.AnkleRight, JointType.FootRight, drawingContext);

            // Left Leg
            DrawBone(joints, jointPoints, JointType.HipLeft, JointType.KneeLeft, drawingContext);
            DrawBone(joints, jointPoints, JointType.KneeLeft, JointType.AnkleLeft, drawingContext);
            DrawBone(joints, jointPoints, JointType.AnkleLeft, JointType.FootLeft, drawingContext);

            // Draw the joints
            foreach (JointType jointType in joints.Keys)
            {
                Brush drawBrush = null;

                TrackingState trackingState = joints[jointType].TrackingState;

                if (trackingState == TrackingState.Tracked)
                    drawBrush = trackedJointBrush;

                else if (trackingState == TrackingState.Inferred)
                    drawBrush = inferredJointBrush;

                if (drawBrush != null)
                    drawingContext.DrawEllipse(drawBrush, null, new System.Windows.Point(jointPoints[jointType].x, jointPoints[jointType].y), JointThickness, JointThickness);
            }
        }

        /// <summary>
        /// Draws one bone of a body (joint to joint)
        /// </summary>
        /// <param name="joints">joints to draw</param>
        /// <param name="jointPoints">translated positions of joints to draw</param>
        /// <param name="jointType0">first joint of bone to draw</param>
        /// <param name="jointType1">second joint of bone to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private void DrawBone(IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, JointType jointType0, JointType jointType1, DrawingContext drawingContext)
        {
            Joint joint0 = joints[jointType0];
            Joint joint1 = joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == TrackingState.NotTracked || joint1.TrackingState == TrackingState.NotTracked)
                return;

            // Don't draw if both points are inferred
            if (joint0.TrackingState == TrackingState.Inferred && joint1.TrackingState == TrackingState.Inferred)
                return;

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = inferredBonePen;

            if ((joint0.TrackingState == TrackingState.Tracked) && (joint1.TrackingState == TrackingState.Tracked))
                drawPen = trackedBonePen;

            drawingContext.DrawLine(drawPen, new System.Windows.Point(jointPoints[jointType0].x, jointPoints[jointType0].y), new System.Windows.Point(jointPoints[jointType1].x, jointPoints[jointType1].y));
        }

        /// <summary>
        /// Draws a hand symbol if the hand is tracked: red circle = closed, green circle = opened; blue circle = lasso
        /// </summary>
        /// <param name="handState">state of the hand</param>
        /// <param name="handPosition">position of the hand</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private void DrawHand(HandState handState, Point handPosition, DrawingContext drawingContext)
        {
            switch (handState)
            {
                case HandState.Closed:
                    drawingContext.DrawEllipse(handClosedBrush, null, new System.Windows.Point(handPosition.x, handPosition.y), HandSize, HandSize);
                    break;

                case HandState.Open:
                    drawingContext.DrawEllipse(handOpenBrush, null, new System.Windows.Point(handPosition.x, handPosition.y), HandSize, HandSize);
                    break;

                case HandState.Lasso:
                    drawingContext.DrawEllipse(handLassoBrush, null, new System.Windows.Point(handPosition.x, handPosition.y), HandSize, HandSize);
                    break;
            }
        }

        /// <summary>
        /// Draws indicators to show which edges are clipping body data
        /// </summary>
        /// <param name="body">body to draw clipping information for</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private void DrawClippedEdges(Body body, DrawingContext drawingContext)
        {
            FrameEdges clippedEdges = body.ClippedEdges;

            if (clippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, RenderHeight - ClipBoundsThickness, RenderWidth, ClipBoundsThickness));
            }

            if (clippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, RenderWidth, ClipBoundsThickness));
            }

            if (clippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, ClipBoundsThickness, RenderHeight));
            }

            if (clippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(RenderWidth - ClipBoundsThickness, 0, ClipBoundsThickness, RenderHeight));
            }
        }
    }
}
