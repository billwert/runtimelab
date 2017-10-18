﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel;
using Xunit;

namespace System.Diagnostics.Tests
{
    public class EventLogWriteEntryTests
    {
        private const string message = "EventLogWriteEntryTestsMessage";

        private readonly byte[] rawData = new byte[4] { 0, 1, 2, 3 };
        private readonly EventInstance eventInstance = new EventInstance(0, 1);
        private readonly string[] insertStrings = { "ExtraText", "MoreText" };

        private EventLogEntry WriteLogEntry(string source, bool type = false, bool instance = false, bool category = false, bool data = false)
        {
            using (EventLog eventLog = new EventLog())
            {
                eventLog.Source = source;
                if (instance)
                {
                    EventLog.WriteEvent(source, eventInstance);
                    if (data)
                    {
                        eventLog.WriteEntry(message, EventLogEntryType.Warning, (int)eventInstance.InstanceId, (short)eventInstance.CategoryId, rawData);
                        return eventLog.Entries[eventLog.Entries.Count - 1];
                    }
                    else if (category)
                    {
                        eventLog.WriteEntry(message, EventLogEntryType.Warning, (int)eventInstance.InstanceId, (short)eventInstance.CategoryId);
                        return eventLog.Entries[eventLog.Entries.Count - 1];
                    }
                    else
                    {
                        eventLog.WriteEntry(message, EventLogEntryType.Warning, (int)eventInstance.InstanceId);
                        return eventLog.Entries[eventLog.Entries.Count - 1];
                    }
                }
                else if (type)
                {
                    eventLog.WriteEntry(message, EventLogEntryType.Warning);
                }
                else
                {
                    eventLog.WriteEntry(message);
                }

                return eventLog.Entries[eventLog.Entries.Count - 1];
            }
        }

        private EventLogEntry WriteLogEntryWithSource(string source, bool type = false, bool instance = false, bool category = false, bool data = false)
        {
            using (EventLog eventLog = new EventLog())
            {
                eventLog.Source = source;
                if (instance)
                {
                    EventLog.WriteEvent(source, eventInstance);
                    if (data)
                    {
                        EventLog.WriteEntry(source, message, EventLogEntryType.Warning, (int)eventInstance.InstanceId, (short)eventInstance.CategoryId, rawData);
                        return eventLog.Entries[eventLog.Entries.Count - 1];
                    }
                    else if (category)
                    {
                        EventLog.WriteEntry(source, message, EventLogEntryType.Warning, (int)eventInstance.InstanceId, (short)eventInstance.CategoryId);
                        return eventLog.Entries[eventLog.Entries.Count - 1];
                    }
                    else
                    {
                        EventLog.WriteEntry(source, message, EventLogEntryType.Warning, (int)eventInstance.InstanceId);
                        return eventLog.Entries[eventLog.Entries.Count - 1];
                    }
                }
                else if (type)
                {
                    EventLog.WriteEntry(source, message, EventLogEntryType.Warning);
                }
                else
                {
                    EventLog.WriteEntry(source, message);
                }

                return eventLog.Entries[eventLog.Entries.Count - 1];
            }
        }

        private EventLogEntry WriteLogEntryEventSource(string source, bool data = false)
        {
            if (data)
                EventLog.WriteEvent(source, eventInstance, rawData, insertStrings);
            else
                EventLog.WriteEvent(source, eventInstance, insertStrings);

            using (EventLog eventLog = new EventLog())
            {
                eventLog.Source = source;
                return eventLog.Entries[eventLog.Entries.Count - 1];
            }
        }

        private EventLogEntry WriteLogEntryEvent(string source, bool data = false)
        {
            using (EventLog eventLog = new EventLog())
            {
                string[] insertStringsSingleton = { "ExtraText" };
                eventLog.Source = source;
                if (data)
                    eventLog.WriteEvent(eventInstance, rawData, insertStringsSingleton);
                else
                    eventLog.WriteEvent(eventInstance, insertStringsSingleton);

                return eventLog.Entries[eventLog.Entries.Count - 1];
            }
        }

        [ConditionalTheory(typeof(Helpers), nameof(Helpers.IsElevatedAndSupportsEventLogs))]
        [InlineData(false)]
        [InlineData(true)]
        public void WriteEntry(bool sourceFlag)
        {
            string log = "Entry";
            string source = "Source" + nameof(WriteEntry);
            try
            {
                EventLog.CreateEventSource(source, log);
                EventLogEntry eventLogEntry;
                if (sourceFlag)
                    eventLogEntry = WriteLogEntry(source);
                else
                    eventLogEntry = WriteLogEntryWithSource(source);

                Assert.Contains(message, eventLogEntry.Message);
                Assert.Equal(source, eventLogEntry.Source);
                Assert.StartsWith(Environment.MachineName.ToLowerInvariant(), eventLogEntry.MachineName.ToLowerInvariant());
                Assert.Equal(eventLogEntry.TimeWritten, eventLogEntry.TimeGenerated);
            }
            finally
            {
                EventLog.DeleteEventSource(source);
                EventLog.Delete(log);
            }
        }

