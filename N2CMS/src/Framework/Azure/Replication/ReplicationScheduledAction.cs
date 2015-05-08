using N2.Plugin.Scheduling;

namespace N2.Azure.Replication
{
    [ScheduleExecution(30, TimeUnit.Seconds)]
    public class ReplicationScheduledAction : ScheduledAction
    {
        private readonly ReplicationManager _replication;
        private readonly bool _isMaster;
        private readonly bool _isSlave;

        public ReplicationScheduledAction(ReplicationManager replication)
        {
            _replication = replication;

            // get config
            _isMaster = replication.IsMaster;
            _isSlave = replication.IsSlave;
        }

        public override void Execute()
        {
            if (_isMaster || _isSlave)
            {
                _replication.Syncronize();
            }
            // TODO remove from Schedule (HOW?)
        }
    }
}