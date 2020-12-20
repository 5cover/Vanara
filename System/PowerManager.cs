﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using Vanara.Collections;
using Vanara.Extensions;
using Vanara.InteropServices;
using Vanara.PInvoke;
using static Vanara.PInvoke.Kernel32;
using static Vanara.PInvoke.PowrProf;
using static Vanara.PInvoke.User32;

namespace Vanara.Diagnostics
{
	/// <summary>Indicates the status of the battery.</summary>
	public enum BatteryStatus
	{
		/// <summary>The battery or battery controller is not present.</summary>
		NotPresent,

		/// <summary>The battery is discharging.</summary>
		Discharging,

		/// <summary>The battery is idle.</summary>
		Idle,

		/// <summary>The battery is charging.</summary>
		Charging
	}

	/// <summary>Specifies the status of battery saver.</summary>
	public enum EnergySaverStatus
	{
		/// <summary>Battery saver is off permanently or the device is plugged in.</summary>
		Disabled,

		/// <summary>Battery saver is off now, but ready to turn on automatically.</summary>
		Off,

		/// <summary>Battery saver is on. Save energy where possible.</summary>
		On
	}

	/// <summary>Specifies the power capabilities of a device.</summary>
	[Flags]
	public enum PowerCapabilities
	{
		/// <summary>There is a system power button.</summary>
		PowerButtonPresent = 1 << 0,

		/// <summary>There is a system sleep button.</summary>
		SleepButtonPresent = 1 << 1,

		/// <summary>There is a lid switch.</summary>
		LidPresent = 1 << 2,

		/// <summary>The operating system supports sleep state S1.</summary>
		SystemS1 = 1 << 3,

		/// <summary>The operating system supports sleep state S2.</summary>
		SystemS2 = 1 << 4,

		/// <summary>The operating system supports sleep state S3.</summary>
		SystemS3 = 1 << 5,

		/// <summary>The operating system supports sleep state S4 (hibernation).</summary>
		SystemS4 = 1 << 6,

		/// <summary>The operating system supports power off state S5 (soft off).</summary>
		SystemS5 = 1 << 7,

		/// <summary>The system hibernation file is present.</summary>
		HiberFilePresent = 1 << 8,

		/// <summary>The system supports wake capabilities.</summary>
		FullWake = 1 << 9,

		/// <summary>The system supports video display dimming capabilities.</summary>
		VideoDimPresent = 1 << 10,

		/// <summary>The system supports APM BIOS power management features.</summary>
		ApmPresent = 1 << 11,

		/// <summary>There is an uninterruptible power supply (UPS).</summary>
		UpsPresent = 1 << 12,

		/// <summary>The system supports thermal zones.</summary>
		ThermalControl = 1 << 13,

		/// <summary>The system supports processor throttling.</summary>
		ProcessorThrottle = 1 << 14,

		/// <summary>The system supports the hybrid sleep state.</summary>
		FastSystemS4 = 1 << 15,

		/// <summary>
		/// The system supports fast startup (aka: hiberboot, hybrid boot, or hybrid shutdown) which is a setting that helps your PC start
		/// up faster after shutdown.
		/// </summary>
		Hiberboot = 1 << 16,

		/// <summary>
		/// The platform has support for ACPI wake alarm devices. For more details on wake alarm devices, please see the ACPI specification
		/// section 9.18.
		/// </summary>
		WakeAlarmPresent = 1 << 17,

		/// <summary>The system supports the S0 low power idle model.</summary>
		AoAc = 1 << 18,

		/// <summary>The system supports allowing the removal of power to fixed disk devices.</summary>
		DiskSpinDown = 1 << 19,

		/// <summary/>
		AoAcConnectivitySupported = 1 << 20,

		/// <summary>There are one or more batteries in the system.</summary>
		SystemBatteriesPresent = 1 << 21,

		/// <summary>The system batteries are short-term. Short-term batteries are used in uninterruptible power supplies (UPS).</summary>
		BatteriesAreShortTerm = 1 << 22
	}

	/// <summary>Represents the device's power supply status.</summary>
	public enum PowerSupplyStatus
	{
		/// <summary>The device has no power supply.</summary>
		NotPresent,

		/// <summary>The device has an inadequate power supply.</summary>
		Inadequate,

		/// <summary>The device has an adequate power supply.</summary>
		Adequate
	}

	/// <summary>
	/// Provides access to information about a device's battery and power supply status and configuration. This extends the capabilities
	/// Windows.System.Power.PowerManager to include more detail, schemes and devices.
	/// </summary>
	public static class PowerManager
	{
		private static readonly Singleton Instance = new Singleton();

		public static event EventHandler<PowerEventArgs<uint>> AwayModeChanged
		{
			add => Instance.AddEvent(GUID_SYSTEM_AWAYMODE, value);
			remove => Instance.RemoveEvent(GUID_SYSTEM_AWAYMODE, value);
		}

		public static event EventHandler<PowerEventArgs<uint>> BatteryCapacityChanged
		{
			add => Instance.AddEvent(GUID_BATTERY_PERCENTAGE_REMAINING, value);
			remove => Instance.RemoveEvent(GUID_BATTERY_PERCENTAGE_REMAINING, value);
		}

		public static event EventHandler<PowerEventArgs<uint>> BatterySaverStatusChanged
		{
			add => Instance.AddEvent(GUID_POWER_SAVING_STATUS, value);
			remove => Instance.RemoveEvent(GUID_POWER_SAVING_STATUS, value);
		}

