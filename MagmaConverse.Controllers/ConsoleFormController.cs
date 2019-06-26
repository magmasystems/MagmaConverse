using System;
using MagmaConverse.Data;
using MagmaConverse.Models;
using MagmaConverse.Views;
using MagmaConverse.Views.Console;

namespace MagmaConverse.Controllers
{
    public class ConsoleFormController : ISBSFormController
    {
        public event Action<ISBSForm> FormClosed = form => { };

        public SBSFormModel Model { get; }
        public ISBSForm Form { get; }
        public ISBSFormView View { get; }

        public ConsoleFormController(string idForm)
        {
            this.Model = SBSFormModel.Instance;
            this.Form = this.Model.GetFormInstance(idForm);
            this.View = new ConsoleFormView(this.Form);

            this.View.Closed += () =>
            {
                this.FormClosed(this.Form);
            };

            this.View.FormFieldChanged += (field, value) =>
            {
            };
        }

        public void Run()
        {
            this.View.Render();
        }
    }
}
