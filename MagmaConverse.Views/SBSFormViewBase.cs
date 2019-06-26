using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MagmaConverse.Data;
using MagmaConverse.Views.Annotations;

namespace MagmaConverse.Views
{
    public abstract class SBSFormViewBase : ISBSFormView
    {
        #region Events
        /// <inheritdoc/>
        public event Action Closed = () => { };

        /// <inheritdoc/>
        public event Action<ISBSFormField, object> FormFieldChanged = (field, value) => { };
        #endregion

        #region Variables
        /// <inheritdoc/>
        public ISBSForm Form { get; protected set; }
        #endregion

        #region Constructors
        protected SBSFormViewBase(ISBSForm form)
        {
            this.Form = form;
        }
        #endregion

        #region Cleanup
        public virtual void Dispose()
        {
        }
        #endregion

        #region Abstract Methods
        /// <inheritdoc/>
        public abstract void Render();

        protected abstract void Bind(ISBSFormField field);
        #endregion

        #region Methods
        protected void FireClosedEvent()
        {
            this.Closed();
        }

        internal void FireFormFieldChanged(IFormFieldView fieldView, object value)
        {
            this.FormFieldChanged(fieldView.SBSFormField, value);
        }
        #endregion

        #region PropertyNotificationChanges
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
