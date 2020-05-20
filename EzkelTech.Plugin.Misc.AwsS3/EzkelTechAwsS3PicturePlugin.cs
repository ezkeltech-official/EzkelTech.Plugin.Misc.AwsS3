using System.Collections.Generic;
using Nop.Core;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;

namespace EzkelTech.Plugin.Misc.AwsS3
{
    public class EzkelTechAwsS3PicturePlugin : BasePlugin, IMiscPlugin
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public EzkelTechAwsS3PicturePlugin(ILocalizationService localizationService,
            ISettingService settingService,
            IWebHelper webHelper)
        {
            _localizationService = localizationService;
            _settingService = settingService;
            _webHelper = webHelper;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/EzkelTechAwsS3/Configure";
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {

            //settings
            _settingService.SaveSetting(new EzkelTechAwsS3Settings
            {
                ObjectUrlTemplate = EzkelTechAwsS3Settings.DefaultObjectUrlTemplate
            });

            //locales
            _localizationService.AddPluginLocaleResource(new Dictionary<string, string>
            {
                ["EzkelTech.Plugin.Misc.AwsS3.Region"] = "AWS S3 Region",
                ["EzkelTech.Plugin.Misc.AwsS3.Region.Hint"] = "Enter your AWS S3 region.",

                ["EzkelTech.Plugin.Misc.AwsS3.Bucket"] = "AWS S3 Bucket",
                ["EzkelTech.Plugin.Misc.AwsS3.Bucket.Hint"] = "Enter your AWS S3 bucket path.",

                ["EzkelTech.Plugin.Misc.AwsS3.SecretAccessKey"] = "AWS S3 Secret Access Key",
                ["EzkelTech.Plugin.Misc.AwsS3.SecretAccessKey.Hint"] = "Enter your secret access key.",

                ["EzkelTech.Plugin.Misc.AwsS3.AccessKeyId"] = "AWS S3 Access ID",
                ["EzkelTech.Plugin.Misc.AwsS3.AccessKeyId.Hint"] = "Enter your access key.",

                ["EzkelTech.Plugin.Misc.AwsS3.ObjectUrlTemplate"] = "AWS S3 Object Url Template",
                ["EzkelTech.Plugin.Misc.AwsS3.ObjectUrlTemplate.Hint"] = "Enter url template link.",
            });

            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<EzkelTechAwsS3Settings>();

            //locales
            _localizationService.DeletePluginLocaleResources("EzkelTech.Plugin.Misc.EzkelTech");

            base.Uninstall();
        }

        #endregion
    }
}