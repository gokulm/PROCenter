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

namespace ProCenter.Mvc.Infrastructure.Security
{
    #region Using Statements

    using System;
    using System.Linq;
    using System.Security.Claims;
    using System.Text.RegularExpressions;
    using System.Web;
    using Common;
    using Domain.SecurityModule;
    using NLog;
    using Pillar.Common.InversionOfControl;
    using ProCenter.Infrastructure;

    #endregion

    /// <summary>The claims authentication manager class.</summary>
    public class ClaimsAuthenticationManager : System.Security.Claims.ClaimsAuthenticationManager
    {
        #region Static Fields

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger ();

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Authenticates a specified resource by its name.
        /// </summary>
        /// <param name="resourceName">
        ///     Name of the resource.
        /// </param>
        /// <param name="claimsPrincipal">
        ///     The claims principal.
        /// </param>
        /// <returns>
        ///     Returns claims principal for given resource.
        /// </returns>
        public override ClaimsPrincipal Authenticate ( string resourceName, ClaimsPrincipal claimsPrincipal )
        {
            if ( claimsPrincipal.Identity.IsAuthenticated )
            {
                var identity = claimsPrincipal.Identity as ClaimsIdentity;

                if ( identity != null )
                {
                    //Note:  NameIdentitider is email by default if it is provided in Identity Server. If email is empty, then nameIdentifier takes username.
                    // This is not the case any more since July 2013 commits
                    var claim = identity.Claims.FirstOrDefault ( c => c.Type == ClaimTypes.NameIdentifier ) ?? identity.Claims.FirstOrDefault ( c => c.Type == ClaimTypes.Email );

                    var nameIdentifier = claim.Value;

                    //make sure nameIdentifier is email address:
                    var regex = new Regex ( @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$" );
                    if ( !regex.IsMatch ( nameIdentifier ) )
                    {
                        nameIdentifier = identity.Claims.First ( c => c.Type == ClaimTypes.Email ).Value;
                    }

                    _logger.Debug ( "Name identifier for authenticated principal ({0}): {1}.", identity.Name, nameIdentifier );

                    _logger.Debug ( "Resolving dependency on ISystemAccountRepository." );
                    var systemAccountRepository = IoC.CurrentContainer.Resolve<ISystemAccountRepository> ();
                    _logger.Debug ( "Resolved dependency on ISystemAccountRepository." );

                    var systemAccount = systemAccountRepository.GetByIdentifier ( nameIdentifier );

                    if ( systemAccount != null )
                    {
                        var shouldLogin = true;
                        if ( systemAccount.IsLocked )
                        {
                            var infoMessage = string.Format ( "System Account {0} attempted to log in when locked.", systemAccount.Identifier );
                            _logger.Info ( infoMessage );

                            var message = string.Format ( "Your account has been permenantly locked please contact your administrator." );
                            HttpContext.Current.AddError ( new UnauthorizedAccessException ( message ) );
                            shouldLogin = false;
                        }
                        else if ( systemAccount.IsTempLocked )
                        {
                            var lockTimeMins = ( DateTime.Now - systemAccount.TempLockedTime.Value ).TotalMinutes;
                            if ( lockTimeMins < 5 )
                            {
                                var infoMessage = string.Format (
                                                                 "System Account {0} attempted to log in when locked.",
                                    systemAccount.Identifier );
                                _logger.Info ( infoMessage );

                                var message =
                                    string.Format (
                                                   "Your account has been temporarily locked please try again in {0} mins. " +
                                                   "If you continue to have issues please contact your administrator.",
                                        5 - Math.Floor ( lockTimeMins ) );
                                HttpContext.Current.AddError ( new UnauthorizedAccessException ( message ) );
                                shouldLogin = false;
                            }
                            else
                            {
                                systemAccount.TemporaryUnLock ();
                                UserContext.Current.RefreshValidationAttempts ();
                                var unitOfWorkProvider = IoC.CurrentContainer.Resolve<IUnitOfWorkProvider> ();
                                unitOfWorkProvider.GetCurrentUnitOfWork ().Commit ();
                            }
                        }
                        if ( shouldLogin )
                        {
                            _logger.Debug ( "Resolving dependency on {0}.", typeof(IPermissionClaimsManager).Name );
                            var permissionClaimsManager = IoC.CurrentContainer.Resolve<IPermissionClaimsManager> ();
                            _logger.Debug ( "Resolved dependency on {0}.", typeof(IPermissionClaimsManager).Name );

                            _logger.Debug ( "Issue more claims for ({0} ({1}))", systemAccount.Identifier, systemAccount.Email.Address );
                            permissionClaimsManager.IssueSystemPermissionClaims ( claimsPrincipal, systemAccount );
                            permissionClaimsManager.IssueAccountClaims ( claimsPrincipal, systemAccount );
                        }
                    }
                    else
                    {
                        var errorMessage = string.Format (
                                                          "Authenticated principal ({0}) with identifier ({1}) does not have a system account in PRO Center.",
                            identity.Name,
                            nameIdentifier );
                        _logger.Error ( errorMessage );
                    }
                }
            }
            else
            {
                _logger.Debug ( "Incoming IClaimsPrincipal was not authenticated. WIF will redirect the request to the identity server." );
            }

            return claimsPrincipal;
        }

        #endregion
    }
}