		/// <summary>
		/// Occurs when the current monitor's display state has changed.
		/// <para>
		/// <strong>Windows 7, Windows Server 2008 R2, Windows Vista and Windows Server 2008</strong>: This notification is available
		/// starting with Windows 8 and Windows Server 2012.
		/// </para>
		/// <para>The Data member is a DWORD with one of the following values.</para>
		/// <list type="bullet">
		/// <item>0x0 - The display is off.</item>
		/// <item>0x1 - The display is on.</item>
		/// <item>0x2 - The display is dimmed.</item>
		/// </list>
		/// </summary>
		public static event EventHandler<PowerEventArgs<uint>> CurrentMonitorDisplayStateChanged
		{
			add => Instance.AddEvent(GUID_CONSOLE_DISPLAY_STATE, value);
			remove => Instance.RemoveEvent(GUID_CONSOLE_DISPLAY_STATE, value);
		}

		public static event EventHandler<PowerEventArgs<uint>> GlobalUserStatusChanged
		{
			add => Instance.AddEvent(GUID_GLOBAL_USER_PRESENCE, value);
			remove => Instance.RemoveEvent(GUID_GLOBAL_USER_PRESENCE, value);
		}

		internal static event EventHandler LowBattery;

		public static event EventHandler<PowerEventArgs<Guid>> PowerSchemePersonalityChanged
		{
			add => Instance.AddEvent(GUID_POWERSCHEME_PERSONALITY, value);
			remove => Instance.RemoveEvent(GUID_POWERSCHEME_PERSONALITY, value);
		}

		public static event EventHandler PowerStatusChanged;

		public static event EventHandler<PowerEventArgs<uint>> PrimarySystemMonitorPowerChanged
		{
			add => Instance.AddEvent(GUID_MONITOR_POWER_ON, value);
			remove => Instance.RemoveEvent(GUID_MONITOR_POWER_ON, value);
		}

		internal static event CancelEventHandler QueryStandby;

		internal static event CancelEventHandler QuerySuspend;

		internal static event EventHandler ResumedAfterFailure;

		public static event EventHandler ResumedAfterSleep;

		internal static event EventHandler ResumedAfterStandby;

		public static event EventHandler ResumedAfterSuspend;

		/// <summary>
		/// <para>Occurs when the display associated with the application's session has been powered on or off.</para>
		/// <para>
		/// <strong>Windows 7, Windows Server 2008 R2, Windows Vista and Windows Server 2008</strong>: This notification is available
		/// starting with Windows 8 and Windows Server 2012.
		/// </para>
		/// <para>
		/// This notification is sent only to user-mode applications.Services and other programs running in session 0 do not receive this notification.
		/// </para>
		/// <para>The Data member is a DWORD with one of the following values.</para>
		/// <list type="bullet">
		/// <item>0x0 - The display is off.</item>
		/// <item>0x1 - The display is on.</item>
		/// <item>0x2 - The display is dimmed.</item>
		/// </list>
		/// </summary>
		public static event EventHandler<PowerEventArgs<uint>> SessionDisplayStatusChanged
		{
			add => Instance.AddEvent(GUID_SESSION_DISPLAY_STATUS, value);
			remove => Instance.RemoveEvent(GUID_SESSION_DISPLAY_STATUS, value);
		}

		public static event EventHandler<PowerEventArgs<uint>> SessionUserStatusChanged
		{
			add => Instance.AddEvent(GUID_SESSION_USER_PRESENCE, value);
			remove => Instance.RemoveEvent(GUID_SESSION_USER_PRESENCE, value);
		}

		internal static event EventHandler StandbyFailed;

		internal static event EventHandler StandingBy;

		internal static event EventHandler SuspendFailed;

		public static event EventHandler Suspending;

		public static event EventHandler<PowerEventArgs<object>> SystemIsBusy
		{
			add => Instance.AddEvent(GUID_IDLE_BACKGROUND_TASK, value);
			remove => Instance.RemoveEvent(GUID_IDLE_BACKGROUND_TASK, value);
		}

		public static event EventHandler<PowerEventArgs<uint>> SystemPowerSourceChanged
		{
			add => Instance.AddEvent(GUID_ACDC_POWER_SOURCE, value);
			remove => Instance.RemoveEvent(GUID_ACDC_POWER_SOURCE, value);
		}

		/// <summary>Gets the device's battery status.</summary>
		/// <value>Returns a <see cref="BatteryStatus"/> value.</value>
		public static BatteryStatus BatteryStatus => GetStatus().BatteryFlag switch
		{
			BATTERY_STATUS.BATTERY_CHARGING => BatteryStatus.Charging,
			BATTERY_STATUS.BATTERY_NONE => BatteryStatus.NotPresent,
			_ => GetStatus().ACLineStatus == AC_STATUS.AC_OFFLINE ? BatteryStatus.Discharging : BatteryStatus.Idle,
		};

		/// <summary>Gets flags indicating the system's power capabilities.</summary>
		/// <value>Returns a <see cref="PowerCapabilities"/> value.</value>
		public static PowerCapabilities DeviceCapabilities
		{
			get
			{
				var c = GetCapabilities();
				PowerCapabilities ret = 0;
				if (c.PowerButtonPresent) ret |= PowerCapabilities.PowerButtonPresent;
				if (c.SleepButtonPresent) ret |= PowerCapabilities.SleepButtonPresent;
				if (c.LidPresent) ret |= PowerCapabilities.LidPresent;
				if (c.SystemS1) ret |= PowerCapabilities.SystemS1;
				if (c.SystemS2) ret |= PowerCapabilities.SystemS2;
				if (c.SystemS3) ret |= PowerCapabilities.SystemS3;
				if (c.SystemS4) ret |= PowerCapabilities.SystemS4;
				if (c.SystemS5) ret |= PowerCapabilities.SystemS5;
				if (c.HiberFilePresent) ret |= PowerCapabilities.HiberFilePresent;
				if (c.FullWake) ret |= PowerCapabilities.FullWake;
				if (c.VideoDimPresent) ret |= PowerCapabilities.VideoDimPresent;
				if (c.ApmPresent) ret |= PowerCapabilities.ApmPresent;
				if (c.UpsPresent) ret |= PowerCapabilities.UpsPresent;
				if (c.ThermalControl) ret |= PowerCapabilities.ThermalControl;
				if (c.ProcessorThrottle) ret |= PowerCapabilities.ProcessorThrottle;
				if (c.FastSystemS4) ret |= PowerCapabilities.FastSystemS4;
				if (c.Hiberboot) ret |= PowerCapabilities.Hiberboot;
				if (c.WakeAlarmPresent) ret |= PowerCapabilities.WakeAlarmPresent;
				if (c.AoAc) ret |= PowerCapabilities.AoAc;
				if (c.DiskSpinDown) ret |= PowerCapabilities.DiskSpinDown;
				if (c.AoAcConnectivitySupported) ret |= PowerCapabilities.AoAcConnectivitySupported;
				if (c.SystemBatteriesPresent) ret |= PowerCapabilities.SystemBatteriesPresent;
				if (c.BatteriesAreShortTerm) ret |= PowerCapabilities.BatteriesAreShortTerm;
				return ret;
			}
		}

