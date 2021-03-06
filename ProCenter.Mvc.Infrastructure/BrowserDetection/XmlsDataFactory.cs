﻿// /*******************************************************************************
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

namespace ProCenter.Mvc.Infrastructure.BrowserDetection
{
    #region Using Statements

    using System;
    using System.Configuration;
    using System.IO;
    using System.Reflection;

    #endregion

    /// <summary>
    ///     This is a factory class to get and set the XML data or XML data file path.
    /// </summary>
    public class XmlsDataFactory
    {
        #region Static Fields

        /// <summary>
        ///     The _path.
        /// </summary>
        private static readonly string _path = ConfigurationManager.AppSettings["SupportedBrowserPath"];

        /// <summary>
        ///     The _m XML data.
        /// </summary>
        private static string _mXmlData;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets a value indicating whether this instance is XML data set.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is XML data set; otherwise, <c>false</c>.
        /// </value>
        public static bool IsXmlDataSet
        {
            get { return !string.IsNullOrWhiteSpace ( _mXmlData ); }
        }

        /// <summary>
        ///     Gets the XML data.
        /// </summary>
        /// <value>
        ///     The XML data.
        /// </value>
        public static string XmlData
        {
            get { return !string.IsNullOrWhiteSpace ( _mXmlData ) ? _mXmlData : SetXmlFilePath (); }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Sets the XML data.
        /// </summary>
        /// <param name="xmlData">The XML data.</param>
        public static void SetXmlData ( string xmlData )
        {
            _mXmlData = xmlData;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Sets the XML file path.
        /// </summary>
        /// <returns>Will return the path and file name of the XML file to load or an empty string if the basePath is null.</returns>
        private static string SetXmlFilePath ()
        {
            var basePath = Path.GetDirectoryName ( ( new Uri ( Assembly.GetExecutingAssembly ().CodeBase ) ).LocalPath );
            return basePath != null ? Path.Combine ( basePath, _path ) : string.Empty;
        }

        #endregion
    }
}