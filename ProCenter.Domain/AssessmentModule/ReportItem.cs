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
namespace ProCenter.Domain.AssessmentModule
{
    #region Using Statements

    using System;
    using System.Collections.Generic;

    #endregion

    /// <summary>
    ///     A Report Item
    /// </summary>
    public class ReportItem : IEquatable<ReportItem>
    {
        public bool Equals ( ReportItem other )
        {
            if ( ReferenceEquals ( null, other ) )
                return false;
            if ( ReferenceEquals ( this, other ) )
                return true;
            return string.Equals ( Text, other.Text ) && string.Equals ( Name, other.Name ) && ShouldShow.Equals ( other.ShouldShow );
        }

        public override bool Equals ( object obj )
        {
            if ( ReferenceEquals ( null, obj ) )
                return false;
            if ( ReferenceEquals ( this, obj ) )
                return true;
            if ( obj.GetType () != this.GetType () )
                return false;
            return Equals ( (ReportItem) obj );
        }

        public override int GetHashCode ()
        {
            unchecked
            {
                int hashCode = ( Text != null ? Text.GetHashCode () : 0 );
                hashCode = ( hashCode * 397 ) ^ ( Name != null ? Name.GetHashCode () : 0 );
                hashCode = ( hashCode * 397 ) ^ ShouldShow.GetHashCode ();
                return hashCode;
            }
        }

        public static bool operator == ( ReportItem left, ReportItem right )
        {
            return Equals ( left, right );
        }

        public static bool operator != ( ReportItem left, ReportItem right )
        {
            return !Equals ( left, right );
        }

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ReportItem" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="shouldShow">The should show.</param>
        /// <param name="reportItems">The report items.</param>
        public ReportItem ( string name, bool? shouldShow = null, IEnumerable<object> formatParameters = null, params ReportItem[] reportItems )
        {
            Name = name;
            ShouldShow = shouldShow;
            EditableReportItems = reportItems;
            FormatParameters = formatParameters ?? new List<object> ();
            ItemMetadata = new ItemMetadata ();
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the item metadata.
        /// </summary>
        /// <value>
        ///     The item metadata.
        /// </value>
        public ItemMetadata ItemMetadata { get; set; }

        /// <summary>
        ///     Gets the name.
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        public string Name { get; private set; }

        /// <summary>
        ///     Gets the report items.
        /// </summary>
        /// <value>
        ///     The report items.
        /// </value>
        public IEnumerable<ReportItem> ReportItems
        {
            get { return EditableReportItems; }
        }

        /// <summary>
        ///     Gets the should show.
        /// </summary>
        /// <value>
        ///     The should show.
        /// </value>
        public bool? ShouldShow { get; private set; }

        /// <summary>
        ///     Gets the text.
        /// </summary>
        /// <value>
        ///     The text.
        /// </value>
        public string Text { get; private set; }

        /// <summary>
        ///     Gets or sets the editable report items.
        /// </summary>
        /// <value>
        ///     The editable report items.
        /// </value>
        internal IList<ReportItem> EditableReportItems { get; set; }

        /// <summary>
        /// Gets the format parameters.
        /// </summary>
        /// <value>
        /// The format parameters.
        /// </value>
        public IEnumerable<object> FormatParameters { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Updates the specified values.
        /// </summary>
        /// <param name="shouldShow">The should show.</param>
        /// <param name="text">The text.</param>
        public void Update (bool? shouldShow, string text )
        {
            ShouldShow = shouldShow;
            Text = text;
        }

        #endregion
    }
}