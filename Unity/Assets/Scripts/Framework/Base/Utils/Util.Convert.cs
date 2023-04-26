//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace Framework
{
	partial class Util
	{
		/// <summary>
		///     类型转换相关的实用函数。
		/// </summary>
		public static class Convert
		{
			private const char SPLIT_COMMA = ','; // 逗号分隔符

			#region Screen
			private const float INCHES_TO_CENTIMETERS = 2.54f; // 1 inch = 2.54 cm
			private const float CENTIMETERS_TO_INCHES = 1f / INCHES_TO_CENTIMETERS; // 1 cm = 0.3937 inches

			/// 获取数据在此计算机结构中存储时的字节顺序。
			public static bool IsLittleEndian => BitConverter.IsLittleEndian;

			/// 获取或设置屏幕每英寸点数。
			public static float ScreenDpi { get; set; }

			/// 将像素转换为厘米。
			public static float GetCentimetersFromPixels(float pixels) {
				Debug.Assert(ScreenDpi > 0, "You must set screen DPI first.");
				return INCHES_TO_CENTIMETERS * pixels / ScreenDpi;
			}

			/// 将厘米转换为像素。
			public static float GetPixelsFromCentimeters(float centimeters) {
				Debug.Assert(ScreenDpi > 0, "You must set screen DPI first.");
				return CENTIMETERS_TO_INCHES * centimeters * ScreenDpi;
			}

			/// 将像素转换为英寸。
			public static float GetInchesFromPixels(float pixels) {
				Debug.Assert(ScreenDpi > 0, "You must set screen DPI first.");
				return pixels / ScreenDpi;
			}
			/// 将英寸转换为像素。
			public static float GetPixelsFromInches(float inches) {
				Debug.Assert(ScreenDpi > 0, "You must set screen DPI first.");
				return inches * ScreenDpi;
			}
			#endregion

			#region Boolean
			public static byte[] GetBytes(bool value) {
				byte[] buffer = new byte[1];
				GetBytes(value, buffer);
				return buffer;
			}
			public static void GetBytes(bool value, byte[] buffer, int startIndex = 0) {
				Debug.Assert(startIndex >= 0 && startIndex + 1 <= buffer.Length, "Start index is invalid.");
				buffer[startIndex] = value ? (byte)1 : (byte)0;
			}
			public static bool GetBoolean(byte[] value, int startIndex = 0) {
				return BitConverter.ToBoolean(value, startIndex);
			}

			public static bool TryParseBoolean(string value, out bool result) {
				result = false;
				value = String.TrimToNull(value);
				if (value == null) return false;
				if (value.Equals("1") || value.Equals("True", StringComparison.OrdinalIgnoreCase) ||
					value.Equals("Yes", StringComparison.OrdinalIgnoreCase) ||
					value.Equals("On", StringComparison.OrdinalIgnoreCase) ||
					value.Equals("T", StringComparison.OrdinalIgnoreCase)) {
					result = true;
					return true;
				}
				if (value.Equals("0") || value.Equals("False", StringComparison.OrdinalIgnoreCase) ||
					value.Equals("No", StringComparison.OrdinalIgnoreCase) ||
					value.Equals("Off", StringComparison.OrdinalIgnoreCase) ||
					value.Equals("F", StringComparison.OrdinalIgnoreCase)) {
					result = false;
					return true;
				}
				return false;
			}
			#endregion

			#region Char
			public static byte[] GetBytes(char value) {
				byte[] buffer = new byte[2];
				GetBytes((short)value, buffer);
				return buffer;
			}
			public static void GetBytes(char value, byte[] buffer, int startIndex = 0) {
				GetBytes((short)value, buffer, startIndex);
			}
			public static char GetChar(byte[] value, int startIndex = 0) {
				return BitConverter.ToChar(value, startIndex);
			}
			#endregion

			#region Int16
			public static byte[] GetBytes(short value) {
				byte[] buffer = new byte[2];
				GetBytes(value, buffer);
				return buffer;
			}
			public static unsafe void GetBytes(short value, byte[] buffer, int startIndex = 0) {
				Debug.Assert(startIndex >= 0 && startIndex + 2 <= buffer.Length, "Start index is invalid.");
				fixed (byte* valueRef = buffer) {
					*(short*)(valueRef + startIndex) = value;
				}
			}
			public static short GetInt16(byte[] value, int startIndex = 0) {
				return BitConverter.ToInt16(value, startIndex);
			}
			#endregion

			#region UInt16
			public static byte[] GetBytes(ushort value) {
				byte[] buffer = new byte[2];
				GetBytes((short)value, buffer);
				return buffer;
			}
			public static void GetBytes(ushort value, byte[] buffer, int startIndex = 0) {
				GetBytes((short)value, buffer, startIndex);
			}
			public static ushort GetUInt16(byte[] value, int startIndex = 0) {
				return BitConverter.ToUInt16(value, startIndex);
			}
			#endregion

			#region Int32
			public static byte[] GetBytes(int value) {
				byte[] buffer = new byte[4];
				GetBytes(value, buffer);
				return buffer;
			}
			public static unsafe void GetBytes(int value, byte[] buffer, int startIndex = 0) {
				Debug.Assert(startIndex >= 0 && startIndex + 4 <= buffer.Length, "Start index is invalid.");
				fixed (byte* valueRef = buffer) {
					*(int*)(valueRef + startIndex) = value;
				}
			}
			public static int GetInt32(byte[] value, int startIndex = 0) {
				return BitConverter.ToInt32(value, startIndex);
			}
			public static int ParseInt32(string s) {
				return int.Parse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo);
			}
			public static bool TryParseInt32(string s, out int i) {
				return int.TryParse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out i);
			}
			#endregion

			#region UInt32
			public static byte[] GetBytes(uint value) {
				byte[] buffer = new byte[4];
				GetBytes((int)value, buffer);
				return buffer;
			}
			public static void GetBytes(uint value, byte[] buffer, int startIndex = 0) {
				GetBytes((int)value, buffer, startIndex);
			}
			public static uint GetUInt32(byte[] value, int startIndex = 0) {
				return BitConverter.ToUInt32(value, startIndex);
			}
			#endregion

			#region Int64
			public static byte[] GetBytes(long value) {
				byte[] buffer = new byte[8];
				GetBytes(value, buffer);
				return buffer;
			}
			public static unsafe void GetBytes(long value, byte[] buffer, int startIndex = 0) {
				Debug.Assert(startIndex >= 0 && startIndex + 8 <= buffer.Length, "Start index is invalid.");
				fixed (byte* valueRef = buffer) {
					*(long*)(valueRef + startIndex) = value;
				}
			}
			public static long GetInt64(byte[] value, int startIndex = 0) {
				return BitConverter.ToInt64(value, startIndex);
			}

			public static bool TryParseInt64(string s, out long i) {
				return long.TryParse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out i);
			}
			#endregion

			#region UInt64
			public static byte[] GetBytes(ulong value) {
				byte[] buffer = new byte[8];
				GetBytes((long)value, buffer);
				return buffer;
			}
			public static void GetBytes(ulong value, byte[] buffer, int startIndex = 0) {
				GetBytes((long)value, buffer, startIndex);
			}
			public static ulong GetUInt64(byte[] value, int startIndex = 0) {
				return BitConverter.ToUInt64(value, startIndex);
			}
			#endregion

			#region Single
			public static unsafe byte[] GetBytes(float value) {
				byte[] buffer = new byte[4];
				GetBytes(*(int*)&value, buffer);
				return buffer;
			}
			public static unsafe void GetBytes(float value, byte[] buffer, int startIndex = 0) {
				GetBytes(*(int*)&value, buffer, startIndex);
			}
			public static float GetSingle(byte[] value, int startIndex = 0) {
				return BitConverter.ToSingle(value, startIndex);
			}
			#endregion

			#region Double
			public static unsafe byte[] GetBytes(double value) {
				byte[] buffer = new byte[8];
				GetBytes(*(long*)&value, buffer);
				return buffer;
			}
			public static unsafe void GetBytes(double value, byte[] buffer, int startIndex = 0) {
				GetBytes(*(long*)&value, buffer, startIndex);
			}
			public static double GetDouble(byte[] value, int startIndex = 0) {
				return BitConverter.ToDouble(value, startIndex);
			}
			#endregion

			#region String
			public static int GetBytes(ReadOnlySpan<char> value, Encoding encoding, Span<byte> buffer) {
				return encoding.GetBytes(value, buffer);
			}
			public static byte[] GetBytes(string value) {
				return GetBytes(value, Encoding.UTF8);
			}
			public static byte[] GetBytes(string value, Encoding encoding) {
				return encoding.GetBytes(value);
			}
			public static int GetBytes(string value, byte[] buffer, int startIndex = 0) {
				return GetBytes(value, Encoding.UTF8, buffer, startIndex);
			}
			public static int GetBytes(string value, Encoding encoding, byte[] buffer, int startIndex = 0) {
				return encoding.GetBytes(value, 0, value.Length, buffer, startIndex);
			}
			public static string GetString(byte[] value) {
				return GetString(value, Encoding.UTF8);
			}
			public static string GetString(byte[] value, Encoding encoding) {
				return encoding.GetString(value);
			}
			public static string GetString(byte[] value, int startIndex, int length) {
				return GetString(value, startIndex, length, Encoding.UTF8);
			}
			public static string GetString(byte[] value, int startIndex, int length, Encoding encoding) {
				return encoding.GetString(value, startIndex, length);
			}
			#endregion

			#region Vector2Int
			public static byte[] GetBytes(Vector2Int value) {
				byte[] buffer = new byte[8];
				GetBytes(value, buffer);
				return buffer;
			}

			public static void GetBytes(Vector2Int value, byte[] buffer, int startIndex = 0) {
				buffer.SetValue(GetBytes(value.x), startIndex);
				buffer.SetValue(GetBytes(value.y), startIndex + 4);
			}

			public static Vector2Int GetVector2Int(byte[] value, int startIndex = 0) {
				return new Vector2Int(GetInt32(value, startIndex), GetInt32(value, startIndex + 4));
			}

			public static bool TryParseVector2Int(string s, out Vector2Int v) {
				v = default;
				if (s == null) return false;
				var parts = s.Split(SPLIT_COMMA, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length != 2) return false;
				if (!TryParseInt32(parts[0], out var x))
					return false;
				if (!TryParseInt32(parts[1], out var y))
					return false;
				v = new Vector2Int(x, y);
				return true;
			}
			#endregion

			#region Color
			public static byte[] GetBytes(Color value) {
				byte[] buffer = new byte[16];
				GetBytes(value, buffer);
				return buffer;
			}

			public static void GetBytes(Color value, byte[] buffer, int startIndex = 0) {
				buffer.SetValue(GetBytes(value.r), startIndex);
				buffer.SetValue(GetBytes(value.g), startIndex + 4);
				buffer.SetValue(GetBytes(value.b), startIndex + 8);
				buffer.SetValue(GetBytes(value.a), startIndex + 12);
			}

			public static Color GetColor(byte[] value, int startIndex = 0) {
				return new Color(GetSingle(value, startIndex), GetSingle(value, startIndex + 4),
					GetSingle(value, startIndex + 8), GetSingle(value, startIndex + 12));
			}
			#endregion

			#region Color32
			public static byte[] GetBytes(Color32 value) {
				byte[] buffer = new byte[4];
				GetBytes(value, buffer);
				return buffer;
			}

			public static void GetBytes(Color32 value, byte[] buffer, int startIndex = 0) {
				buffer.SetValue(GetBytes(value.r), startIndex);
				buffer.SetValue(GetBytes(value.g), startIndex + 1);
				buffer.SetValue(GetBytes(value.b), startIndex + 2);
				buffer.SetValue(GetBytes(value.a), startIndex + 3);
			}

			public static Color32 GetColor32(byte[] value, int startIndex = 0) {
				return new Color32(value[startIndex], value[startIndex + 1], value[startIndex + 2],
					value[startIndex + 3]);
			}

			public static Color32 ParseColor32(string value) {
				if (!TryParseColor32(value, out Color32 color)) {
					throw new FormatException("Invalid color format");
				}
				return color;
			}

			public static bool TryParseColor32(string value, out Color32 color) {
				color = default;
				value = value.Trim();
				if (value.Length != 6 && value.Length != 8) return false;
				byte alpha = 255;
				if (byte.TryParse(value.AsSpan(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture,
						out var red)) return false;
				if (byte.TryParse(value.AsSpan(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture,
						out var green)) return false;
				if (byte.TryParse(value.AsSpan(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture,
						out var blue)) return false;
				if (value.Length == 8 && byte.TryParse(value.AsSpan(6, 2), NumberStyles.HexNumber,
						CultureInfo.InvariantCulture, out alpha)) return false;
				color = new Color32(red, green, blue, alpha);
				return true;
			}
			#endregion

		}
	}
}