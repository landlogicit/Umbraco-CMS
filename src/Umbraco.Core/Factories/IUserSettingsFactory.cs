using Umbraco.Cms.Core.Models.Installer;

namespace Umbraco.Cms.Core.Factories;

public interface IUserSettingsFactory
{
    UserSettingsModel GetUserSettings();
}
