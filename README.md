# MonumentMap
Projekat iz predmeta "Interakcija čovek računar".

WPF Aplikacija za vođenje evidencije o geografskoj distribuciji svetskih prirodnih spomenika, odnosno mesta izuzetne prirodne lepote i značaja. Distribucija realizovana preko "Bing"-ove mape sveta na koju se prevlače i spuštaju simboli različitih spomenika, nakon što se ti simboli dodaju.

Uputstvo za pokretanje:
- Korišćena je "Bing" mapa, tako da je neophodno uključiti biblioteku "Microsoft.Maps.MapControl.WPF.dll", smeštenu u početnom direktorijumu projekta (za VS: Project -> Add reference -> Microsoft.Maps.MapControl.WPF.dll).
- Ukoliko i tad dođe do problema, najbolje bi bilo skinuti i instalirati mape sa sledećeg linka:
https://www.microsoft.com/en-us/download/details.aspx?id=27165 , pa potom uključiti istu biblioteku iz instalacionog foldera.
- Korišćen je i "Color picker" iz "WpfToolKit Extended" biblioteke. Postoji mogućnost da je neophodno instalirati i ovu biblioteku (Za VS: Project -> Manage NuGet Packages -> Pronaći i instalirati Extended WPF Toolkit biblioteku).
- MainWindow.xaml file ponekad zna da prijavi "Invalid Markup", iako nema nikakve greške i ne bude nikakvih problema prilikom pokretanja aplikacije.
- Internet konekcija je neophodna kako se ne bi prikazivao "Invalid credentials..." dijalog preko mape. 
