#region License Header
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
namespace ProCenter.Domain.CommonModule
{
    #region Using Statements

    using System;
    using Lookups;

    #endregion

    /// <summary>
    ///     Attribute for defining registration lookup type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class LookupRegistration : Attribute
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="LookupRegistration" /> class.
        /// </summary>
        /// <param name="lookupType">Type of the lookup.</param>
        /// <exception cref="System.InvalidOperationException">Cannot use type that does not inherit from type Lookup.</exception>
        public LookupRegistration ( Type lookupType )
        {
            if ( !typeof(Lookup).IsAssignableFrom ( lookupType ) )
            {
                throw new InvalidOperationException ( string.Format ( "Cannot use type {0} because it does not inherit from type Lookup.", lookupType ) );
            }
            LookupType = lookupType;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the type of the lookup.
        /// </summary>
        /// <value>
        ///     The type of the lookup.
        /// </value>
        public Type LookupType { get; private set; }

        #endregion
    }
}