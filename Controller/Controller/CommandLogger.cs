using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Exolutio.Controller.Commands;
using Exolutio.Controller.Commands.Reflection;

namespace Exolutio.Controller
{
    public class CommandLogger
    {
        public CommandSerializer Serializer { get; set; }

        public CommandLogger()
        {
            Serializer = new CommandSerializer();
        }

        public bool LoggingStarted { get; protected set; }

        public bool LoggingPaused { get; set; }

        public event Action StateChanged;

        private void InvokeStateChanged()
        {
            Action handler = StateChanged;
            if (handler != null) handler();
        }

        public void StartLogging()
        {
            Serializer.CreateEmptySerializationDocument();

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
                Serializer.Serialize(command, isUndo, isRedo);
            }
        }

        public XDocument GetLoggedDocument()
        {
            return Serializer.SerializationDocument;
        }

        public void LoadLogFile(string fileName)
        {
            Commands = Serializer.DeserializeDocument(fileName);
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