        [ConditionalTheory(typeof(Helpers), nameof(Helpers.IsElevatedAndSupportsEventLogs))]
        [InlineData(false)]
        [InlineData(true)]
        public void WriteEntryWithType(bool sourceFlag)
        {
            string source = "Source" + nameof(WriteEntryWithType);
            string log = "TypeEntry";
            try
            {
                EventLog.CreateEventSource(source, log);
                EventLogEntry eventLogEntry;
                if (sourceFlag)
                    eventLogEntry = WriteLogEntry(source, type: true);
                else
                    eventLogEntry = WriteLogEntryWithSource(source, type: true);

                Assert.Contains(message, eventLogEntry.Message);
                Assert.Equal(EventLogEntryType.Warning, eventLogEntry.EntryType);
            }
            finally
            {
                EventLog.DeleteEventSource(source);
                EventLog.Delete(log);
            }
        }

        [ConditionalTheory(typeof(Helpers), nameof(Helpers.IsElevatedAndSupportsEventLogs))]
        [InlineData(false)]
        [InlineData(true)]
        public void WriteEntryWithTypeAndId(bool sourceFlag)
        {
            if (!AdminHelpers.IsProcessElevated())
                return;

            string source = "Source" + nameof(WriteEntryWithTypeAndId);
            string log = "InstanceEntry";
            try
            {
                EventLog.CreateEventSource(source, log);
                EventLogEntry eventLogEntry;
                if (sourceFlag)
                    eventLogEntry = WriteLogEntry(source, type: true, instance: true);
                else
                    eventLogEntry = WriteLogEntryWithSource(source, type: true, instance: true);

                Assert.Contains(message, eventLogEntry.Message);
                Assert.Equal((int)eventInstance.InstanceId, eventLogEntry.InstanceId);
            }
            finally
            {
                EventLog.DeleteEventSource(source);
                EventLog.Delete(log);
            }
        }

        [ConditionalTheory(typeof(Helpers), nameof(Helpers.IsElevatedAndSupportsEventLogs))]
        [InlineData(false)]
        [InlineData(true)]
        public void WriteEntryWithTypeIdAndCategory(bool sourceFlag)
        {
            string source = "Source" + nameof(WriteEntryWithTypeIdAndCategory);
            string log = "CategoryEntry";
            try
            {
                EventLog.CreateEventSource(source, log);
                EventLogEntry eventLogEntry;
                if (sourceFlag)
                    eventLogEntry = WriteLogEntry(source, type: true, instance: true, category: true);
                else
                    eventLogEntry = WriteLogEntryWithSource(source, type: true, instance: true, category: true);

                // There is some prefix string already attached to the message passed
                // The description for Event ID '0' in Source 'SourceWriteEntryWithTypeIDAndCategory' cannot be found.  The local computer may not have the necessary registry information or message DLL files to display the message, or you may not have permission
                // to access them.  The following information is part of the event:'EventLogWriteEntryTestsMessage'
                // The last part is the associated message
                // The initial message is due in insufficient permission to access resource library EventLogMsgs.dll
                Assert.Contains(message, eventLogEntry.Message);
                Assert.Equal((short)eventInstance.CategoryId, eventLogEntry.CategoryNumber);
                Assert.Equal("(" + eventLogEntry.CategoryNumber + ")", eventLogEntry.Category);
            }
            finally
            {
                EventLog.DeleteEventSource(source);
                EventLog.Delete(log);
            }
        }

        [ConditionalTheory(typeof(Helpers), nameof(Helpers.IsElevatedAndSupportsEventLogs))]
        [InlineData(false)]
        [InlineData(true)]
        public void WriteEntryWithTypeIdCategoryAndData(bool sourceFlag)
        {
            string source = "Source" + nameof(WriteEntryWithTypeIdCategoryAndData);
            string log = "EntryData";
            try
            {
                EventLog.CreateEventSource(source, log);
                EventLogEntry eventLogEntry;
                if (sourceFlag)
                    eventLogEntry = WriteLogEntry(source, type: true, instance: true, category: true, data: true);
                else
                    eventLogEntry = WriteLogEntryWithSource(source, type: true, instance: true, category: true, data: true);

                Assert.Contains(message, eventLogEntry.Message);
                Assert.Equal(rawData, eventLogEntry.Data);
            }
            finally
            {
                EventLog.DeleteEventSource(source);
                EventLog.Delete(log);
            }
        }

