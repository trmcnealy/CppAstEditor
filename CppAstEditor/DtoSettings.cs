using System.Collections.Specialized;

namespace CppAstEditor
{
    public sealed class DtoSettings
    {
        public StringCollection Defines { get; set; }

        public StringCollection AdditionalArguments { get; set; }

        public StringCollection IncludeFolders { get; set; }

        public StringCollection SystemIncludeFolders { get; set; }

        public DtoSettings()
        {
        }

        public DtoSettings(StringCollection defines,
                           StringCollection additionalArguments,
                           StringCollection includeFolders,
                           StringCollection systemIncludeFolders)
        {
            Defines              = defines;
            AdditionalArguments  = additionalArguments;
            IncludeFolders       = includeFolders;
            SystemIncludeFolders = systemIncludeFolders;
        }

        internal DtoSettings(Settings settings)
        {
            Defines              = settings.Defines;
            AdditionalArguments  = settings.AdditionalArguments;
            IncludeFolders       = settings.IncludeFolders;
            SystemIncludeFolders = settings.SystemIncludeFolders;
        }
    }
}
