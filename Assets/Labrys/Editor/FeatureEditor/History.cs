using System.Collections.Generic;
using UnityEngine;

namespace Labrys.Editor.FeatureEditor
{
	internal static class History
	{
		private static RingBuffer<Command> commands;
		private static int allowedRedos;

		public static Command NextUndo => commands.Peek();

		static History()
		{
			commands = new RingBuffer<Command>(capacity: 100);
			allowedRedos = 0;
		}

		public static void RecordCommand(Command command)
		{
			commands.Add(command);
			allowedRedos = 0;
		}

		public static void Undo()
		{
			Command c = commands.Pop();
			if (c != null)
			{
				allowedRedos++;
				c.Undo();
			}
		}

		public static bool CanRedo()
		{
			return allowedRedos > 0;
		}

		public static void Redo()
		{
			if (CanRedo())
			{
				commands.Seek(1);
				commands.Peek()?.Do();
				allowedRedos--;
			}
		}

		private class RingBuffer<T>
		{
			private T[] array;
			private int index;

			public RingBuffer() : this(100) { }
			public RingBuffer(int capacity)
			{
				array = new T[capacity];
				index = 0;
			}

			public void Add(T obj)
			{
				array[index] = obj;
				Seek(delta: 1);
			}

			public void Seek(int delta)
			{
				index = (index + delta) % array.Length;
				if (index < 0) index = array.Length - 1;
			}

			public T Peek()
			{
				return array[index];
			}

			public T Pop()
			{
				T obj = array[index];
				Seek(-1);
				return obj;
			}
		}
	}
}
