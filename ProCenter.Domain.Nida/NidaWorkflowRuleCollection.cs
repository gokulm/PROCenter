﻿#region License Header

// /*******************************************************************************
//  * Open Behavioral Health Information Technology Architecture (OBHITA.org)
//  * 
//  * Redistribution and use in source and binary forms, with or without
//  * modification, are permitted provided that the following conditions are met:
//  *     * Redistributions of source code must retain the above copyright
//  *       notice, this list of conditions and the following disclaimer.
//  *     * Redistributions in binary form must reproduce the above copyright
//  *       notice, this list of conditions and the following disclaimer in the
//  *       documentation and/or other materials provided with the distribution.
//  *     * Neither the name of the <organization> nor the
//  *       names of its contributors may be used to endorse or promote products
//  *       derived from this software without specific prior written permission.
//  * 
//  * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//  * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//  * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//  * DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
//  * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//  * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//  * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//  * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//  * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//  * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//  ******************************************************************************/

#endregion

namespace ProCenter.Domain.Nida
{
    #region Using Statements

    using System;
    using System.Drawing;
    using System.Reflection;

    using Pillar.FluentRuleEngine;

    using ProCenter.Common;
    using ProCenter.Domain.AssessmentModule;
    using ProCenter.Domain.AssessmentModule.Attributes;
    using ProCenter.Domain.MessageModule;

    #endregion

    /// <summary>The nida workflow rule collection class.</summary>
    public class NidaWorkflowRuleCollection : AbstractRuleCollection<AssessmentInstance>
    {
        #region Fields

