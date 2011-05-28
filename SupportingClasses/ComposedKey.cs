namespace Exolutio.SupportingClasses
{
	public class ComposedKeyBase<T1, T2> 
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Object"/> class.
		/// </summary>
		public ComposedKeyBase(T1 first, T2 second)
		{
			First = first;
			Second = second;
		}

		public T1 First { get; private set; }
		public T2 Second { get; private set; }

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}

			if (!(obj is ComposedKeyBase<T1, T2>))
			{
				return false;
			}

			ComposedKeyBase<T1, T2> _obj = (ComposedKeyBase<T1, T2>)obj;

			return this.First.Equals(_obj.First) && this.Second.Equals(_obj.Second);
		}

		public override int GetHashCode()
		{
			return First.GetHashCode() * Second.GetHashCode();
		}
	}

	public class ComposedKey<T1, T2> : ComposedKeyBase<T1, T2> 
		where T1: class
		where T2: class

	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Object"/> class.
		/// </summary>
		public ComposedKey(T1 first, T2 second) : base(first, second)
		{
		}

		public static bool operator ==(ComposedKey<T1, T2> key1, ComposedKey<T1, T2> key2)
		{
			if (key1 == null && key2 == null)
			{
				return true; 
			}

			if (key1 == null || key2 == null)
			{
				return false; 
			}

			return (key1.First == key2.First && key1.Second == key2.Second);
		}

		public static bool operator !=(ComposedKey<T1, T2> key1, ComposedKey<T1, T2> key2)
		{
			return !(key1 == key2);
		}

		public bool Equals(ComposedKey<T1, T2> other)
		{
			return base.Equals(other);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as ComposedKey<T1, T2>);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}