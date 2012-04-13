using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Exolutio.Controller.Commands;
using Exolutio.Controller.Commands.Reflection;

namespace Exolutio.Controller
{
    public class CommandLogger
    {
        public bool LoggingStarted { get; protected set; }

        public bool LoggingPaused { get; set; }

        public event Action StateChanged;

        private void InvokeStateChanged()
        {
            Action handler = StateChanged;
            if (handler != null) handler();
        }

        public XDocument SerializationDocument { get; private set; }

        public void StartLogging()
        {
            SerializationDocument = CommandSerializer.CreateEmptySerializationDocument();

            LoggingStarted = true;
            InvokeStateChanged();
        }

        public void PauseLogging()
        {
            LoggingPaused = true;
            InvokeStateChanged();
        }

        public void ContinueLogging()
        {
            LoggingPaused = false;
            InvokeStateChanged();
        }

        public void StopLogging()
        {
            LoggingStarted = false;
            InvokeStateChanged();
        }

        public void OnCommandExecuted(CommandBase command, bool isPartOfMacro, CommandBase macroCommand, bool isUndo, bool isRedo)
        {
            if (isPartOfMacro)
                return;
            else if (LoggingStarted && !LoggingPaused)
            {
                XElement serializedCommand = CommandSerializer.SerializeCommand(command, isUndo, isRedo);
                SerializationDocument.Root.Add(serializedCommand);
            }
        }
        
        public void LoadLogFile(string fileName)
        {
            Commands = CommandSerializer.DeserializeDocument(fileName);
            ReplayStarted = true;
            ReplayIndex = 0;
            InvokeStateChanged();
        }

        public IList<CommandBase> Commands;

        public bool ReplayStarted { get; set; }

        private int ReplayIndex { get; set; }

        public bool CanContinueReplay
        {
            get { return ReplayStarted && Commands != null && Commands.Count > ReplayIndex; }
        }

        public CommandBase GetNextCommand()
        {
            CommandBase commandBase = Commands[ReplayIndex++];
            InvokeStateChanged();
            return commandBase;
        }

        public void EndReplay()
        {
            Commands = null;
            ReplayStarted = false;
            ReplayIndex = -1;
            InvokeStateChanged();
        }
    }
}