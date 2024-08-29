using SkytearHorde.Business.Middleware;
using Umbraco.Cms.Core.Composing;

namespace SkytearHorde.Business.Startup
{
    public class ViewSessionComponent : IComponent
    {
        private readonly ViewSessionGenerator _viewSessionGenerator;

        public ViewSessionComponent(ViewSessionGenerator viewSessionGenerator)
        {
            _viewSessionGenerator = viewSessionGenerator;
        }

        public void Initialize()
        {
            _viewSessionGenerator.GenerateSalt();
        }

        public void Terminate()
        {

        }
    }
}