		/// <summary>Gets the device's battery saver status, indicating when to save energy.</summary>
		/// <value>Returns a <see cref="EnergySaverStatus"/> value.</value>
		public static EnergySaverStatus EnergySaverStatus
		{
			get
			{
				var s = GetStatus();
				return s.ACLineStatus == AC_STATUS.AC_ONLINE || s.BatteryFlag == BATTERY_STATUS.BATTERY_NONE ? EnergySaverStatus.Disabled : (s.SystemStatusFlag == 0 ? EnergySaverStatus.Off : EnergySaverStatus.On);
			}
		}

		/// <summary>Gets a value indicating whether the system is on AC power (plugged in).</summary>
		/// <value><see langword="true"/> if on AC power; otherwise, <see langword="false"/>.</value>
		public static bool OnACPower => GetStatus().ACLineStatus == AC_STATUS.AC_ONLINE;

		/// <summary>Indicates the OEM's preferred power management profile for this device.</summary>
		public static POWER_PLATFORM_ROLE PlatformRole => PInvokeClient.Windows8.IsPlatformSupported() ? PowerDeterminePlatformRoleEx(PowerPlatformRoleVersion.POWER_PLATFORM_ROLE_V2) : PowerDeterminePlatformRole();

		/// <summary>A collection of the powered devices on the system. Filters on this list can be set as properties.</summary>
		/// <value>Returns a <see cref="PoweredDeviceCollection"/> value.</value>
		public static PoweredDeviceCollection PoweredDevices { get; } = new PoweredDeviceCollection();

		/// <summary>Gets the device's power supply status.</summary>
		/// <value>Returns a <see cref="PowerSupplyStatus"/> value.</value>
		public static PowerSupplyStatus PowerSupplyStatus => GetStatus().ACLineStatus switch
		{
			AC_STATUS.AC_ONLINE => PowerSupplyStatus.Adequate,
			AC_STATUS.AC_LINE_BACKUP_POWER => PowerSupplyStatus.Inadequate,
			_ => PowerSupplyStatus.NotPresent,
		};

		/// <summary>Gets the total percentage of charge remaining from all batteries connected to the device.</summary>
		/// <value>Returns a <c>int?</c> value from 0 to 100, or <see langword="null"/> if the status is unknown.</value>
		public static int? RemainingChargePercent { get { var p = GetStatus().BatteryLifePercent; return p == 255 ? (int?)null : p; } }

		/// <summary>Gets the total runtime remaining from all batteries connected to the device.</summary>
		/// <value>
		/// The total runtime remaining from all batteries connected to the device, or <see langword="null"/> if the status is unknown or
		/// connected to AC power.
		/// </value>
		public static TimeSpan? RemainingDischargeTime { get { var s = GetStatus().BatteryLifeTime; return s == uint.MaxValue ? (TimeSpan?)null : TimeSpan.FromSeconds(s); } }

		/// <summary>Gets the collection of all power schemes on this device.</summary>
		/// <value>Returns a <see cref="PowerSchemeCollection"/> value.</value>
		public static PowerSchemeCollection Schemes { get; } = new PowerSchemeCollection();

