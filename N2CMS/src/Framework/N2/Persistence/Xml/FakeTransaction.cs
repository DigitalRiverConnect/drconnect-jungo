using System;

namespace N2.Persistence.Xml
{
	class FakeTransaction : ITransaction
	{
        public event EventHandler Committed;
        public event EventHandler Rollbacked;
        public event EventHandler Disposed;

        public void Commit()
		{
			// dummy 
            if (Committed != null)
                Committed(this, new EventArgs());
		}

		public void Rollback()
		{
            if (Rollbacked != null)
                Rollbacked(this, new EventArgs()); 
            
            // throw new NotImplementedException();
		}

	    public void Dispose()
		{
            if (Disposed != null)
                Disposed(this, new EventArgs());
		}
	}
}
