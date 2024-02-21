using System.Collections.Generic;
using UnityEngine;

public class CommandHandler {
	private List<ICommand> _commands = new();
	private int _index;

	///<summary>
	///Adds the given command to the command list and executes it
	///</summary>
	///<params name="command">The command to be added and executed</params>
	public void AddCommand(ICommand command) {
		if (_commands.Count > _index) _commands.RemoveRange(_index, _commands.Count - _index);
		_commands.Add(command);
		ExecuteCommand();
	}

	private void ExecuteCommand() {
		if (_commands.Count == _index) return;
		_commands[_index++].Execute();
	}

	///<summary>
	///Undoes the previous command
	///</summary>
	public void UndoCommand() {
		if (_index == 0) return;
		_commands[_index--].Undo();
	}

	///<summary>
	///Redoes the previous command
	///</summary>
	public void RedoCommand() {
		ExecuteCommand();
	}
}

public interface ICommand {
	///<summary>
	///Executes the command
	///</summary>
	void Execute();

	///<summary>
	///Undoes the command
	///</summary>
	void Undo();
}