		internal static string NameForGuid(Guid guid) => typeof(PowrProf).GetFields(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(i => i.FieldType == typeof(Guid) && guid.Equals(i.GetValue(null)))?.Name;

		private static SYSTEM_POWER_CAPABILITIES GetCapabilities() => GetPwrCapabilities(out var c) ? c : default;

		private static SYSTEM_POWER_STATUS GetStatus() => GetSystemPowerStatus(out var s) ? s : default;

		private class Singleton : IDisposable
		{
			private readonly Dictionary<Guid, Delegate> eventHandles = new Dictionary<Guid, Delegate>();
			private readonly BasicMessageWindow msgWindow;
			private readonly Dictionary<Guid, SafeHPOWERSETTINGNOTIFY> regs = new Dictionary<Guid, SafeHPOWERSETTINGNOTIFY>();

			internal Singleton() => msgWindow = new BasicMessageWindow(ListenerWndProc);

			public void AddEvent<T>(Guid guid, EventHandler<PowerEventArgs<T>> value)
			{
				lock (regs)
				{
					if (!eventHandles.TryGetValue(guid, out var h))
					{
						eventHandles.Add(guid, value);
						regs.Add(guid, RegisterNotification(guid));
					}
					else
						eventHandles[guid] = Delegate.Combine(h, value);
				}
			}

			public void InvokeEvent(Guid guid)
			{
				if (!eventHandles.TryGetValue(guid, out var h))
					throw new InvalidOperationException($"Event for {guid} is not registered.");
				h.DynamicInvoke(null, new PowerEventArgs<object>(null));
			}

			public void InvokeEvent<T>(Guid guid, in POWERBROADCAST_SETTING pbs)
			{
				if (!eventHandles.TryGetValue(guid, out var h))
					throw new InvalidOperationException($"Event for {guid} is not registered.");
				object value = typeof(T) switch
				{
					var t when t == typeof(uint) => BitConverter.ToUInt32(pbs.Data, 0),
					var t when t == typeof(Guid) => new Guid(pbs.Data),
					_ => throw new InvalidOperationException("Unrecognized invoke type.")
				};
				h.DynamicInvoke(null, new PowerEventArgs<T>((T)value));
			}

			public void RemoveEvent<T>(Guid guid, EventHandler<PowerEventArgs<T>> value)
			{
				lock (regs)
				{
					if (eventHandles.TryGetValue(guid, out var h))
					{
						h = Delegate.Remove(h, value);
						if (h is null || h.GetInvocationList().Length == 0)
						{
							eventHandles.Remove(guid);
							Unreg();
						}
						else
							eventHandles[guid] = h;
					}
					else
						Unreg();
				}

				void Unreg()
				{
					if (regs.TryGetValue(guid, out var reg))
					{
						reg.Dispose();
						regs.Remove(guid);
					}
				}
			}

			void IDisposable.Dispose()
			{
				msgWindow.Dispose();
				lock (regs)
				{
					foreach (var reg in regs.Values)
						reg.Dispose();
					regs.Clear();
				}
			}

			private bool ListenerWndProc(HWND hwnd, uint msg, IntPtr wParam, IntPtr lParam, out IntPtr lReturn)
			{
				System.Diagnostics.Debug.WriteLine($"Message={msg}");
				lReturn = default;
				switch (msg)
				{
					case (uint)WindowMessage.WM_POWERBROADCAST:
						var pbt = (PowerBroadcastType)wParam.ToInt32();
						System.Diagnostics.Debug.WriteLine($"WM_POWERBROADCAST : {pbt}");
						switch (pbt)
						{
							case PowerBroadcastType.PBT_APMQUERYSUSPEND:
								var qscancel = new CancelEventArgs();
								PowerManager.QuerySuspend?.Invoke(null, qscancel);
								lReturn = qscancel.Cancel ? BROADCAST_QUERY_DENY : new IntPtr(-1);
								return true;

							case PowerBroadcastType.PBT_APMQUERYSTANDBY:
								var qsbcancel = new CancelEventArgs();
								PowerManager.QueryStandby?.Invoke(null, qsbcancel);
								lReturn = qsbcancel.Cancel ? BROADCAST_QUERY_DENY : new IntPtr(-1);
								return true;

							case PowerBroadcastType.PBT_APMQUERYSUSPENDFAILED:
								PowerManager.SuspendFailed?.Invoke(null, EventArgs.Empty);
								break;

							case PowerBroadcastType.PBT_APMQUERYSTANDBYFAILED:
								PowerManager.StandbyFailed?.Invoke(null, EventArgs.Empty);
								break;

							case PowerBroadcastType.PBT_APMSUSPEND:
								PowerManager.Suspending?.Invoke(null, EventArgs.Empty);
								break;

							case PowerBroadcastType.PBT_APMSTANDBY:
								PowerManager.StandingBy?.Invoke(null, EventArgs.Empty);
								break;

							case PowerBroadcastType.PBT_APMRESUMECRITICAL:
								PowerManager.ResumedAfterFailure?.Invoke(null, EventArgs.Empty);
								break;

							case PowerBroadcastType.PBT_APMRESUMESUSPEND:
								PowerManager.ResumedAfterSuspend?.Invoke(null, EventArgs.Empty);
								break;

							case PowerBroadcastType.PBT_APMRESUMESTANDBY:
								PowerManager.ResumedAfterStandby?.Invoke(null, EventArgs.Empty);
								break;

							case PowerBroadcastType.PBT_APMBATTERYLOW:
								PowerManager.LowBattery?.Invoke(null, EventArgs.Empty);
								break;

							case PowerBroadcastType.PBT_APMPOWERSTATUSCHANGE:
								PowerManager.PowerStatusChanged?.Invoke(null, EventArgs.Empty);
								break;

							case PowerBroadcastType.PBT_APMOEMEVENT:
								break;

							case PowerBroadcastType.PBT_APMRESUMEAUTOMATIC:
								PowerManager.ResumedAfterSleep?.Invoke(null, EventArgs.Empty);
								break;

							case PowerBroadcastType.PBT_POWERSETTINGCHANGE:
								var setting = lParam.ToStructure<POWERBROADCAST_SETTING>();
								switch (setting.PowerSetting)
								{
									case var g when g == GUID_IDLE_BACKGROUND_TASK:
										InvokeEvent(g);
										break;

									case var g when g == GUID_POWERSCHEME_PERSONALITY:
										InvokeEvent<Guid>(g, setting);
										break;

									//case var g when g == GUID_CONSOLE_DISPLAY_STATE:
									//case var g when g == GUID_ACDC_POWER_SOURCE:
									//case var g when g == GUID_BATTERY_PERCENTAGE_REMAINING:
									//case var g when g == GUID_GLOBAL_USER_PRESENCE:
									//case var g when g == GUID_MONITOR_POWER_ON:
									//case var g when g == GUID_POWER_SAVING_STATUS:
									//case var g when g == GUID_SESSION_DISPLAY_STATUS:
									//case var g when g == GUID_SESSION_USER_PRESENCE:
									//case var g when g == GUID_SYSTEM_AWAYMODE:
									default:
										InvokeEvent<uint>(setting.PowerSetting, setting);
										break;
								}
								break;

							default:
								break;
						}
						break;
				}
				return false;
			}

			private SafeHPOWERSETTINGNOTIFY RegisterNotification(Guid guid) =>
				Win32Error.ThrowLastErrorIfInvalid(RegisterPowerSettingNotification((IntPtr)msgWindow.Handle, guid, User32.DEVICE_NOTIFY.DEVICE_NOTIFY_WINDOW_HANDLE));
		}
	}

