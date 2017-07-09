using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FoE.Farmer.Library.Windows
{
    /// <summary>
    /// Pro použití tohoto vlastního ovládacího prvku v souboru XAML postupujte podle kroků 1a nebo 1b a pak 2.
    ///
    /// Krok 1a) Použití tohoto vlastního ovládacího prvku v XAML souboru, který už je v aktuálním projektu.
    /// Přidejte tento XmlNamespace atribut do kořenového elementu označovacího souboru, kde 
    /// bude použit:
    ///
    ///     xmlns:MyNamespace="clr-namespace:FoE.Farmer.Library.Windows"
    ///
    ///
    /// Krok 1b) Použití tohoto vlastního ovládacího prvku v souboru XAML, který je v jiném projektu.
    /// Přidejte tento XmlNamespace atribut do kořenového elementu označovacího souboru, kde 
    /// bude použit:
    ///
    ///     xmlns:MyNamespace="clr-namespace:FoE.Farmer.Library.Windows;assembly=FoE.Farmer.Library.Windows"
    ///
    /// Budete taky muset přidat odkaz na projekt z projektu, kde se nachází XAML soubor,
    /// ve kterém se soubor XAML nachází, a pro vyloučení chyb kompilace projekt znovu sestavit:
    ///
    ///     V Průzkumníku řešení klikněte pravým tlačítkem na cílový projekt a
    ///     v nabídce "Přidat odkaz"->"Projekty"->[Vyberte tento projekt]
    ///
    ///
    /// Krok 2)
    /// Pokračujte dále a použijte svůj ovládací prvek v souboru XAML.
    ///
    ///     <MyNamespace:CustomControl1/>
    ///
    /// </summary>
    public class CustomControl1 : Control
    {
        static CustomControl1()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomControl1), new FrameworkPropertyMetadata(typeof(CustomControl1)));
        }
    }
}
