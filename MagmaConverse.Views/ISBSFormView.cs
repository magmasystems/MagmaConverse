using System;
using System.ComponentModel;
using MagmaConverse.Data;

namespace MagmaConverse.Views
{
    public interface ISBSFormView : IDisposable, INotifyPropertyChanged
    {
        #region Events
        /// <summary>
        /// Fires when the form is closed
        /// </summary>
        event Action Closed;

        /// <summary>
        /// Fires after the user changes the value of a field in the view
        /// </summary>
        event Action<ISBSFormField, object> FormFieldChanged;
        #endregion

        #region Variables
        /// <summary>
        /// The materialized form (model) that this view displays
        /// </summary>
        ISBSForm Form { get; }
        #endregion

        #region Methods
        /// <summary>
        /// Draw the form somewhere
        /// </summary>
        void Render();
        #endregion
    }
}