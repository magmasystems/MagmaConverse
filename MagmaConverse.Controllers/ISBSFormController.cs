using MagmaConverse.Data;
using MagmaConverse.Models;
using MagmaConverse.Views;

namespace MagmaConverse.Controllers
{
    public interface ISBSFormController
    {
        SBSFormModel Model { get; }
        ISBSFormView View { get; }
        ISBSForm Form { get; }
    }
}
