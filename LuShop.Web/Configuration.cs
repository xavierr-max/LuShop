using MudBlazor;
using MudBlazor.Utilities;

namespace LuShop.Web;

public static class Configuration
{
    public static bool ThemeMode = true;
    public const string HttpClientName = "lushop";
    public static string BackendUrl { get; set; } = "http://localhost:5047";
    
    public static MudTheme Theme = new()
    {
        Typography = new Typography
        {
            Default = new DefaultTypography()
            {
                FontFamily = new[] { "Raleway", "Montserrat", "sans-serif" },
                FontSize = "0.875rem",
                FontWeight = "400" // Correção: Deve ser string
            },
            H6 = new H6Typography()
            {
                FontWeight = "700", // Correção: Deve ser string
                LetterSpacing = ".1rem"
            },
            Button = new ButtonTypography()
            {
                // TextTransform removido (não existe no objeto C#). 
                // Botões do MudBlazor já são uppercase por padrão.
                FontWeight = "600" // Correção: Deve ser string
            }
        },
        PaletteLight = new PaletteLight
        {
            // --- TEMA SOFISTICADO (Light Luxury) ---
            
            // Elementos principais em Cinza Carvão
            Primary = new MudColor("#2C2C2C"), 
            PrimaryContrastText = Colors.Shades.White,

            // Detalhes em Dourado Muted
            Secondary = new MudColor("#A69076"), 

            // Fundo Off-white
            Background = new MudColor("#F9F9F9"),
            
            // Superfícies em Branco Puro
            Surface = Colors.Shades.White,
            
            // Appbar Clean
            AppbarBackground = Colors.Shades.White,
            AppbarText = new MudColor("#2C2C2C"),

            // Drawer Clean
            DrawerBackground = Colors.Shades.White,
            DrawerText = new MudColor("#616161"),
            
            // Linhas
            LinesDefault = new MudColor("#E0E0E0"),
            
            // Cores de Estado
            Info = Colors.LightBlue.Default,
            Success = Colors.Green.Default,
            Warning = Colors.Orange.Default,
            Error = Colors.Red.Default,
        },
        PaletteDark = new PaletteDark
        {
            // --- TEMA ESCURO (Dark Luxury) ---
            
            // Dourado como destaque principal
            Primary = new MudColor("#DEC286"), 
            PrimaryContrastText = new MudColor("#000000"),

            // Detalhes em Cinza Prata
            Secondary = new MudColor("#4A4136"), 
            
            // Fundo Preto Profundo
            Background = new MudColor("#121212"), 
            Surface = new MudColor("#1E1E1E"), 
            
            // Textos
            TextPrimary = new MudColor("#E0E0E0"), 
            TextSecondary = new MudColor("#BDBDBD"), 
            
            // Appbar Dark
            AppbarBackground = new MudColor("#1E1E1E"), 
            AppbarText = new MudColor("#DEC286"),

            // Drawer Dark
            DrawerBackground = new MudColor("#1E1E1E"),
            DrawerText = new MudColor("#BDBDBD"),
            
            LinesDefault = new MudColor("#333333"),
            Divider = new MudColor("#292929"),
        }
    };
}