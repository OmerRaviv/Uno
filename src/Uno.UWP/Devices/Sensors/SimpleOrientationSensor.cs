using System;

namespace Windows.Devices.Sensors
{
	public partial class SimpleOrientationSensor
	{
		#region Static

		public static SimpleOrientationSensor _instance;

		public static SimpleOrientationSensor GetDefault()
		{
			if (_instance == null)
			{
				_instance = new SimpleOrientationSensor();
			}

			return _instance;
		}

		#endregion

		private SimpleOrientation _currentOrientation;

		/// <summary>
		/// Represents a simple orientation sensor.
		/// </summary>
		private SimpleOrientationSensor()
		{
			Initialize();
		}

		partial void Initialize();

		/// <summary>
		/// Gets the device identifier.
		/// </summary>
		[Uno.NotImplemented]
		public string DeviceId { get; }

		/// <summary>
		/// Gets or sets the transformation that needs to be applied to sensor data. Transformations to be applied are tied to the display orientation with which to align the sensor data.
		/// </summary>
		/// <remarks>
		/// This is not currently implemented, and acts as if <see cref="ReadingTransform" /> was set to <see cref="DisplayOrientation.Portrait" />.
		/// </remarks>
		[Uno.NotImplemented]
		public Graphics.Display.DisplayOrientations ReadingTransform { get; set; } = Graphics.Display.DisplayOrientations.Portrait;

		/// <summary>
		/// Gets the default simple orientation sensor.
		/// </summary>
		/// <returns></returns>
		public SimpleOrientation GetCurrentOrientation() => _currentOrientation;

		/// <summary>
		/// Occurs each time the simple orientation sensor reports a new sensor reading.
		/// </summary>
		public event Foundation.TypedEventHandler<SimpleOrientationSensor, SimpleOrientationSensorOrientationChangedEventArgs> OrientationChanged;

		private void SetCurrentOrientation(SimpleOrientation orientation)
		{
			if (_currentOrientation != orientation)
			{
				_currentOrientation = orientation;
				var args = new SimpleOrientationSensorOrientationChangedEventArgs()
				{
					Orientation = orientation,
					Timestamp = DateTimeOffset.Now,
				};
				OrientationChanged?.Invoke(this, args);
			}
		}
	}
}
