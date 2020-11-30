using System;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations.Install;
using Umbraco.Web.Install;

namespace Umbraco.Web.Compose
{
    public class UnattendedInstallComponent : IComponent
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRuntimeState _runtimeState;
        private readonly ILogger _logger;
        private readonly DatabaseBuilder _databaseBuilder;

        public UnattendedInstallComponent(IHttpContextAccessor httpContextAccessor, IRuntimeState runtimeState, ILogger logger, DatabaseBuilder databaseBuilder)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _runtimeState = runtimeState ?? throw new ArgumentNullException(nameof(runtimeState));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseBuilder = databaseBuilder ?? throw new ArgumentNullException(nameof(databaseBuilder));
        }

        public void Initialize()
        {
            // check if we are doing an unattended install
            if (_runtimeState.Reason != RuntimeLevelReason.InstallEmptyDatabase) return;

            _logger.Info<UnattendedInstallComponent>("Installing Umbraco.");
            var result = _databaseBuilder.CreateSchemaAndData();
            _logger.Info<UnattendedInstallComponent>("Umbraco Installed.");

            if (result.Success == false)
                throw new InstallException("An error occurred while running the unattended installation.\n" + result.Message);

            // mark application to be restarted
            if (_httpContextAccessor.HttpContext != null)
                _httpContextAccessor.HttpContext.Application["UmbracoRestartRequired"] = true;
        }

        public void Terminate()
        {
        }
    }
}
