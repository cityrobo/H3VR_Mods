using System;

namespace ShotTimer
{
	public class OverflowArray<T>
	{
		private readonly T[] _buffer;
		
		private int _cursor;

		public readonly int Capacity;
		public int Count { get; private set; }

		public OverflowArray(int size)
		{
			_buffer = new T[size];
			
			Capacity = size;
		}

		public void Add(T value)
		{
			this[_cursor++] = value;
			
			if (Count < Capacity)
			{
				++Count;
			}
		}

		public void Clear()
		{
			_cursor = 0;
			Count = 0;
		}

		public T this[int index]
		{
			get
			{
				if (index >= Count)
				{
					throw new IndexOutOfRangeException();
				}
				
				return _buffer[index % Capacity];
			}
			private set
			{
				_buffer[index % Capacity] = value;
			}
		}

		private bool TryGet(Func<int, int> index, out T value)
		{
			if (Count > 0)
			{
				value = this[index(Count)];
				return true;
			}

			value = default(T);
			return false;
		}

		public bool TryGetFirst(out T value)
		{
			return TryGet(_ => 0, out value);
		}

		public bool TryGetLast(out T value)
		{
			return TryGet(c => c - 1, out value);
		}
	}
}