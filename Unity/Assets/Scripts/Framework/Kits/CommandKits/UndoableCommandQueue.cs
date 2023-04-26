using System.Collections.Generic;
using Framework.Kits.ReferencePoolKits;

namespace Framework.Kits.CommandKits
{
	public class UndoableCommandQueue
	{
		private UndoableCommand[] _commands;
		private int _pointer, _top;
		public int Pointer {
			get => _pointer;
			private set {
				if (value > Top) {
					Top = value;
				}
				_pointer = value;
			}
		}
		public int Top {
			get => _top;
			private set {
				if (value > _commands.Length) {
					var tmp = _commands;
					_commands = new UndoableCommand[2 * _commands.Length];
					tmp.CopyTo(_commands, 0);
					_top = value;
					return;
				}
				while (_top > value) {
					ReferencePool.Release(_commands[_top]);
					_commands[_top] = null;
					_top--;
				}
			}
		}
		private int _step;
		public UndoableCommandQueue() {
			_commands = new UndoableCommand[10];
			_pointer = -1;
			_top = -1;
		}
		public bool IsLast() => Pointer == Top;
		public bool IsEmpty() => Pointer == -1;

		public void ExecuteCommand(UndoableCommand command) {
			command.Execute();
			Pointer++;
			Top = Pointer;
			_commands[Pointer] = command;
			//throw new FrameworkException($"{nameof(ExecuteCommand)} {command} false");
		}
		public bool Undo() {
			if (IsEmpty()) return false;
			_commands[Pointer].Undo();
			Pointer--;
			//else throw new FrameworkException($"{nameof(Undo)} {_commands[Pointer]} false");
			return true;
		}
		public void Undo(int step) {
			while (step > 0 && Undo()) {
				step--;
			}
		}
		public int UndoAll() {
			int counter = 0;
			while (Undo()) {
				counter++;
			}
			return counter;
		}

		public bool Redo() {
			if (IsLast()) return false;
			Pointer++;
			_commands[Pointer].Execute();
			return true;
		}
		public void Redo(int step) {
			while (step > 0 && Redo()) {
				step--;
			}
		}
		public int RedoAll() {
			int counter = 0;
			while (Redo()) {
				counter++;
			}
			return counter;
		}

		public void Clear() {
			Top = -1;
			Pointer = -1;
		}
	}
}