	/// <summary>Represents a device on the system that has power requirements.</summary>
	public class PoweredDevice
	{
		private readonly bool hw;

		internal PoweredDevice(string id, bool isHW)
		{
			Id = id;
			hw = isHW;
		}

		/// <summary>Gets or sets the identifier for the device.</summary>
		/// <value>The identifier.</value>
		public string Id { get; protected set; }

		/// <summary>Gets or sets a value indicating whether the device is wake enabled.</summary>
		/// <value><see langword="true"/> if wake enabled; otherwise, <see langword="false"/>.</value>
		public bool WakeEnabled
		{
			get => PoweredDeviceCollection.GetEnumValues((hw ? PDQUERY.DEVICEPOWER_HARDWAREID : 0) | PDQUERY.DEVICEPOWER_FILTER_WAKEENABLED).Any(s => s == Id);
			set => DevicePowerSetDeviceState(Id, value ? PDSET.DEVICEPOWER_SET_WAKEENABLED : PDSET.DEVICEPOWER_CLEAR_WAKEENABLED);
		}

		/// <inheritdoc/>
		public override string ToString() => Id;
	}

	/// <summary>Retrieves the list, optionally filtered, of the powered devices on the system.</summary>
	/// <seealso cref="VirtualDictionary{TKey, TValue}"/>
	public class PoweredDeviceCollection : VirtualDictionary<string, PoweredDevice>
	{
		private static Finalizer finalizer;

		internal PoweredDeviceCollection() : base(true)
		{
		}

		/// <inheritdoc/>
		public override ICollection<string> Keys => GetEnumValues(PDQUERY).ToList();

		/// <summary>Gets or sets a value indicating whether to display only devices that are currently present in the system.</summary>
		/// <value><see langword="true"/> to retrieve only present devices; otherwise, <see langword="false"/>.</value>
		public bool OnlyPresentDevices { get; set; }

		/// <summary>Gets or sets a value indicating whether to display only devices that are wake-enabled.</summary>
		/// <value><see langword="true"/> if to retrieve only wake enabled devices; otherwise, <see langword="false"/>.</value>
		public bool OnlyWakeEnabledDevices { get; set; }

		/// <summary>Gets or sets a value indicating whether to display the hardware identifier rather than a friendly name.</summary>
		/// <value>
		/// <see langword="true"/> to display the hardware identifier; otherwise, <see langword="false"/> to display the friendly name.
		/// </value>
		public bool UseHardwareId { get; set; }

		/// <inheritdoc/>
		public override ICollection<PoweredDevice> Values => GetEnumValues(PDQUERY).Select(s => new PoweredDevice(s, UseHardwareId)).ToList();

		/// <inheritdoc/>
		protected override IEnumerable<KeyValuePair<string, PoweredDevice>> Items => GetEnumValues(PDQUERY).Select(s => new KeyValuePair<string, PoweredDevice>(s, new PoweredDevice(s, UseHardwareId)));

		private PDQUERY PDQUERY => (UseHardwareId ? PDQUERY.DEVICEPOWER_HARDWAREID : 0) | (OnlyPresentDevices ? PDQUERY.DEVICEPOWER_FILTER_DEVICES_PRESENT : 0) | (OnlyWakeEnabledDevices ? PDQUERY.DEVICEPOWER_FILTER_WAKEENABLED : 0);

		/// <inheritdoc/>
		public override bool TryGetValue(string key, out PoweredDevice value)
		{
			var ret = GetEnumValues(UseHardwareId ? PDQUERY.DEVICEPOWER_HARDWAREID : 0).Contains(key);
			value = ret ? new PoweredDevice(key, UseHardwareId) : null;
			return ret;
		}

		internal static IEnumerable<string> GetEnumValues(PDQUERY filter = 0, PDCAP flags = 0)
		{
			const uint bufSz = 1024;

			ValidateOpened();
			var sz = bufSz;
			var sb = new StringBuilder((int)sz / 2, (int)sz / 2);
			for (var i = 0U; ; i++)
			{
				sb.Clear();
				if (DevicePowerEnumDevices(i, filter, flags, sb, ref sz))
					yield return sb.ToString();
				else
					break;
			}
		}

		private static void ValidateOpened()
		{
			if (finalizer is null) finalizer = new Finalizer();
		}

		private class Finalizer
		{
			internal Finalizer() => DevicePowerOpen();

			~Finalizer() => DevicePowerClose();
		}
	}

	/// <summary>Event arguments for power events.</summary>
	/// <typeparam name="T">The data type.</typeparam>
	/// <seealso cref="System.EventArgs"/>
	public class PowerEventArgs<T> : EventArgs
	{
		/// <summary>Initializes a new instance of the <see cref="PowerEventArgs{T}"/> class.</summary>
		/// <param name="value">The data value.</param>
		public PowerEventArgs(T value)
		{
			Data = value;
		}

		/// <summary>Gets the data associated with the power event.</summary>
		/// <value>The data.</value>
		public T Data { get; }
	}

	/// <summary>Represents a system power scheme (power plan).</summary>
	public class PowerScheme : IEquatable<Guid>, IEquatable<PowerScheme>
	{
		/// <summary>The well-known, system defined Balance (or Typical) power scheme with fairly aggressive power savings measures.</summary>
		public static readonly PowerScheme Balanced = new PowerScheme(GUID_TYPICAL_POWER_SAVINGS);

		/// <summary>The well-known, system defined High Performance (or Minimum) power scheme with almost no power savings measures.</summary>
		public static readonly PowerScheme HighPerformance = new PowerScheme(GUID_MIN_POWER_SAVINGS);

		/// <summary>
		/// The well-known, system defined Power Saver (or Max) power scheme with very aggressive power savings measures to help stretch
		/// battery life.
		/// </summary>
		public static readonly PowerScheme PowerSaver = new PowerScheme(GUID_MAX_POWER_SAVINGS);