        [ConditionalTheory(typeof(Helpers), nameof(Helpers.IsElevatedAndSupportsEventLogs))]
        public void WriteEntryWithoutSource()
        {
            using (EventLog eventLog = new EventLog())
            {
                Assert.Throws<ArgumentException>(() => eventLog.WriteEntry(message));
            }
        }

        [ConditionalTheory(typeof(Helpers), nameof(Helpers.IsElevatedAndSupportsEventLogs))]
        public void WriteEntryWithInvalidType()
        {
            using (EventLog eventLog = new EventLog())
            {
                string source = "Source_" + nameof(WriteEntryWithInvalidType);
                eventLog.Source = source;
                Assert.Throws<InvalidEnumArgumentException>(() => eventLog.WriteEntry(message, (EventLogEntryType)7)); // 7 is a random number which is not associated with any type in EventLogEntryType
            }
        }

        [ConditionalTheory(typeof(Helpers), nameof(Helpers.IsElevatedAndSupportsEventLogs))]
        public void WriteEntryWithNullOrEmptySource()
        {
            Assert.Throws<ArgumentException>(() => EventLog.WriteEntry(null, message));
            Assert.Throws<ArgumentException>(() => EventLog.WriteEntry("", message));
        }

        [ConditionalTheory(typeof(Helpers), nameof(Helpers.IsElevatedAndSupportsEventLogs))]
        [InlineData(false)]
        [InlineData(true)]
        public void WriteEvent(bool SourceFlag)
        {
            string source = "Source_" + nameof(WriteEvent);
            string log = "Event";
            try
            {
                EventLog.CreateEventSource(source, log);
                EventLogEntry eventLogEntry;
                if (SourceFlag)
                    eventLogEntry = WriteLogEntryEventSource(source);
                else
                    eventLogEntry = WriteLogEntryEvent(source);

                Assert.All(insertStrings, message => eventLogEntry.Message.Contains(message));
            }
            finally
            {
                EventLog.DeleteEventSource(source);
                EventLog.Delete(log);
            }
        }

        [ConditionalTheory(typeof(Helpers), nameof(Helpers.IsElevatedAndSupportsEventLogs))]
        [InlineData(false)]
        [InlineData(true)]
        public void WriteEventWithData(bool SourceFlag)
        {
            string log = "EventData";
            string source = "Source_" + nameof(WriteEventWithData);
            try
            {
                EventLog.CreateEventSource(source, log);
                EventLogEntry eventLogEntry;
                if (SourceFlag)
                    eventLogEntry = WriteLogEntryEventSource(source, data: true);
                else
                    eventLogEntry = WriteLogEntryEvent(source, data: true);

                Assert.Equal(rawData, eventLogEntry.Data);
            }
            finally
            {
                EventLog.DeleteEventSource(source);
                EventLog.Delete(log);
            }
        }

        [ConditionalTheory(typeof(Helpers), nameof(Helpers.IsElevatedAndSupportsEventLogs))]
        public void WriteEventInstanceNull()
        {
            string source = "Source_" + nameof(WriteEventInstanceNull);
            Assert.Throws<ArgumentNullException>(() => EventLog.WriteEvent(source, null, insertStrings));
        }

        [ConditionalTheory(typeof(Helpers), nameof(Helpers.IsElevatedAndSupportsEventLogs))]
        public void WriteEventMessageValues_OutOfRange()
        {
            string source = "Source_" + nameof(WriteEventMessageValues_OutOfRange);
            string[] message = new string[1];
            message[0] = new string('c', 32767);
            Assert.Throws<ArgumentException>(() => EventLog.WriteEvent(source, eventInstance, message));
        }

        [ConditionalFact(typeof(Helpers), nameof(Helpers.IsElevatedAndSupportsEventLogs))]
        public void WriteWithoutExistingSource()
        {
            string source = "Source_" + nameof(WriteWithoutExistingSource);
            try
            {
                EventLog.WriteEvent(source, eventInstance, rawData, null);
                Assert.Equal("Application", EventLog.LogNameFromSourceName(source, "."));
            }
            finally
            {
                EventLog.DeleteEventSource(source);
            }
        }

        [ConditionalTheory(typeof(Helpers), nameof(Helpers.IsElevatedAndSupportsEventLogs))]
        public void SourceNameMaxLengthExceeded()
        {
            string source = new string('s', 254);
            Assert.Throws<ArgumentException>(() => EventLog.WriteEntry(source, message));
        }
    }
}
