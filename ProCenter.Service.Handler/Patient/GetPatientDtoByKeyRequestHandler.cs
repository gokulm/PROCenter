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
namespace ProCenter.Service.Handler.Patient
{
    #region

    using System;
    using System.Linq;
    using Common;
    using Dapper;
    using Domain.PatientModule;
    using Domain.SecurityModule;
    using Infrastructure.Service.ReadSideService;
    using Service.Message.Patient;
    using Service.Message.Security;
    using global::AutoMapper;

    #endregion

    public class GetPatientDtoByKeyRequestHandler :
        ServiceRequestHandler<GetPatientDtoByKeyRequest, GetPatientDtoResponse>
    {
        private readonly IPatientRepository _patientRepository;
        private readonly ISystemAccountRepository _systemAccountRepository;
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public GetPatientDtoByKeyRequestHandler(IPatientRepository patientRepository, ISystemAccountRepository systemAccountRepository, IDbConnectionFactory dbConnectionFactory)
        {
            _patientRepository = patientRepository;
            _systemAccountRepository = systemAccountRepository;
            _dbConnectionFactory = dbConnectionFactory;
        }

        protected override void Handle(GetPatientDtoByKeyRequest request, GetPatientDtoResponse response)
        {
            var patient = _patientRepository.GetByKey(request.PatientKey);
            var patientDto = Mapper.Map<Patient, PatientDto>(patient);

            //get system account associated with staff
            Guid? systemAccountKey;
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                systemAccountKey =
                    connection.Query<Guid?>("SELECT SystemAccountKey FROM SecurityModule.SystemAccount WHERE PatientKey=@PatientKey", new {request.PatientKey}).FirstOrDefault();
            }
            if (systemAccountKey.HasValue)
            {
                var systemAccount = _systemAccountRepository.GetByKey(systemAccountKey.Value);
                var systemAccountDto = Mapper.Map<SystemAccount, SystemAccountDto>(systemAccount);
                //if (systemAccount.RoleKeys.Any())
                //{
                //    var roleKeys = string.Join(", ", systemAccount.RoleKeys);
                //    roleKeys = "'" + roleKeys.Replace(", ", "', '") + "'";
                //    var query = string.Format("SELECT RoleKey as 'Key', Name FROM SecurityModule.Role WHERE RoleKey IN ({0})", roleKeys);
                //    using (var connection = _dbConnectionFactory.CreateConnection())
                //    {
                //        var roleDtos = connection.Query<RoleDto>(query).OrderBy(r => r.Name);
                //        systemAccountDto.Roles = roleDtos;
                //    }
                //}

                patientDto.SystemAccount = systemAccountDto;
            }
            response.DataTransferObject = patientDto;
        }
    }
}