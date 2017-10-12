using Microsoft.Kinect;
using System;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace Kinect_Recorder
{
	class DepthRenderer
	{
		#region instance variables

		/// <summary>
		/// Size of the RGB pixel in the bitmap
		/// </summary>
		private readonly int cbytesPerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;

		/// <summary>
		/// Active Kinect sensor
		/// </summary>
		private KinectSensor kinectSensor = null;

		/// <summary>
		/// Reader for depth frames
		/// </summary>
		private DepthFrameReader reader = null;

		/// <summary>
		/// Bitmap to display
		/// </summary>
		private WriteableBitmap bitmap = null;

		/// <summary>
		/// Intermediate storage for receiving frame data from the sensor
		/// </summary>
		public ushort[] frameData = null;

		/// <summary>
		/// Intermediate storage for frame data converted to color
		/// </summary>
		private byte[] pixels = null;

		/// <summary>
		/// Image that will view depth data
		/// </summary>
		private Image Image;

		#endregion

		/// <summary>
		/// Gets the bitmap to display
		/// </summary>
		public ImageSource ImageSource
		{
			get
			{
				return bitmap;
			}
		}

		public DepthRenderer(Image image, KinectSensor sensor)
		{
			// Set the Kinect sensor
			kinectSensor = sensor;
			FrameDescription frameDescription = kinectSensor.DepthFrameSource.FrameDescription;

			// open the reader for the depth frames
			reader = kinectSensor.DepthFrameSource.OpenReader();

			// allocate space to put the pixels being received and converted
			frameData = new ushort[frameDescription.Width * frameDescription.Height];
			pixels = new byte[frameDescription.Width * frameDescription.Height * cbytesPerPixel];

			// create the bitmap to display
			bitmap = new WriteableBitmap(frameDescription.Width, frameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);

			// Initialize the image
			Image = image;

			// Set the image we display to point to the bitmap where we'll put the image data
			Image.Source = bitmap;
		}

		public void RenderDepthFrame(DepthFrameArrivedEventArgs e)
		{
			DepthReader_FrameArrived(e);
		}

		private void DepthReader_FrameArrived(DepthFrameArrivedEventArgs e)
		{
			DepthFrameReference frameReference = e.FrameReference;

			try
			{
				DepthFrame frame = frameReference.AcquireFrame();

				if (frame != null)
				{
					// DepthFrame is IDisposable
					using (frame)
					{
						FrameDescription frameDescription = frame.FrameDescription;

						// verify data and write the new depth frame data to the display bitmap
						if (((frameDescription.Width * frameDescription.Height) == frameData.Length) &&
							(frameDescription.Width == bitmap.PixelWidth) && (frameDescription.Height == bitmap.PixelHeight))
						{
							// Copy the pixel data from the image to a temporary array
							frame.CopyFrameDataToArray(frameData);

							// Get the min and max reliable depth for the current frame
							ushort minDepth = frame.DepthMinReliableDistance;
							ushort maxDepth = frame.DepthMaxReliableDistance;

							// Convert the depth to RGB
							int colorPixelIndex = 0;
							for (int i = 0; i < frameData.Length; ++i)
							{
								// Get the depth for this pixel
								ushort depth = frameData[i];

								// To convert to a byte, we're discarding the most-significant
								// rather than least-significant bits.
								// We're preserving detail, although the intensity will "wrap."
								// Values outside the reliable depth range are mapped to 0 (black).
								byte intensity = (byte)(depth >= minDepth && depth <= maxDepth ? depth : 0);

								// Write out blue byte
								pixels[colorPixelIndex++] = intensity;

								// Write out green byte
								pixels[colorPixelIndex++] = intensity;

								// Write out red byte                        
								pixels[colorPixelIndex++] = intensity;

								// We're outputting BGR, the last byte in the 32 bits is unused so skip it
								// If we were outputting BGRA, we would write alpha here.
								++colorPixelIndex;
							}

							bitmap.WritePixels(
								new Int32Rect(0, 0, frameDescription.Width, frameDescription.Height),
								pixels,
								frameDescription.Width * cbytesPerPixel,
								0);
						}
					}
				}
			}
			catch (Exception)
			{
				// ignore if the frame is no longer available
			}
		}
	}
}
