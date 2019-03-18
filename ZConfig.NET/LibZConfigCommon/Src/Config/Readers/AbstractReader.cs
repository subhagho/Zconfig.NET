using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Diagnostics.Contracts;
using LibZConfig.Common.Utils;

namespace LibZConfig.Common.Config.Readers
{
    /// <summary>
    /// Exception class to be used to propogate configuration errors.
    /// </summary>
    public class ReaderException : Exception
    {
        private static readonly string __PREFIX = "Reader Error : {0}";

        /// <summary>
        /// Constructor with error message.
        /// </summary>
        /// <param name="mesg">Error message</param>
        public ReaderException(string mesg) : base(String.Format(__PREFIX, mesg))
        {

        }

        /// <summary>
        /// Constructor with error message and cause.
        /// </summary>
        /// <param name="mesg">Error message</param>
        /// <param name="cause">Cause</param>
        public ReaderException(string mesg, Exception cause) : base(String.Format(__PREFIX, mesg), cause)
        {

        }

        /// <summary>
        /// Constructor with cause.
        /// </summary>
        /// <param name="exception">Cause</param>
        public ReaderException(Exception exception) : base(String.Format(__PREFIX, exception.Message), exception)
        {

        }

        /// <summary>
        /// Create a property missing exception.
        /// </summary>
        /// <param name="property">Property name</param>
        /// <returns>Configuration Exception</returns>
        public static ConfigurationException PropertyMissingException(string property)
        {
            return new ConfigurationException(String.Format("Required property missing : [name={0}]", property));
        }
    }

    /// <summary>
    /// Enum to define the state of a configuration reader instance.
    /// </summary>
    public enum EReaderState
    {
        /// <summary>
        /// State is Unknown
        /// </summary>
        Unknown,
        /// <summary>
        /// Reader is open and available.
        /// </summary>
        Open,
        /// <summary>
        /// Reader has been closed
        /// </summary>
        Closed,
        /// <summary>
        /// Reader is in exception state.
        /// </summary>
        Error
    }

    /// <summary>
    /// State object for maintaining configuration reader state.
    /// </summary>
    public class ReaderState : AbstractState<EReaderState>
    {
        /// <summary>
        /// Is the reader open and available?
        /// </summary>
        /// <returns>Is Open?</returns>
        public bool IsOpen()
        {
            return (State == EReaderState.Open);
        }

        /// <summary>
        /// Has the reader been closed?
        /// </summary>
        /// <returns>Is Closed?</returns>
        public bool IsClosed()
        {
            return (State == EReaderState.Closed);
        }

        /// <summary>
        /// Get the error state of this reader.
        /// </summary>
        /// <returns>Error state</returns>
        public override EReaderState GetErrorState()
        {
            return EReaderState.Error;
        }
    }

    /// <summary>
    /// Abstract base class for defining cofniguration readers.
    /// </summary>
    public abstract class AbstractReader : IDisposable
    {
        /// <summary>
        /// Instance Reader State
        /// </summary>
        public ReaderState State { get; set; }

        /// <summary>
        /// Open this reader instance.
        /// </summary>
        public abstract void Open();

        /// <summary>
        /// Dispose this reader instance.
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// Get the stream reader handle.
        /// </summary>
        /// <returns>Stream Reader handle</returns>
        public abstract StreamReader GetStream();
    }

    /// <summary>
    /// Reader implementation to read from local file.
    /// </summary>
    public class FileReader : AbstractReader
    {
        private string filename;
        private StreamReader stream = null;

        /// <summary>
        /// Constructor with Local file path.
        /// </summary>
        /// <param name="filename">File path</param>
        public FileReader(string filename)
        {
            Contract.Requires(!String.IsNullOrWhiteSpace(filename));
            this.filename = filename;
        }

        /// <summary>
        /// Constructor with file URI.
        /// </summary>
        /// <param name="location">File URI</param>
        public FileReader(Uri location)
        {
            Contract.Requires(location != null);
            if (!location.IsWellFormedOriginalString())
            {
                throw new ReaderException(String.Format("Malformed URI : [location={0}]", location.ToString()));
            }
            EUriType type = URIUtils.GetUriType(location);
            if (type != EUriType.file)
            {
                var ex = new ReaderException(String.Format("Invalid URI : [expected={0}][actual={1}]", EUriType.file.ToString(), type.ToString()));
                State.SetError(ex);
                throw ex;
            }
            filename = location.LocalPath;
        }

        /// <summary>
        /// Disposable handler
        /// </summary>
        public override void Dispose()
        {
            if (stream != null)
            {
                stream.Dispose();
            }
            if (!State.HasError())
            {
                State.State = EReaderState.Closed;
            }
        }

        /// <summary>
        /// Get the stream associated with this reader.
        /// </summary>
        /// <returns>Stream reader</returns>
        public override StreamReader GetStream()
        {
            if (!State.IsOpen())
            {
                throw new ReaderException(String.Format("Reader is not Open: [state={0}]", State.State.ToString()));
            }
            return stream;
        }

        /// <summary>
        /// Open this reader.
        /// </summary>
        public override void Open()
        {
            try
            {
                FileInfo file = new FileInfo(filename);
                if (!file.Exists)
                {
                    throw new ReaderException(String.Format("File not found: [file={0}]", filename));
                }
                LogUtils.Debug(String.Format("Opeing File stream: [path={0}]", file.FullName));
                stream = new StreamReader(file.FullName);
                State.State = EReaderState.Open;
            }
            catch (Exception ex)
            {
                State.SetError(ex);
                LogUtils.Error(ex);
                throw ex;
            }
        }
    }

    /// <summary>
    /// Reader implementation to read from URI location.
    /// </summary>
    public class RemoteReader : AbstractReader
    {
        private Uri location;
        private StreamReader stream;

        /// <summary>
        /// Constructor with Uri string.
        /// </summary>
        /// <param name="location">Uri String</param>
        public RemoteReader(string location)
        {
            Contract.Requires(!String.IsNullOrWhiteSpace(location));
            this.location = new Uri(location);
            if (!this.location.IsWellFormedOriginalString())
            {
                throw new ReaderException(String.Format("Malformed URI : [location={0}]", location));
            }
        }

        /// <summary>
        /// Constructor with Uri
        /// </summary>
        /// <param name="location">Uri location</param>
        public RemoteReader(Uri location)
        {
            Contract.Requires(location != null);
            this.location = location;
            if (!this.location.IsWellFormedOriginalString())
            {
                throw new ReaderException(String.Format("Malformed URI : [location={0}]", location.ToString()));
            }
        }

        /// <summary>
        /// Disposable handler
        /// </summary>
        public override void Dispose()
        {
            if (stream != null)
            {
                stream.Dispose();
            }
            State.State = EReaderState.Closed;
        }

        /// <summary>
        /// Get the stream associated with this reader.
        /// </summary>
        /// <returns>Stream reader</returns>
        public override StreamReader GetStream()
        {
            if (!State.IsOpen())
            {
                throw new ReaderException(String.Format("Reader is not Open: [state={0}]", State.State.ToString()));
            }
            return stream;
        }

        /// <summary>
        /// Open this reader.
        /// </summary>
        public override void Open()
        {
            try
            {
                LogUtils.Debug("Opening URI Stream: [uri={0}]", location.ToString());
                WebRequest request = WebRequest.Create(location);
                WebResponse response = request.GetResponse();
                Stream dataStream = response.GetResponseStream();
                stream = new StreamReader(dataStream);
            }
            catch (Exception ex)
            {
                State.SetError(ex);
                LogUtils.Error(ex);
                throw ex;
            }
        }
    }
}
