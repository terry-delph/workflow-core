using System;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using Xunit;
using FluentAssertions;
using WorkflowCore.Testing;

namespace WorkflowCore.IntegrationTests.Scenarios
{
    public class ExceptionPersistenceWorkflow : IWorkflow
    {
        internal static int ExceptionThrownStep = 0;

        public string Id => "ExceptionPersistenceWorkflow";
        public int Version => 1;
        public void Build(IWorkflowBuilder<Object> builder)
        {
            builder
                .StartWith<ExceptionThrownStep>()
                    .OnError(WorkflowErrorHandling.Terminate)
                .Then(context =>
                {
                    ExceptionThrownStep++;
                    return ExecutionResult.Next();
                });
        }
    }

    internal class ExceptionThrownStep : StepBody
    {
        public const string HelpLink = "help link";
        public const string ExceptionMessage = "ExceptionThrownStep is not implemented.";

        public override ExecutionResult Run(IStepExecutionContext context)
        {
            ExceptionPersistenceWorkflow.ExceptionThrownStep++;

            throw new NotImplementedException(ExceptionMessage)
            {
                HelpLink = HelpLink
            };

            return ExecutionResult.Next();
        }
    }

    public class ExceptionPersistenceScenario : WorkflowTest<ExceptionPersistenceWorkflow, Object>
    {   
        public ExceptionPersistenceScenario()
        {
            Setup();
        }

        [Fact]
        public void Scenario()
        {
            ExceptionPersistenceWorkflow.ExceptionThrownStep = 0;

            var workflowId = StartWorkflow(null);
            WaitForWorkflowToComplete(workflowId, TimeSpan.FromSeconds(30));

            GetStatus(workflowId).Should().Be(WorkflowStatus.Terminated);
            UnhandledStepErrors.Count.Should().Be(1);
            ExceptionPersistenceWorkflow.ExceptionThrownStep.Should().Be(1);

            //Get Persisted Errors
            //PersistenceProvider.
            var firstException = UnhandledStepErrors[0].Exception;
            firstException.Should().NotBe(null);
            firstException.Message.Should().Be(ExceptionThrownStep.ExceptionMessage);
            firstException.HelpLink.Should().Be(ExceptionThrownStep.HelpLink);
            firstException.TargetSite.Name.Should().NotBeEmpty();
            firstException.TargetSite.Module.Name.Should().NotBeEmpty();
        }
    }
}
