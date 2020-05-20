using Microsoft.AspNetCore.Mvc;
using EzkelTech.Plugin.Misc.AwsS3.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace EzkelTech.Plugin.Misc.AwsS3.Controllers
{
    public class EzkelTechAwsS3Controller : BasePluginController
    {
        #region Fields

        private readonly EzkelTechAwsS3Settings _awsS3Settings;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public EzkelTechAwsS3Controller(EzkelTechAwsS3Settings awsS3Settings,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService)
        {
            _awsS3Settings = awsS3Settings;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
        }

        #endregion

        #region Methods

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageExternalAuthenticationMethods))
                return AccessDeniedView();

            var model = new ConfigurationModel
            {
                AccessKeyId = _awsS3Settings.AccessKeyId,
                Bucket = _awsS3Settings.Bucket,
                Region = _awsS3Settings.Region,
                SecretAccessKey = _awsS3Settings.SecretAccessKey,
                ObjectUrlTemplate = string.IsNullOrEmpty(_awsS3Settings.ObjectUrlTemplate)
                                ? EzkelTechAwsS3Settings.DefaultObjectUrlTemplate
                                : _awsS3Settings.ObjectUrlTemplate
            };

            return View("~/Plugins/EzkelTech.Plugin.Misc.AwsS3/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageExternalAuthenticationMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            //save settings
            _awsS3Settings.AccessKeyId = model.AccessKeyId;
            _awsS3Settings.Bucket = model.Bucket;
            _awsS3Settings.Region = model.Region;
            _awsS3Settings.SecretAccessKey = model.SecretAccessKey;
            _awsS3Settings.ObjectUrlTemplate = model.ObjectUrlTemplate;

            _settingService.SaveSetting(_awsS3Settings);

             _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }
        #endregion
    }
}