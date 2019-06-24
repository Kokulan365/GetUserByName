namespace KED365.Workflows
{
    using System;
    using System.Activities;
    using System.ServiceModel;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Workflow;
    using Microsoft.Xrm.Sdk.Query;
    using System.Linq;

    public sealed class GetUserByFullName : WorkFlowActivityBase
    {
        [Input("User Full Name")]
        public InArgument<string> UserFullName { get; set; }

        [Output("Prepared By")]
        [ReferenceTarget("systemuser")]
        public OutArgument<EntityReference> PreparedBy { get; set; }


        [Output("IsSuccess")]
        public OutArgument<bool> IsSuccess { get; set; }

        [Output("Message")]
        public OutArgument<string> Message { get; set; }

        protected override void Execute(CodeActivityContext activityContext, IWorkflowContext workflowContext, IOrganizationService CrmService, ITracingService trace)
        {
            try
            {
                string userName = UserFullName.Get(activityContext);

                if (string.IsNullOrWhiteSpace(userName))
                {
                    IsSuccess.Set(activityContext, false);
                    Message.Set(activityContext, "User's Full Name  is not provided");
                    return;
                }

                var QEsystemuser = new QueryExpression("systemuser");
                QEsystemuser.ColumnSet.AddColumns("fullname");
                var QEsystemuser_Criteria_0 = new FilterExpression();
                QEsystemuser.Criteria.AddFilter(QEsystemuser_Criteria_0);
                QEsystemuser_Criteria_0.FilterOperator = LogicalOperator.Or;
                QEsystemuser_Criteria_0.AddCondition("fullname", ConditionOperator.Equal, userName);
                QEsystemuser_Criteria_0.AddCondition("fullname", ConditionOperator.Equal, ChangeNameOrder(userName));
                var results = CrmService.RetrieveMultiple(QEsystemuser);

                if (results == null || !results.Entities.Any())
                {
                    IsSuccess.Set(activityContext, false);
                    Message.Set(activityContext, "User with " + userName + " not found") ;
                    return;
                }

                if (results.Entities.Count > 1)
                {
                    IsSuccess.Set(activityContext, false);
                    Message.Set(activityContext, "Multiple users found with same name : " + userName);
                    return;
                }


                IsSuccess.Set(activityContext, true);
                PreparedBy.Set(activityContext, results.Entities.Single().ToEntityReference());
            }
            catch (Exception ex)
            {
                IsSuccess.Set(activityContext, false);
                Message.Set(activityContext, "An error occurred trying to find user : " + ex.Message);
            }
        }

        private string ChangeNameOrder(string name)
        {
            string[] nameParts = name.Split(' ');
            return nameParts[1] + ", " + nameParts[0];  // If you do not need the comma you can remove 
        }

    }
}