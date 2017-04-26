using Microsoft.CoreWf.Tracking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace eric.coreminimal.wf
{
    public class WorkflowTracking : TrackingParticipant, IWfInfoExtension
    {
        public string WorkflowName { get; set; }
        public long JobId { get; set; }
        public Guid WFIdentifier { get; set; }

        public WorkflowTracking()
        {
            JobId = -1;
        }

        protected override void Track(TrackingRecord record, TimeSpan timeout)
        {
            WorkflowRecord message = new WorkflowRecord();

            message.WFId = WFIdentifier;
            message.JobId = JobId;

            message.EventId = (long)record.RecordNumber;
            message.EventTime = record.EventTime.ToLocalTime();
            message.RunId = record.InstanceId;

            WorkflowInstanceRecord wfRecord = record as WorkflowInstanceRecord;
            if (wfRecord != null)
            {
                message.State = wfRecord.State;
                message.Name = WorkflowName;

                if (wfRecord.State == WorkflowInstanceStates.Idle) return; //do not track idle status
            }

            ActivityStateRecord actRecord = record as ActivityStateRecord;
            if (actRecord != null)
            {
                message.State = actRecord.State;
                message.Name = actRecord.Activity.Name;
                if (actRecord.Activity.Name.Equals("DynamicActivity"))
                {
                    return;
                }

                if (actRecord.State == ActivityStates.Executing && actRecord.Activity.TypeName == "System.Activities.Statements.WriteLine")
                {
                    using (StringWriter writer = new StringWriter())
                    {
                        writer.Write(actRecord.Arguments["Text"]);
                        message.Message = writer.ToString();
                    }
                }
            }

            WorkflowInstanceUnhandledExceptionRecord exRecord = record as WorkflowInstanceUnhandledExceptionRecord;
            if (exRecord != null)
            {
                message.State = exRecord.State;
                message.Name = WorkflowName;
                message.Message = exRecord.UnhandledException.Message;
            }

            CustomTrackingRecord cuRecord = record as CustomTrackingRecord;
            if (cuRecord != null)
            {
                message.Name = cuRecord.Activity.Name;
                message.State = cuRecord.Name;
                message.Message = cuRecord.Data["Message"] as string;
            }

            message.Print();
            /*using (hermes = new HermesOracleDBStore().GetDbIO())
            {
                hermes.InsertEvent(message);
            }*/
        }
    }
}
