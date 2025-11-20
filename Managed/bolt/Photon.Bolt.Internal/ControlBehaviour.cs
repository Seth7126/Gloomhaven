#define DEBUG
using System;
using System.Collections.Generic;
using Photon.Bolt.Utils;
using UnityEngine;

namespace Photon.Bolt.Internal;

public class ControlBehaviour : MonoBehaviour
{
	private readonly Queue<ControlCommand> commands = new Queue<ControlCommand>();

	internal void QueueControlCommand(ControlCommand command)
	{
		commands.Enqueue(command);
	}

	internal void FinishPendingCommands()
	{
		foreach (ControlCommand command in commands)
		{
			command.State = ControlState.Finished;
			command.FinishedEvent.Set();
		}
	}

	private void Update()
	{
		if (commands.Count <= 0)
		{
			return;
		}
		ControlCommand controlCommand = commands.Peek();
		switch (controlCommand.State)
		{
		case ControlState.Pending:
			if (--controlCommand.PendingFrames < 0)
			{
				try
				{
					controlCommand.Run();
					controlCommand.State = ControlState.Started;
					break;
				}
				catch (Exception message)
				{
					BoltLog.Error("Exception while running command {0}", controlCommand);
					BoltLog.Error(message);
					controlCommand.State = ControlState.Failed;
					controlCommand.FinishedEvent.Set();
					break;
				}
			}
			break;
		case ControlState.Started:
			if (controlCommand.FinishedEvent.WaitOne(0))
			{
				controlCommand.State = ControlState.Finished;
			}
			break;
		case ControlState.Failed:
			commands.Dequeue();
			break;
		case ControlState.Finished:
			if (--controlCommand.FinishedFrames < 0)
			{
				commands.Dequeue();
				try
				{
					controlCommand.Done();
					break;
				}
				catch (Exception exception)
				{
					BoltLog.Exception(exception);
					break;
				}
			}
			break;
		}
	}
}
