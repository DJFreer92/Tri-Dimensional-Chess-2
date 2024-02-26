using System.Collections.Generic;
using UnityEngine;

public class CommandHandler {
	private List<ICommand> _commands = new();
	private int _index;

	///<summary>
	///Adds the given command to the command list and executes it
	///</summary>
	///<param name="command">The command to be added and executed</param>
	public void AddCommand(ICommand command) {
		if (_commands.Count > _index) _commands.RemoveRange(_index, _commands.Count - _index);
		_commands.Add(command);
		ExecuteCommand();
	}

	///<summary>
	///Adds the given command to the list but only executes it if it is the next command
	///</summary>
	///<param name="command">The command to add</param>
	public void AddCommandToEnd(ICommand command) {
		_commands.Add(command);
		if (_index + 1 == _commands.Count) ExecuteCommand();
	}

	///<summary>
	///Executes the next command
	///</summary>
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
	///Undoes the last command and removes it from the list
	///</summary>
	public void UndoAndRemoveCommand() {
		UndoCommand();
		_commands.RemoveAt(_index);
	}

	///<summary>
	///Undoes all the commands
	///</summary>
	public void UndoAllCommands() {
		while (_index > 0) UndoCommand();
	}

	///<summary>
	///Redoes all the commands
	///</summary>
	public void RedoAllCommands() {
		while (_index < _commands.Count) ExecuteCommand();
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