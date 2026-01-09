#if !UNITY_2021_3_OR_NEWER
namespace UIThemes
{
	public static class HashCode
	{
		static int GetHash<T>(T value) => value != null ? value.GetHashCode() : 0;

		public static int Combine<T1>(T1 value1)
		{
			unchecked
			{
				return GetHash(value1);
			}
		}

		public static int Combine<T1, T2>(T1 value1, T2 value2)
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 31 + GetHash(value1);
				hash = hash * 31 + GetHash(value2);
				return hash;
			}
		}

		public static int Combine<T1, T2, T3>(T1 value1, T2 value2, T3 value3)
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 31 + GetHash(value1);
				hash = hash * 31 + GetHash(value2);
				hash = hash * 31 + GetHash(value3);
				return hash;
			}
		}

		public static int Combine<T1, T2, T3, T4>(T1 value1, T2 value2, T3 value3, T4 value4)
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 31 + GetHash(value1);
				hash = hash * 31 + GetHash(value2);
				hash = hash * 31 + GetHash(value3);
				hash = hash * 31 + GetHash(value4);
				return hash;
			}
		}

		public static int Combine<T1, T2, T3, T4, T5>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5)
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 31 + GetHash(value1);
				hash = hash * 31 + GetHash(value2);
				hash = hash * 31 + GetHash(value3);
				hash = hash * 31 + GetHash(value4);
				hash = hash * 31 + GetHash(value5);
				return hash;
			}
		}

		public static int Combine<T1, T2, T3, T4, T5, T6>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6)
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 31 + GetHash(value1);
				hash = hash * 31 + GetHash(value2);
				hash = hash * 31 + GetHash(value3);
				hash = hash * 31 + GetHash(value4);
				hash = hash * 31 + GetHash(value5);
				hash = hash * 31 + GetHash(value6);
				return hash;
			}
		}

		public static int Combine<T1, T2, T3, T4, T5, T6, T7>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7)
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 31 + GetHash(value1);
				hash = hash * 31 + GetHash(value2);
				hash = hash * 31 + GetHash(value3);
				hash = hash * 31 + GetHash(value4);
				hash = hash * 31 + GetHash(value5);
				hash = hash * 31 + GetHash(value6);
				hash = hash * 31 + GetHash(value7);
				return hash;
			}
		}

		public static int Combine<T1, T2, T3, T4, T5, T6, T7, T8>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8)
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 31 + GetHash(value1);
				hash = hash * 31 + GetHash(value2);
				hash = hash * 31 + GetHash(value3);
				hash = hash * 31 + GetHash(value4);
				hash = hash * 31 + GetHash(value5);
				hash = hash * 31 + GetHash(value6);
				hash = hash * 31 + GetHash(value7);
				hash = hash * 31 + GetHash(value8);
				return hash;
			}
		}

		public static int Combine<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9)
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 31 + GetHash(value1);
				hash = hash * 31 + GetHash(value2);
				hash = hash * 31 + GetHash(value3);
				hash = hash * 31 + GetHash(value4);
				hash = hash * 31 + GetHash(value5);
				hash = hash * 31 + GetHash(value6);
				hash = hash * 31 + GetHash(value7);
				hash = hash * 31 + GetHash(value8);
				hash = hash * 31 + GetHash(value9);
				return hash;
			}
		}

		public static int Combine<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9, T10 value10)
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 31 + GetHash(value1);
				hash = hash * 31 + GetHash(value2);
				hash = hash * 31 + GetHash(value3);
				hash = hash * 31 + GetHash(value4);
				hash = hash * 31 + GetHash(value5);
				hash = hash * 31 + GetHash(value6);
				hash = hash * 31 + GetHash(value7);
				hash = hash * 31 + GetHash(value8);
				hash = hash * 31 + GetHash(value9);
				hash = hash * 31 + GetHash(value10);
				return hash;
			}
		}
	}
}
#endif