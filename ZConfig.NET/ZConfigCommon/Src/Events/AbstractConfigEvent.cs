using System;
using System.Collections.Generic;
using System.Text;
using LibZConfig.Common;

namespace LibZConfigTransport.Events
{
    /// <summary>
    /// Class to define the header of a configuration update transaction batch.
    /// </summary>
    public class ConfigUpdateHeader
    {
        /// <summary>
        /// Application Group name.
        /// </summary>
        public string Group { get; set; }
        /// <summary>
        /// Application name.
        /// </summary>
        public string Application { get; set; }
        /// <summary>
        /// Configuration name this batch is for.
        /// </summary>
        public string ConfigName { get; set; }
        /// <summary>
        /// Pre-Update version (base version) of the configuration.
        /// </summary>
        public string PreviousVersion { get; set; }
        /// <summary>
        /// Updated version post applying changes.
        /// </summary>
        public string UpdatedVersion { get; set; }
        /// <summary>
        /// Transaction ID of this update batch.
        /// </summary>
        public string TransactionId { get; set; }
        /// <summary>
        /// Timestamp of the update.
        /// </summary>
        public long Timestamp { get; set; }
    }

    /// <summary>
    /// Enum to indicate the update type in update events.
    /// </summary>
    public enum EUpdateEventType
    {
        /// <summary>
        /// Event reflects a newly added node.
        /// </summary>
        Add,
        /// <summary>
        /// Event for a node value being updated.
        /// </summary>
        Update,
        /// <summary>
        /// Event for a node being deleted.
        /// </summary>
        Remove
    }

    /// <summary>
    /// Update event structure for a configuration node update.
    /// <p>
    /// Event Structure:
    /// <pre>
    ///      {
    ///             "transaction" : [unique tnx ID],
    ///             "group" : [application group],
    ///             "application" : [application],
    ///             "config" : [config name],
    ///             "version" : [config version],
    ///             "path" : [node path],
    ///             "eventType" : [ADD/UPDATE/DELETE],
    ///             "timestamp" : [timestamp]
    ///     }
    /// </pre>
    /// </summary>
    public abstract class AbstractConfigEvent<T>
    {
        public ConfigUpdateHeader Header { get; set; }
        public EUpdateEventType EventType { get; set; }
        public string Path { get; set; }
        public string TransactionId { get; set; }
        public long TransactionSeq { get; set; }
        public long Timestamp { get; set; }
        public T Data { get; set; }

        public AbstractConfigEvent(string group, string application, string config)
        {
            Preconditions.CheckArgument(group);
            Preconditions.CheckArgument(application);

            Header = new ConfigUpdateHeader();
            Header.Timestamp = DateTime.Now.Ticks;
            Header.Group = group;
            Header.Application = application;
            Header.ConfigName = config;
        }
    }

    public abstract class AbstractConfigBatch<T>
    {
        public ConfigUpdateHeader Header { get; set; }
        public int BatchCount { get; set; }
        public List<AbstractConfigEvent<T>> Data { get; set; }

        public AbstractConfigBatch(string group, string application, string config)
        {
            Preconditions.CheckArgument(group);
            Preconditions.CheckArgument(application);

            Header = new ConfigUpdateHeader();
            Header.TransactionId = Guid.NewGuid().ToString();
            Header.Timestamp = DateTime.Now.Ticks;
            Header.Group = group;
            Header.Application = application;
            Header.ConfigName = config;
        }

        public AbstractConfigBatch<T> Add(AbstractConfigEvent<T> data)
        {
            Preconditions.CheckArgument(data);
            if (Data == null)
            {
                Data = new List<AbstractConfigEvent<T>>();
            }
            Data.Add(data);
            BatchCount++;
            return this;
        }
    }
}