		internal PowerScheme(Guid g)
		{
			Guid = g;
			Groups = new PowerSchemeGroupCollection(g);
		}

		/// <summary>Gets the active power scheme for the system.</summary>
		/// <value>Returns a <see cref="PowerScheme"/> value for the active scheme.</value>
		public static PowerScheme Active => PowerGetActiveScheme(out var g).Succeeded ? new PowerScheme(g) : throw new SystemException();

		/// <summary>Gets the variable name of the GUID defined in the Windows SDK for this scheme.</summary>
		/// <value>The API based variable name.</value>
		public string ApiName => PowerManager.NameForGuid(Guid);

		/// <summary>Gets or sets the description of the scheme.</summary>
		/// <value>The description.</value>
		public string Description
		{
			get => PowerReadDescription(Guid);
			set => PowerWriteDescription(Guid, null, null, value).ThrowIfFailed();
		}

		/// <summary>Gets the subgroups defined for this scheme.</summary>
		/// <value>Returns a <see cref="PowerSchemeGroupCollection"/> value.</value>
		public PowerSchemeGroupCollection Groups { get; }

		/// <summary>Gets or sets a value indicating whether this scheme is the active scheme for the system.</summary>
		/// <value><see langword="true"/> if this instance is active; otherwise, <see langword="false"/>.</value>
		public bool IsActive
		{
			get => PowerGetActiveScheme(out var g).Succeeded && Guid.Equals(g);
			set => PowerSetActiveScheme(default, Guid).ThrowIfFailed();
		}

		/// <summary>Gets or sets the friendly name of the scheme.</summary>
		/// <value>The friendly name.</value>
		public string Name
		{
			get => PowerReadFriendlyName(Guid);
			set => PowerWriteFriendlyName(Guid, null, null, value).ThrowIfFailed();
		}

		/// <summary>Gets the underlying GUID for the scheme.</summary>
		/// <value>Returns a <see cref="Guid"/> value.</value>
		protected internal Guid Guid { get; }

		/// <summary>Returns a duplicates this scheme that can be edited and set as the new active scheme.</summary>
		/// <returns>An editable duplicate of the current scheme.</returns>
		public PowerScheme Duplicate()
		{
			PowerDuplicateScheme(default, Guid, out var h).ThrowIfFailed();
			return new PowerScheme(h.ToStructure<Guid>());
		}

		/// <summary>Determines whether the specified <see cref="Guid"/>, is equal to this instance.</summary>
		/// <param name="other">The <see cref="Guid"/> to compare with this instance.</param>
		/// <returns><see langword="true"/> if the specified <see cref="Guid"/> is equal to this instance; otherwise, <see langword="false"/>.</returns>
		public bool Equals(Guid other) => Guid.Equals(other);

		/// <summary>Determines whether the specified <see cref="PowerScheme"/>, is equal to this instance.</summary>
		/// <param name="other">The <see cref="PowerScheme"/> to compare with this instance.</param>
		/// <returns><see langword="true"/> if the specified <see cref="PowerScheme"/> is equal to this instance; otherwise, <see langword="false"/>.</returns>
		public bool Equals(PowerScheme other) => !(other is null) && Equals(other.Guid);

		/// <summary>Determines whether the specified <see cref="object"/>, is equal to this instance.</summary>
		/// <param name="obj">The <see cref="object"/> to compare with this instance.</param>
		/// <returns><see langword="true"/> if the specified <see cref="object"/> is equal to this instance; otherwise, <see langword="false"/>.</returns>
		public override bool Equals(object obj) => obj is Guid g ? Equals(g) : obj is PowerScheme s && Equals(s);

		/// <summary>Returns a hash code for this instance.</summary>
		/// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
		public override int GetHashCode() => Guid.GetHashCode();

		/// <inheritdoc/>
		public override string ToString() => Name + (IsActive ? " (Active)" : "");
	}

	/// <summary>Represents a collection of all the power schemes available on the system.</summary>
	/// <seealso cref="VirtualDictionary{Guid, PowerScheme}"/>
	public class PowerSchemeCollection : VirtualDictionary<Guid, PowerScheme>
	{
		internal PowerSchemeCollection() : base(!CanUserWritePwrScheme())
		{
		}

		/// <inheritdoc/>
		public override ICollection<Guid> Keys => PowerEnumerate<Guid>(null, null).ToList();

		/// <summary>Imports a power scheme from a file.</summary>
		/// <param name="importFilePath">The path to a power scheme backup file created by <c>PowerCfg.Exe /Export</c>.</param>
		/// <returns>A <see cref="PowerScheme"/> derived from the file.</returns>
		public static PowerScheme ImportFromFile(string importFilePath)
		{
			PowerImportPowerScheme(default, importFilePath, out var memG).ThrowIfFailed();
			return new PowerScheme(memG.ToStructure<Guid>());
		}

		/// <summary>
		/// Replaces the default power schemes with the current user's power schemes. This allows an administrator to change the default
		/// power schemes for the system. Replacing the default schemes enables users to use the <c>Restore Defaults</c> option in the
		/// Control Panel <c>Power Options</c> application to restore customized power scheme defaults instead of the original Windows power
		/// scheme defaults.
		/// </summary>
		/// <remarks>The caller must be a member of the local Administrators group.</remarks>
		public static void ReplaceDefaultPowerSchemes() => PowerReplaceDefaultPowerSchemes().ThrowIfFailed();

		/// <summary>
		/// Replaces the power schemes for the system with default power schemes. All current power schemes and settings are deleted and
		/// replaced with the default system power schemes.
		/// </summary>
		/// <remarks>The caller must be a member of the local Administrators group.</remarks>
		public static void RestoreDefaultPowerSchemes() => PowerRestoreDefaultPowerSchemes().ThrowIfFailed();