        private readonly IResourcesManager _resourcesManager;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NidaWorkflowRuleCollection"/> class.
        /// </summary>
        /// <param name="assessmentDefinitionRepository">The assessment definition repository.</param>
        /// <param name="repository">The repository.</param>
        /// <param name="assessmentInstanceRepository">The assessment instance repository.</param>
        /// <param name="resourcesManager">The resources manager.</param>
        public NidaWorkflowRuleCollection (
            IAssessmentDefinitionRepository assessmentDefinitionRepository,
            IWorkflowMessageRepository repository,
            IAssessmentInstanceRepository assessmentInstanceRepository,
            IResourcesManager resourcesManager = null )
        {
            _resourcesManager = resourcesManager;
            NewRule ( () => ShouldRecommendDastRule ).When ( assessment => (int)assessment.Score.Value > 0 )
                .Then (
                       ( assessment, ctx ) =>
                       {
                           var messageReporter =
                               ctx.WorkingMemory.GetContextObject<IMessageCollector> ( "MessageCollector" );
                           var assessmentDefinitionKey =
                               assessmentDefinitionRepository.GetKeyByCode ( DrugAbuseScreeningTest.AssessmentCodedConcept.Code );
                           WorkflowMessage message = null;
                           if ( assessment.WorkflowKey.HasValue )
                           {
                               message = repository.GetByKey ( assessment.WorkflowKey.Value );
                           }
                           message = message ?? new WorkflowMessage (
                               assessment.PatientKey,
                               assessment.Key,
                               NidaSingleQuestionScreener.AssessmentCodedConcept.Code,
                               assessmentDefinitionKey,
                               DrugAbuseScreeningTest.AssessmentCodedConcept.Code,
                               assessment.Score );
                           if ( assessment.CanSelfAdminister )
                           {
                               message.AllowSelfAdministration ();
                           }
                           assessment.AddToWorkflow ( message.Key );
                           messageReporter.AddMessage ( message );
                       } )
                .ElseThen (
                           ( assessment, ctx ) =>
                           {
                               WorkflowMessage message = null;
                               if ( assessment.WorkflowKey.HasValue )
                               {
                                   message = repository.GetByKey ( assessment.WorkflowKey.Value );
                               }
                               message = message ?? new WorkflowMessage (
                                   assessment.PatientKey,
                                   assessment.Key,
                                   NidaSingleQuestionScreener.AssessmentCodedConcept.Code,
                                   Guid.Empty,
                                   null,
                                   assessment.Score );

                               if ( assessment.CanSelfAdminister )
                               {
                                   message.AllowSelfAdministration ();
                               }

                               message.Complete ( NidaPatientSummaryReportModelBuilder.GetGreenReportModel () );
                           } );

            NewRuleSet ( () => NidaSingleQuestionScreenerRuleSet, ShouldRecommendDastRule );

            NewRule ( () => ShouldRecommendNidaAssessFurtherRule ).When ( assessment => assessment.WorkflowKey.HasValue )
                .Then (
                       ( assessment, ctx ) =>
                       {
                           var messageReporter =
                               ctx.WorkingMemory
                                   .GetContextObject<IMessageCollector> ( "MessageCollector" );
                           var assessmentDefinitionKey =
                               assessmentDefinitionRepository.GetKeyByCode ( NidaAssessFurther.AssessmentCodedConcept.Code );
                           var message = repository.GetByKey ( assessment.WorkflowKey.Value );
                           message.Advance (
                                            assessment.Key,
                               DrugAbuseScreeningTest.AssessmentCodedConcept.Code,
                               assessmentDefinitionKey,
                               NidaAssessFurther.AssessmentCodedConcept.Code,
                               assessment.Score );
                           messageReporter.AddMessage ( message );
                       } );

            NewRuleSet ( () => DrugAbuseScreeningTestRuleSet, ShouldRecommendNidaAssessFurtherRule );

            NewRule ( () => ShouldCompleteWorkflowStatusRule ).When ( assessment => assessment.WorkflowKey.HasValue )
                .Then (
                       ( assessment, ctx ) =>
                       {
                           var message = repository.GetByKey ( assessment.WorkflowKey.Value );
                           var dastKey = message.GetAssessmentKeyforCodeInWorkflow ( DrugAbuseScreeningTest.AssessmentCodedConcept.Code );
                           var dastInstance = assessmentInstanceRepository.GetByKey ( dastKey.Value );
                           ReportModel reportModel;
                           if ( ( (long)dastInstance.Score.Value ) <= 2 && !( ((NidaAssessFurtherScoring)assessment.Score.Value).TotalScore ) )
                           {
                               reportModel = NidaPatientSummaryReportModelBuilder.GetAmberReportModel (
                                                                                                       _resourcesManager,
                                   dastInstance,
                                   assessment );
                           }
                           else
                           {
                               reportModel = NidaPatientSummaryReportModelBuilder.GetRedReportModel (
                                                                                                     _resourcesManager,
                                   dastInstance,
                                   assessment );
                           }
                           message.Complete ( reportModel );
                       } );
            NewRuleSet ( () => NidaAssessFurtherRuleSet, ShouldCompleteWorkflowStatusRule );
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the drug abuse screening test rule set.
        /// </summary>
        /// <value>
        /// The drug abuse screening test rule set.
        /// </value>
        public IRuleSet DrugAbuseScreeningTestRuleSet { get; private set; }

        /// <summary>
        /// Gets the nida assess further rule set.
        /// </summary>
        /// <value>
        /// The nida assess further rule set.
        /// </value>
        public IRuleSet NidaAssessFurtherRuleSet { get; private set; }

        /// <summary>
        /// Gets the nida single question screener rule set.
        /// </summary>
        /// <value>
        /// The nida single question screener rule set.
        /// </value>
        public IRuleSet NidaSingleQuestionScreenerRuleSet { get; private set; }

        /// <summary>
        /// Gets or sets the should complete workflow status rule.
        /// </summary>
        /// <value>
        /// The should complete workflow status rule.
        /// </value>
        public IRule ShouldCompleteWorkflowStatusRule { get; set; }

        /// <summary>
        /// Gets or sets the should recommend dast rule.
        /// </summary>
        /// <value>
        /// The should recommend dast rule.
        /// </value>
        public IRule ShouldRecommendDastRule { get; set; }

        /// <summary>
        /// Gets or sets the should recommend nida assess further rule.
        /// </summary>
        /// <value>
        /// The should recommend nida assess further rule.
        /// </value>
        public IRule ShouldRecommendNidaAssessFurtherRule { get; set; }

        #endregion
    }
}