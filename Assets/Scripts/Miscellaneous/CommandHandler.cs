using System.Collections.Generic;
using UnityEngine;

public class CommandHandler {
	private readonly List<ICommand> _commands = new();
	private int _index;

	///<summary>
	///Adds the given command to the command list and executes it
	///</summary>
	///<param name="command">The command to be added and executed</param>
	public void AddCommand(ICommand command) {
		if (AreCommandsWaiting()) _commands.RemoveRange(_index, _commands.Count - _index);
		_commands.Add(command);
		ExecuteCommand();
	}

	///<summary>
	///Executes the next command
	///</summary>
	private void ExecuteCommand() {
		if (!AreCommandsWaiting()) return;
		_commands[_index++].Execute();
	}

	///<summary>
	///Undoes the previous command
	///</summary>
	public void UndoCommand() {
		if (_index == 0) return;
		_commands[--_index].Undo();
	}

	///<summary>
	///Undoes the last command and removes it from the list
	///</summary>
	public bool UndoAndRemoveCommand() {
		if (_index == 0) return false;
		UndoCommand();
		_commands.RemoveAt(_index);
		return true;
	}

	///<summary>
	///Undoes all the commands
	///</summary>
	public void UndoAllCommands() {
		while (AreCommandsWaiting()) UndoCommand();
	}

	///<summary>
	///Redo the next command
	///</summary>
	public void RedoCommand() {
		if (!AreCommandsWaiting()) return;
		_commands[_index++].Redo();
	}

	///<summary>
	///Redoes all the commands
	///</summary>
	public void RedoAllCommands() {
		while (AreCommandsWaiting()) RedoCommand();
	}

	///<summary>
	///Returns whether there are any commands waiting to be executed
	///</summary>
	///<returns>Whether there are any commands waiting to be executed
	public bool AreCommandsWaiting() => _index < _commands.Count;
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

	///<summary>
	///Redoes the command
	///</summary>
	void Redo();
}