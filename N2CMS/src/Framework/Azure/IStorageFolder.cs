using System;

namespace N2.Azure {
    public interface IStorageFolder {
        string GetPath();
        string GetName();
        long GetSize();
        DateTime GetLastUpdated();
        IStorageFolder GetParent();
    }
}