using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using N2.Edit.FileSystem;
using N2.Engine;
using N2.Persistence.Serialization;

namespace N2.Azure.Replication
{
    public class ReplicatedItem
    {
        public int ID;
        public DateTime PublishedDateUtc;
        public bool Processed = false;
        internal string Path;

        public ReplicatedItem(FileData fileData)
        {
            this.ID = ExtractID(fileData.Name);

            // assumes file date equals published
            this.PublishedDateUtc = fileData.Updated.ToUniversalTime();
            this.Path = fileData.VirtualPath;
        }

        public DateTime Normalize(DateTime dateTime)
        {
            dateTime = dateTime.ToUniversalTime();
            return new DateTime(dateTime.Ticks - (dateTime.Ticks % TimeSpan.TicksPerSecond), dateTime.Kind);
        }

        public bool IsNewerThan(DateTime other)
        {
            return Normalize(PublishedDateUtc) > Normalize(other);
        }

        public bool IsOlderThan(DateTime other)
        {
            return Normalize(PublishedDateUtc) < Normalize(other);
        }

        public bool PublishedDateEquals(DateTime other)
        {
            var otherUtc = other.ToUniversalTime();
            // A single tick represents one hundred nanoseconds or one ten-millionth of a second. There are 10,000 ticks in a millisecond.
            return Math.Abs(PublishedDateUtc.Ticks - otherUtc.Ticks) < 10000000;
        }

        public static int ExtractID(string name)
        {
            try
            {
                var number = new StringBuilder();
                int ix = 0;
                while (ix < name.Length && !Char.IsDigit(name[ix]))
                    ix++;

                while (ix < name.Length && Char.IsDigit(name[ix]))
                    number.Append(name[ix++]);

                return Int32.Parse(number.ToString());
            }
            catch (Exception)
            {
                Logger.Error("could not extract id from: " + name);
                throw;
            }
        }
    }

    public interface IReplicationStorage
    {
        // list of items in storage
        IEnumerable<ReplicatedItem> GetItems();

        void ExportItem(ContentItem item);
        IImportRecord SyncItem(ReplicatedItem item);
        void DeleteItem(ReplicatedItem replicatedItem);
        void DeleteItem(int ID);
        //void WriteFile(string name, string data);
    }
}