		/// <inheritdoc/>
		public override bool Remove(Guid key) => PowerDeleteScheme(default, key).Succeeded;

		/// <inheritdoc/>
		public override bool TryGetValue(Guid key, out PowerScheme value)
		{
			value = new PowerScheme(key); return true;
		}

		// public static POWER_POLICY ActivePowerPolicy => GetCurrentPowerPolicies(out var _, out var p) ? p : default; public static
		// GLOBAL_POWER_POLICY GlobalPowerPolicy => GetCurrentPowerPolicies(out var p, out var _) ? p : default;
	}

	/// <summary>Represents a subgroup of a system power scheme (power plan).</summary>
	public class PowerSchemeGroup
	{
		/// <summary>The scheme's guid.</summary>
		protected Guid scheme;

		/// <summary>The scheme's subgroup's guid.</summary>
		protected Guid subgroup;

		internal PowerSchemeGroup(Guid scheme, Guid group)
		{
			this.scheme = scheme;
			subgroup = group;
			Settings = new PowerSchemeSettingCollection(scheme, subgroup);
		}

		/// <summary>Gets the variable name of the GUID defined in the Windows SDK for this subgroup.</summary>
		/// <value>The API based variable name.</value>
		public string ApiName => PowerManager.NameForGuid(subgroup);

		/// <summary>Gets or sets the description of the subgroup.</summary>
		/// <value>The description.</value>
		public string Description
		{
			get => PowerReadDescription(scheme, subgroup);
			set => PowerWriteDescription(scheme, subgroup, null, value).ThrowIfFailed();
		}

		/// <summary>Gets or sets the friendly name of the subgroup.</summary>
		/// <value>The friendly name.</value>
		public string Name
		{
			get => PowerReadFriendlyName(scheme, subgroup);
			set => PowerWriteFriendlyName(scheme, subgroup, null, value).ThrowIfFailed();
		}

		/// <summary>Gets the settings defined for this subgroup.</summary>
		/// <value>Returns a <see cref="PowerSchemeSettingCollection"/> value.</value>
		public PowerSchemeSettingCollection Settings { get; }

		/// <inheritdoc/>
		public override string ToString() => Name;
	}

	/// <summary>Represents a collection of all the subgroups available under a power scheme on the system.</summary>
	public class PowerSchemeGroupCollection : VirtualDictionary<Guid, PowerSchemeGroup>
	{
		/// <summary>The scheme's guid.</summary>
		protected Guid scheme;

		internal PowerSchemeGroupCollection(Guid scheme) : base(false) => this.scheme = scheme;

		/// <inheritdoc/>
		public override ICollection<Guid> Keys => PowerEnumerate<Guid>(scheme, null).Concat(new[] { NO_SUBGROUP_GUID }).ToList();

		/// <inheritdoc/>
		public override bool TryGetValue(Guid key, out PowerSchemeGroup value)
		{
			value = new PowerSchemeGroup(scheme, key); return true;
		}
	}

	/// <summary>Represents a setting on a subgroup.</summary>
	public class PowerSchemeSetting
	{
		/// <summary>The scheme's guid.</summary>
		protected Guid scheme;

		/// <summary>The scheme's setting guid.</summary>
		protected Guid setting;

		/// <summary>The scheme's subgroup guid.</summary>
		protected Guid subgroup;

		internal PowerSchemeSetting(Guid scheme, Guid group, Guid setting)
		{
			this.scheme = scheme;
			subgroup = group;
			this.setting = setting;
		}

		private delegate Win32Error IdxWriter(HKEY rootSystemPowerKey, in Guid schemePersonalityGuid, in Guid subGroupOfPowerSettingsGuid, in Guid powerSettingGuid, uint index);

		/// <summary>Retrieves the AC power value for the specified power setting.</summary>
		/// <value>Returns the data value.</value>
		public object ACValue
		{
			get
			{
				var sz = 0U;
				PowerReadACValue(default, scheme, subgroup, setting, out _, IntPtr.Zero, ref sz).ThrowIfFailed();
				using var mem = new SafeHGlobalHandle((int)sz);
				PowerReadACValue(default, scheme, subgroup, setting, out var regType, mem, ref sz).ThrowIfFailed();
				return regType.GetValue(mem, sz);
			}
		}

		/// <summary>Gets or sets the default AC index of the specified power setting.</summary>
		/// <value>The default AC index.</value>
		public int ACValueDefaultIndex
		{
			get => PowerReadACDefaultIndex(default, scheme, subgroup, setting, out var i).Succeeded ? (int)i : -1;
			set => WriteIndex(PowerWriteACDefaultIndex, value);
		}

		/// <summary>Gets or sets the AC index of the specified power setting.</summary>
		/// <value>The AC index.</value>
		public int ACValueIndex
		{
			get => PowerReadACValueIndex(default, scheme, subgroup, setting, out var i).Succeeded ? (int)i : -1;
			set => WriteIndex(PowerWriteACValueIndex, value);
		}

		/// <summary>Gets the variable name of the GUID defined in the Windows SDK for this setting.</summary>
		/// <value>The API based variable name.</value>
		public string ApiName => PowerManager.NameForGuid(setting);

		/// <summary>Retrieves the DC power value for the specified power setting.</summary>
		/// <value>Returns the data value.</value>
		public object DCValue
		{
			get
			{
				var sz = 0U;
				PowerReadDCValue(default, scheme, subgroup, setting, out _, IntPtr.Zero, ref sz).ThrowIfFailed();
				using var mem = new SafeHGlobalHandle((int)sz);
				PowerReadDCValue(default, scheme, subgroup, setting, out var regType, mem, ref sz).ThrowIfFailed();
				return regType.GetValue(mem, sz);
			}
		}

