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

namespace ProCenter.Service.Handler.Security
{
    #region Using Statements

    using System.Linq;
    using Common;
    using Domain.SecurityModule;
    using global::AutoMapper;
    using Service.Message.Common;
    using Service.Message.Security;

    #endregion

    /// <summary>The get role dto by key request handler class.</summary>
    public class GetRoleDtoByKeyRequestHandler : ServiceRequestHandler<GetRoleDtoByKeyRequest, DtoResponse<RoleDto>>
    {
        #region Fields

        private readonly IRoleRepository _roleRepository;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GetRoleDtoByKeyRequestHandler"/> class.
        /// </summary>
        /// <param name="roleRepository">The role repository.</param>
        public GetRoleDtoByKeyRequestHandler ( IRoleRepository roleRepository )
        {
            _roleRepository = roleRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handles the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="response">The response.</param>
        protected override void Handle ( GetRoleDtoByKeyRequest request, DtoResponse<RoleDto> response )
        {
            var role = _roleRepository.GetByKey ( request.Key );
            if ( role != null )
            {
                var roleDto = Mapper.Map<Role, RoleDto> ( role );
                roleDto.Permissions = roleDto.Permissions.OrderBy ( p => p );
                response.DataTransferObject = roleDto;
            }
        }

        #endregion
    }
}