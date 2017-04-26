using Microsoft.CoreWf;
using Microsoft.CoreWf.Tracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eric.coreminimal.wf
{
    public class WorkflowAppRunner
    {
        public void ExecuteWorkflowHost(Guid Id, long ScheduleId, Activity act, Dictionary<string, object> ArgumentsObject)
        {
            //Tracking Participant
            WorkflowTracking track = new WorkflowTracking();
            track.WFIdentifier = Id;
            track.JobId = ScheduleId;
            track.WorkflowName = "WorkflowName";
            //Tracking Profile
            TrackingProfile prof = new TrackingProfile();
            prof.Name = "CustomTrackingProfile";
            prof.Queries.Add(new WorkflowInstanceQuery { States = { "*" } });
            prof.Queries.Add(new ActivityStateQuery { States = { "*" }, Arguments = { "*" } });
            prof.Queries.Add(new CustomTrackingQuery() { Name = "*", ActivityName = "*" });
            prof.ImplementationVisibility = ImplementationVisibility.RootScope;
            track.TrackingProfile = prof;

            WorkflowApplication host = new WorkflowApplication(act, ArgumentsObject);
            //add extensions
            host.Extensions.Add(new EmailSetting());

            host.Extensions.Add(track);
            //add behaviour at completion
            host.Completed = WorkflowCompletedAction;
            try
            {
                host.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString()); 
            }
        }

        private void WorkflowCompletedAction(WorkflowApplicationCompletedEventArgs e)
        {
            if (e.CompletionState == ActivityInstanceState.Faulted)
            {
                Console.WriteLine("Workflow {0} Terminated.", e.InstanceId);
                Console.WriteLine("Exception: {0}\n{1}",
                    e.TerminationException.GetType().FullName,
                    e.TerminationException.Message);
            }
            else if (e.CompletionState == ActivityInstanceState.Canceled)
            {
                Console.WriteLine("Workflow {0} Canceled.", e.InstanceId);
            }
            else
            {
                Console.WriteLine("Workflow {0} Completed.", e.InstanceId);

                // Outputs can be retrieved from the Outputs dictionary, 
                // keyed by argument name.
                //Console.WriteLine("{0} / {1} = {2} Remainder {3}",
                //    dividend, divisor, e.Outputs["Result"], e.Outputs["Remainder"]);

                if (e.Outputs.Count > 0)
                {
                    Console.WriteLine("OutArguments:");
                    foreach (var item in e.Outputs)
                    {
                        Console.WriteLine("\t{0} = {1}", item.Key, item.Value.ToString());
                    }
                }
            }
        }
    }
}
