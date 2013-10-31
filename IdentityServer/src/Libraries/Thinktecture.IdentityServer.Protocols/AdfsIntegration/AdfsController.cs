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
using System;
using System.ComponentModel.Composition;
using System.IdentityModel.Tokens;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Thinktecture.IdentityModel.Constants;
using Thinktecture.IdentityModel.Extensions;
using Thinktecture.IdentityServer.Protocols.OAuth2;
using Thinktecture.IdentityServer.Repositories;

namespace Thinktecture.IdentityServer.Protocols.AdfsIntegration
{
    public class AdfsController : ApiController
    {
        [Import]
        public IConfigurationRepository ConfigurationRepository { get; set; }

        public AdfsController()
        {
            Container.Current.SatisfyImportsOnce(this);
        }

        public AdfsController(IConfigurationRepository configurationRepository)
        {
            ConfigurationRepository = ConfigurationRepository;
        }

        public HttpResponseMessage Post(TokenRequest request)
        {
            if (request == null)
            {
                return OAuthErrorResponseMessage(OAuth2Constants.Errors.InvalidRequest);
            }

            Uri uri;
            if (string.IsNullOrEmpty(request.Scope) ||
                !Uri.TryCreate(request.Scope, UriKind.Absolute, out uri))
            {
                Tracing.Error("Starting ADFS integration request with invalid scope: " + request.Scope);
                return OAuthErrorResponseMessage(OAuth2Constants.Errors.InvalidScope);
            }

            if (request.Grant_Type.Equals(OAuth2Constants.GrantTypes.Password) &&
                ConfigurationRepository.AdfsIntegration.UsernameAuthenticationEnabled)
            {
                return ProcessUserNameRequest(request);
            }

            // federation via SAML
            if (request.Grant_Type.Equals(OAuth2Constants.GrantTypes.Saml2) &&
                ConfigurationRepository.AdfsIntegration.SamlAuthenticationEnabled)
            {
                return ProcessSamlRequest(request);
            }

            // federation via JWT
            if (request.Grant_Type.Equals(OAuth2Constants.GrantTypes.JWT) &&
                ConfigurationRepository.AdfsIntegration.JwtAuthenticationEnabled)
            {
                return ProcessJwtRequest(request);
            }

            Tracing.Error("Unsupported grant type: " + request.Grant_Type);
            return OAuthErrorResponseMessage(OAuth2Constants.Errors.UnsupportedGrantType);
        }

        #region JWT
        private HttpResponseMessage ProcessJwtRequest(TokenRequest request)
        {
            if (string.IsNullOrEmpty(request.Assertion))
            {
                Tracing.Error("ADFS integration authentication request (JWT) with empty assertion");
                return OAuthErrorResponseMessage(OAuth2Constants.Errors.InvalidGrant);
            }

            Tracing.Information("Starting ADFS integration authentication request (JWT) for scope: " + request.Scope);

            var bridge = new AdfsBridge(ConfigurationRepository);
            GenericXmlSecurityToken token;
            try
            {
                Tracing.Verbose("Starting ADFS integration authentication request (JWT) for assertion: " + request.Assertion);

                token = bridge.AuthenticateJwt(request.Assertion, request.Scope);
            }
            catch (Exception ex)
            {
                Tracing.Error("Error while communicating with ADFS: " + ex.ToString());
                return OAuthErrorResponseMessage(OAuth2Constants.Errors.InvalidRequest);
            }

            var response = CreateTokenResponse(token, request.Scope);
            Tracing.Verbose("ADFS integration JWT authentication successful");

            return response;
        }
        #endregion

        #region SAML
        private HttpResponseMessage ProcessSamlRequest(TokenRequest request)
        {
            if (string.IsNullOrEmpty(request.Assertion))
            {
                Tracing.Error("ADFS integration SAML authentication request with empty assertion");
                return OAuthErrorResponseMessage(OAuth2Constants.Errors.InvalidGrant);
            }

            // un-base64 saml token string
            string incomingSamlToken;
            try
            {
                incomingSamlToken = Encoding.UTF8.GetString(Convert.FromBase64String(request.Assertion));
            }
            catch
            {
                Tracing.Error("ADFS integration SAML authentication request with malformed SAML assertion: " + request.Assertion);
                return OAuthErrorResponseMessage(OAuth2Constants.Errors.InvalidGrant);
            }

            Tracing.Information("Starting ADFS integration SAML authentication request for scope: " + request.Scope);

            var bridge = new AdfsBridge(ConfigurationRepository);
            GenericXmlSecurityToken token;
            try
            {
                Tracing.Verbose("ADFS integration SAML authentication request for assertion: " + request.Assertion);

                token = bridge.AuthenticateSaml(incomingSamlToken, request.Scope);
            }
            catch (Exception ex)
            {
                Tracing.Error("Error while communicating with ADFS: " + ex.ToString());
                return OAuthErrorResponseMessage(OAuth2Constants.Errors.InvalidRequest);
            }

            var response = CreateTokenResponse(token, request.Scope);
            Tracing.Verbose("ADFS integration SAML authentication request successful");

            return response;
        }
        #endregion

        #region UserName
        private HttpResponseMessage ProcessUserNameRequest(TokenRequest request)
        {
            if (string.IsNullOrEmpty(request.UserName) ||
                string.IsNullOrEmpty(request.Password))
            {
                Tracing.Error("ADFS integration username authentication request with empty username or password");
                return OAuthErrorResponseMessage(OAuth2Constants.Errors.InvalidGrant);
            }

            Tracing.Information("Starting ADFS integration username authentication request for scope: " + request.Scope);

            var bridge = new AdfsBridge(ConfigurationRepository);
            GenericXmlSecurityToken token;
            try
            {
                Tracing.Verbose("ADFS integration username authentication request for user: " + request.UserName);

                token = bridge.AuthenticateUserName(
                    request.UserName, 
                    request.Password, 
                    request.Scope);
            }
            catch (Exception ex)
            {
                Tracing.Error("Error while communicating with ADFS: " + ex.ToString());
                return OAuthErrorResponseMessage(OAuth2Constants.Errors.InvalidRequest);
            }

            var response = CreateTokenResponse(token, request.Scope);
            Tracing.Verbose("ADFS integration username authentication request successful");

            return response;
        }
        #endregion

        private HttpResponseMessage CreateTokenResponse(GenericXmlSecurityToken token, string scope)
        {
            var response = new TokenResponse();

            if (ConfigurationRepository.AdfsIntegration.PassThruAuthenticationToken)
            {
                response.AccessToken = token.TokenXml.OuterXml;
                response.ExpiresIn = (int)(token.ValidTo.Subtract(DateTime.UtcNow).TotalSeconds);
            }
            else
            {
                var bridge = new AdfsBridge(ConfigurationRepository);

                response = bridge.ConvertSamlToJwt(token.ToSecurityToken(), scope);
            }

            return Request.CreateResponse<TokenResponse>(HttpStatusCode.OK, response);
        }

        private HttpResponseMessage OAuthErrorResponseMessage(string error)
        {
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                string.Format("{{ \"{0}\": \"{1}\" }}", OAuth2Constants.Errors.Error, error));
        }
    }
}