		/// <summary>Gets or sets the default DC index of the specified power setting.</summary>
		/// <value>The default DC index.</value>
		public int DCValueDefaultIndex
		{
			get => PowerReadDCDefaultIndex(default, scheme, subgroup, setting, out var i).Succeeded ? (int)i : -1;
			set => WriteIndex(PowerWriteDCDefaultIndex, value);
		}

		/// <summary>Gets or sets the DC index of the specified power setting.</summary>
		/// <value>The DC index.</value>
		public int DCValueIndex
		{
			get => PowerReadDCValueIndex(default, scheme, subgroup, setting, out var i).Succeeded ? (int)i : -1;
			set => WriteIndex(PowerWriteDCValueIndex, value);
		}

		/// <summary>Gets or sets the description of the setting.</summary>
		/// <value>The description.</value>
		public string Description
		{
			get => PowerReadDescription(scheme, subgroup, setting);
			set { PowerWriteDescription(scheme, subgroup, setting, value).ThrowIfFailed(); PowerSetActiveScheme(default, scheme).ThrowIfFailed(); }
		}

		/// <summary>Gets a value indicating whether a range is defined.</summary>
		/// <value><see langword="true"/> if a range is defined; otherwise, <see langword="false"/>.</value>
		public bool IsRange => PowerIsSettingRangeDefined(subgroup, setting);

		/// <summary>Gets or sets the friendly name of the setting.</summary>
		/// <value>The friendly name.</value>
		public string Name
		{
			get => PowerReadFriendlyName(scheme, subgroup, setting);
			set { PowerWriteFriendlyName(scheme, subgroup, setting, value).ThrowIfFailed(); PowerSetActiveScheme(default, scheme).ThrowIfFailed(); }
		}

		/// <summary>Gets the possible values for this setting.</summary>
		/// <value>Returns a <see cref="IEnumerable{T}"/> value.</value>
		public IEnumerable<(object value, string name, string description)> PossibleValues
		{
			get
			{
				if (IsRange) yield break;
				for (var i = 0U; ; i++)
				{
					var err = FunctionHelper.CallMethodWithStrBuf((StringBuilder sb, ref uint ssz) => PowerReadPossibleFriendlyName(default, subgroup, setting, i, sb, ref ssz), out var name, (ssz, r) => ssz > 0);
					if (err.Failed)
						break;
					err = FunctionHelper.CallMethodWithStrBuf((StringBuilder sb, ref uint ssz) => PowerReadPossibleDescription(default, subgroup, setting, i, sb, ref ssz), out var desc, (ssz, r) => ssz > 0);
					if (err.Failed)
						break;
					var sz = 0U;
					object obj = null;
					err = PowerReadPossibleValue(default, subgroup, setting, out var regType, i, IntPtr.Zero, ref sz);
					if (err.Succeeded)
					{
						using var mem = new SafeHGlobalHandle((int)sz);
						err = PowerReadPossibleValue(default, subgroup, setting, out regType, i, mem, ref sz);
						if (err.Failed)
							break;
						obj = regType.GetValue(mem, sz);
					}
					yield return (obj, name, desc);
				}
			}
		}

		/// <summary>Gets or sets the range information for this setting.</summary>
		/// <value>The range detail (min/max value, increment and unit specifier.</value>
		/// <exception cref="InvalidOperationException">Setting is not a range value.</exception>
		public (uint min, uint max, uint increment, string unitsSpecifier) Range
		{
			get
			{
				try
				{
					if (IsRange)
					{
						PowerReadValueMin(default, subgroup, setting, out var min).ThrowIfFailed();
						PowerReadValueMax(default, subgroup, setting, out var max).ThrowIfFailed();
						PowerReadValueIncrement(default, subgroup, setting, out var incr).ThrowIfFailed();
						FunctionHelper.CallMethodWithStrBuf((StringBuilder sb, ref uint sz) => PowerReadValueUnitsSpecifier(default, subgroup, setting, sb, ref sz), out var spec, (sz, r) => sz > 0).ThrowIfFailed();
						return (min, max, incr, spec);
					}
				}
				catch { }
				return (0, 0, 0, null);
			}
			set
			{
				if (!IsRange) throw new InvalidOperationException("Setting is not a range value.");
				PowerWriteValueMin(default, subgroup, setting, value.min).ThrowIfFailed();
				PowerWriteValueMax(default, subgroup, setting, value.max).ThrowIfFailed();
				PowerWriteValueIncrement(default, subgroup, setting, value.increment).ThrowIfFailed();
				PowerWriteValueUnitsSpecifier(default, subgroup, setting, value.unitsSpecifier, (uint)((value.unitsSpecifier?.Length + 1) * 2 ?? 0)).ThrowIfFailed();
				PowerSetActiveScheme(default, scheme).ThrowIfFailed();
			}
		}

		private void WriteIndex(IdxWriter func, int value)
		{
			func(default, scheme, subgroup, setting, unchecked((uint)value)).ThrowIfFailed();
			PowerSetActiveScheme(default, scheme).ThrowIfFailed();
		}
	}

	/// <summary>Represents a collection of all settings for a subgroup and power scheme on the system.</summary>
	public class PowerSchemeSettingCollection : VirtualDictionary<Guid, PowerSchemeSetting>
	{
		/// <summary>The scheme's guid.</summary>
		protected Guid scheme;

		/// <summary>The scheme's subgroup guid.</summary>
		protected Guid subgroup;

		internal PowerSchemeSettingCollection(Guid scheme, Guid subgroup) : base(false)
		{
			this.scheme = scheme; this.subgroup = subgroup;
		}

		/// <inheritdoc/>
		public override ICollection<Guid> Keys => PowerEnumerate<Guid>(scheme, subgroup).ToList();

		/// <inheritdoc/>
		public override bool TryGetValue(Guid key, out PowerSchemeSetting value)
		{
			value = new PowerSchemeSetting(scheme, subgroup, key); return true;
		}
	}
}