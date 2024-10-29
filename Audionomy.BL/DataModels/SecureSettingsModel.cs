namespace Audionomy.BL.DataModels
{
    public class SecureSettingsModel
    {
        public string AzureSpeechServiceKey { get; set; } = string.Empty;

        public string AzureSpeechServiceLocation { get; set; } = string.Empty;

        public List<string> AzureSpeechServiceLanguageSupport { get; } = [
                "af-ZA","am-ET","ar-AE","ar-BH","ar-DZ","ar-EG","ar-IL","ar-IQ","ar-JO","ar-KW","ar-LB","ar-LY","ar-MA","ar-OM","ar-PS","ar-QA","ar-SA","ar-SY"
            ,"ar-TN","ar-YE","az-AZ","bg-BG","bn-IN","bs-BA","ca-ES","cs-CZ","cy-GB","da-DK","de-AT","de-CH","de-DE","el-GR","en-AU","en-CA","en-GB","en-GH"
            ,"en-HK","en-IE","en-IN","en-KE","en-NG","en-NZ","en-PH","en-SG","en-TZ","en-US","en-ZA","es-AR","es-BO","es-CL","es-CO","es-CR","es-CU","es-DO"
            ,"es-EC","es-ES","es-GQ","es-GT","es-HN","es-MX","es-NI","es-PA","es-PE","es-PR","es-PY","es-SV","es-US","es-UY","es-VE","et-EE","eu-ES","fa-IR"
            ,"fi-FI","fil-PH","fr-BE","fr-CA","fr-CH","fr-FR","ga-IE","gl-ES","gu-IN","he-IL","hi-IN","hr-HR","hu-HU","hy-AM","id-ID","is-IS","it-CH","it-IT"
            ,"ja-JP","jv-ID","ka-GE","kk-KZ","km-KH","kn-IN","ko-KR","lo-LA","lt-LT","lv-LV","mk-MK","ml-IN","mn-MN","mr-IN","ms-MY","mt-MT","my-MM","nb-NO"
            ,"ne-NP","nl-BE","nl-NL","pa-IN","pl-PL","ps-AF","pt-BR","pt-PT","ro-RO","ru-RU","si-LK","sk-SK","sl-SI","so-SO","sq-AL","sr-RS","sv-SE","sw-KE"
            ,"sw-TZ","ta-IN","te-IN","th-TH","tr-TR","uk-UA","ur-IN","uz-UZ","vi-VN","wuu-CN","yue-CN","zh-CN","zh-CN-shandong","zh-CN-sichuan","zh-HK","zh-TW","zu-ZA"
            ];

        public List<string> AzureSpeechServiceLanguageSelection { get; } = ["ar-SA", "cs-CZ", "de-DE", "el-GR", "en-GB", "es-ES", "fr-FR", "hu-HU", "it-IT", "nl-NL", "pl-PL", "pt-PT", "ru-RU", "sk-SK"];
    }
}
