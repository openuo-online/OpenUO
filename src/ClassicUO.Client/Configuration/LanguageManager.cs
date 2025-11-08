using System;
using System.Globalization;
using System.IO;
using ClassicUO.Utility.Logging;

namespace ClassicUO.Configuration
{
    /// <summary>
    /// 语言管理器 - 负责管理和切换游戏语言
    /// </summary>
    public static class LanguageManager
    {
        private static string _currentLanguage = "en";
        
        /// <summary>
        /// 当前语言代码 (例如: "en", "zh-CN")
        /// </summary>
        public static string CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                if (_currentLanguage != value)
                {
                    _currentLanguage = value;
                    ApplyLanguage(value);
                }
            }
        }
        
        /// <summary>
        /// 初始化语言系统
        /// </summary>
        public static void Initialize()
        {
            // 直接使用系统语言检测，因为 Settings.Language 用于其他目的
            string detectedLanguage = DetectSystemLanguage();
            CurrentLanguage = detectedLanguage;
            Log.Trace($"Language system initialized: {detectedLanguage}");
        }
        
        /// <summary>
        /// 应用指定的语言
        /// </summary>
        private static void ApplyLanguage(string languageCode)
        {
            try
            {
                CultureInfo culture;
                
                switch (languageCode.ToLower())
                {
                    case "zh-cn":
                    case "zh":
                    case "chinese":
                        culture = new CultureInfo("zh-CN");
                        break;
                        
                    case "en":
                    case "english":
                    default:
                        culture = new CultureInfo("en");
                        break;
                }
                
                // 设置当前线程的文化信息
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;
                
                // 设置默认线程的文化（用于新线程）
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;
                
                // 设置资源文件的文化信息
                ClassicUO.Resources.ResGumps.Culture = culture;
                ClassicUO.Resources.ResGeneral.Culture = culture;
                ClassicUO.Resources.ResErrorMessages.Culture = culture;
                
                // 重新加载Language.json
                Language.Load();
                
                Log.Trace($"Language changed to: {culture.Name}");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to apply language '{languageCode}': {ex.Message}");
            }
        }
        
        /// <summary>
        /// 检测系统语言
        /// </summary>
        private static string DetectSystemLanguage()
        {
            try
            {
                var systemCulture = CultureInfo.CurrentUICulture;
                
                // 检查是否是中文
                if (systemCulture.TwoLetterISOLanguageName == "zh")
                {
                    return "zh-CN";
                }
                
                // 默认返回英语
                return "en";
            }
            catch
            {
                return "en";
            }
        }
        
        /// <summary>
        /// 获取可用的语言列表
        /// </summary>
        public static string[] GetAvailableLanguages()
        {
            return new[]
            {
                "en",      // English
                "zh-CN"    // 简体中文
            };
        }
        
        /// <summary>
        /// 获取语言的显示名称
        /// </summary>
        public static string GetLanguageDisplayName(string languageCode)
        {
            return languageCode.ToLower() switch
            {
                "en" => "English",
                "zh-cn" => "简体中文",
                _ => languageCode
            };
        }
    }
}
